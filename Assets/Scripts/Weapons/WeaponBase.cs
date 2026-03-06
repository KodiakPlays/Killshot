using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Base Weapon Stats")]
    [SerializeField] protected float range;
    [SerializeField] protected float angleOfFire;
    [SerializeField] protected float reloadTime;
    [SerializeField] protected int maxAmmo;
    [SerializeField] protected float baseDamage;
    [SerializeField] protected float damageModifier = 1f;
    
    protected float fireRate = 1f;        // Time between shots in seconds
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

    protected virtual void Reload()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    public abstract void Fire(Vector3 target);
}
