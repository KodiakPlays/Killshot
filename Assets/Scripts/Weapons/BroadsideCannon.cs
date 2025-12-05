using UnityEngine;
using System.Linq;

public class BroadsideCannon : WeaponBase
{
    [Header("Broadside Settings")]
    [SerializeField] private float lockRadius = 100f;
    [SerializeField] private float lockTime = 2f;
    [SerializeField] private Transform leftFirePoint;
    [SerializeField] private Transform rightFirePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 100f;

    private Transform currentTarget;
    private float currentLockTime;
    private bool isLocked;
    private float lastFireCallTime;

    protected override void Start()
    {
        base.Start();
        // Ensure range is set from lockRadius if not manually set
        if (range < lockRadius) range = lockRadius;
    }

    private void Update()
    {
        // Reset lock if we haven't tried to fire (held button) for a short time
        if (Time.time - lastFireCallTime > 0.2f)
        {
            ResetLock();
        }
    }

    public override void Fire(Vector3 targetPos) // targetPos is ignored for auto-targeting
    {
        lastFireCallTime = Time.time;

        if (currentTarget == null)
        {
            FindTarget();
        }

        if (currentTarget != null)
        {
            // Check distance
            float dist = Vector3.Distance(transform.position, currentTarget.position);
            if (dist > lockRadius)
            {
                ResetLock();
                return;
            }

            // Process locking
            if (!isLocked)
            {
                currentLockTime += Time.deltaTime;
                // Optional: UI feedback for locking progress could go here
                
                if (currentLockTime >= lockTime)
                {
                    isLocked = true;
                    Debug.Log("Broadside Locked!");
                }
            }
            else
            {
                // Fire if locked
                if (CanFire())
                {
                    Shoot();
                }
            }
        }
    }

    private void FindTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, lockRadius);
        // Filter for EnemyShip
        var enemy = colliders
            .Select(c => c.GetComponentInParent<EnemyShip>())
            .Where(e => e != null)
            .OrderBy(e => Vector3.Distance(transform.position, e.transform.position))
            .FirstOrDefault();

        if (enemy != null)
        {
            currentTarget = enemy.transform;
        }
    }

    private void ResetLock()
    {
        currentTarget = null;
        currentLockTime = 0f;
        isLocked = false;
    }

    private void Shoot()
    {
        // Fire from both sides
        FireProjectile(leftFirePoint);
        FireProjectile(rightFirePoint);
        
        lastFireTime = Time.time;
        currentAmmo--;
    }

    private void FireProjectile(Transform firePoint)
    {
        if (firePoint == null || projectilePrefab == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        
        // Point at target
        if (currentTarget != null)
        {
            proj.transform.LookAt(currentTarget);
        }
        
        // Add velocity
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = proj.transform.forward * projectileSpeed;
        }
        
        // Initialize projectile script if it exists (e.g. Shell)
        var shell = proj.GetComponent<Shell>();
        if (shell != null)
        {
            shell.Initialize(proj.transform.forward * projectileSpeed, baseDamage * damageModifier);
        }
    }
    
    // Public getter for UI
    public float GetLockProgress()
    {
        if (isLocked) return 1f;
        return Mathf.Clamp01(currentLockTime / lockTime);
    }
    
    public bool IsLocked() => isLocked;
}
