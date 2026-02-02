using UnityEngine;

public class Missile : MonoBehaviour
{
    [SerializeField] private float thrust = 1000f;
    [SerializeField] private float turnRate = 180f;
    [SerializeField] private float armedDistance = 100f;
    [SerializeField] private float proximityFuseRadius = 50f;
    [SerializeField] private float maxLifetime = 30f;
    [SerializeField] private float explosionRadius = 100f;
    
    private Transform target;
    private float damage;
    private float creationTime;
    private bool armed = false;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Transform targetTransform, float damageAmount)
    {
        target = targetTransform;
        damage = damageAmount;
        creationTime = Time.time;
    }

    private void FixedUpdate()
    {
        if (Time.time - creationTime > maxLifetime)
        {
            Detonate();
            return;
        }

        if (target == null)
        {
            // Target lost - self destruct
            Destroy(gameObject);
            return;
        }

        // Check if missile has traveled far enough to arm
        if (!armed && Vector3.Distance(transform.position, target.position) >= armedDistance)
        {
            armed = true;
        }

        // Calculate steering
        Vector3 targetDirection = (target.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        
        // Apply rotation
        rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, turnRate * Time.fixedDeltaTime);

        // Apply thrust in forward direction
        rb.AddForce(transform.forward * thrust);

        // Check proximity fuse
        if (armed && Vector3.Distance(transform.position, target.position) <= proximityFuseRadius)
        {
            Detonate();
        }
    }

    private void Detonate()
    {
        // Find all colliders in explosion radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (Collider hitCollider in hitColliders)
        {
            IDamageable damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Calculate damage falloff based on distance
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                float damageMultiplier = 1f - (distance / explosionRadius);
                damageable.TakeDamage(damage * damageMultiplier);
            }
        }

        // TODO: Spawn explosion effect

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (armed)
        {
            Detonate();
        }
    }
}
