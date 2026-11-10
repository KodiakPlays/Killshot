// =================================================================================================
// PDCBullet.cs
//
// What it does:
// One bullet from the older physics PDC, the PointDefenseCanon script. No prefab has this script.
// The battle board does not use this script. The board's PDCs are in WeaponSystem.
//
// How it works:
// - The PDC that fires it gives it a speed, a direction and a damage amount.
// - Every frame it moves itself along that line.
// - It removes itself after 3 seconds.
// - When it passes into something, it deals its damage through IDamageable, if that thing can
//   take damage. Then it removes itself. Unity only reports this when one of the two is set as
//   a trigger.
//
// Technical:
// - Initialize(velocity, damage) is called by PointDefenseCanon, right after it makes the bullet.
//   The velocity is in units per second.
// - It moves by setting transform.position each frame, outside physics.
// - Trap: Unity needs a Rigidbody on the bullet or on the thing hit, or no hit is reported.
// - Trap: a fast bullet can skip past a thin collider between two frames.
// - Trap: it does not skip the ship that fired it. Any trigger touch removes it.
// - It does not ask CanBeDamaged before calling IDamageable.TakeDamage.
// - Numbers: the 3-second life is fixed in code. Speed and damage come from PointDefenseCanon.
// =================================================================================================

using UnityEngine;

public class PDCBullet : MonoBehaviour
{
    private Vector3 velocity;
    private float damage;
    private float maxLifetime = 3f;
    private float creationTime;

    public void Initialize(Vector3 initialVelocity, float damageAmount)
    {
        velocity = initialVelocity;
        damage = damageAmount;
        creationTime = Time.time;
    }

    private void Update()
    {
        transform.position += velocity * Time.deltaTime;

        if (Time.time - creationTime > maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
