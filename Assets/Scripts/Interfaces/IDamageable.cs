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
