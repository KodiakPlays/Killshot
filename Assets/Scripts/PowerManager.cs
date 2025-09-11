using UnityEngine;
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
}
