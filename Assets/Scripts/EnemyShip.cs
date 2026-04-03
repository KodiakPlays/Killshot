using UnityEngine;

public class EnemyShip : MonoBehaviour, IDamageable
{
    // ── State machine ─────────────────────────────────────────────────────────
    private enum EnemyState { Patrol, Chase, Combat }

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Detection and Combat")]
    public float detectionRadius = 30f;
    public float optimalCombatDistance = 15f;
    public GameObject laserPrefab;
    public Transform firePoint;
    public float fireRate = 0.5f;
    public int maxHealth = 3;

    [Header("Patrol")]
    [Tooltip("If true, the enemy wanders between waypoints. Disable to keep it stationary until the player is detected.")]
    public bool patrolEnabled = true;
    [Tooltip("How far the enemy wanders from its spawn position.")]
    public float patrolRadius = 40f;
    [Tooltip("Distance at which a patrol waypoint is considered reached.")]
    public float waypointReachedThreshold = 3f;
    [Tooltip("How far ahead to look for obstacles.")]
    public float obstacleAvoidanceDistance = 8f;
    [Tooltip("Layer mask for obstacles the enemy should avoid. Exclude player/projectile layers.")]
    public LayerMask obstacleLayerMask = ~0;

    [Header("Movement Settings")]
    public float thrustForce = 8f;
    public float rotationSpeed = 90f;
    public float maxSpeed = 15f;
    public float orbitForce = 5f;

    // ── Private state ─────────────────────────────────────────────────────────
    private Transform playerTransform;
    private Rigidbody rb;
    private float lastFireTime;
    private int currentHealth;

    private EnemyState state = EnemyState.Patrol;
    private Vector3 patrolOrigin;
    private Vector3 patrolWaypoint;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        currentHealth = maxHealth;
        patrolOrigin = transform.position;
        PickNewPatrolWaypoint();
    }

    private void FixedUpdate()
    {
        // No player reference — patrol if enabled, otherwise stay put
        if (playerTransform == null)
        {
            if (patrolEnabled) UpdatePatrol();
            EnforceSpeedLimit();
            return;
        }

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        UpdateState(distToPlayer);

        switch (state)
        {
            case EnemyState.Patrol: if (patrolEnabled) UpdatePatrol(); break;
            case EnemyState.Chase:  UpdateChase();               break;
            case EnemyState.Combat: UpdateCombat(distToPlayer);  break;
        }

        EnforceSpeedLimit();
    }

    // ── State transitions ─────────────────────────────────────────────────────
    private void UpdateState(float distToPlayer)
    {
        switch (state)
        {
            case EnemyState.Patrol:
                if (distToPlayer <= detectionRadius)
                    state = EnemyState.Chase;
                break;

            case EnemyState.Chase:
                if (distToPlayer > detectionRadius * 1.15f)
                {
                    // Player left detection range — resume patrol from current position
                    patrolOrigin = transform.position;
                    PickNewPatrolWaypoint();
                    state = EnemyState.Patrol;
                }
                else if (distToPlayer <= optimalCombatDistance)
                    state = EnemyState.Combat;
                break;

            case EnemyState.Combat:
                if (distToPlayer > detectionRadius * 1.15f)
                {
                    patrolOrigin = transform.position;
                    PickNewPatrolWaypoint();
                    state = EnemyState.Patrol;
                }
                else if (distToPlayer > optimalCombatDistance * 1.2f)
                    state = EnemyState.Chase;
                break;
        }
    }

    // ── Patrol behavior ───────────────────────────────────────────────────────
    private void PickNewPatrolWaypoint()
    {
        Vector2 offset = Random.insideUnitCircle * patrolRadius;
        patrolWaypoint = patrolOrigin + new Vector3(offset.x, offset.y, 0f);
    }

    private void UpdatePatrol()
    {
        Vector3 toWaypoint = patrolWaypoint - transform.position;
        toWaypoint.z = 0f;

        if (toWaypoint.magnitude < waypointReachedThreshold)
        {
            PickNewPatrolWaypoint();
            return;
        }

        Vector3 steerDir = GetObstacleAvoidedDirection(toWaypoint.normalized);
        RotateTowards(steerDir);
        rb.AddForce(transform.up * thrustForce * 0.5f, ForceMode.Force);
    }

    // ── Chase behavior ────────────────────────────────────────────────────────
    private void UpdateChase()
    {
        Vector3 dirToPlayer = (playerTransform.position - transform.position);
        dirToPlayer.z = 0f;
        dirToPlayer.Normalize();

        Vector3 steerDir = GetObstacleAvoidedDirection(dirToPlayer);
        RotateTowards(steerDir);
        rb.AddForce(transform.up * thrustForce, ForceMode.Force);
    }

    // ── Combat behavior ───────────────────────────────────────────────────────
    private void UpdateCombat(float distToPlayer)
    {
        Vector3 dirToPlayer = (playerTransform.position - transform.position);
        dirToPlayer.z = 0f;
        dirToPlayer.Normalize();

        // Always face the player
        RotateTowards(dirToPlayer);

        // Maintain combat distance: back off if too close, orbit if at range
        if (distToPlayer < optimalCombatDistance * 0.7f)
        {
            rb.AddForce(-transform.up * thrustForce, ForceMode.Force);
        }
        else
        {
            // Strafe perpendicular to the player to orbit
            Vector3 orbitDir = Vector3.Cross(dirToPlayer, Vector3.forward).normalized;
            rb.AddForce(orbitDir * orbitForce, ForceMode.Force);
        }

        // Fire when facing the player and cooldown has elapsed
        bool facingPlayer = Vector3.Dot(transform.up, dirToPlayer) > 0.7f;
        if (facingPlayer && Time.time - lastFireTime > fireRate)
            FireLaser();
    }

    // ── Obstacle avoidance ────────────────────────────────────────────────────
    /// <summary>
    /// Casts a fan of five rays (±30°, ±60°, centre) around <paramref name="desiredDir"/>
    /// and returns the direction of the clearest ray. Falls back to
    /// <paramref name="desiredDir"/> when the centre ray is unobstructed.
    /// </summary>
    private Vector3 GetObstacleAvoidedDirection(Vector3 desiredDir)
    {
        // angles relative to desired direction, in the XY plane (Z-axis rotation)
        float[] angles  = { -60f, -30f, 0f, 30f, 60f };
        float   maxClearance = 0f;
        int     bestIndex    = 2; // default: straight ahead

        for (int i = 0; i < angles.Length; i++)
        {
            Vector3 rayDir = Quaternion.Euler(0f, 0f, angles[i]) * desiredDir;
            float clearance;

            if (Physics.Raycast(transform.position, rayDir, out RaycastHit hit,
                                obstacleAvoidanceDistance, obstacleLayerMask,
                                QueryTriggerInteraction.Ignore))
                clearance = hit.distance;
            else
                clearance = obstacleAvoidanceDistance;

            if (clearance > maxClearance)
            {
                maxClearance = clearance;
                bestIndex    = i;
            }
        }

        // Centre ray is sufficiently clear — keep original heading
        if (bestIndex == 2 || maxClearance > obstacleAvoidanceDistance * 0.5f && bestIndex == 2)
            return desiredDir;

        return (Quaternion.Euler(0f, 0f, angles[bestIndex]) * desiredDir).normalized;
    }

    // ── Shared movement helpers ───────────────────────────────────────────────
    private void RotateTowards(Vector3 direction)
    {
        direction.z = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(Vector3.forward, direction);
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
    }

    private void EnforceSpeedLimit()
    {
        Vector3 vel = rb.linearVelocity;
        Vector3 flat = new Vector3(vel.x, vel.y, 0f);
        if (flat.magnitude > maxSpeed)
        {
            flat = flat.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(flat.x, flat.y, vel.z);
        }
    }

    // ── Firing ────────────────────────────────────────────────────────────────
    private void FireLaser()
    {
        if (playerTransform == null || laserPrefab == null) return;

        lastFireTime = Time.time;
        Vector3 spawnPos = firePoint ? firePoint.position : transform.position;
        Vector3 dirToPlayer = (playerTransform.position - spawnPos).normalized;
        Quaternion spawnRot = Quaternion.LookRotation(Vector3.forward, dirToPlayer);

        GameObject laserObj = Instantiate(laserPrefab, spawnPos, spawnRot);
        Laser laser = laserObj.GetComponent<Laser>();
        if (laser != null)
            laser.Fire(dirToPlayer, true); // true = enemy projectile
    }

    // ── IDamageable ───────────────────────────────────────────────────────────
    // Called directly by Laser.cs (int overload kept for backward compatibility)
    public void TakeDamage(int damage = 1)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            Explode();
    }

    float IDamageable.TakeDamage(float amount)
    {
        currentHealth -= Mathf.RoundToInt(amount);
        if (currentHealth <= 0)
            Explode();
        return amount;
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth()     => maxHealth;
    public bool  CanBeDamaged()     => currentHealth > 0;

    // ── Destruction ───────────────────────────────────────────────────────────
    private void Explode()
    {
        GameObject explosionObj = new GameObject("Explosion");
        explosionObj.transform.position = transform.position;
        ParticleSystem particles = explosionObj.AddComponent<ParticleSystem>();

        var main = particles.main;
        main.startSize     = 2f;
        main.startSpeed    = 5f;
        main.startLifetime = 1f;
        main.maxParticles  = 100;

        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 50));

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.1f;

        Destroy(explosionObj, main.startLifetime.constant);
        Destroy(gameObject);
    }

    // OnTriggerEnter removed — collision handling is done by projectile scripts (Laser.cs, Shell.cs, etc.)
    // Having it here caused double-damage and race conditions with Destroy calls.

    // ── Editor gizmos ─────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        // Detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Combat distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, optimalCombatDistance);

        // Patrol radius (centred on spawn origin at runtime, otherwise on current position)
        Gizmos.color = Color.cyan;
        Vector3 origin = Application.isPlaying ? patrolOrigin : transform.position;
        Gizmos.DrawWireSphere(origin, patrolRadius);

        // Current patrol waypoint
        if (Application.isPlaying && state == EnemyState.Patrol)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(patrolWaypoint, 0.5f);
            Gizmos.DrawLine(transform.position, patrolWaypoint);
        }
    }
}
