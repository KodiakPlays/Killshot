using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for displaying weapon information
/// Attach to a UI Canvas element to show current weapon status
/// </summary>
public class WeaponUIDisplay : MonoBehaviour
{
    [Header("Weapon System")]
    [SerializeField] private WeaponManager weaponManager;
    
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI weaponTypeText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Image rechargeBar;
    [SerializeField] private Image weaponIcon;
    
    [Header("Weapon Icons")]
    [SerializeField] private Sprite laserIcon;
    [SerializeField] private Sprite macrocannonIcon;
    [SerializeField] private Sprite missileIcon;
    [SerializeField] private Sprite pointDefenseIcon;
    [SerializeField] private Sprite boardingPodIcon;
    [SerializeField] private Sprite railgunIcon;
    
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
        WeaponSlot activeSlot = weaponManager.GetActiveWeaponSlot();
        
        if (activeSlot == null || activeSlot.weaponInstance == null)
        {
            DisplayNoWeapon();
            return;
        }

        // Update weapon name
        if (weaponNameText != null)
        {
            weaponNameText.text = activeSlot.slotName;
        }

        // Update weapon type
        if (weaponTypeText != null)
        {
            weaponTypeText.text = activeSlot.weaponType.ToString();
        }

        // Update weapon icon
        if (weaponIcon != null)
        {
            weaponIcon.sprite = GetWeaponIcon(activeSlot.weaponType);
        }

        // Update status
        bool canFire = weaponManager.CanActiveWeaponFire();
        if (statusText != null)
        {
            statusText.text = canFire ? "READY" : "RELOADING";
            statusText.color = canFire ? Color.green : Color.yellow;
        }

        // Update weapon-specific info
        UpdateWeaponSpecificUI(activeSlot);
    }

    private void UpdateWeaponSpecificUI(WeaponSlot slot)
    {
        // Handle LaserWeapon specific UI
        if (slot.weaponInstance is LaserWeapon laser)
        {
            if (ammoText != null)
            {
                ammoText.text = $"{laser.GetCurrentAmmo()}/{laser.GetMaxAmmo()}";
            }

            if (rechargeBar != null)
            {
                rechargeBar.fillAmount = laser.GetRechargeProgress();
                rechargeBar.color = laser.IsRecharging() ? Color.yellow : Color.cyan;
            }
        }
        // Handle Macrocannon
        else if (slot.weaponInstance is Macrocannon macrocannon)
        {
            if (ammoText != null)
            {
                ammoText.text = macrocannon.HasValidLock() ? "LOCKED" : "SCANNING";
            }
            
            if (rechargeBar != null)
            {
                rechargeBar.fillAmount = 1f;
            }
        }
        // Handle MissileLauncher
        else if (slot.weaponInstance is MissileLauncher missile)
        {
            if (ammoText != null)
            {
                ammoText.text = "MISSILES";
            }
            
            if (rechargeBar != null)
            {
                // Could show lock progress for first tube
                rechargeBar.fillAmount = missile.GetLockProgress(0);
            }
        }
        // Handle Railgun
        else if (slot.weaponInstance is Railgun railgun)
        {
            if (ammoText != null)
            {
                if (railgun.IsCharging())
                {
                    ammoText.text = $"CHARGING {(railgun.GetChargeProgress() * 100):F0}%";
                }
                else
                {
                    ammoText.text = $"{railgun.GetCurrentAmmo()}/{railgun.GetMaxAmmo()}";
                }
            }
            
            if (rechargeBar != null)
            {
                rechargeBar.fillAmount = railgun.IsCharging() ? railgun.GetChargeProgress() : 1f;
                rechargeBar.color = railgun.IsCharging() ? Color.yellow : Color.green;
            }
            
            if (statusText != null)
            {
                statusText.text = railgun.IsCharging() ? "CHARGING" : (railgun.CanFire() ? "READY" : "COOLDOWN");
            }
        }
        // Default for other weapons
        else
        {
            if (ammoText != null)
            {
                ammoText.text = "ARMED";
            }
            
            if (rechargeBar != null)
            {
                rechargeBar.fillAmount = 1f;
            }
        }
    }

    private void DisplayNoWeapon()
    {
        if (weaponNameText != null)
            weaponNameText.text = "NO WEAPON";
        
        if (weaponTypeText != null)
            weaponTypeText.text = "---";
        
        if (ammoText != null)
            ammoText.text = "---";
        
        if (statusText != null)
        {
            statusText.text = "OFFLINE";
            statusText.color = Color.red;
        }
        
        if (rechargeBar != null)
            rechargeBar.fillAmount = 0f;
        
        if (weaponIcon != null)
            weaponIcon.sprite = null;
    }

    private Sprite GetWeaponIcon(WeaponType type)
    {
        return type switch
        {
            WeaponType.Laser => laserIcon,
            WeaponType.Macrocannon => macrocannonIcon,
            WeaponType.Missile => missileIcon,
            WeaponType.PointDefense => pointDefenseIcon,
            WeaponType.BoardingPod => boardingPodIcon,
            WeaponType.Railgun => railgunIcon,
            _ => null
        };
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
