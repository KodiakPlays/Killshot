// =================================================================================================
// IDamageable.cs
//
// What it does:
// This lists four things that any object that can be hurt must be able to do. The older physics
// ships, shots and asteroids use it. The battle board does not use this script. Board damage goes
// through DamageSystem.
//
// How it works:
// - TakeDamage takes an amount of damage and gives back the damage it really took.
// - GetCurrentHealth and GetMaxHealth give the object's health now and at most.
// - CanBeDamaged says if the object can be hurt right now.
// - PlayerShip, EnemyShip and Asteroid fill in these four. Shell, PDCBullet, Missile, Railgun and
//   BoardingPod look for them on whatever they hit, and call TakeDamage.
// - The file has no code that runs by itself.
//
// Technical:
// - An interface with four methods: TakeDamage(float), GetCurrentHealth(), GetMaxHealth() and
//   CanBeDamaged().
// - Health and damage are plain numbers. The interface sets no units or limits.
// - Shooters find it with GetComponent<IDamageable>() on the collider they hit.
// - Only Railgun asks CanBeDamaged() first. The others call TakeDamage straight away.
// - Trap: PlayerShip and EnemyShip also have a public TakeDamage. The interface one is
//   separate and only reached through IDamageable.
// - New board damage goes in DamageSystem. This interface is for the physics ships and shots only.
// =================================================================================================

using UnityEngine;

public interface IDamageable
{
    /// <summary>
    /// Apply damage to this entity
    /// </summary>
    /// <param name="amount">The amount of damage to apply</param>
    /// <returns>The actual amount of damage that was applied (after shields/armor/resistances)</returns>
    float TakeDamage(float amount);

    /// <summary>
    /// Get the current health of this entity
    /// </summary>
    /// <returns>The current health value</returns>
    float GetCurrentHealth();

    /// <summary>
    /// Get the maximum health of this entity
    /// </summary>
    /// <returns>The maximum health value</returns>
    float GetMaxHealth();

    /// <summary>
    /// Check if this entity can be damaged
    /// </summary>
    /// <returns>True if the entity can take damage, false otherwise</returns>
    bool CanBeDamaged();
}
