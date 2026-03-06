using UnityEngine;

public class EnemyShip : MonoBehaviour, IDamageable
{
    [Header("Detection and Combat")]
    public float detectionRadius = 30f;
    public float optimalCombatDistance = 15f;
    public GameObject laserPrefab;
    public Transform firePoint;
    public float fireRate = 0.5f;
    public int maxHealth = 3;

    [Header("Movement Settings")]
    public float thrustForce = 8f;
    public float rotationSpeed = 90f;
    public float maxSpeed = 15f;
    public float orbitSpeed = 30f;

    private Transform playerTransform;
    private Rigidbody rb;
    private float lastFireTime;
    private int currentHealth;
    private bool playerDetected = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        }

        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        currentHealth = maxHealth;
    }

    private void FixedUpdate()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        playerDetected = distanceToPlayer <= detectionRadius;

        if (playerDetected)
        {
            // Calculate direction to player
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            
            // Rotate to face the player (fixed: removed negative sign so ship faces towards player)
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, directionToPlayer);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

            // Move towards optimal combat distance
            if (distanceToPlayer > optimalCombatDistance * 1.1f)
            {
                // Too far — thrust towards player
                rb.AddForce(transform.up * thrustForce, ForceMode.Force);
            }
            else if (distanceToPlayer < optimalCombatDistance * 0.5f)
            {
                // Too close — thrust away
                rb.AddForce(-transform.up * thrustForce, ForceMode.Force);
            }
            else
            {
                // At optimal range — orbit the player
                Vector3 orbitDir = Vector3.Cross(directionToPlayer, Vector3.forward).normalized;
                rb.AddForce(orbitDir * orbitSpeed, ForceMode.Force);
            }

            // Check if we're in combat range and facing the player
            bool inCombatRange = distanceToPlayer <= detectionRadius;
            bool facingPlayer = Vector3.Dot(transform.up, directionToPlayer) > 0.7f; // About 45 degrees or less

            // Fire at player if in combat range, facing player, and enough time has passed
            if (inCombatRange && facingPlayer && Time.time - lastFireTime > fireRate)
            {
                FireLaser();
            }
        }

        // Apply speed limit
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, horizontalVelocity.y, rb.linearVelocity.z);
        }
    }

    private void FireLaser()
    {
        if (playerTransform == null || laserPrefab == null) return;

        lastFireTime = Time.time;
        Vector3 spawnPos = firePoint ? firePoint.position : transform.position;
        
        // Calculate direction to player (2D — use transform.up as forward in top-down)
        Vector3 directionToPlayer = (playerTransform.position - spawnPos).normalized;
        Quaternion spawnRotation = Quaternion.LookRotation(Vector3.forward, directionToPlayer);
        
        // Spawn and orient the laser
        GameObject laserObj = Instantiate(laserPrefab, spawnPos, spawnRotation);
        Laser laser = laserObj.GetComponent<Laser>();
        if (laser != null)
        {
            laser.Fire(directionToPlayer, true);  // true indicates enemy projectile
        }
    }

    public void TakeDamage(int damage = 1)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Explode();
        }
    }

    // IDamageable implementation
    float IDamageable.TakeDamage(float amount)
    {
        currentHealth -= Mathf.RoundToInt(amount);
        if (currentHealth <= 0)
        {
            Explode();
        }
        return amount;
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool CanBeDamaged() => currentHealth > 0;

    private void Explode()
    {
        // Create simple particle effect for explosion
        ParticleSystem.MainModule mainModule;

        // Create explosion particle system
        GameObject explosionObj = new GameObject("Explosion");
        explosionObj.transform.position = transform.position;
        ParticleSystem particles = explosionObj.AddComponent<ParticleSystem>();
        
        // Configure particle system for explosion effect
        mainModule = particles.main;
        mainModule.startSize = 2f;
        mainModule.startSpeed = 5f;
        mainModule.startLifetime = 1f;
        mainModule.maxParticles = 100;
        
        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 50));
        
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;
        
        // Destroy the explosion after particles are done
        Destroy(explosionObj, mainModule.startLifetime.constant);
        
        // Destroy the enemy ship
        Destroy(gameObject);
    }

    // OnTriggerEnter removed — collision handling is done by projectile scripts (Laser.cs, Shell.cs, etc.)
    // Having it here caused double-damage and race conditions with Destroy calls.

    // Optional: Visualize detection radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, optimalCombatDistance);
    }
}
