// =================================================================================================
// Shell.cs
//
// What it does:
// One cannon shell for the older physics weapons. Macrocannon and BroadsideCannon fire it.
// No prefab has this script. The battle board does not use this script.
//
// How it works:
// - The gun that fires it gives it a speed, a direction and a damage amount.
// - Every frame it moves itself along that line.
// - It removes itself after 15 seconds.
// - When it passes into something, it deals its damage through IDamageable, if that thing can
//   take damage. Then it removes itself. Unity only reports this when one of the two is set as
//   a trigger.
//
// Technical:
// - Initialize(velocity, damage) is called by Macrocannon and BroadsideCannon, right after they
//   make the shell. The velocity is in units per second.
// - It moves by setting transform.position each frame, outside physics.
// - Trap: BroadsideCannon also sets the shell's Rigidbody velocity. A moving Rigidbody then
//   doubles the shell's speed.
// - Trap: Unity needs a Rigidbody on the shell or on the thing hit, or no hit is reported.
// - Trap: it does not skip the ship that fired it. Any trigger touch removes it.
// - Numbers: the 15-second life is fixed in code. Speed and damage come from the gun.
// =================================================================================================

using UnityEngine;

public class Shell : MonoBehaviour
{
    private Vector3 velocity;
    private float damage;
    private float maxLifetime = 15f; // Maximum time the shell can exist
    private float creationTime;

    public void Initialize(Vector3 initialVelocity, float damageAmount)
    {
        velocity = initialVelocity;
        damage = damageAmount;
        creationTime = Time.time;
    }

    private void Update()
    {
        // Move the shell
        transform.position += velocity * Time.deltaTime;

        // Check if shell has exceeded its lifetime
        if (Time.time - creationTime > maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if we hit something damageable
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        // Create impact effect here if desired

        // Destroy the shell
        Destroy(gameObject);
    }
}
