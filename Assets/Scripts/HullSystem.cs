using UnityEngine;
using System;

/// <summary>
/// Directional hull system per GDD spec.
/// Hull is divided into four quadrants: Port (Left), Starboard (Right), Prow (Front), Aft (Rear).
/// Each side has its own health. Incoming damage affects only the side it hits.
/// Once breached, internal components on that side are exposed to damage.
/// </summary>
public enum HullSide
{
    Port,       // Left
    Starboard,  // Right
    Prow,       // Front
    Aft         // Rear
}

[System.Serializable]
public class HullQuadrant
{
    public HullSide side;
    public float maxHealth;
    public float currentHealth;
    public bool isBreached => currentHealth <= 0f;

    public HullQuadrant(HullSide side, float maxHealth)
    {
        this.side = side;
        this.maxHealth = maxHealth;
        this.currentHealth = maxHealth;
    }

    /// <summary>
    /// Apply damage to this quadrant. Returns overflow damage (0 if fully absorbed).
    /// </summary>
    public float TakeDamage(float damage)
    {
        float overflow = 0f;
        currentHealth -= damage;
        if (currentHealth < 0f)
        {
            overflow = -currentHealth;
            currentHealth = 0f;
        }
        return overflow;
    }

    public void Repair(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public float GetHealthPercentage()
    {
        return maxHealth > 0f ? currentHealth / maxHealth : 0f;
    }
}

public class HullSystem : MonoBehaviour
{
    [Header("Hull Quadrant Health (per GDD)")]
    [SerializeField] private float portMaxHealth = 50f;
    [SerializeField] private float starboardMaxHealth = 50f;
    [SerializeField] private float prowMaxHealth = 40f;
    [SerializeField] private float aftMaxHealth = 10f;

    public HullQuadrant port { get; private set; }
    public HullQuadrant starboard { get; private set; }
    public HullQuadrant prow { get; private set; }
    public HullQuadrant aft { get; private set; }

    /// <summary>
    /// Fired when a hull quadrant is breached for the first time.
    /// Listeners (e.g. InternalSubsystems) should subscribe to this.
    /// </summary>
    public event Action<HullSide> OnHullBreached;

    /// <summary>
    /// Fired when any hull damage occurs. Passes (side, damage, remainingHealth).
    /// </summary>
    public event Action<HullSide, float, float> OnHullDamaged;

    /// <summary>
    /// Fired when all hull quadrants are breached (ship destroyed).
    /// </summary>
    public event Action OnShipDestroyed;

    private bool[] breachNotified = new bool[4];

    private void Awake()
    {
        port = new HullQuadrant(HullSide.Port, portMaxHealth);
        starboard = new HullQuadrant(HullSide.Starboard, starboardMaxHealth);
        prow = new HullQuadrant(HullSide.Prow, prowMaxHealth);
        aft = new HullQuadrant(HullSide.Aft, aftMaxHealth);
    }

    /// <summary>
    /// Apply damage to a specific hull side.
    /// If the hull is already breached, overflow damage is passed to internal subsystems via events.
    /// </summary>
    public void TakeDamage(HullSide side, float damage)
    {
        HullQuadrant quadrant = GetQuadrant(side);
        if (quadrant == null) return;

        bool wasBreached = quadrant.isBreached;
        float overflow = quadrant.TakeDamage(damage);

        OnHullDamaged?.Invoke(side, damage, quadrant.currentHealth);

        // Notify breach if this is the first time
        if (quadrant.isBreached && !wasBreached)
        {
            breachNotified[(int)side] = true;
            OnHullBreached?.Invoke(side);
            Debug.Log($"[HullSystem] {side} hull BREACHED!");
        }

        // If already breached, overflow damage goes to internals (handled by InternalSubsystems listener)
        if (wasBreached && overflow > 0f)
        {
            // The overflow is the full damage since hull was already at 0
            // InternalSubsystems subscribes to OnHullDamaged and checks if breached
        }

        // Check total destruction
        if (port.isBreached && starboard.isBreached && prow.isBreached && aft.isBreached)
        {
            OnShipDestroyed?.Invoke();
        }
    }

    /// <summary>
    /// Determine which hull side was hit based on the impact direction relative to the ship.
    /// Call this from collision/damage handlers to determine the correct quadrant.
    /// </summary>
    public HullSide DetermineHitSide(Vector3 hitDirection)
    {
        // Transform hit direction into local space
        Vector3 localDir = transform.InverseTransformDirection(hitDirection.normalized);

        // In Unity 2D top-down: transform.up = forward (Prow), transform.right = right (Starboard)
        float forward = Vector3.Dot(localDir, Vector3.up);    // Prow (+) vs Aft (-)
        float right = Vector3.Dot(localDir, Vector3.right);   // Starboard (+) vs Port (-)

        // Determine primary axis
        if (Mathf.Abs(forward) > Mathf.Abs(right))
        {
            return forward > 0 ? HullSide.Prow : HullSide.Aft;
        }
        else
        {
            return right > 0 ? HullSide.Starboard : HullSide.Port;
        }
    }

    public HullQuadrant GetQuadrant(HullSide side)
    {
        return side switch
        {
            HullSide.Port => port,
            HullSide.Starboard => starboard,
            HullSide.Prow => prow,
            HullSide.Aft => aft,
            _ => null
        };
    }

    public bool IsBreached(HullSide side)
    {
        HullQuadrant q = GetQuadrant(side);
        return q != null && q.isBreached;
    }

    /// <summary>
    /// Get total remaining hull integrity as a percentage (0-1).
    /// </summary>
    public float GetTotalIntegrityPercentage()
    {
        float totalMax = portMaxHealth + starboardMaxHealth + prowMaxHealth + aftMaxHealth;
        float totalCurrent = port.currentHealth + starboard.currentHealth + prow.currentHealth + aft.currentHealth;
        return totalMax > 0 ? totalCurrent / totalMax : 0f;
    }

    /// <summary>
    /// Get total current health across all quadrants.
    /// </summary>
    public float GetTotalHealth()
    {
        return port.currentHealth + starboard.currentHealth + prow.currentHealth + aft.currentHealth;
    }

    /// <summary>
    /// Get total max health across all quadrants.
    /// </summary>
    public float GetTotalMaxHealth()
    {
        return portMaxHealth + starboardMaxHealth + prowMaxHealth + aftMaxHealth;
    }
}
