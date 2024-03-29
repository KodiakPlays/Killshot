using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }


    public bool eActive, bActive, sActive, pActive;

    public bool enterKeyActivate;
    public GameObject eGreenImage, eRedImage;
    public GameObject bGreenImage, bRedImage;
    public GameObject sGreenImage, sRedImage;
    public GameObject pGreenImage, pRedImage;

    public Power power;

    //bool to get that enemy is detected or not
    public bool isEnemyDetect;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        eActive = false;
        bActive = false;
        sActive = false;
        pActive = false;
        eRedImage.SetActive(true);
        eGreenImage.SetActive(false);
        bRedImage.SetActive(true);
        bGreenImage.SetActive(false);
        sRedImage.SetActive(true);
        sGreenImage.SetActive(false);
        pRedImage.SetActive(true);
        pGreenImage.SetActive(false);
    }
    private void Start()
    {
    }
    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void EButtonActive()
    {
        if (!eActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            eActive = true;
            eGreenImage.SetActive(true);
            eRedImage.SetActive(false);
        }
        else if( eActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            eActive = false;
            eGreenImage.SetActive(false);
            eRedImage.SetActive(true);
        }
        sRedImage.SetActive(true);
        sGreenImage.SetActive(false);
        bRedImage.SetActive(true);
        bGreenImage.SetActive(false);
        pRedImage.SetActive(true);
        pGreenImage.SetActive(false);

        sActive = false;
        bActive = false;
        pActive = false;
    }
    public void BButtonActivate()
    {
        if (!bActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            bActive = true;
            bGreenImage.SetActive(true);
            bRedImage.SetActive(false);
        }
        else if(bActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            bActive = false;
            bGreenImage.SetActive(false);
            bRedImage.SetActive(true);
        }
        eRedImage.SetActive(true);
        eGreenImage.SetActive(false);
        sRedImage.SetActive(true);
        sGreenImage.SetActive(false);
        pRedImage.SetActive(true);
        pGreenImage.SetActive(false);

        eActive = false;
        sActive = false;
        pActive = false;
    }
    public void SButtonActivate()
    {
        eRedImage.SetActive(false);
        if (!sActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            sActive = true;
            sGreenImage.SetActive(true);
            sRedImage.SetActive(false);
        }
        else if (sActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            sActive = false;
            sGreenImage.SetActive(false);
            sRedImage.SetActive(true);
        }
        eRedImage.SetActive(true);
        eGreenImage.SetActive(false);
        bRedImage.SetActive(true);
        bGreenImage.SetActive(false);
        pRedImage.SetActive(true);
        pGreenImage.SetActive(false);

        eActive = false;
        bActive = false;
        pActive = false;
    }
    public void pButtonActivate()
    {
        //pRedImage.SetActive(false);
        if (!pActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            pActive = true;
            pGreenImage.SetActive(true);
            pRedImage.SetActive(false);
        }
        else if (pActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            pActive = false;
            pGreenImage.SetActive(false);
            pRedImage.SetActive(true);
        }
        eRedImage.SetActive(true);
        eGreenImage.SetActive(false);
        bRedImage.SetActive(true);
        bGreenImage.SetActive(false);
        sRedImage.SetActive(true);
        eGreenImage.SetActive(false);

        eActive = false;
        bActive = false;
        sActive = false;
    }
    public void returnButton()
    {
        enterKeyActivate = true;
    }
    public void CloseApp()
    {
        Application.Quit();
    }
    public void PowerEnergyAdd()
    {
        if (pActive)
        {
            if((power.enginePower >= 0 && power.enginePower <= 10) && (power.reactorPower >=0 && power.reactorPower <= 10))
            {
                Invoke("EnergyAdd", 3f);
            }
        }
    }
    public void PowerEnergyMinus()
    {
        if(pActive)
        {
            if(power.enginePower >= 0 && power.enginePower <= 10 && (power.reactorPower >= 0 && power.reactorPower <= 10))
            {
                power.enginePower--;
                power.reactorPower++;
            }
        }
    }
    public void PowerWeaponAdd()
    {
        if(pActive)
        {
            if(power.weaponPower >= 0 && power.weaponPower <= 10 && (power.reactorPower >= 0 && power.reactorPower <= 10))
            {
                Invoke("WeaponAdd", 3f);
            }
        }
    }
    public void PowerWeaponMinus()
    {
        if (pActive)
        {
            if(power.weaponPower >= 0 && power.weaponPower <= 10 && (power.reactorPower >= 0 && power.reactorPower <= 10))
            {
                power.weaponPower--;
                power.reactorPower++;
            }
            
        }
    }
    public void PowerSensorAdd()
    {
        if(power.sensorPower >= 0 && power.sensorPower <= 10 && (power.reactorPower >= 0 && power.reactorPower <= 10))
        {
            Invoke("SensorAdd", 3f);
        }
    }
    public void PowerSensorMinus()
    {
        if(power.sensorPower >= 0 && power.sensorPower <= 10 && (power.reactorPower >= 0 && power.reactorPower <= 10))
        {
            power.sensorPower--;
            power.reactorPower++;
        }
    }
    void EnergyAdd()
    {
        power.enginePower++;
        power.reactorPower--;
    }
    void WeaponAdd()
    {
        power.weaponPower++;
        power.reactorPower--;
    }
    void SensorAdd()
    {
        power.sensorPower++;
        power.reactorPower--;
    }
}
