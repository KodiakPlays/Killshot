using UnityEngine;
using System;

/// <summary>
/// Internal ship subsystems per GDD spec.
/// Each system is associated with a hull side. When that side is breached,
/// damage to that side can randomly hit one of the associated subsystems.
/// 
/// Associations:
///   Prow  -> Bridge, Sensors
///   Aft   -> Reactor, Engines
///   Starboard -> Magazine
///   Port  -> Life Support, Crew
/// 
/// Effects when damaged:
///   Bridge       - Movement lag, loss of steering
///   Magazine     - Damages weapons (guns, missiles, PDCs, PODs)
///   Life Support - Activates a Game Over timer
///   Engines      - Reduces max speed, increases turn time  
///   Reactor      - Limits total power, drains power from random active system
///   Sensors      - Disables scanner, reduces radar range
///   Crew         - Disables abilities
/// </summary>
public enum SubsystemType
{
    Bridge,
    Magazine,
    LifeSupport,
    Engines,
    Reactor,
    Sensors,
    Crew
}

[System.Serializable]
public class Subsystem
{
    public SubsystemType type;
    public HullSide associatedSide;
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDamaged => currentHealth < maxHealth;
    public bool isDestroyed => currentHealth <= 0f;

    public Subsystem(SubsystemType type, HullSide side, float maxHealth = 100f)
    {
        this.type = type;
        this.associatedSide = side;
        this.maxHealth = maxHealth;
        this.currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0f, currentHealth - damage);
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

public class InternalSubsystems : MonoBehaviour
{
    [Header("Subsystem Health")]
    [SerializeField] private float subsystemBaseHealth = 100f;

    [Header("Life Support Game Over")]
    [SerializeField] private float lifeSupportTimerDuration = 120f; // Seconds before game over
    private float lifeSupportTimer = -1f;
    private bool lifeSupportTimerActive = false;

    // Subsystem instances
    public Subsystem bridge { get; private set; }
    public Subsystem magazine { get; private set; }
    public Subsystem lifeSupport { get; private set; }
    public Subsystem engines { get; private set; }
    public Subsystem reactor { get; private set; }
    public Subsystem sensors { get; private set; }
    public Subsystem crew { get; private set; }

    // Events
    public event Action<SubsystemType, float> OnSubsystemDamaged;
    public event Action<SubsystemType> OnSubsystemDestroyed;
    public event Action OnLifeSupportCritical; // Game Over timer started
    public event Action OnLifeSupportFailed;   // Timer expired = Game Over

    // References
    private HullSystem hullSystem;
    private PowerManager powerManager;

    // Debuff tracking
    private float reactorDrainTimer = 0f;
    [SerializeField] private float reactorDrainInterval = 5f; // Drain random system every 5s when reactor damaged

    private void Awake()
    {
        // Initialize all subsystems with their hull side associations per GDD
        bridge      = new Subsystem(SubsystemType.Bridge, HullSide.Prow, subsystemBaseHealth);
        sensors     = new Subsystem(SubsystemType.Sensors, HullSide.Prow, subsystemBaseHealth);
        reactor     = new Subsystem(SubsystemType.Reactor, HullSide.Aft, subsystemBaseHealth);
        engines     = new Subsystem(SubsystemType.Engines, HullSide.Aft, subsystemBaseHealth);
        magazine    = new Subsystem(SubsystemType.Magazine, HullSide.Starboard, subsystemBaseHealth);
        lifeSupport = new Subsystem(SubsystemType.LifeSupport, HullSide.Port, subsystemBaseHealth);
        crew        = new Subsystem(SubsystemType.Crew, HullSide.Port, subsystemBaseHealth);
    }

    private void Start()
    {
        hullSystem = GetComponent<HullSystem>();
        powerManager = GetComponent<PowerManager>();

        if (hullSystem != null)
        {
            hullSystem.OnHullDamaged += HandleHullDamage;
        }
    }

    private void OnDestroy()
    {
        if (hullSystem != null)
        {
            hullSystem.OnHullDamaged -= HandleHullDamage;
        }
    }

    private void Update()
    {
        // Life support timer
        if (lifeSupportTimerActive)
        {
            lifeSupportTimer -= Time.deltaTime;
            if (lifeSupportTimer <= 0f)
            {
                lifeSupportTimerActive = false;
                OnLifeSupportFailed?.Invoke();
                Debug.Log("[InternalSubsystems] LIFE SUPPORT FAILED - GAME OVER");
            }
        }

        // Reactor damage effect: drain power from random active system periodically
        if (reactor.isDamaged && !reactor.isDestroyed && powerManager != null)
        {
            reactorDrainTimer += Time.deltaTime;
            if (reactorDrainTimer >= reactorDrainInterval)
            {
                reactorDrainTimer = 0f;
                DrainRandomSystem();
            }
        }
    }

    /// <summary>
    /// Called when hull takes damage. If the hull side is breached, randomly damage an internal subsystem.
    /// </summary>
    private void HandleHullDamage(HullSide side, float damage, float remainingHealth)
    {
        // Only damage internals if hull is breached (remainingHealth <= 0)
        if (remainingHealth > 0f) return;

        // Get subsystems associated with this hull side
        Subsystem[] candidates = GetSubsystemsForSide(side);
        if (candidates.Length == 0) return;

        // Randomly pick one of the associated subsystems
        Subsystem target = candidates[UnityEngine.Random.Range(0, candidates.Length)];
        
        // Apply damage to the subsystem
        bool wasDestroyed = target.isDestroyed;
        target.TakeDamage(damage);

        OnSubsystemDamaged?.Invoke(target.type, target.currentHealth);
        Debug.Log($"[InternalSubsystems] {target.type} took {damage} damage! Health: {target.currentHealth}/{target.maxHealth}");

        // Check for subsystem destruction
        if (target.isDestroyed && !wasDestroyed)
        {
            OnSubsystemDestroyed?.Invoke(target.type);
            ApplySubsystemDestroyedEffect(target.type);
            Debug.Log($"[InternalSubsystems] {target.type} DESTROYED!");
        }
    }

    private Subsystem[] GetSubsystemsForSide(HullSide side)
    {
        return side switch
        {
            HullSide.Prow => new[] { bridge, sensors },
            HullSide.Aft => new[] { reactor, engines },
            HullSide.Starboard => new[] { magazine },
            HullSide.Port => new[] { lifeSupport, crew },
            _ => new Subsystem[0]
        };
    }

    private void ApplySubsystemDestroyedEffect(SubsystemType type)
    {
        switch (type)
        {
            case SubsystemType.LifeSupport:
                // Start Game Over timer
                lifeSupportTimerActive = true;
                lifeSupportTimer = lifeSupportTimerDuration;
                OnLifeSupportCritical?.Invoke();
                Debug.Log($"[InternalSubsystems] LIFE SUPPORT CRITICAL! {lifeSupportTimerDuration}s until Game Over!");
                break;

            // Other effects are applied via GetDebuff methods queried by other systems
            case SubsystemType.Bridge:
                Debug.Log("[InternalSubsystems] Bridge destroyed - movement lag and steering loss!");
                break;
            case SubsystemType.Magazine:
                Debug.Log("[InternalSubsystems] Magazine hit - weapons damaged!");
                break;
            case SubsystemType.Engines:
                Debug.Log("[InternalSubsystems] Engines destroyed - reduced speed and turn rate!");
                break;
            case SubsystemType.Reactor:
                Debug.Log("[InternalSubsystems] Reactor damaged - power limited, draining systems!");
                break;
            case SubsystemType.Sensors:
                Debug.Log("[InternalSubsystems] Sensors destroyed - scanner disabled, radar range reduced!");
                break;
            case SubsystemType.Crew:
                Debug.Log("[InternalSubsystems] Crew incapacitated - abilities disabled!");
                break;
        }
    }

    private void DrainRandomSystem()
    {
        if (powerManager == null) return;

        // Pick a random active system to drain per GDD: "Drains power from random active system"
        string[] systems = { "engines", "weapons", "sensors" };
        string target = systems[UnityEngine.Random.Range(0, systems.Length)];
        
        powerManager.DrainSystemPower(target);
    }

    // === Debuff Query Methods (called by other systems) ===

    /// <summary>
    /// Movement speed multiplier. Engines damage reduces max speed.
    /// </summary>
    public float GetSpeedMultiplier()
    {
        if (engines.isDestroyed) return 0.3f;
        if (engines.isDamaged) return Mathf.Lerp(0.5f, 1f, engines.GetHealthPercentage());
        return 1f;
    }

    /// <summary>
    /// Turn rate multiplier. Bridge damage causes movement lag. Engines damage increases turn time.
    /// </summary>
    public float GetTurnRateMultiplier()
    {
        float mult = 1f;
        if (bridge.isDestroyed) mult *= 0.2f;
        else if (bridge.isDamaged) mult *= Mathf.Lerp(0.4f, 1f, bridge.GetHealthPercentage());

        if (engines.isDestroyed) mult *= 0.3f;
        else if (engines.isDamaged) mult *= Mathf.Lerp(0.5f, 1f, engines.GetHealthPercentage());

        return mult;
    }

    /// <summary>
    /// Reactor power multiplier. Reactor damage limits total available power.
    /// </summary>
    public float GetReactorMultiplier()
    {
        if (reactor.isDestroyed) return 0.2f;
        if (reactor.isDamaged) return Mathf.Lerp(0.3f, 1f, reactor.GetHealthPercentage());
        return 1f;
    }

    /// <summary>
    /// Sensor range multiplier. Sensor damage reduces radar range.
    /// </summary>
    public float GetSensorRangeMultiplier()
    {
        if (sensors.isDestroyed) return 0f; // scanner disabled
        if (sensors.isDamaged) return Mathf.Lerp(0.3f, 1f, sensors.GetHealthPercentage());
        return 1f;
    }

    /// <summary>
    /// Whether weapons are impaired from Magazine damage.
    /// </summary>
    public float GetWeaponMultiplier()
    {
        if (magazine.isDestroyed) return 0f; // weapons disabled
        if (magazine.isDamaged) return Mathf.Lerp(0.3f, 1f, magazine.GetHealthPercentage());
        return 1f;
    }

    /// <summary>
    /// Whether abilities are disabled from Crew damage.
    /// </summary>
    public bool AreAbilitiesDisabled()
    {
        return crew.isDestroyed;
    }

    /// <summary>
    /// Whether scanner is disabled from Sensor damage.
    /// </summary>
    public bool IsScannerDisabled()
    {
        return sensors.isDestroyed;
    }

    /// <summary>
    /// Life support timer remaining. -1 if not active.
    /// </summary>
    public float GetLifeSupportTimeRemaining()
    {
        return lifeSupportTimerActive ? lifeSupportTimer : -1f;
    }

    /// <summary>
    /// Repair a specific subsystem.
    /// </summary>
    public void RepairSubsystem(SubsystemType type, float amount)
    {
        Subsystem sub = GetSubsystem(type);
        if (sub == null) return;

        bool wasDestroyed = sub.isDestroyed;
        sub.Repair(amount);

        // If life support was repaired, cancel game over timer
        if (type == SubsystemType.LifeSupport && wasDestroyed && !sub.isDestroyed)
        {
            lifeSupportTimerActive = false;
            lifeSupportTimer = -1f;
            Debug.Log("[InternalSubsystems] Life support repaired! Game over timer cancelled.");
        }
    }

    public Subsystem GetSubsystem(SubsystemType type)
    {
        return type switch
        {
            SubsystemType.Bridge => bridge,
            SubsystemType.Magazine => magazine,
            SubsystemType.LifeSupport => lifeSupport,
            SubsystemType.Engines => engines,
            SubsystemType.Reactor => reactor,
            SubsystemType.Sensors => sensors,
            SubsystemType.Crew => crew,
            _ => null
        };
    }
}
