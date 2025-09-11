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

    private void OnTriggerEnter(Collider other)
    {
        bool shouldDamage = false;

        // Handle enemy projectiles
        if (CompareTag("EnemyProjectile"))
        {
            // Enemy projectiles can only damage player and their shields
            Shields shields = other.GetComponent<Shields>();
            if (shields != null)
            {
                shields.TakeDamage(damage);
                shouldDamage = true;
            }
            else
            {
                PlayerShip playerShip = other.GetComponent<PlayerShip>();
                if (playerShip != null)
                {
                    playerShip.TakeDamage(damage);
                    shouldDamage = true;
                }
            }
        }
        // Handle player projectiles
        else if (CompareTag("PlayerProjectile"))
        {
            // Player projectiles can only damage enemies
            EnemyShip enemyShip = other.GetComponent<EnemyShip>();
            if (enemyShip != null)
            {
                enemyShip.TakeDamage();
                shouldDamage = true;
            }
        }

        // Destroy the laser if it hit something it can damage, or if it hit environment
        if (shouldDamage || !other.CompareTag("PlayerProjectile") && !other.CompareTag("EnemyProjectile"))
        {
            Destroy(gameObject);
        }
    }
}
