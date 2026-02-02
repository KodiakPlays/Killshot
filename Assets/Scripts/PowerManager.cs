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
    public bool isDrawing = false;
}

public class PowerManager : MonoBehaviour
{
    [Header("Power Systems")]
    public PowerSystem engines = new PowerSystem { name = "Engines" };
    public PowerSystem weapons = new PowerSystem { name = "Weapons" };
    public PowerSystem sensors = new PowerSystem { name = "Sensors" };

    [Header("Power Settings")]
    [SerializeField] private float powerFillRate = 0.5f; // 2 seconds per bar
    [SerializeField] private float ventRate = 1f; // 1 bar per second
    [SerializeField] private int reactorMaxPower = 8;
    [SerializeField] private int currentReactorPower = 8;

    private List<PowerSystem> activeSystems = new List<PowerSystem>();
    private Dictionary<PowerSystem, float> powerAccumulator = new Dictionary<PowerSystem, float>();

    private void Start()
    {
        // Initialize power accumulators
        powerAccumulator[engines] = 0f;
        powerAccumulator[weapons] = 0f;
        powerAccumulator[sensors] = 0f;
        
        // Start with engines powered up for basic movement
        engines.currentState = PowerState.Draw;
        engines.currentPower = 3; // Start with some power in engines
        currentReactorPower -= 3;
    }

    private void Update()
    {
        UpdatePowerSystems();
    }

    private void UpdatePowerSystems()
    {
        // Update active systems
        activeSystems.Clear();
        foreach (var system in new[] { engines, weapons, sensors })
        {
            if (system.currentState == PowerState.Draw)
            {
                activeSystems.Add(system);
            }
            else if (system.currentState == PowerState.Vent)
            {
                VentPower(system);
            }
        }

        // Calculate power distribution
        if (activeSystems.Count > 0)
        {
            float baseRate = powerFillRate / activeSystems.Count;
            foreach (var system in activeSystems)
            {
                if (currentReactorPower > 0 && system.currentPower < system.maxPower)
                {
                    powerAccumulator[system] += baseRate * Time.deltaTime;
                    if (powerAccumulator[system] >= 1f)
                    {
                        powerAccumulator[system] = 0f;
                        system.currentPower++;
                        currentReactorPower--;
                    }
                }
            }
        }
    }

    public void ToggleSystemState(PowerSystem system)
    {
        switch (system.currentState)
        {
            case PowerState.Standby:
                system.currentState = PowerState.Draw;
                break;
            case PowerState.Draw:
                system.currentState = PowerState.Vent;
                break;
            case PowerState.Vent:
                system.currentState = PowerState.Standby;
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
            system.currentState = PowerState.Standby;
        }
    }

    public float GetSystemEfficiency(string systemName)
    {
        PowerSystem system = null;
        switch (systemName.ToLower())
        {
            case "engines":
                system = engines;
                break;
            case "weapons":
                system = weapons;
                break;
            case "sensors":
                system = sensors;
                break;
        }

        if (system != null)
        {
            return (float)system.currentPower / system.maxPower;
        }
        return 0f;
    }

    // UI Button Methods for Power Management Panel
    
    // Individual System Controls
    public void ToggleEngines()
    {
        ToggleSystemState(engines);
    }
    
    public void ToggleWeapons()
    {
        ToggleSystemState(weapons);
    }
    
    public void ToggleCrew()
    {
        // Placeholder for crew system - currently maps to sensors
        ToggleSystemState(sensors);
    }
    
    public void ToggleShields()
    {
        // Placeholder for shields system - you may want to add this as a new PowerSystem
        Debug.Log("Shields system not yet implemented");
    }
    
    public void ToggleSensors()
    {
        ToggleSystemState(sensors);
    }
    
    // Special Actions
    public void EmergencyVent()
    {
        // Instantly vent all systems to standby and return power to reactor
        VentAllSystems();
        Debug.Log("Emergency Vent Activated - All systems venting to standby");
    }
    
    public void BlackAlert()
    {
        // Emergency power redistribution - prioritize engines and weapons
        VentAllSystems();
        
        // Wait a frame then auto-activate critical systems
        StartCoroutine(ActivateBlackAlertSystems());
        Debug.Log("Black Alert Activated - Emergency power redistribution");
    }
    
    public void VentAllSystems()
    {
        engines.currentState = PowerState.Vent;
        weapons.currentState = PowerState.Vent;
        sensors.currentState = PowerState.Vent;
    }
    
    private System.Collections.IEnumerator ActivateBlackAlertSystems()
    {
        yield return new WaitForEndOfFrame();
        
        // Auto-activate engines and weapons for combat readiness
        engines.currentState = PowerState.Draw;
        weapons.currentState = PowerState.Draw;
        
        // Keep sensors in standby to save power
        sensors.currentState = PowerState.Standby;
    }
    
    // Utility Methods for UI
    public PowerState GetSystemState(string systemName)
    {
        switch (systemName.ToLower())
        {
            case "engines":
                return engines.currentState;
            case "weapons":
                return weapons.currentState;
            case "sensors":
                return sensors.currentState;
            default:
                return PowerState.Standby;
        }
    }
    
    public int GetSystemPower(string systemName)
    {
        switch (systemName.ToLower())
        {
            case "engines":
                return engines.currentPower;
            case "weapons":
                return weapons.currentPower;
            case "sensors":
                return sensors.currentPower;
            default:
                return 0;
        }
    }
    
    public int GetSystemMaxPower(string systemName)
    {
        switch (systemName.ToLower())
        {
            case "engines":
                return engines.maxPower;
            case "weapons":
                return weapons.maxPower;
            case "sensors":
                return sensors.maxPower;
            default:
                return 0;
        }
    }
    
    public int GetReactorPower()
    {
        return currentReactorPower;
    }
    
    public int GetMaxReactorPower()
    {
        return reactorMaxPower;
    }
}
