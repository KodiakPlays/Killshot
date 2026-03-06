using UnityEngine;

/// <summary>
/// Example implementation showing how to use the modular weapon system
/// This can be attached to a player ship or enemy ship to demonstrate the weapon manager
/// </summary>
public class WeaponSystemExample : MonoBehaviour
{
    [Header("Weapon Manager")]
    [SerializeField] private WeaponManager weaponManager;
    
    [Header("Input Settings")]
    [SerializeField] private KeyCode fireKey = KeyCode.Space;
    [SerializeField] private KeyCode nextWeaponKey = KeyCode.E;
    [SerializeField] private KeyCode previousWeaponKey = KeyCode.Q;
    
    [Header("Targeting")]
    [SerializeField] private Transform targetReticle;
    [SerializeField] private float maxTargetDistance = 1000f;

    void Start()
    {
        // Get or add weapon manager
        if (weaponManager == null)
        {
            weaponManager = GetComponent<WeaponManager>();
            if (weaponManager == null)
            {
                weaponManager = gameObject.AddComponent<WeaponManager>();
                Debug.LogWarning("WeaponManager not assigned. Added component automatically.");
            }
        }
    }

    void Update()
    {
        HandleWeaponInput();
    }

    private void HandleWeaponInput()
    {
        // Switch to next weapon
        if (Input.GetKeyDown(nextWeaponKey))
        {
            weaponManager.SwitchToNextWeapon();
            DisplayCurrentWeapon();
        }

        // Switch to previous weapon
        if (Input.GetKeyDown(previousWeaponKey))
        {
            weaponManager.SwitchToPreviousWeapon();
            DisplayCurrentWeapon();
        }

        // Fire weapon
        if (Input.GetKey(fireKey))
        {
            FireAtTarget();
        }

        // Quick switch examples (number keys)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            weaponManager.SwitchToWeaponType(WeaponType.Laser);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            weaponManager.SwitchToWeaponType(WeaponType.Macrocannon);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            weaponManager.SwitchToWeaponType(WeaponType.Missile);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            weaponManager.SwitchToWeaponType(WeaponType.PointDefense);
        }
    }

    private void FireAtTarget()
    {
        Vector3 targetPosition;

        // Use reticle if available, otherwise use forward direction
        if (targetReticle != null)
        {
            targetPosition = targetReticle.position;
        }
        else
        {
            // Fire in forward direction at max distance
            targetPosition = transform.position + transform.forward * maxTargetDistance;
        }

        // Fire with full power (1.0f)
        bool fired = weaponManager.FireActiveWeapon(targetPosition, 1.0f);

        if (!fired)
        {
            // Could play a "out of ammo" sound or show UI feedback here
        }
    }

    private void DisplayCurrentWeapon()
    {
        WeaponSlot activeSlot = weaponManager.GetActiveWeaponSlot();
        if (activeSlot != null)
        {
            Debug.Log($"Active Weapon: {activeSlot.slotName} ({activeSlot.weaponType})");
        }
    }

    // Public method to get weapon status for UI
    public string GetWeaponStatus()
    {
        WeaponSlot activeSlot = weaponManager.GetActiveWeaponSlot();
        if (activeSlot == null || activeSlot.weaponInstance == null)
        {
            return "No weapon equipped";
        }

        string status = $"{activeSlot.slotName}\n";
        status += $"Type: {activeSlot.weaponType}\n";
        status += $"Can Fire: {weaponManager.CanActiveWeaponFire()}";

        // Get specific info for LaserWeapon
        if (activeSlot.weaponInstance is LaserWeapon laser)
        {
            status += $"\nAmmo: {laser.GetCurrentAmmo()}/{laser.GetMaxAmmo()}";
            status += $"\nRecharging: {laser.IsRecharging()}";
            if (laser.IsRecharging())
            {
                status += $"\nRecharge: {(laser.GetRechargeProgress() * 100):F0}%";
            }
        }

        return status;
    }
}
