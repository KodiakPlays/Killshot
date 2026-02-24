using UnityEngine;
using UnityEngine.UI;
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

    [Header("Power UI")]
    [SerializeField] private List<Button> btnPowerBool = new List<Button>();
    [SerializeField] private List<Image> imgPowerMet = new List<Image>();
    private List<UIPowerClass> uiPowerMetClass = new List<UIPowerClass>();
    public Dictionary<int, IEnumerator> powerAnimCoroutine = new Dictionary<int, IEnumerator>();
    [SerializeField] private Shader shaPowerMet;
    [SerializeField] private Shader shaReactorMet;
    private int reactorCharges = 0;
    [SerializeField] private List<Sprite> uiSprite = new List<Sprite>();
    [SerializeField] private List<Image> btnPowerBoolImage = new List<Image>();
    [SerializeField] private Shader powerNodeSha;
    [SerializeField] private Image[] powerNodeImg;
    [SerializeField] private List<int> powerState = new List<int>();

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

        // Initialize power UI
        if (btnPowerBool.Count > 0)
        {
            StartPower();
        }
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

    // ===== Power UI Methods (moved from UIController) =====

    private void StartPower()
    {
        for (int i = 0; i < btnPowerBool.Count - 1; i++)
        {
            uiPowerMetClass.Add(new UIPowerClass(new Material(shaPowerMet), 6, 1, 0f, false));

            imgPowerMet[i].material = uiPowerMetClass[i].mat;
        }

        uiPowerMetClass.Add(new UIPowerClass(new Material(shaReactorMet), 15, 15, 0f, false));

        imgPowerMet[btnPowerBool.Count - 1].material = uiPowerMetClass[btnPowerBool.Count - 1].mat;

        for (int i = 0; i < powerNodeImg.Length; i++)
        {
            powerNodeImg[i].material = new Material(powerNodeSha);

            powerNodeImg[i].material.SetInt("_On", 0);
        }

        for (int i = 0; i < btnPowerBool.Count - 1; i++)
        {
            powerAnimCoroutine[i] = null;
        }
    }

    private void PowerNodes(int i, int state)
    {
        if (state == 0)
        {
            powerNodeImg[i].material.SetInt("_On", 0);
            powerNodeImg[i].material.SetInt("_Charge", 0);
            powerNodeImg[i].material.SetInt("_End", 0);
        }
        else if (state > 0)
        {
            powerNodeImg[i].material.SetInt("_On", 1);
        }

        if (state == 1)
        {
            powerNodeImg[i].material.SetInt("_Charge", 0);
            powerNodeImg[i].material.SetFloat("_DegreeHor", 0);
            powerNodeImg[i].material.SetInt("_End", 0);
        }
        else if (state == 2)
        {
            powerNodeImg[i].material.SetInt("_Charge", 1);
            powerNodeImg[i].material.SetFloat("_DegreeVert", 270);
            powerNodeImg[i].material.SetFloat("_DegreeHor", 0);
            powerNodeImg[i].material.SetInt("_End", 0);
        }
        else if (state == 3)
        {
            powerNodeImg[i].material.SetInt("_Charge", 1);
            powerNodeImg[i].material.SetFloat("_DegreeVert", 270);
            powerNodeImg[i].material.SetFloat("_DegreeHor", 0);
            powerNodeImg[i].material.SetInt("_End", 1);
        }

        if (state == 4)
        {
            powerNodeImg[i].material.SetInt("_Charge", 0);
            powerNodeImg[i].material.SetFloat("_DegreeHor", 180);
            powerNodeImg[i].material.SetInt("_End", 0);
        }
        else if (state == 5)
        {
            powerNodeImg[i].material.SetInt("_Charge", 1);
            powerNodeImg[i].material.SetFloat("_DegreeVert", 90);
            powerNodeImg[i].material.SetFloat("_DegreeHor", 180);
            powerNodeImg[i].material.SetInt("_End", 0);
        }
        else if (state == 6)
        {
            powerNodeImg[i].material.SetInt("_Charge", 1);
            powerNodeImg[i].material.SetFloat("_DegreeVert", 90);
            powerNodeImg[i].material.SetFloat("_DegreeHor", 180);
            powerNodeImg[i].material.SetInt("_End", 1);
        }
        else if (state == 7)
        {
            powerNodeImg[i].material.SetInt("_Charge", 1);
            powerNodeImg[i].material.SetFloat("_DegreeVert", 90);
            powerNodeImg[i].material.SetFloat("_DegreeHor", 0);
            powerNodeImg[i].material.SetInt("_Reactor", 1);
        }
        else if (state == 8)
        {
            powerNodeImg[i].material.SetInt("_Charge", 1);
            powerNodeImg[i].material.SetFloat("_DegreeVert", 270);
            powerNodeImg[i].material.SetFloat("_DegreeHor", 180);
            powerNodeImg[i].material.SetInt("_Reactor", 1);
        }
    }

    public void ChargeBtn(int i)
    {
        if (!uiPowerMetClass[i].charge)
        {
            ChargeOn(i);
        }
        else if (uiPowerMetClass[i].charge)
        {
            ChargeOff(i);
        }
    }

    public void ChargeOn(int i)
    {
        uiPowerMetClass[i].Charge(true);

        powerAnimCoroutine[i] = ChargeOnAnim(i);

        StartCoroutine(powerAnimCoroutine[i]);

        btnPowerBoolImage[i].sprite = uiSprite[1];

        ChargeNodeCheck();
    }

    public void ChargeOff(int i)
    {
        uiPowerMetClass[i].Charge(false);

        StopCoroutine(powerAnimCoroutine[i]);

        btnPowerBoolImage[i].sprite = uiSprite[0];

        ChargeNodeCheck();
    }

    private void ChargeNodeCheck()
    {
        int lastCharge = 0;
        int noCharge = 0;

        PowerNodes(5, 7);

        for (int j = 0; j < uiPowerMetClass.Count - 1; j++)
        {
            if (uiPowerMetClass[j].charge)
            {
                lastCharge = j;
                break;
            }
            else if (!uiPowerMetClass[j].charge)
            {
                PowerNodes(j, 0);
                noCharge++;
            }
        }

        if (noCharge == uiPowerMetClass.Count - 1)
        {
            PowerNodes(5, 0);
            return;
        }

        for (int j = lastCharge; j < uiPowerMetClass.Count - 1; j++)
        {
            if (j != lastCharge)
            {
                if (!uiPowerMetClass[j].charge)
                {
                    PowerNodes(j, 1);
                }
                else if (uiPowerMetClass[j].charge)
                {
                    PowerNodes(j, 2);
                }
            }
            else if (j == lastCharge)
            {
                PowerNodes(j, 3);
            }
        }
    }

    private void VentNodeCheck()
    {
        int lastVent = 0;
        int noVent = 0;

        PowerNodes(5, 8);

        for (int j = 0; j < uiPowerMetClass.Count - 1; j++)
        {
            if (uiPowerMetClass[j].cur > 1)
            {
                lastVent = j;
                break;
            }
            else if (uiPowerMetClass[j].cur <= 1)
            {
                PowerNodes(j, 0);
                noVent++;
            }
        }

        if (noVent == uiPowerMetClass.Count - 1)
        {
            PowerNodes(5, 0);
            return;
        }

        for (int j = lastVent; j < uiPowerMetClass.Count - 1; j++)
        {
            if (j != lastVent)
            {
                if (uiPowerMetClass[j].cur <= 1)
                {
                    PowerNodes(j, 4);
                }
                else if (uiPowerMetClass[j].cur > 1)
                {
                    PowerNodes(j, 5);
                }
            }
            else if (j == lastVent)
            {
                PowerNodes(j, 6);
            }
        }
    }

    public IEnumerator ChargeOnAnim(int i)
    {
        float speed = 1f;
        float time = 0;
        float crgAmt = 5f;

        while (time < crgAmt)
        {
            Mathf.MoveTowards(0, crgAmt, time);
            time += (Time.deltaTime) * (speed);
            yield return null;
        }

        if (uiPowerMetClass[i].cur >= uiPowerMetClass[i].max || uiPowerMetClass[btnPowerBool.Count - 1].cur <= 0)
        {
            ChargeOff(i);
        }
        else if (uiPowerMetClass[i].cur < uiPowerMetClass[i].max && uiPowerMetClass[btnPowerBool.Count - 1].cur > 0)
        {
            uiPowerMetClass[i].cur++;
            uiPowerMetClass[uiPowerMetClass.Count - 1].cur--;

            uiPowerMetClass[i].mat.SetFloat("_PowerCur", uiPowerMetClass[i].cur);
            uiPowerMetClass[uiPowerMetClass.Count - 1].mat.SetFloat("_PowerCur", uiPowerMetClass[uiPowerMetClass.Count - 1].cur);

            ChargeOn(i);
        }
    }

    public void VentBtn()
    {
        for (int i = 0; i < uiPowerMetClass.Count - 1; i++)
        {
            if (powerAnimCoroutine[i] == null)
            {
                continue;
            }

            uiPowerMetClass[i].Charge(false);
            StopCoroutine(powerAnimCoroutine[i]);
        }

        StartCoroutine(VentAnim());
    }

    private IEnumerator VentAnim()
    {
        float time = 0;
        float speed = 1;
        float crgAmt = 5f;

        while (time < crgAmt)
        {
            Mathf.MoveTowards(0, crgAmt, time);
            time += (Time.deltaTime) * (speed);

            for (int i = 0; i < uiPowerMetClass.Count - 1; i++)
            {
                if (uiPowerMetClass[i].cur > 1)
                {
                    uiPowerMetClass[i].cur--;
                    uiPowerMetClass[uiPowerMetClass.Count - 1].cur++;

                    uiPowerMetClass[i].mat.SetFloat("_PowerCur", uiPowerMetClass[i].cur);
                    uiPowerMetClass[uiPowerMetClass.Count - 1].mat.SetFloat("_PowerCur", uiPowerMetClass[uiPowerMetClass.Count - 1].cur);

                    yield return new WaitForSeconds(.5f);
                }

                VentNodeCheck();
            }
        }
    }
}
