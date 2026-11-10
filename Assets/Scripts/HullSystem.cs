// =================================================================================================
// HullSystem.cs
//
// What it does:
// The hull of the older physics player ship, split into four sides with separate health: port
// (left), starboard (right), prow (front) and aft (back). It is on the PlayerShip prefab.
// The battle board does not use this script. The board's damage is in DamageSystem.
//
// How it works:
// - At the start it sets each side to full health. The defaults are port 50, starboard 50,
//   prow 40 and aft 10, set in the Inspector.
// - When PlayerShip asks, it works out the side a hit came from. A hit more along the ship is
//   front or back. A hit more across it is left or right.
// - A hit takes health from that one side only. A side cannot go below 0.
// - After each hit it tells anyone listening the side hit, the damage, and the health left.
//   InternalSubsystems listens and damages the parts inside when that side is broken.
// - The first time a side reaches 0, it tells listeners the side is breached. It writes a line to
//   the Console.
// - When all four sides are at 0, it tells listeners the ship is destroyed. PlayerShip and
//   GameManager listen.
// - Other scripts can ask for each side's health, the total, and the total as a share of full.
//   The older UIController screen shows the four sides this way.
//
// Technical:
// - Data: four HullQuadrant objects (port, starboard, prow, aft). Each has maxHealth and
//   currentHealth, in hull points.
// - Awake copies the Inspector maximums into the quadrants. GetTotalMaxHealth and
//   GetTotalIntegrityPercentage read the Inspector values directly.
// - TakeDamage(side, damage) and DetermineHitSide(direction) are called by PlayerShip. PlayerShip
//   adds a HullSystem if its object has none.
// - Events: OnHullDamaged(side, damage, health left), OnHullBreached(side), OnShipDestroyed.
//   InternalSubsystems, PlayerShip and GameManager listen.
// - UIController reads each quadrant's GetHealthPercentage for its hull display.
// - DetermineHitSide takes a direction from the ship toward the hit, with the nose along
//   transform.up.
// - Trap: Laser passes its travel direction, the reverse. A hit on the nose counts as aft. On the
//   PlayerShip prefab this path never runs: Shields takes laser hits first.
// - Trap: OnShipDestroyed fires again on every hit after all four sides reach 0.
// - The overflow from HullQuadrant.TakeDamage and the breachNotified list are never read.
// =================================================================================================

using UnityEngine;
using System;

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
