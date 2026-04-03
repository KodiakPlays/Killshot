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
