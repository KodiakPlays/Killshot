// =================================================================================================
// Missile.cs
//
// What it does:
// One guided torpedo for the older physics weapons, fired by MissileLauncher. It chases a target
// ship and blows up near it. It is on the Missile prefab, and no scene or other prefab uses that
// prefab. The battle board does not use this script.
//
// How it works:
// - MissileLauncher gives it a target and a damage amount.
// - Every physics step it turns toward the target at up to 180 degrees per second and pushes
//   forward.
// - It arms once it is 100 units or more from the target.
// - Once armed, it blows up within 50 units of the target, or on touching anything.
// - The blast reaches 100 units. Each thing in reach that can take damage, through IDamageable,
//   is hurt. The full damage is scaled down by its distance from the blast.
// - After 30 seconds it blows up anyway. If the target is gone, it removes itself with no blast.
//
// Technical:
// - Initialize(target, damage) is called by MissileLauncher, right after it makes the torpedo.
// - It steers its Rigidbody with transform.forward as the nose. It needs a Rigidbody on the
//   prefab. It does not add one.
// - The blast uses Physics.OverlapSphere and calls IDamageable.TakeDamage on each collider in
//   reach.
// - Numbers: push (1,000), turn rate (180), arming and fuse distances, life (30 s) and blast
//   radius are in the Inspector.
// - Trap: arming counts distance to the target. Fired from over 100 units away, it is armed at
//   once. A touch on the launching ship sets it off.
// - Trap: no top speed. The prefab's Rigidbody has no drag. It speeds up every step.
// - Trap: the blast does not skip the launching ship. A ship with several colliders is hurt once
//   per collider.
// - Trap: the falloff measures to each object's position. An object more than 100 units away,
//   with a collider in reach, gets negative damage.
// =================================================================================================

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
