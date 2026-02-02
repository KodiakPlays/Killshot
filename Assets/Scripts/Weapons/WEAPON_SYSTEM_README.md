# Modular Weapon System

## Overview
The modular weapon system provides a flexible, type-based architecture for managing multiple weapon types on ships. This replaces the monolithic `Weapons.cs` approach with a cleaner, more maintainable design.

## Architecture

### Core Components

1. **WeaponBase** (Abstract Base Class)
   - Located: `Assets/Scripts/Weapons/WeaponBase.cs`
   - Provides common functionality for all weapons
   - Properties: range, damage, ammo, reload time, fire rate
   - Abstract method: `Fire(Vector3 target)`

2. **WeaponType** (Enum)
   - Located: `Assets/Scripts/Weapons/WeaponManager.cs`
   - Defines available weapon categories
   - Types: Laser, Macrocannon, Missile, PointDefense, BoardingPod

3. **WeaponSlot** (Data Structure)
   - Located: `Assets/Scripts/Weapons/WeaponManager.cs`
   - Represents a single weapon slot on a ship
   - Contains: weapon type, instance reference, active state, slot info

4. **WeaponManager** (Manager Class)
   - Located: `Assets/Scripts/Weapons/WeaponManager.cs`
   - Manages multiple weapons and switching between them
   - Handles firing, power management, and weapon state

### Weapon Implementations

Each weapon type extends `WeaponBase`:

- **LaserWeapon** - Energy-based rapid-fire weapon with recharge mechanic
- **Macrocannon** - Heavy ballistic weapon with shell loading
- **MissileLauncher** - Lock-on guided missile system
- **PointDefenseCanon** - Anti-missile/fighter defense system
- **BoardingPodLauncher** - Launches boarding craft

## Usage

### Setup on GameObject

1. **Add WeaponManager to your ship:**
   ```csharp
   GameObject ship = new GameObject("PlayerShip");
   WeaponManager weaponMgr = ship.AddComponent<WeaponManager>();
   ```

2. **Add specific weapon types:**
   ```csharp
   // Add laser weapon
   LaserWeapon laser = ship.AddComponent<LaserWeapon>();
   laser.laserPrefab = laserPrefabReference;
   laser.firePoint = firePointTransform;
   
   // Register with manager
   weaponMgr.AddWeapon(WeaponType.Laser, laser, "Primary Laser");
   ```

3. **Or configure in Inspector:**
   - Attach `WeaponManager` component
   - Add weapon components (LaserWeapon, Macrocannon, etc.)
   - Configure weapon slots in inspector
   - Set active weapon index

### Code Examples

#### Switching Weapons
```csharp
// Switch to next weapon
weaponManager.SwitchToNextWeapon();

// Switch to previous weapon
weaponManager.SwitchToPreviousWeapon();

// Switch to specific weapon type
weaponManager.SwitchToWeaponType(WeaponType.Missile);

// Switch by index
weaponManager.SwitchToWeapon(0);
```

#### Firing Weapons
```csharp
// Fire at a target position
Vector3 targetPos = enemyShip.transform.position;
float powerEfficiency = 1.0f; // Full power
bool fired = weaponManager.FireActiveWeapon(targetPos, powerEfficiency);

if (!fired)
{
    Debug.Log("Cannot fire - check ammo/power/cooldown");
}
```

#### Query Weapon Status
```csharp
// Check if can fire
bool canFire = weaponManager.CanActiveWeaponFire();

// Get active weapon
WeaponBase activeWeapon = weaponManager.GetActiveWeapon();

// Get active weapon slot
WeaponSlot slot = weaponManager.GetActiveWeaponSlot();
Debug.Log($"Active: {slot.slotName} - Type: {slot.weaponType}");

// Get all weapons of a type
List<WeaponBase> allLasers = weaponManager.GetWeaponsByType(WeaponType.Laser);
```

#### Weapon Management
```csharp
// Add a weapon at runtime
weaponManager.AddWeapon(WeaponType.Laser, laserWeaponInstance, "Secondary Laser");

// Remove a weapon
weaponManager.RemoveWeapon(slotIndex);

// Enable/disable weapon slots
weaponManager.SetWeaponActive(slotIndex, false); // Disable weapon
weaponManager.SetWeaponActive(slotIndex, true);  // Enable weapon
```

## Creating Custom Weapons

To create a new weapon type:

1. **Create a new class extending WeaponBase:**
```csharp
using UnityEngine;

public class PlasmaWeapon : WeaponBase
{
    [Header("Plasma Settings")]
    [SerializeField] private GameObject plasmaPrefab;
    [SerializeField] private float plasmaSpeed = 40f;
    [SerializeField] private float heatGeneration = 10f;
    
    private float currentHeat = 0f;
    private float maxHeat = 100f;
    
    protected override void Start()
    {
        base.Start();
        // Custom initialization
    }
    
    public override void Fire(Vector3 target)
    {
        if (!CanFire()) return;
        
        // Custom firing logic
        Vector3 direction = (target - transform.position).normalized;
        GameObject plasma = Instantiate(plasmaPrefab, transform.position, Quaternion.identity);
        
        // Apply velocity, damage, etc.
        Rigidbody rb = plasma.GetComponent<Rigidbody>();
        rb.velocity = direction * plasmaSpeed;
        
        // Update heat
        currentHeat += heatGeneration;
        lastFireTime = Time.time;
        currentAmmo--;
    }
    
    public override bool CanFire()
    {
        return base.CanFire() && currentHeat < maxHeat;
    }
}
```

2. **Add to WeaponType enum:**
```csharp
public enum WeaponType
{
    Laser,
    Macrocannon,
    Missile,
    PointDefense,
    BoardingPod,
    Plasma  // Add new type
}
```

3. **Use in WeaponManager:**
```csharp
PlasmaWeapon plasma = ship.AddComponent<PlasmaWeapon>();
weaponManager.AddWeapon(WeaponType.Plasma, plasma, "Experimental Plasma");
```

## Migration from Legacy Weapons.cs

The old `Weapons.cs` has been converted to a compatibility wrapper:

### Automatic Migration
The legacy `Weapons` class now automatically delegates to `LaserWeapon`:
```csharp
// Old code still works
Weapons legacyWeapon = GetComponent<Weapons>();
bool fired = legacyWeapon.TryFire(powerEfficiency);
```

### Manual Migration (Recommended)
Replace legacy code with the new system:

**Before:**
```csharp
Weapons weapon = ship.AddComponent<Weapons>();
weapon.laserPrefab = prefab;
weapon.TryFire(1.0f);
```

**After:**
```csharp
WeaponManager weaponMgr = ship.AddComponent<WeaponManager>();
LaserWeapon laser = ship.AddComponent<LaserWeapon>();
weaponMgr.AddWeapon(WeaponType.Laser, laser);
weaponMgr.FireActiveWeapon(targetPos, 1.0f);
```

## Best Practices

1. **Use WeaponManager** for all weapon operations rather than accessing weapons directly
2. **Configure weapons in Inspector** for easier balancing and iteration
3. **Extend WeaponBase** for all custom weapons to ensure compatibility
4. **Check CanFire()** before attempting to fire
5. **Handle power efficiency** for energy-based weapons
6. **Use WeaponType enum** for type-safe weapon identification

## Integration with Existing Systems

### Power System Integration
```csharp
// Get power efficiency from ship's power system
float powerEff = shipPowerSystem.GetWeaponPowerEfficiency();
weaponManager.FireActiveWeapon(target, powerEff);
```

### UI Integration
```csharp
// Display current weapon
WeaponSlot slot = weaponManager.GetActiveWeaponSlot();
weaponNameText.text = slot.slotName;
weaponTypeText.text = slot.weaponType.ToString();

// Display ammo (for LaserWeapon)
if (slot.weaponInstance is LaserWeapon laser)
{
    ammoText.text = $"{laser.GetCurrentAmmo()}/{laser.GetMaxAmmo()}";
    rechargeBar.fillAmount = laser.GetRechargeProgress();
}
```

## File Structure
```
Assets/Scripts/
├── Weapons.cs (Legacy compatibility wrapper - deprecated)
└── Weapons/
    ├── WeaponBase.cs (Abstract base class)
    ├── WeaponManager.cs (Manager + WeaponType + WeaponSlot)
    ├── LaserWeapon.cs (Laser implementation)
    ├── Macrocannon.cs (Existing)
    ├── MissileLauncher.cs (Existing)
    ├── PointDefenseCanon.cs (Existing)
    ├── BoardingPodLauncher.cs (Existing)
    └── WeaponSystemExample.cs (Example usage)
```

## Notes

- The legacy `Weapons.cs` is marked as `[Obsolete]` and will show warnings
- All existing weapon types (Macrocannon, Missile, etc.) already extend WeaponBase
- WeaponManager supports runtime weapon addition/removal
- Each weapon maintains its own state (ammo, cooldowns, etc.)
- Power requirements can be configured per weapon or globally in WeaponManager
