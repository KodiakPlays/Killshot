using UnityEngine;

public class LaserWeapon : WeaponBase
{
    [Header("Laser Weapon Settings")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float laserSpeed = 50f;
    
    [Header("Power Integration")]
    [SerializeField] private float minPowerToFire = 0.2f;

    private bool isRecharging;
    private float rechargeTimer;

    protected override void Start()
    {
        base.Start();
        
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    void Update()
    {
        if (isRecharging)
        {
            rechargeTimer += Time.deltaTime;
            if (rechargeTimer >= reloadTime)
            {
                currentAmmo = maxAmmo;
                isRecharging = false;
            }
        }
        else if (currentAmmo <= 0)
        {
            isRecharging = true;
            rechargeTimer = 0f;
        }
    }

    public bool TryFire(float weaponPowerEfficiency)
    {
        if (currentAmmo <= 0 || 
            Time.time - lastFireTime < fireRate || 
            isRecharging || 
            weaponPowerEfficiency < minPowerToFire ||
            laserPrefab == null)
        {
            return false;
        }

        FireLaser(firePoint.forward);
        return true;
    }

    private void FireLaser(Vector3 fireDirection)
    {
        lastFireTime = Time.time;
        currentAmmo--;
        
        // Get spawn position
        Vector3 spawnPos = firePoint.position;
        
        // Instantiate laser
        GameObject laser = Instantiate(laserPrefab, spawnPos, Quaternion.LookRotation(fireDirection));
        
        // Get Laser component and fire it
        Laser laserScript = laser.GetComponent<Laser>();
        if (laserScript != null)
        {
            laserScript.Fire(fireDirection, false);
        }
        else
        {
            // Fallback for legacy laser prefabs
            Rigidbody rb = laser.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = laser.AddComponent<Rigidbody>();
                rb.useGravity = false;
            }
            rb.AddForce(fireDirection * laserSpeed, ForceMode.Impulse);
            laser.tag = "PlayerProjectile";
            Destroy(laser, 5f);
        }
    }

    public override void Fire(Vector3 target)
    {
        if (!CanFire()) return;
        
        // Calculate direction to target
        Vector3 fireDirection = (target - firePoint.position).normalized;
        
        FireLaser(fireDirection);
    }

    public override bool CanFire()
    {
        return base.CanFire() && !isRecharging && laserPrefab != null;
    }

    // Public getters for UI
    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => maxAmmo;
    public bool IsRecharging() => isRecharging;
    public float GetRechargeProgress() => isRecharging ? (rechargeTimer / reloadTime) : 1f;
    public float GetMinPowerToFire() => minPowerToFire;
}
