# Railgun Weapon Setup Guide

## Overview
The Railgun is a high-damage, penetrating weapon with a charge-up mechanic. It uses a LineRenderer for instant beam visuals and can pierce through multiple targets with damage falloff.

## Features
- ✅ **Penetration System** - Shoots through asteroids, debris, and enemies
- ✅ **Charge Mechanic** - 2-second charge time before firing
- ✅ **LineRenderer Beam** - Instant visual beam effect with fade
- ✅ **Damage Falloff** - 15% damage reduction per penetration
- ✅ **Configurable** - Max penetrations, range, charge time all adjustable
- ✅ **UI Support** - Shows charge progress and ammo count

## Setup on GameObject

### Method 1: Using WeaponManager (Recommended)

1. **Add Railgun component to your ship:**
   - Select Player GameObject
   - Add Component → Scripts → Railgun

2. **Configure Railgun in Inspector:**
   ```
   Range: 2000
   Angle Of Fire: 0 (straight shot)
   Reload Time: 5 (seconds between shots)
   Max Ammo: 10
   Base Damage: 100
   Damage Modifier: 1
   
   --- Railgun Settings ---
   Max Range: 2000
   Charge Time: 2
   Max Penetrations: 5
   Damage Dropoff Per Penetration: 0.15 (15%)
   
   --- Visual Effects ---
   Beam Duration: 0.3
   Beam Width: 0.5
   Beam Color: Cyan (or your choice)
   
   --- Fire Point ---
   Fire Point: (assign the transform where beam should originate)
   ```

3. **Add to WeaponManager:**
   - In WeaponManager component
   - Add new weapon slot
   - Weapon Type: Railgun
   - Weapon Instance: Drag the Railgun component
   - Is Active: ✓ (checked)

### Method 2: Standalone (Without WeaponManager)

Call directly from your ship controller:
```csharp
Railgun railgun = GetComponent<Railgun>();
if (railgun != null && railgun.CanFire())
{
    Vector3 targetPos = GetTargetPosition();
    railgun.Fire(targetPos);
}
```

## Configuration Options

### Damage & Penetration
- **Base Damage**: Starting damage (default: 100)
- **Max Penetrations**: How many objects it can pierce (default: 5)
- **Damage Dropoff**: Damage reduction per hit (0.15 = 15% loss)

**Example:** 
- Shot 1: 100 damage
- Shot 2: 85 damage (15% reduction)
- Shot 3: 72.25 damage
- Shot 4: 61.4 damage
- Shot 5: 52.2 damage

### Charge System
- **Charge Time**: Seconds to charge before firing (default: 2)
- **Can cancel charge**: Call `railgun.CancelCharge()`
- **Check charging**: Use `railgun.IsCharging()`
- **Get progress**: Use `railgun.GetChargeProgress()` (0-1)

### Visual Effects
- **Beam Renderer**: Auto-created if not assigned
- **Beam Duration**: How long beam stays visible (default: 0.3s)
- **Beam Width**: Thickness of beam (default: 0.5)
- **Beam Color**: Color of the beam (default: cyan)
- **Charge Effect**: Optional GameObject to show while charging
- **Impact Effect**: Optional particle effect at hit points

### Audio
- **Charge Sound**: AudioClip played during charge
- **Fire Sound**: AudioClip played when firing

## Penetration Rules

### Objects the Railgun Penetrates:
- ✅ Asteroids (tag: "Asteroid")
- ✅ Debris (tag: "Debris")
- ✅ Enemy ships
- ✅ Shields
- ✅ Most obstacles

### Objects that STOP the beam:
- ❌ Heavy Armor (tag: "HeavyArmor")
- ❌ Stations (tag: "Station")

You can customize this in `ShouldStopBeam()` method.

## Damage System

The railgun automatically detects and damages:
1. **EnemyShip** components
2. **PlayerShip** components (respects friendly fire tags)
3. **Shields** components
4. **IDamageable** interface implementations
5. Objects with "Asteroid" or "Debris" tags (destroys them)

### Implementing IDamageable

For custom destructible objects:
```csharp
public class MyObstacle : MonoBehaviour, IDamageable
{
    private float health = 50f;
    
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0) Destroy(gameObject);
    }
    
    public float GetHealth() => health;
    public bool IsDestroyed() => health <= 0;
}
```

## UI Integration

The railgun supports UI display through `WeaponUIDisplay`:

**Shows:**
- Ammo count when ready
- "CHARGING XX%" when charging
- Charge progress bar (0-100%)
- Status: "READY", "CHARGING", or "COOLDOWN"

## Controls (if using WeaponManager)

With the new modular system:
- **Space/Left Ctrl** - Fire active weapon (starts charge if railgun)
- **5** - Quick switch to railgun (Alpha5 key)
- **E/Q** - Cycle through weapons

## Testing the Railgun

1. **Create test asteroids:**
   - Create 3D objects (cubes/spheres)
   - Add `Asteroid` script
   - Tag them as "Asteroid"
   - Add colliders
   - Line them up in front of your ship

2. **Fire the railgun:**
   - Switch to railgun weapon
   - Press fire (Space/Ctrl)
   - Watch 2-second charge
   - See beam pierce through all asteroids

3. **Debug mode:**
   - Check console for penetration count
   - Each asteroid shows damage taken
   - Beam draws through all hits

## Advanced: FireInstant Mode

For AI enemies or testing without charge time:
```csharp
railgun.FireInstant(targetPosition); // Fires immediately
```

## Performance Notes

- LineRenderer is very efficient
- Raycasts are performed sequentially (not all at once)
- Beam visibility is short-lived (0.3s default)
- No physics objects spawned (unlike projectiles)

## Example Stats Configurations

### **Sniper Railgun** (Long range, slow)
- Charge Time: 3s
- Reload Time: 8s
- Base Damage: 150
- Max Range: 3000
- Max Penetrations: 3

### **Assault Railgun** (Faster, less powerful)
- Charge Time: 1s
- Reload Time: 3s
- Base Damage: 75
- Max Range: 1500
- Max Penetrations: 7

### **Anti-Capital Railgun** (Massive damage, very slow)
- Charge Time: 5s
- Reload Time: 15s
- Base Damage: 500
- Max Range: 5000
- Max Penetrations: 2
