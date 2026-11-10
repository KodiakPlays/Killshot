// =================================================================================================
// BoardingPod.cs
//
// What it does:
// A small craft for the older physics weapons, fired by BoardingPodLauncher. It flies to a target
// ship, sticks to it, and deals damage after a wait. No prefab has this script.
// The battle board does not use this script.
//
// How it works:
// - BoardingPodLauncher gives it a target, a starting speed and a damage amount.
// - Every physics step it turns toward the target at 45 degrees per second and keeps its speed.
// - It removes itself after 20 seconds, or when the target is gone.
// - If it touches the target, it gives the target a push, sticks to it and stops moving.
// - Stuck on, it waits 5 seconds, deals its damage through IDamageable, and removes itself.
// - If it touches anything else, it removes itself.
//
// Technical:
// - Initialize(target, starting velocity, damage) is called by BoardingPodLauncher, right after
//   it makes the pod.
// - It steers with transform.forward as its nose. The older ships use transform.up.
// - It needs a Rigidbody on the prefab. It does not add one.
// - On a hit it makes itself a child of the target and sets its Rigidbody to kinematic.
// - The damage goes only to an IDamageable on the target object itself. Its parents are not
//   checked.
// - Numbers: turn rate (45), lifetime (20 s), push (1,000) and boarding wait (5 s) are in the
//   Inspector.
// - Trap: the 20-second limit keeps counting after it sticks. A pod that sticks after 15 seconds
//   deals no damage.
// - Times use Time.time and the physics step, both scaled time.
// =================================================================================================

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
