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
