// =================================================================================================
// WeaponSystemExample.cs
//
// What it does:
// A sample that shows how to drive WeaponManager, the older physics gun switcher, from the
// keyboard. No scene or prefab has it. The battle board does not use this script.
//
// How it works:
// - At the start it finds WeaponManager on the same object, or adds one and writes a warning.
// - Every frame it reads keys. E picks the next gun and Q the previous one. It writes the gun's
//   name to the Console.
// - Keys 1 to 4 pick the laser, the Macrocannon, the torpedo launcher or the PDC.
// - Holding Space fires the picked gun at full power. It aims at an aiming mark set in the
//   Inspector, or 1,000 units straight ahead.
// - Other code can ask it for a short status text: the gun's name, type, whether it can fire,
//   and for a laser its ammo and refill progress.
//
// Technical:
// - It calls WeaponManager: SwitchToNextWeapon, SwitchToPreviousWeapon, SwitchToWeaponType,
//   FireActiveWeapon, GetActiveWeaponSlot and CanActiveWeaponFire.
// - GetWeaponStatus() has no caller.
// - Keys are read through the old Input Manager. The fire, next and previous keys are in the
//   Inspector. Keys 1 to 4 are fixed in code.
// - Trap: with no aiming mark, it aims along transform.forward. The older ships face along
//   transform.up.
// - Trap: on PlayerShip, its keys clash. Q and E dodge there, and 1 to 3 switch power.
// - Numbers: the 1,000-unit aim distance is in the Inspector.
// =================================================================================================

using UnityEngine;

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
