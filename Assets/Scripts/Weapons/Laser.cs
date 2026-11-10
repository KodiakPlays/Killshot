// =================================================================================================
// Laser.cs
//
// What it does:
// One laser shot for the older physics weapons. It flies as a physics object and damages the ship
// it hits. It is on the Weapon_Laser prefab, fired by the PlayerShip, EnemyShip1 and EnemyShip2
// prefabs. The battle board does not use this script.
//
// How it works:
// - At the start it adds a physics body if it has none. It removes itself after its life time:
//   3 seconds by default, 30 seconds on the Weapon_Laser prefab.
// - The gun that fires it sets its direction and marks it as a player shot or an enemy shot.
//   It flies at 30 units per second by default, 500 on the Weapon_Laser prefab. BroadsideCannon
//   changes the speed first.
// - An enemy shot that hits the player's Shields script damages that. Otherwise, if it hits the
//   PlayerShip, it damages one side of the hull, picked by the shot's direction.
// - A player shot that hits an EnemyShip takes health from it, rounded to a whole number.
// - Each hit does 20 damage by default and writes a line to the Console.
// - After a hit it removes itself. It does the same when it hits anything that is not another
//   shot. A shot that touches another shot is not removed.
//
// Technical:
// - Data: public speed (units per second), lifeTime (seconds) and damage. The Weapon_Laser prefab
//   sets 500, 30 and 20.
// - Fire(direction, isEnemy) sets the velocity and the tag. LaserWeapon and BroadsideCannon fire
//   player shots. EnemyShip fires enemy shots.
// - Speed must be set before Fire. BroadsideCannon does this.
// - Tags: "PlayerProjectile" and "EnemyProjectile". Both are in the project's tag list.
// - Hits: Shields.TakeDamage, PlayerShip.TakeDamage(damage, direction), or
//   EnemyShip.TakeDamage(whole number).
// - It looks for Shields and PlayerShip on the hit collider only. It looks for EnemyShip on the
//   collider and its parents.
// - Trap: it passes its travel direction to PlayerShip. HullSystem reads that as pointing toward
//   the hit. A shot on the nose counts as aft. On the PlayerShip prefab, Shields gets every hit
//   first.
// - The life time is counted in scaled time.
// =================================================================================================

using UnityEngine;

public class Laser : MonoBehaviour
{
    public float speed = 30f;
    public float lifeTime = 3f;
    public float damage = 20f;  // Amount of damage each laser deals

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.isKinematic = false;  // We want physics collisions
        }
    }

    public void Fire(Vector3 direction, bool isEnemyProjectile = false)
    {
        rb.linearVelocity = direction * speed;
        gameObject.tag = isEnemyProjectile ? "EnemyProjectile" : "PlayerProjectile";
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;
        bool shouldDamage = false;

        // Handle enemy projectiles
        if (CompareTag("EnemyProjectile"))
        {
            // Enemy projectiles can only damage player and their shields
            Shields shields = other.GetComponent<Shields>();
            if (shields != null)
            {
                Debug.Log($"[Laser] EnemyProjectile hit player shields on '{other.name}', dealing {damage} damage.");
                shields.TakeDamage(damage);
                shouldDamage = true;
            }
            else
            {
                PlayerShip playerShip = other.GetComponent<PlayerShip>();
                if (playerShip != null)
                {
                    Debug.Log($"[Laser] EnemyProjectile hit PlayerShip '{other.name}', dealing {damage} damage.");
                    // Pass hit direction for directional hull damage per GDD
                    Vector3 hitDirection = rb.linearVelocity.normalized;
                    playerShip.TakeDamage(damage, hitDirection);
                    shouldDamage = true;
                }
            }
        }
        // Handle player projectiles
        else if (CompareTag("PlayerProjectile"))
        {
            // Player projectiles can only damage enemies
            EnemyShip enemyShip = other.GetComponentInParent<EnemyShip>();
            if (enemyShip != null)
            {
                Debug.Log($"[Laser] PlayerProjectile hit EnemyShip '{other.name}', dealing {damage} damage.");
                enemyShip.TakeDamage(Mathf.RoundToInt(damage));
                Debug.Log($"[Laser] EnemyShip '{enemyShip.name}' remaining health: {enemyShip.GetCurrentHealth()}/{enemyShip.GetMaxHealth()}");
                shouldDamage = true;
            }
        }

        // Destroy the laser if it hit something it can damage, or if it hit environment
        if (shouldDamage || !other.CompareTag("PlayerProjectile") && !other.CompareTag("EnemyProjectile"))
        {
            if (!shouldDamage)
                Debug.Log($"[Laser] Hit non-damageable object '{other.name}' (tag: {other.tag}), destroying laser.");
            Destroy(gameObject);
        }
    }
}
