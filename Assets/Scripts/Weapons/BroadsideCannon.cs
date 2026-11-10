// =================================================================================================
// BroadsideCannon.cs
//
// What it does:
// An older physics weapon: guns on the left and right sides of the player's ship. While fire is
// held, each side locks onto an enemy beside the ship and fires a shell at it. It is on the
// PlayerShip prefab, one of the guns WeaponManager switches between. The battle board does not
// use this script.
//
// How it works:
// - It is built on WeaponBase. That gives it range, reload time, ammo and damage settings.
// - WeaponManager tells it fire is pressed, if it has ammo left. Fire counts as held for 0.2
//   seconds after each press. When fire is let go, both sides lose their locks.
// - While fire is held, every frame each side looks for EnemyShips within range and beside the
//   ship: more than 45 degrees off the nose and off the tail. It picks the one nearest straight
//   out from that side.
// - Each side locks on after 2 seconds on the same target. A new target starts the count again.
// - A locked side fires once its reload time has passed and ammo is left. Each shot uses one ammo
//   and gives a controller buzz through ControllerHaptics. The lock then starts again.
// - The shot goes straight out sideways from that side's fire point at 100 units per second.
//   It works with a Laser shot or a Shell shot. A Shell gets the base damage times the damage
//   factor.
// - WeaponUIDisplay asks it for each side's lock and the lock progress.
//
// Technical:
// - It extends WeaponBase. Fire(point) only stamps lastFireInputTime. The real firing is in
//   Update.
// - WeaponManager.FireActiveWeapon calls Fire after WeaponBase.CanFire passes.
// - WeaponUIDisplay reads IsPortLocked, IsStarboardLocked, HasPortTarget, HasStarboardTarget and
//   GetLockProgress.
// - Targets: Physics.OverlapSphere at range, then EnemyShip on each hit collider or its parents.
//   This runs twice per frame while fire is held.
// - Numbers: shell speed (100) and lock time (2 s) are in the Inspector. Range, reload, ammo and
//   damage are WeaponBase fields.
// - The 0.2-second hold window and the 45-degree arc are fixed in code.
// - Trap: the shot flies straight out sideways. It is not aimed at the locked ship.
// - Trap: both sides share one ammo count. Ammo never refills.
// - Trap: on the PlayerShip prefab its range, ammo, reload and damage are 0. It never fires there.
// - Trap: WeaponManager buzzes the controller on every fire press. Each shot buzzes again.
// =================================================================================================

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
