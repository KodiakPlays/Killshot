using UnityEngine;

/// <summary>
/// UI component for displaying weapon information
/// Gathers weapon data and routes display through UIController
/// </summary>
public class WeaponUIDisplay : MonoBehaviour
{
    [Header("Weapon System")]
    [SerializeField] private WeaponManager weaponManager;
    
    [Header("Settings")]
    [SerializeField] private bool autoFindWeaponManager = true;
    [SerializeField] private float updateInterval = 0.1f;
    
    private float updateTimer;

    void Start()
    {
        if (autoFindWeaponManager && weaponManager == null)
        {
            weaponManager = FindFirstObjectByType<WeaponManager>();
            if (weaponManager == null)
            {
                Debug.LogWarning("WeaponUIDisplay: No WeaponManager found in scene!");
            }
        }
    }

    void Update()
    {
        if (weaponManager == null) return;
        
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            UpdateUI();
            updateTimer = 0f;
        }
    }

    private void UpdateUI()
    {
        var ui = UIController.Instance;
        if (ui == null) return;

        WeaponSlot activeSlot = weaponManager.GetActiveWeaponSlot();
        
        if (activeSlot == null || activeSlot.weaponInstance == null)
        {
            ui.UpdateWeaponDisplay("NO WEAPON", "---", null, "OFFLINE", Color.red, "---", 0f, Color.white);
            return;
        }

        bool canFire = weaponManager.CanActiveWeaponFire();
        string status = canFire ? "READY" : "RELOADING";
        Color statusColor = canFire ? Color.green : Color.yellow;
        string ammo = "ARMED";
        float fill = 1f;
        Color fillColor = Color.white;

        if (activeSlot.weaponInstance is LaserWeapon laser)
        {
            ammo = $"{laser.GetCurrentAmmo()}/{laser.GetMaxAmmo()}";
            fill = laser.GetRechargeProgress();
            fillColor = laser.IsRecharging() ? Color.yellow : Color.cyan;
        }
        else if (activeSlot.weaponInstance is Macrocannon macrocannon)
        {
            ammo = macrocannon.HasValidLock() ? "LOCKED" : "SCANNING";
        }
        else if (activeSlot.weaponInstance is MissileLauncher missile)
        {
            ammo = "MISSILES";
            fill = missile.GetLockProgress(0);
        }
        else if (activeSlot.weaponInstance is Railgun railgun)
        {
            if (railgun.IsCharging())
            {
                ammo = $"CHARGING {(railgun.GetChargeProgress() * 100):F0}%";
                status = "CHARGING";
                statusColor = Color.yellow;
            }
            else
            {
                ammo = $"{railgun.GetCurrentAmmo()}/{railgun.GetMaxAmmo()}";
                status = railgun.CanFire() ? "READY" : "COOLDOWN";
                statusColor = railgun.CanFire() ? Color.green : Color.yellow;
            }
            fill = railgun.IsCharging() ? railgun.GetChargeProgress() : 1f;
            fillColor = railgun.IsCharging() ? Color.yellow : Color.green;
        }

        ui.UpdateWeaponDisplay(activeSlot.slotName, activeSlot.weaponType.ToString(), UIController.Instance.GetWeaponIcon(activeSlot.weaponType), status, statusColor, ammo, fill, fillColor);
    }

    /// <summary>
    /// Manually set the weapon manager reference
    /// </summary>
    public void SetWeaponManager(WeaponManager manager)
    {
        weaponManager = manager;
    }

    /// <summary>
    /// Force immediate UI update
    /// </summary>
    public void ForceUpdate()
    {
        UpdateUI();
    }
}
