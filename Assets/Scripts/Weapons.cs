using UnityEngine;

public class Weapons : WeaponBase
{
    [Header("Weapon Settings")]
    public GameObject laserPrefab;
    public Transform firePoint;
    public float laserSpeed = 50f;
    
    [Header("Power Integration")]
    public float minPowerToFire = 0.2f;

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

        FireLaser();
        return true;
    }

    void FireLaser()
    {
        lastFireTime = Time.time;
        currentAmmo--;
        
        // Get spawn position and fire direction
        Vector3 spawnPos = firePoint.position;
        Vector3 fireDirection = firePoint.forward;
        
        // Instantiate laser
        GameObject laser = Instantiate(laserPrefab, spawnPos, Quaternion.LookRotation(fireDirection));
        
        // Get or add Rigidbody
        Rigidbody rb = laser.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = laser.AddComponent<Rigidbody>();
            rb.useGravity = false;
        }
        
        // Apply impulse force
        rb.AddForce(fireDirection * laserSpeed, ForceMode.Impulse);
        
        // Set tag for collision detection
        laser.tag = "PlayerProjectile";
        
        // Destroy after 5 seconds
        Destroy(laser, 5f);
    }

    // Implement the abstract Fire method from WeaponBase
    public override void Fire(Vector3 target)
    {
        if (!CanFire()) return;
        
        lastFireTime = Time.time;
        currentAmmo--;
        
        // Calculate direction to target
        Vector3 fireDirection = (target - firePoint.position).normalized;
        
        // Instantiate laser
        GameObject laser = Instantiate(laserPrefab, firePoint.position, Quaternion.LookRotation(fireDirection));
        
        // Get or add Rigidbody
        Rigidbody rb = laser.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = laser.AddComponent<Rigidbody>();
            rb.useGravity = false;
        }
        
        // Apply impulse force
        rb.AddForce(fireDirection * laserSpeed, ForceMode.Impulse);
        
        // Set tag for collision detection
        laser.tag = "PlayerProjectile";
        
        // Destroy after 5 seconds
        Destroy(laser, 5f);
    }

    // Public getters for UI
    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => maxAmmo;
    public bool IsRecharging() => isRecharging;
    public float GetRechargeProgress() => isRecharging ? (rechargeTimer / reloadTime) : 1f;
}
