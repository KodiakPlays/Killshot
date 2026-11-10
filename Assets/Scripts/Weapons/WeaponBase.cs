// =================================================================================================
// WeaponBase.cs
//
// What it does:
// The shared base for every older physics gun: LaserWeapon, Macrocannon, MissileLauncher,
// PointDefenseCanon, Railgun, BroadsideCannon and BoardingPodLauncher. It is never put on an
// object by itself. The battle board does not use this script.
//
// How it works:
// - It gives every gun the same Inspector settings. The defaults are range 500, a 45-degree angle
//   of fire and a 1-second reload. Ammo is 30, base damage 10 and the damage factor 1.
// - At the start it fills the ammo to the maximum.
// - By default a gun can fire when it is not reloading and has ammo. The reload time must have
//   passed after the last shot. Each gun can change this check.
// - Each gun fills in what happens when it fires at a target point.
// - Other scripts can ask for the ammo left, the maximum ammo and the reload progress (0 to 1).
//   WeaponManager reads the reload this way.
// - The angle of fire is stored but no gun uses it to aim or block a shot.
//
// Technical:
// - An abstract class. The seven guns above extend it. WeaponSlot.weaponInstance (WeaponType.cs)
//   is typed as WeaponBase.
// - Inspector fields (protected): range, angleOfFire, reloadTime, maxAmmo, baseDamage and
//   damageModifier.
// - Runtime fields: currentAmmo, lastFireTime (Time.time), isReloading and fireRate (seconds
//   between shots). The Inspector does not show fireRate.
// - WeaponManager calls CanFire, Fire(point) and GetReloadProgress through this type.
// - Trap: several guns overwrite range, or fill zero fields with defaults, in Start.
//   The Inspector value may not be the one used.
// - Trap: the base CanFire checks reloadTime. Only LaserWeapon checks fireRate.
// - isReloading never turns on, and Reload() has no caller.
// =================================================================================================

using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Base Weapon Stats")]
    [SerializeField] protected float range = 500f;
    [SerializeField] protected float angleOfFire = 45f;
    [SerializeField] protected float reloadTime = 1f;
    [SerializeField] protected int maxAmmo = 30;
    [SerializeField] protected float baseDamage = 10f;
    [SerializeField] protected float damageModifier = 1f;
    
    protected float fireRate = 0.2f;      // Time between shots in seconds (5 shots/sec)
    protected int currentAmmo;
    protected bool isReloading;
    protected float lastFireTime;

    protected virtual void Start()
    {
        currentAmmo = maxAmmo;
    }

    public virtual bool CanFire()
    {
        return !isReloading && currentAmmo > 0 && Time.time - lastFireTime >= reloadTime;
    }

    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => maxAmmo;

    /// <summary>
    /// Returns 0..1 reload progress (1 = fully ready to fire).
    /// </summary>
    public float GetReloadProgress()
    {
        if (reloadTime <= 0f) return 1f;
        float elapsed = Time.time - lastFireTime;
        return Mathf.Clamp01(elapsed / reloadTime);
    }

    protected virtual void Reload()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    public abstract void Fire(Vector3 target);
}
