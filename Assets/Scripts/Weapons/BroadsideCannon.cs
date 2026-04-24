using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BroadsideCannon : WeaponBase
{
    [Header("Broadside Settings")]
    [SerializeField] private Transform portFirePoint;       // Left / port side
    [SerializeField] private Transform starboardFirePoint;  // Right / starboard side
    [SerializeField] private GameObject shellPrefab;
    [SerializeField] private float shellSpeed = 100f;

    // Per-side fire tracking (reloadTime from WeaponBase controls the rate)
    private float portLastFireTime = -999f;
    private float starboardLastFireTime = -999f;

    // Per-side current targets (set each frame)
    private EnemyShip portTarget;
    private EnemyShip starboardTarget;

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        ScanForTargets();
        AutoFire();
    }

    // Required by WeaponBase — proxies into the auto-fire logic
    public override void Fire(Vector3 targetPos)
    {
        ScanForTargets();
        AutoFire();
    }

    private void ScanForTargets()
    {
        portTarget = null;
        starboardTarget = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, range);

        float portBestDist = float.MaxValue;
        float starboardBestDist = float.MaxValue;

        foreach (Collider col in hits)
        {
            EnemyShip enemy = col.GetComponentInParent<EnemyShip>();
            if (enemy == null) continue;

            Vector3 toEnemy = enemy.transform.position - transform.position;
            float dist = toEnemy.magnitude;

            // Only consider enemies in the broadside arc (45°–135° from bow).
            // Enemies within 45° of the ship's heading (forward or aft) are ignored.
            float forwardDot = Vector3.Dot(transform.up, toEnemy.normalized);
            if (Mathf.Abs(forwardDot) > 0.707f) continue; // < 45° from bow/stern

            float sideDot = Vector3.Dot(transform.right, toEnemy);

            if (sideDot < 0f) // port
            {
                if (dist < portBestDist)
                {
                    portBestDist = dist;
                    portTarget = enemy;
                }
            }
            else // starboard
            {
                if (dist < starboardBestDist)
                {
                    starboardBestDist = dist;
                    starboardTarget = enemy;
                }
            }
        }
    }

    private void AutoFire()
    {
        if (portTarget != null && currentAmmo > 0 && Time.time - portLastFireTime >= reloadTime)
        {
            FireProjectile(portFirePoint, -transform.right);
            portLastFireTime = Time.time;
            currentAmmo--;
            ControllerHaptics.BroadsideFired();
        }

        if (starboardTarget != null && currentAmmo > 0 && Time.time - starboardLastFireTime >= reloadTime)
        {
            FireProjectile(starboardFirePoint, transform.right);
            starboardLastFireTime = Time.time;
            currentAmmo--;
            ControllerHaptics.BroadsideFired();
        }
    }

    private void FireProjectile(Transform firePoint, Vector3 fireDirection)
    {
        if (firePoint == null || shellPrefab == null) return;

        Quaternion rot = Quaternion.LookRotation(Vector3.forward, fireDirection);
        GameObject proj = Instantiate(shellPrefab, firePoint.position, rot);

        // Support both Laser-based and Shell-based prefabs
        Laser laser = proj.GetComponent<Laser>();
        if (laser != null)
        {
            laser.speed = shellSpeed;
            laser.Fire(fireDirection.normalized, false);
        }
        else
        {
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = fireDirection * shellSpeed;

            Shell shell = proj.GetComponent<Shell>();
            if (shell != null)
                shell.Initialize(fireDirection * shellSpeed, baseDamage * damageModifier);
        }
    }

    // UI helpers
    public bool HasPortTarget()      => portTarget != null;
    public bool HasStarboardTarget() => starboardTarget != null;
}
