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
public class PowerSystem
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
    public PowerSystem engines = new PowerSystem { name = "Engines" };
    public PowerSystem arms    = new PowerSystem { name = "Arms" };
    public PowerSystem bay     = new PowerSystem { name = "Bay" };
    public PowerSystem support = new PowerSystem { name = "Support" };
    public PowerSystem sig     = new PowerSystem { name = "Sig" };

    [Header("Power Settings")]
    [SerializeField] private float powerFillRate = 0.5f; // bars/sec base rate, divided evenly across active systems
    [SerializeField] private float ventRate = 1f;        // 1 bar per second
    [SerializeField] private float reactorRegenRate = 1f; // bars/sec the reactor passively regenerates
    [SerializeField] private int reactorMaxPower = 15;
    [SerializeField] private int currentReactorPower = 15;

    private PowerSystem[] allSystems;
    private List<PowerSystem> activeSystems = new List<PowerSystem>();
    private Dictionary<PowerSystem, float> powerAccumulator = new Dictionary<PowerSystem, float>();
    private float reactorRegenAccumulator = 0f;
    private bool reactorOnline = true; // false during railgun post-fire reboot; blocks passive regen
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
        // Reactor passively regenerates only when online
        if (reactorOnline)
        {
            int regenMax = reactorMaxPower;
            if (internalSubsystems != null)
                regenMax = Mathf.RoundToInt(reactorMaxPower * internalSubsystems.GetReactorMultiplier());

            if (currentReactorPower < regenMax)
            {
                reactorRegenAccumulator += reactorRegenRate * Time.deltaTime;
                while (reactorRegenAccumulator >= 1f)
                {
                    reactorRegenAccumulator -= 1f;
                    currentReactorPower = Mathf.Min(currentReactorPower + 1, regenMax);
                }
            }
            else
            {
                reactorRegenAccumulator = 0f;
                if (currentReactorPower > regenMax)
                    currentReactorPower = regenMax;
            }
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

                if (currentReactorPower > 0 && system.currentPower < system.maxPower)
                {
                    powerAccumulator[system] += rate * Time.deltaTime;
                    if (powerAccumulator[system] >= 1f)
                    {
                        powerAccumulator[system] = 0f;
                        system.currentPower++;
                        currentReactorPower--;
                        if (system.finishingCurrentBar)
                            system.finishingCurrentBar = false;
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
    public void ToggleSystemState(PowerSystem system)
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
                    // Second standby (after draw) → start venting; cannot be cancelled
                    system.currentState = PowerState.Vent;
                    system.readyToVent = false;
                    powerAccumulator[system] = 0f;
                }
                break;

            case PowerState.Draw:
                // Stop drawing; finish the current partial bar then hold in standby
                system.currentState = PowerState.Standby;
                system.readyToVent = true;
                if (powerAccumulator[system] > 0f)
                    system.finishingCurrentBar = true;
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
    public void ToggleDrawState(PowerSystem system)
    {
        if (system.currentState == PowerState.Draw)
        {
            system.currentState = PowerState.Standby;
            system.readyToVent = false; // reset so next button press starts Draw, not Vent
            if (powerAccumulator.ContainsKey(system) && powerAccumulator[system] > 0f)
                system.finishingCurrentBar = true;
        }
        else if (system.currentState == PowerState.Standby)
        {
            system.currentState = PowerState.Draw;
            system.finishingCurrentBar = false;
        }
        // Vent state: ignore — let the vent complete naturally
    }

    private void VentPower(PowerSystem system)
    {
        if (system.currentPower > 0)
        {
            powerAccumulator[system] += ventRate * Time.deltaTime;
            if (powerAccumulator[system] >= 1f)
            {
                powerAccumulator[system] = 0f;
                system.currentPower--;
                currentReactorPower++;
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

    private int BonusBars(PowerSystem system) => Mathf.Max(0, system.currentPower - 1);

    // Engines: +10% acceleration per bonus bar; -5% stability decay per bonus bar; MAX PWR = Supercruise
    public float GetEngineAccelerationMultiplier() { return 1f + BonusBars(engines) * 0.10f; }
    public float GetEngineStabilityDecayMultiplier() { return 1f - BonusBars(engines) * 0.05f; }
    public bool IsSupercruiseUnlocked() { return engines.currentPower >= engines.maxPower; }

    // Arms: -2.5% cannon load time per bonus bar
    public float GetCannonLoadTimeMultiplier() { return 1f - BonusBars(arms) * 0.025f; }

    // Bay: +10% boarding pod range per bonus bar
    public float GetBoardingPodRangeMultiplier() { return 1f + BonusBars(bay) * 0.10f; }

    // Support: -5% ability cooldown per bonus bar
    public float GetAbilityCooldownMultiplier() { return 1f - BonusBars(support) * 0.05f; }

    // Sig: +1% perfect-hit window per bonus bar; +1s comms intercept per bonus bar
    public float GetScannerPerfectHitBonus() { return BonusBars(sig) * 0.01f; }
    public float GetCommsInterceptTimeBonus() { return BonusBars(sig) * 1f; }

    // Legacy efficiency accessor (0–1). Kept so existing callers don't break.
    public float GetSystemEfficiency(string systemName)
    {
        //Debug.Log($"[PowerManager] GetSystemEfficiency called for {systemName}");
        PowerSystem system = GetSystemByName(systemName);
        return system != null ? (float)system.currentPower / system.maxPower : 0f;
    }

    // --- Manual +/- Power Allocation (UI panel buttons) ----------------------

    public void AddPower(PowerSystem system)
    {
        if (currentReactorPower > 0 && system.currentPower < system.maxPower)
        {
            system.currentPower++;
            currentReactorPower--;
        }
    }

    public void RemovePower(PowerSystem system)
    {
        if (system.currentPower > 0)
        {
            system.currentPower--;
            currentReactorPower++;
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
        currentReactorPower += toDrain;
    }

    /// <summary>Instantly zeros all system power AND the reactor (used by railgun after firing). Call RebootReactor() to bring it back online.</summary>
    public void DrainAllPowerInstantly()
    {
        foreach (var system in allSystems)
        {
            system.currentPower = 0;
            system.currentState = PowerState.Standby;
            system.readyToVent = false;
            system.finishingCurrentBar = false;
            if (powerAccumulator.ContainsKey(system))
                powerAccumulator[system] = 0f;
        }
        // Drain reactor completely — ship goes fully dark
        currentReactorPower = 0;
        reactorRegenAccumulator = 0f;
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
        reactorRegenAccumulator = 0f;
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

    public int GetReactorPower() { return currentReactorPower; }

    public int GetMaxReactorPower()
    {
        return internalSubsystems != null
            ? Mathf.RoundToInt(reactorMaxPower * internalSubsystems.GetReactorMultiplier())
            : reactorMaxPower;
    }

    public void DrainSystemPower(string systemName)
    {
        PowerSystem system = GetSystemByName(systemName);
        if (system != null && system.currentPower > 0)
        {
            system.currentPower--;
            currentReactorPower++;
        }
    }

    private PowerSystem GetSystemByName(string name)
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
