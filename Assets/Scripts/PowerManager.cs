// =================================================================================================
// PowerManager.cs
//
// What it does:
// The power system of the older physics player ship. A reactor feeds five power channels:
// engines, arms (weapons), bay, support and sig (sensors). It is on the PlayerShip prefab. The
// battle board does not use this script. The board's power is in PowerSystem.
//
// How it works:
// - The reactor has 15 power, 8 on the PlayerShip prefab. Reactor damage from InternalSubsystems
//   lowers this. Each channel has 5 bars. At the start every channel is off at 0.
// - Each channel is off, drawing or venting. Every frame, drawing channels fill at 0.5 bars per
//   second, shared between all channels drawing. Each bar costs the reactor 0.5 power.
//   A channel stops drawing when it is full.
// - Venting empties a channel at 1 bar per second and gives the power back to the reactor.
//   Venting cannot be stopped once it starts.
// - Keys 1, 2 and 3 on PlayerShip step a channel through off, draw, off, vent, off. The power
//   buttons in the Demo scene and the Canvas prefab do the same. The UIController panel buttons
//   and the controller d-pad only switch between off and draw.
// - Other code can add or remove one bar at once. Hyperspeed on PlayerShip uses this to drain
//   engine power.
// - Vent All vents every channel. Black Alert sets engines and arms drawing and the rest off.
//   The older UIController screen can empty every channel at once.
// - The old Railgun empties every channel at once and takes the reactor offline. No channel can
//   draw until the reactor restarts.
// - Other scripts ask it for a channel's power as a share of full (0 to 1). PlayerShip, Autopilot
//   and the old Railgun use this.
// - Each bar above the first gives a bonus. Among them are more engine push, slower stability
//   loss and faster cannon loading. Full engine power unlocks supercruise. Only the stability
//   bonus is read. PlayerShip reads it.
// - PowerManagerEditor shows it in the Inspector. The older UIController screen shows the bars.
//
// Technical:
// - Data: five PowerChannel objects (engines, arms, bay, support, sig). Each has a PowerState,
//   whole bars (currentPower, maxPower 5) and a readyToVent flag.
// - currentReactorPower is a float. GetReactorPower rounds it down. Each bar costs 0.5.
// - GetSystemEfficiency(name) gives bars over max. Names: engines, arms or weapons, bay, support,
//   sig or sensors. Any other name gives 0.
// - Callers: PlayerShip, Railgun, UIController, InternalSubsystems (DrainSystemPower) and
//   Autopilot.
// - Scene buttons in Demo and the Canvas prefab call ToggleEngines, ToggleWeapons, ToggleSensors,
//   ToggleCrew and ToggleShields. Renaming these breaks the buttons.
// - ToggleCrew switches the support channel. ToggleShields switches the bay channel.
// - Numbers: fill rate, vent rate and reactor size are in the Inspector. Start resets every
//   channel to 0. Rates count scaled Time.deltaTime.
// - Order: [DefaultExecutionOrder(-10)] runs it before UIController each frame.
// - Trap: Black Alert vents nothing. Its end-of-frame step replaces the vent states in the same
//   frame.
// - Nothing sets finishingCurrentBar to true. The code that finishes a part-filled bar never runs.
// =================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PowerState
{
    Standby,
    Draw,
    Vent
}

[System.Serializable]
public class PowerChannel
{
    public string name;
    public PowerState currentState = PowerState.Standby;
    public int maxPower = 5;
    public int currentPower = 0;
    // True after Draw→Standby; next toggle starts Vent (spec: Standby→Draw→Standby→Vent→Standby)
    public bool readyToVent = false;
    // True while completing a partial bar after switching from Draw to Standby
    public bool finishingCurrentBar = false;
}

[DefaultExecutionOrder(-10)] // Ensures PowerManager.Start() runs before UIController.Start()
public class PowerManager : MonoBehaviour
{
    [Header("Power Systems")]
    public PowerChannel engines = new PowerChannel { name = "Engines" };
    public PowerChannel arms    = new PowerChannel { name = "Arms" };
    public PowerChannel bay     = new PowerChannel { name = "Bay" };
    public PowerChannel support = new PowerChannel { name = "Support" };
    public PowerChannel sig     = new PowerChannel { name = "Sig" };

    [Header("Power Settings")]
    [SerializeField] private float powerFillRate = 0.5f; // bars/sec base rate, divided evenly across active systems
    [SerializeField] private float ventRate = 1f;        // 1 bar per second
    [SerializeField] private int reactorMaxPower = 15;
    [SerializeField] private float currentReactorPower = 15f;

    private PowerChannel[] allSystems;
    private List<PowerChannel> activeSystems = new List<PowerChannel>();
    private Dictionary<PowerChannel, float> powerAccumulator = new Dictionary<PowerChannel, float>();
    private bool reactorOnline = true; // false during railgun post-fire reboot; blocks drawing
    private InternalSubsystems internalSubsystems;

    private void Awake()
    {
        allSystems = new[] { engines, arms, bay, support, sig };
    }

    private void Start()
    {
        internalSubsystems = GetComponent<InternalSubsystems>();

        foreach (var system in allSystems)
            powerAccumulator[system] = 0f;

        // All systems start in Standby at 0 power. Reactor starts full.
        // The player uses the charge buttons to direct power to individual systems.
        foreach (var system in allSystems)
        {
            system.currentState = PowerState.Standby;
            system.currentPower = 0;
            system.readyToVent = false;
            system.finishingCurrentBar = false;
        }
        currentReactorPower = reactorMaxPower;
    }

    private void Update()
    {
        UpdatePowerSystems();
    }

    private void UpdatePowerSystems()
    {
        // Reactor power is clamped if internal subsystems are damaged
        if (internalSubsystems != null)
        {
            int regenMax = Mathf.RoundToInt(reactorMaxPower * internalSubsystems.GetReactorMultiplier());
            if (currentReactorPower > regenMax)
                currentReactorPower = regenMax;
        }

        // Separate drawing/finishing systems from venting systems
        activeSystems.Clear();
        int drawingCount = 0;
        foreach (var system in allSystems)
        {
            if (system.currentState == PowerState.Draw)
            {
                activeSystems.Add(system);
                drawingCount++;
            }
            else if (system.finishingCurrentBar)
            {
                activeSystems.Add(system);
            }
            else if (system.currentState == PowerState.Vent)
            {
                VentPower(system);
            }
        }

        if (activeSystems.Count > 0)
        {
            // Divide fill rate evenly across truly-drawing systems only;
            // finishing systems complete their bar at full solo rate
            float baseRate = drawingCount > 0 ? powerFillRate / drawingCount : powerFillRate;

            foreach (var system in activeSystems)
            {
                float rate = system.finishingCurrentBar ? powerFillRate : baseRate;

                // Only allow drawing if reactor is online
                if (reactorOnline && currentReactorPower >= 0.5f && system.currentPower < system.maxPower)
                {
                    powerAccumulator[system] += rate * Time.deltaTime;
                    if (powerAccumulator[system] >= 1f)
                    {
                        powerAccumulator[system] = 0f;
                        system.currentPower++;
                        currentReactorPower -= 0.5f;
                        if (system.finishingCurrentBar)
                            system.finishingCurrentBar = false;
                        // Auto-stop drawing when the system is full; the UI switch will flip off automatically
                        if (system.currentState == PowerState.Draw && system.currentPower >= system.maxPower)
                        {
                            system.currentState = PowerState.Standby;
                            system.readyToVent = false;
                            powerAccumulator[system] = 0f;
                        }
                    }
                }
                else if (system.finishingCurrentBar)
                {
                    // Reactor ran out or system full — stop finishing
                    system.finishingCurrentBar = false;
                    powerAccumulator[system] = 0f;
                }
            }
        }
    }

    // Spec state cycle (fixed): Standby → Draw → Standby → Vent → Standby
    public void ToggleSystemState(PowerChannel system)
    {
        switch (system.currentState)
        {
            case PowerState.Standby:
                if (!system.readyToVent)
                {
                    // First standby → start drawing
                    system.currentState = PowerState.Draw;
                }
                else
                {
                    // Second standby (after draw) → start venting, unless already at full power
                    if (system.currentPower >= system.maxPower)
                    {
                        // System is full — reset the vent flag so the next press draws again
                        system.readyToVent = false;
                    }
                    else
                    {
                        system.currentState = PowerState.Vent;
                        system.readyToVent = false;
                        powerAccumulator[system] = 0f;
                    }
                }
                break;

            case PowerState.Draw:
                // Stop drawing instantly
                system.currentState = PowerState.Standby;
                system.readyToVent = true;
                system.finishingCurrentBar = false;
                powerAccumulator[system] = 0f;
                break;

            case PowerState.Vent:
                // Spec: "You cannot stop venting once triggered" — ignore input
                break;
        }
    }

    /// <summary>
    /// Simple Draw ↔ Standby toggle used by the charge/discharge UI buttons.
    /// Never triggers Vent — that is reserved for VentBtn / VentAllSystems.
    /// </summary>
    public void ToggleDrawState(PowerChannel system)
    {
        if (system.currentState == PowerState.Draw)
        {
            system.currentState = PowerState.Standby;
            system.readyToVent = false; // reset so next button press starts Draw, not Vent
            system.finishingCurrentBar = false;
            if (powerAccumulator.ContainsKey(system))
                powerAccumulator[system] = 0f;
        }
        else if (system.currentState == PowerState.Standby)
        {
            system.currentState = PowerState.Draw;
            system.finishingCurrentBar = false;
        }
        // Vent state: ignore — let the vent complete naturally
    }

    private void VentPower(PowerChannel system)
    {
        if (system.currentPower > 0)
        {
            powerAccumulator[system] += ventRate * Time.deltaTime;
            if (powerAccumulator[system] >= 1f)
            {
                powerAccumulator[system] = 0f;
                system.currentPower--;
                currentReactorPower += 0.5f;
            }
        }
        else
        {
            // Vent complete — return to clean Standby
            system.currentState = PowerState.Standby;
            system.readyToVent = false;
            powerAccumulator[system] = 0f;
        }
    }

    // --- Bonus Calculations (spec: bonuses apply per PWR above the minimum of 1) ----

    private int BonusBars(PowerChannel system) => Mathf.Max(0, system.currentPower - 1);

    // Engines: +10% acceleration per bonus bar; -5% stability decay per bonus bar; MAX PWR = Supercruise
    public float GetEngineAccelerationMultiplier() { return 1f + BonusBars(engines) * 0.10f; }
    public float GetEngineStabilityDecayMultiplier() { return 1f - BonusBars(engines) * 0.05f; }
    public bool IsSupercruiseUnlocked() { return engines.currentPower >= engines.maxPower; }

    // Arms: -2.5% cannon load time per bonus bar
    public float GetCannonLoadTimeMultiplier() { return 1f - BonusBars(arms) * 0.025f; }

    // Arms/Weapons: +10% boarding pod range per bonus bar
    public float GetBoardingPodRangeMultiplier() { return 1f + BonusBars(arms) * 0.10f; }

    // Support: -5% ability cooldown per bonus bar
    public float GetAbilityCooldownMultiplier() { return 1f - BonusBars(support) * 0.05f; }

    // Sig: +1% perfect-hit window per bonus bar; +1s comms intercept per bonus bar
    public float GetScannerPerfectHitBonus() { return BonusBars(sig) * 0.01f; }
    public float GetCommsInterceptTimeBonus() { return BonusBars(sig) * 1f; }

    // Legacy efficiency accessor (0–1). Kept so existing callers don't break.
    public float GetSystemEfficiency(string systemName)
    {
        //Debug.Log($"[PowerManager] GetSystemEfficiency called for {systemName}");
        PowerChannel system = GetSystemByName(systemName);
        return system != null ? (float)system.currentPower / system.maxPower : 0f;
    }

    // --- Manual +/- Power Allocation (UI panel buttons) ----------------------

    public void AddPower(PowerChannel system)
    {
        if (reactorOnline && currentReactorPower >= 0.5f && system.currentPower < system.maxPower)
        {
            system.currentPower++;
            currentReactorPower -= 0.5f;
        }
    }

    public void RemovePower(PowerChannel system)
    {
        if (system.currentPower > 0)
        {
            system.currentPower--;
            currentReactorPower += 0.5f;
        }
    }

    public void AddEnginesPower()  { AddPower(engines); }
    public void AddArmsPower()     { AddPower(arms); }
    public void AddBayPower()      { AddPower(bay); }
    public void AddSupportPower()  { AddPower(support); }
    public void AddSigPower()      { AddPower(sig); }

    public void RemoveEnginesPower()  { RemovePower(engines); }
    public void RemoveArmsPower()     { RemovePower(arms); }
    public void RemoveBayPower()      { RemovePower(bay); }
    public void RemoveSupportPower()  { RemovePower(support); }
    public void RemoveSigPower()      { RemovePower(sig); }

    // --- Toggle helpers ------------------------------------------------------

    public void ToggleEngines() { ToggleSystemState(engines); }
    public void ToggleArms()    { ToggleSystemState(arms); }
    public void ToggleBay()     { ToggleSystemState(bay); }
    public void ToggleSupport() { ToggleSystemState(support); }
    public void ToggleSig()     { ToggleSystemState(sig); }

    // Legacy names kept for existing prefab references
    public void ToggleWeapons() { ToggleSystemState(arms); }
    public void ToggleSensors() { ToggleSystemState(sig); }
    public void ToggleCrew()    { ToggleSystemState(support); }  // key_CRW button
    public void ToggleShields() { ToggleSystemState(bay); }      // key_SHD button

    // --- Vent all / Emergency ------------------------------------------------

    /// <summary>Immediately removes up to <paramref name="bars"/> of power from the Arms system (returns removed bars to the reactor).</summary>
    public void DrainArmsPower(int bars)
    {
        int toDrain = Mathf.Min(bars, arms.currentPower);
        arms.currentPower -= toDrain;
        currentReactorPower += toDrain * 0.5f;
    }

    /// <summary>Instantly zeros all system power, returning it to the reactor. Reactor goes offline (used by railgun after firing). Call RebootReactor() to bring it back online.</summary>
    public void DrainAllPowerInstantly()
    {
        foreach (var system in allSystems)
        {
            currentReactorPower += system.currentPower * 0.5f;
            system.currentPower = 0;
            system.currentState = PowerState.Standby;
            system.readyToVent = false;
            system.finishingCurrentBar = false;
            if (powerAccumulator.ContainsKey(system))
                powerAccumulator[system] = 0f;
        }
        currentReactorPower = Mathf.Min(currentReactorPower, reactorMaxPower);
        // Reactor goes offline — ship systems go fully dark
        reactorOnline = false;
    }

    /// <summary>
    /// Brings the reactor back online after a railgun standby reboot.
    /// All ship systems remain in hard Standby — the player must manually
    /// restart each one. The reactor must be online before any system can draw.
    /// </summary>
    public void RebootReactor()
    {
        reactorOnline = true;
        // No systems are auto-engaged — manual startup required.
    }

    public void VentAllSystems()
    {
        foreach (var system in allSystems)
        {
            if (system.currentPower > 0)
            {
                system.currentState = PowerState.Vent;
                system.readyToVent = false;
                system.finishingCurrentBar = false;
                powerAccumulator[system] = 0f;
            }
        }
    }

    public void EmergencyVent() { VentAllSystems(); }

    /// <summary>
    /// Instantly zeros every system's power and returns it all to the reactor.
    /// Unlike <see cref="DrainAllPowerInstantly"/>, the reactor itself is NOT drained
    /// and stays online — the player can immediately start redirecting power.
    /// All systems are placed into a clean Standby (no Draw, no Vent, no pending bars).
    /// </summary>
    public void EmergencyVentSystemsOnly()
    {
        foreach (var system in allSystems)
        {
            currentReactorPower += system.currentPower * 0.5f; // return power to reactor
            system.currentPower = 0;
            system.currentState = PowerState.Standby;
            system.readyToVent = false;
            system.finishingCurrentBar = false;
            if (powerAccumulator.ContainsKey(system))
                powerAccumulator[system] = 0f;
        }
        // Clamp to max in case of floating point drift
        currentReactorPower = Mathf.Min(currentReactorPower, reactorMaxPower);
    }

    public void BlackAlert()
    {
        VentAllSystems();
        StartCoroutine(ActivateBlackAlertSystems());
    }

    private IEnumerator ActivateBlackAlertSystems()
    {
        yield return new WaitForEndOfFrame();
        engines.currentState = PowerState.Draw;
        arms.currentState    = PowerState.Draw;
        bay.currentState     = PowerState.Standby;
        support.currentState = PowerState.Standby;
        sig.currentState     = PowerState.Standby;
    }

    // --- Utility / UI --------------------------------------------------------

    public PowerState GetSystemState(string systemName)
    {
        return GetSystemByName(systemName)?.currentState ?? PowerState.Standby;
    }

    public int GetSystemPower(string systemName)
    {
        return GetSystemByName(systemName)?.currentPower ?? 0;
    }

    public int GetSystemMaxPower(string systemName)
    {
        return GetSystemByName(systemName)?.maxPower ?? 0;
    }

    public int GetReactorPower() { return Mathf.FloorToInt(currentReactorPower); }

    public int GetMaxReactorPower()
    {
        return internalSubsystems != null
            ? Mathf.RoundToInt(reactorMaxPower * internalSubsystems.GetReactorMultiplier())
            : reactorMaxPower;
    }

    public void DrainSystemPower(string systemName)
    {
        PowerChannel system = GetSystemByName(systemName);
        if (system != null && system.currentPower > 0)
        {
            system.currentPower--;
            currentReactorPower += 0.5f;
        }
    }

    private PowerChannel GetSystemByName(string name)
    {
        switch (name.ToLower())
        {
            case "engines":             return engines;
            case "arms": case "weapons": return arms;
            case "bay":                 return bay;
            case "support":             return support;
            case "sig": case "sensors": return sig;
            default:                    return null;
        }
    }
}
