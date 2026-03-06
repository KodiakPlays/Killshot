using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages multiple weapon systems and handles weapon switching, firing, and power management
/// </summary>
public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Slots")]
    [SerializeField] private List<WeaponSlot> weaponSlots = new List<WeaponSlot>();
    
    [Header("Active Weapon")]
    [SerializeField] private int activeWeaponIndex = 0;
    
    [Header("Power Integration")]
    [SerializeField] private bool requiresPower = true;
    [SerializeField] private float minPowerForWeapons = 0.1f;
    
    private WeaponSlot currentWeapon;

    void Start()
    {
        InitializeWeapons();
        SwitchToWeapon(activeWeaponIndex);
    }

    /// <summary>
    /// Initialize all weapon slots and their instances
    /// </summary>
    private void InitializeWeapons()
    {
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (weaponSlots[i].weaponInstance != null)
            {
                weaponSlots[i].slotIndex = i;
                if (string.IsNullOrEmpty(weaponSlots[i].slotName))
                {
                    weaponSlots[i].slotName = $"{weaponSlots[i].weaponType} {i + 1}";
                }
            }
        }
    }

    /// <summary>
    /// Switch to a specific weapon by index
    /// </summary>
    public bool SwitchToWeapon(int index)
    {
        if (index < 0 || index >= weaponSlots.Count)
        {
            Debug.LogWarning($"Invalid weapon index: {index}");
            return false;
        }

        if (weaponSlots[index].weaponInstance == null || !weaponSlots[index].isActive)
        {
            Debug.LogWarning($"Weapon at index {index} is null or inactive");
            return false;
        }

        activeWeaponIndex = index;
        currentWeapon = weaponSlots[index];
        
        OnWeaponSwitched(currentWeapon);
        return true;
    }

    /// <summary>
    /// Switch to next available weapon
    /// </summary>
    public void SwitchToNextWeapon()
    {
        int startIndex = activeWeaponIndex;
        int nextIndex = (activeWeaponIndex + 1) % weaponSlots.Count;

        while (nextIndex != startIndex)
        {
            if (SwitchToWeapon(nextIndex))
            {
                return;
            }
            nextIndex = (nextIndex + 1) % weaponSlots.Count;
        }
    }

    /// <summary>
    /// Switch to previous available weapon
    /// </summary>
    public void SwitchToPreviousWeapon()
    {
        int startIndex = activeWeaponIndex;
        int prevIndex = (activeWeaponIndex - 1 + weaponSlots.Count) % weaponSlots.Count;

        while (prevIndex != startIndex)
        {
            if (SwitchToWeapon(prevIndex))
            {
                return;
            }
            prevIndex = (prevIndex - 1 + weaponSlots.Count) % weaponSlots.Count;
        }
    }

    /// <summary>
    /// Switch to a specific weapon type
    /// </summary>
    public bool SwitchToWeaponType(WeaponType type)
    {
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (weaponSlots[i].weaponType == type && weaponSlots[i].isActive)
            {
                return SwitchToWeapon(i);
            }
        }
        return false;
    }

    /// <summary>
    /// Fire the current active weapon at a target position
    /// </summary>
    public bool FireActiveWeapon(Vector3 target, float weaponPowerEfficiency = 1f)
    {
        if (currentWeapon == null || currentWeapon.weaponInstance == null)
        {
            return false;
        }

        if (requiresPower && weaponPowerEfficiency < minPowerForWeapons)
        {
            return false;
        }

        if (!currentWeapon.weaponInstance.CanFire())
        {
            return false;
        }

        // Special handling for LaserWeapon with power requirements
        if (currentWeapon.weaponInstance is LaserWeapon laserWeapon)
        {
            return laserWeapon.TryFire(weaponPowerEfficiency);
        }

        // Standard weapon firing
        currentWeapon.weaponInstance.Fire(target);
        return true;
    }

    /// <summary>
    /// Add a new weapon to the weapon slots
    /// </summary>
    public void AddWeapon(WeaponType type, WeaponBase weaponInstance, string slotName = "")
    {
        WeaponSlot newSlot = new WeaponSlot
        {
            weaponType = type,
            weaponInstance = weaponInstance,
            isActive = true,
            slotIndex = weaponSlots.Count,
            slotName = string.IsNullOrEmpty(slotName) ? $"{type} {weaponSlots.Count + 1}" : slotName
        };

        weaponSlots.Add(newSlot);
    }

    /// <summary>
    /// Remove a weapon from the specified slot
    /// </summary>
    public void RemoveWeapon(int index)
    {
        if (index >= 0 && index < weaponSlots.Count)
        {
            weaponSlots.RemoveAt(index);
            
            // Update indices
            for (int i = 0; i < weaponSlots.Count; i++)
            {
                weaponSlots[i].slotIndex = i;
            }

            // Switch to valid weapon if current was removed
            if (activeWeaponIndex >= weaponSlots.Count)
            {
                SwitchToWeapon(0);
            }
        }
    }

    /// <summary>
    /// Disable/Enable a weapon slot
    /// </summary>
    public void SetWeaponActive(int index, bool active)
    {
        if (index >= 0 && index < weaponSlots.Count)
        {
            weaponSlots[index].isActive = active;
            
            // Switch to another weapon if current was deactivated
            if (!active && index == activeWeaponIndex)
            {
                SwitchToNextWeapon();
            }
        }
    }

    /// <summary>
    /// Get all weapons of a specific type
    /// </summary>
    public List<WeaponBase> GetWeaponsByType(WeaponType type)
    {
        return weaponSlots
            .Where(slot => slot.weaponType == type && slot.weaponInstance != null)
            .Select(slot => slot.weaponInstance)
            .ToList();
    }

    /// <summary>
    /// Get the current active weapon
    /// </summary>
    public WeaponBase GetActiveWeapon()
    {
        return currentWeapon?.weaponInstance;
    }

    /// <summary>
    /// Get the current active weapon slot
    /// </summary>
    public WeaponSlot GetActiveWeaponSlot()
    {
        return currentWeapon;
    }

    /// <summary>
    /// Get all weapon slots
    /// </summary>
    public List<WeaponSlot> GetAllWeaponSlots()
    {
        return new List<WeaponSlot>(weaponSlots);
    }

    /// <summary>
    /// Called when weapon is switched - override for custom behavior
    /// </summary>
    protected virtual void OnWeaponSwitched(WeaponSlot newWeapon)
    {
        Debug.Log($"Switched to weapon: {newWeapon.slotName} ({newWeapon.weaponType})");
    }

    /// <summary>
    /// Check if the active weapon can fire
    /// </summary>
    public bool CanActiveWeaponFire()
    {
        return currentWeapon != null && 
               currentWeapon.weaponInstance != null && 
               currentWeapon.weaponInstance.CanFire();
    }

    // Public getters for UI and other systems
    public int GetActiveWeaponIndex() => activeWeaponIndex;
    public int GetWeaponCount() => weaponSlots.Count;
    public WeaponType GetActiveWeaponType() => currentWeapon?.weaponType ?? WeaponType.Laser;
}
