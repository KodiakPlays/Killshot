// =================================================================================================
// Weapons.cs
//
// What it does:
// An older laser gun for the physics ship that passes every request on to LaserWeapon. No scene,
// prefab or other script uses it. The battle board does not use this script. The board's guns
// are in WeaponSystem.
//
// How it works:
// - The code marks it as replaced by WeaponManager and LaserWeapon. Unity shows a warning where
//   other code uses it.
// - At the start it finds LaserWeapon on the same object, or adds one. It writes a warning to the
//   Console.
// - Fire, can-fire, ammo and recharge questions all go straight to LaserWeapon.
// - The Inspector settings on this script (laser prefab, fire point, speed, power, ammo, reload)
//   are not passed to LaserWeapon. LaserWeapon uses the settings on LaserWeapon.
//
// Technical:
// - The class has [System.Obsolete]. Any code that names Weapons gets a compiler warning.
// - Methods: TryFire(power 0 to 1), Fire(target point), CanFire, GetCurrentAmmo, GetMaxAmmo,
//   IsRecharging, GetRechargeProgress and GetLaserWeapon. No script calls them.
// - Each method finds or adds the LaserWeapon first, if Start has not run yet.
// - With no LaserWeapon, GetMaxAmmo returns this script's maxAmmo and GetRechargeProgress
//   returns 1.
// - Trap: a LaserWeapon it adds starts with LaserWeapon's defaults and no laser prefab. It
//   cannot fire until one is set.
// =================================================================================================

using UnityEngine;

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
