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
    [SerializeField] private int reactorMaxPower = 8;
    [SerializeField] private int currentReactorPower = 8;

    private PowerSystem[] allSystems;
    private List<PowerSystem> activeSystems = new List<PowerSystem>();
    private Dictionary<PowerSystem, float> powerAccumulator = new Dictionary<PowerSystem, float>();
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

        // Start with engines drawing
        engines.currentState = PowerState.Draw;
        engines.currentPower = 3;
        currentReactorPower -= 3;
    }

    private void Update()
    {
        UpdatePowerSystems();
    }

    private void UpdatePowerSystems()
    {
        // Reactor damage limits total power available
        if (internalSubsystems != null)
        {
            int effectiveMax = Mathf.RoundToInt(reactorMaxPower * internalSubsystems.GetReactorMultiplier());
            if (currentReactorPower > effectiveMax)
                currentReactorPower = effectiveMax;
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
        Debug.Log($"[PowerManager] ToggleSystemState called for {system.name}");
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
    public float GetEngineAccelerationMultiplier() { Debug.Log("[PowerManager] GetEngineAccelerationMultiplier called"); return 1f + BonusBars(engines) * 0.10f; }
    public float GetEngineStabilityDecayMultiplier() { Debug.Log("[PowerManager] GetEngineStabilityDecayMultiplier called"); return 1f - BonusBars(engines) * 0.05f; }
    public bool IsSupercruiseUnlocked() { Debug.Log("[PowerManager] IsSupercruiseUnlocked called"); return engines.currentPower >= engines.maxPower; }

    // Arms: -2.5% cannon load time per bonus bar
    public float GetCannonLoadTimeMultiplier() { Debug.Log("[PowerManager] GetCannonLoadTimeMultiplier called"); return 1f - BonusBars(arms) * 0.025f; }

    // Bay: +10% boarding pod range per bonus bar
    public float GetBoardingPodRangeMultiplier() { Debug.Log("[PowerManager] GetBoardingPodRangeMultiplier called"); return 1f + BonusBars(bay) * 0.10f; }

    // Support: -5% ability cooldown per bonus bar
    public float GetAbilityCooldownMultiplier() { Debug.Log("[PowerManager] GetAbilityCooldownMultiplier called"); return 1f - BonusBars(support) * 0.05f; }

    // Sig: +1% perfect-hit window per bonus bar; +1s comms intercept per bonus bar
    public float GetScannerPerfectHitBonus() { Debug.Log("[PowerManager] GetScannerPerfectHitBonus called"); return BonusBars(sig) * 0.01f; }
    public float GetCommsInterceptTimeBonus() { Debug.Log("[PowerManager] GetCommsInterceptTimeBonus called"); return BonusBars(sig) * 1f; }

    // Legacy efficiency accessor (0–1). Kept so existing callers don't break.
    public float GetSystemEfficiency(string systemName)
    {
        Debug.Log($"[PowerManager] GetSystemEfficiency called for {systemName}");
        PowerSystem system = GetSystemByName(systemName);
        return system != null ? (float)system.currentPower / system.maxPower : 0f;
    }

    // --- Manual +/- Power Allocation (UI panel buttons) ----------------------

    public void AddPower(PowerSystem system)
    {
        Debug.Log($"[PowerManager] AddPower called for {system.name}");
        if (currentReactorPower > 0 && system.currentPower < system.maxPower)
        {
            system.currentPower++;
            currentReactorPower--;
        }
    }

    public void RemovePower(PowerSystem system)
    {
        Debug.Log($"[PowerManager] RemovePower called for {system.name}");
        if (system.currentPower > 0)
        {
            system.currentPower--;
            currentReactorPower++;
        }
    }

    public void AddEnginesPower()  { Debug.Log("[PowerManager] AddEnginesPower called"); AddPower(engines); }
    public void AddArmsPower()     { Debug.Log("[PowerManager] AddArmsPower called"); AddPower(arms); }
    public void AddBayPower()      { Debug.Log("[PowerManager] AddBayPower called"); AddPower(bay); }
    public void AddSupportPower()  { Debug.Log("[PowerManager] AddSupportPower called"); AddPower(support); }
    public void AddSigPower()      { Debug.Log("[PowerManager] AddSigPower called"); AddPower(sig); }

    public void RemoveEnginesPower()  { Debug.Log("[PowerManager] RemoveEnginesPower called"); RemovePower(engines); }
    public void RemoveArmsPower()     { Debug.Log("[PowerManager] RemoveArmsPower called"); RemovePower(arms); }
    public void RemoveBayPower()      { Debug.Log("[PowerManager] RemoveBayPower called"); RemovePower(bay); }
    public void RemoveSupportPower()  { Debug.Log("[PowerManager] RemoveSupportPower called"); RemovePower(support); }
    public void RemoveSigPower()      { Debug.Log("[PowerManager] RemoveSigPower called"); RemovePower(sig); }

    // --- Toggle helpers ------------------------------------------------------

    public void ToggleEngines() { Debug.Log("[PowerManager] ToggleEngines called"); ToggleSystemState(engines); }
    public void ToggleArms()    { Debug.Log("[PowerManager] ToggleArms called"); ToggleSystemState(arms); }
    public void ToggleBay()     { Debug.Log("[PowerManager] ToggleBay called"); ToggleSystemState(bay); }
    public void ToggleSupport() { Debug.Log("[PowerManager] ToggleSupport called"); ToggleSystemState(support); }
    public void ToggleSig()     { Debug.Log("[PowerManager] ToggleSig called"); ToggleSystemState(sig); }

    // Legacy names kept for existing prefab references
    public void ToggleWeapons() { Debug.Log("[PowerManager] ToggleWeapons called"); ToggleSystemState(arms); }
    public void ToggleSensors() { Debug.Log("[PowerManager] ToggleSensors called"); ToggleSystemState(sig); }
    public void ToggleCrew()    { Debug.Log("[PowerManager] ToggleCrew called"); ToggleSystemState(support); }  // key_CRW button
    public void ToggleShields() { Debug.Log("[PowerManager] ToggleShields called"); ToggleSystemState(bay); }      // key_SHD button

    // --- Vent all / Emergency ------------------------------------------------

    public void VentAllSystems()
    {
        Debug.Log("[PowerManager] VentAllSystems called");
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

    public void EmergencyVent() { Debug.Log("[PowerManager] EmergencyVent called"); VentAllSystems(); }

    public void BlackAlert()
    {
        Debug.Log("[PowerManager] BlackAlert called");
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
        Debug.Log($"[PowerManager] GetSystemState called for {systemName}");
        return GetSystemByName(systemName)?.currentState ?? PowerState.Standby;
    }

    public int GetSystemPower(string systemName)
    {
        Debug.Log($"[PowerManager] GetSystemPower called for {systemName}");
        return GetSystemByName(systemName)?.currentPower ?? 0;
    }

    public int GetSystemMaxPower(string systemName)
    {
        Debug.Log($"[PowerManager] GetSystemMaxPower called for {systemName}");
        return GetSystemByName(systemName)?.maxPower ?? 0;
    }

    public int GetReactorPower() { Debug.Log("[PowerManager] GetReactorPower called"); return currentReactorPower; }

    public int GetMaxReactorPower()
    {
        Debug.Log("[PowerManager] GetMaxReactorPower called");
        return internalSubsystems != null
            ? Mathf.RoundToInt(reactorMaxPower * internalSubsystems.GetReactorMultiplier())
            : reactorMaxPower;
    }

    public void DrainSystemPower(string systemName)
    {
        Debug.Log($"[PowerManager] DrainSystemPower called for {systemName}");
        PowerSystem system = GetSystemByName(systemName);
        if (system != null && system.currentPower > 0)
        {
            system.currentPower--;
            currentReactorPower++;
            Debug.Log($"[PowerManager] Reactor damage drained 1 power from {systemName}");
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
