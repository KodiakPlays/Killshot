using UnityEngine;

/// <summary>
/// LEGACY COMPATIBILITY WRAPPER
/// This class maintains backward compatibility with existing code.
/// For new implementations, use WeaponManager with specific weapon types (LaserWeapon, Macrocannon, etc.)
/// 
/// This wrapper delegates to a LaserWeapon instance for actual functionality.
/// </summary>
[System.Obsolete("Use WeaponManager with LaserWeapon instead for modular weapon system")]
public class Weapons : MonoBehaviour
{
    [Header("Legacy Weapon Settings - Use LaserWeapon Instead")]
    public GameObject laserPrefab;
    public Transform firePoint;
    public float laserSpeed = 50f;
    
    [Header("Power Integration")]
    public float minPowerToFire = 0.2f;
    
    [Header("Weapon Stats")]
    public int maxAmmo = 100;
    public float reloadTime = 3f;

    // Internal LaserWeapon instance for delegation
    private LaserWeapon laserWeapon;
    private bool isInitialized = false;

    void Start()
    {
        InitializeLaserWeapon();
    }

    /// <summary>
    /// Initialize the internal LaserWeapon component
    /// </summary>
    private void InitializeLaserWeapon()
    {
        if (isInitialized) return;

        // Add LaserWeapon component if it doesn't exist
        laserWeapon = GetComponent<LaserWeapon>();
        if (laserWeapon == null)
        {
            laserWeapon = gameObject.AddComponent<LaserWeapon>();
        }

        isInitialized = true;
        
        Debug.LogWarning("Weapons.cs is deprecated. Migrating to modular LaserWeapon. Please use WeaponManager for new implementations.");
    }

    /// <summary>
    /// Try to fire the laser weapon with power efficiency check
    /// </summary>
    public bool TryFire(float weaponPowerEfficiency)
    {
        if (!isInitialized) InitializeLaserWeapon();
        
        if (laserWeapon != null)
        {
            return laserWeapon.TryFire(weaponPowerEfficiency);
        }

        return false;
    }

    /// <summary>
    /// Fire at a specific target (legacy compatibility)
    /// </summary>
    public void Fire(Vector3 target)
    {
        if (!isInitialized) InitializeLaserWeapon();
        
        if (laserWeapon != null)
        {
            laserWeapon.Fire(target);
        }
    }

    /// <summary>
    /// Check if weapon can fire
    /// </summary>
    public bool CanFire()
    {
        if (!isInitialized) InitializeLaserWeapon();
        return laserWeapon != null && laserWeapon.CanFire();
    }

    // Public getters for UI (delegate to LaserWeapon)
    public int GetCurrentAmmo()
    {
        if (!isInitialized) InitializeLaserWeapon();
        return laserWeapon != null ? laserWeapon.GetCurrentAmmo() : 0;
    }

    public int GetMaxAmmo()
    {
        if (!isInitialized) InitializeLaserWeapon();
        return laserWeapon != null ? laserWeapon.GetMaxAmmo() : maxAmmo;
    }

    public bool IsRecharging()
    {
        if (!isInitialized) InitializeLaserWeapon();
        return laserWeapon != null && laserWeapon.IsRecharging();
    }

    public float GetRechargeProgress()
    {
        if (!isInitialized) InitializeLaserWeapon();
        return laserWeapon != null ? laserWeapon.GetRechargeProgress() : 1f;
    }

    /// <summary>
    /// Get the underlying LaserWeapon instance for migration
    /// </summary>
    public LaserWeapon GetLaserWeapon()
    {
        if (!isInitialized) InitializeLaserWeapon();
        return laserWeapon;
    }
}
