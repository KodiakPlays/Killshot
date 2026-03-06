using UnityEngine;

public class BoardingPod : MonoBehaviour
{
    [SerializeField] private float turnRate = 45f;
    [SerializeField] private float maxLifetime = 20f;
    [SerializeField] private float attachmentForce = 1000f;
    [SerializeField] private float boardingTime = 5f;
    
    private Transform target;
    private Vector3 velocity;
    private float damage;
    private float creationTime;
    private bool attached = false;
    private float attachmentTimer = 0f;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Transform targetTransform, Vector3 initialVelocity, float damageAmount)
    {
        target = targetTransform;
        velocity = initialVelocity;
        damage = damageAmount;
        creationTime = Time.time;
        rb.linearVelocity = velocity;
    }

    private void FixedUpdate()
    {
        if (Time.time - creationTime > maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (attached)
        {
            HandleAttached();
            return;
        }

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Calculate steering
        Vector3 targetDirection = (target.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        
        // Apply rotation
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnRate * Time.fixedDeltaTime);
        
        // Maintain velocity in forward direction
        rb.linearVelocity = transform.forward * velocity.magnitude;
    }

    private void HandleAttached()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        attachmentTimer += Time.fixedDeltaTime;
        if (attachmentTimer >= boardingTime)
        {
            // Boarding complete - apply damage
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            // TODO: Trigger boarding party effects/animations

            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (attached) return;

        if (collision.gameObject.transform == target)
        {
            // Attach to target
            attached = true;
            
            // Apply force at point of impact
            collision.rigidbody?.AddForceAtPosition(
                rb.linearVelocity.normalized * attachmentForce,
                collision.contacts[0].point,
                ForceMode.Impulse
            );

            // Make pod child of target
            transform.parent = target;
            
            // Disable rigidbody
            rb.isKinematic = true;
        }
        else
        {
            // Hit something else - destroy pod
            Destroy(gameObject);
        }
    }
}
