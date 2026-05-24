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
    [SerializeField] private float lockTime = 2.0f;          // Time in seconds to paint/lock on target

    // Per-side fire tracking (reloadTime from WeaponBase controls the rate)
    private float portLastFireTime = -999f;
    private float starboardLastFireTime = -999f;

    // Per-side current targets (set each frame)
    private EnemyShip portTarget;
    private EnemyShip starboardTarget;

    // Per-side lock-on state
    private float portLockProgress = 0f;
    private bool portLocked = false;

    private float starboardLockProgress = 0f;
    private bool starboardLocked = false;

    // Hold fire detection
    private float lastFireInputTime = -999f;

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        bool isHoldingFire = (Time.time - lastFireInputTime) <= 0.2f;

        if (isHoldingFire)
        {
            UpdateTargetingAndLock();
            TryFireBroadside();
        }
        else
        {
            ClearLocks();
        }
    }

    // Required by WeaponBase — proxies into the auto-fire logic
    public override void Fire(Vector3 targetPos)
    {
        lastFireInputTime = Time.time;
    }

    private void UpdateTargetingAndLock()
    {
        // 1. Port side targeting and lock
        EnemyShip bestPort = GetTargetClosestToCenterLine(true);
        if (bestPort == null)
        {
            portTarget = null;
            portLockProgress = 0f;
            portLocked = false;
        }
        else
        {
            if (portTarget != bestPort)
            {
                portTarget = bestPort;
                portLockProgress = 0f;
                portLocked = false;
            }

            if (!portLocked)
            {
                portLockProgress += Time.deltaTime;
                if (portLockProgress >= lockTime)
                {
                    portLocked = true;
                    portLockProgress = lockTime;
                }
            }
        }

        // 2. Starboard side targeting and lock
        EnemyShip bestStarboard = GetTargetClosestToCenterLine(false);
        if (bestStarboard == null)
        {
            starboardTarget = null;
            starboardLockProgress = 0f;
            starboardLocked = false;
        }
        else
        {
            if (starboardTarget != bestStarboard)
            {
                starboardTarget = bestStarboard;
                starboardLockProgress = 0f;
                starboardLocked = false;
            }

            if (!starboardLocked)
            {
                starboardLockProgress += Time.deltaTime;
                if (starboardLockProgress >= lockTime)
                {
                    starboardLocked = true;
                    starboardLockProgress = lockTime;
                }
            }
        }
    }

    private void TryFireBroadside()
    {
        if (portLocked && currentAmmo > 0 && Time.time - portLastFireTime >= reloadTime)
        {
            FireProjectile(portFirePoint, -transform.right);
            portLastFireTime = Time.time;
            currentAmmo--;
            ControllerHaptics.BroadsideFired();

            // Reset lock after firing so it has to re-paint
            portLocked = false;
            portLockProgress = 0f;
        }

        if (starboardLocked && currentAmmo > 0 && Time.time - starboardLastFireTime >= reloadTime)
        {
            FireProjectile(starboardFirePoint, transform.right);
            starboardLastFireTime = Time.time;
            currentAmmo--;
            ControllerHaptics.BroadsideFired();

            // Reset lock after firing so it has to re-paint
            starboardLocked = false;
            starboardLockProgress = 0f;
        }
    }

    private void ClearLocks()
    {
        portTarget = null;
        portLockProgress = 0f;
        portLocked = false;

        starboardTarget = null;
        starboardLockProgress = 0f;
        starboardLocked = false;
    }

    private EnemyShip GetTargetClosestToCenterLine(bool isPortSide)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        EnemyShip bestEnemy = null;
        float bestDot = -1f;

        Vector3 centerLineDir = isPortSide ? -transform.right : transform.right;

        foreach (Collider col in hits)
        {
            EnemyShip enemy = col.GetComponentInParent<EnemyShip>();
            if (enemy == null) continue;

            Vector3 toEnemy = enemy.transform.position - transform.position;
            float dist = toEnemy.magnitude;

            if (dist > range) continue;

            // Only consider enemies in the broadside arc (45°–135° from bow).
            float forwardDot = Vector3.Dot(transform.up, toEnemy.normalized);
            if (Mathf.Abs(forwardDot) > 0.707f) continue;

            float sideDot = Vector3.Dot(transform.right, toEnemy);
            if (isPortSide && sideDot >= 0f) continue;
            if (!isPortSide && sideDot < 0f) continue;

            float dot = Vector3.Dot(centerLineDir, toEnemy.normalized);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestEnemy = enemy;
            }
        }
        return bestEnemy;
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

    public bool IsPortLocked() => portLocked;
    public bool IsStarboardLocked() => starboardLocked;

    public float GetPortLockProgress() => portTarget != null ? (portLockProgress / lockTime) : 0f;
    public float GetStarboardLockProgress() => starboardTarget != null ? (starboardLockProgress / lockTime) : 0f;

    public bool IsLocked() => portLocked || starboardLocked;

    public float GetLockProgress()
    {
        if (lockTime <= 0f) return 1f;
        float portProg = portTarget != null ? (portLockProgress / lockTime) : 0f;
        float stbdProg = starboardTarget != null ? (starboardLockProgress / lockTime) : 0f;
        return Mathf.Max(portProg, stbdProg);
    }
}
