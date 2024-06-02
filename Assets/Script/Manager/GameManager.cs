using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }
    public bool eActive, bActive, fActive, pActive, lActive;

    public bool enterKeyActivate;
    public GameObject eGreenImage, eRedImage;
    public GameObject bGreenImage, bRedImage;
    public GameObject fGreenImage, fRedImage;
    public GameObject pGreenImage, pRedImage;
    public GameObject lGreenImage, lRedImage;

    public bool engineActivate, weaponActive, sensorActive;

    public Power power;
    //bool to get that enemy is detected or not
    public bool isEnemyDetect;
    //public bool isShipInside;

    [SerializeField] GameObject gameOverPanal;

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
        fActive = false;
        pActive = false;
        lActive = false;

        eRedImage.SetActive(true);
        eGreenImage.SetActive(false);
        bRedImage.SetActive(true);
        bGreenImage.SetActive(false);
        fRedImage.SetActive(true);
        fGreenImage.SetActive(false);
        pRedImage.SetActive(true);
        pGreenImage.SetActive(false);
        lRedImage.SetActive(true);
        lGreenImage.SetActive(false);

        gameOverPanal.SetActive(false);
    }
   
    private void Update()
    {
        if(!(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Application.Quit();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                EButtonActive();
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                BButtonActivate();
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                FButtonActivate();
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                pButtonActivate();
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                LButtonActive();
            }
            if (pActive)
            {
                if (Input.GetKeyDown(KeyCode.G))
                {
                    BalanceBetweenSystem();
                }
                if (Input.GetKeyDown(KeyCode.H))
                {
                    FocusOnEngine();
                }
                if (Input.GetKeyDown(KeyCode.J))
                {
                    FocuseOnWeapons();
                }
                if (Input.GetKeyDown(KeyCode.K))
                {
                    FocusOnSensor();
                }
            }
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
        fRedImage.SetActive(true);
        fGreenImage.SetActive(false);
        bRedImage.SetActive(true);
        bGreenImage.SetActive(false);
        pRedImage.SetActive(true);
        pGreenImage.SetActive(false);
        lRedImage.SetActive(true);
        lGreenImage.SetActive(false);

        fActive = false;
        bActive = false;
        pActive = false;
        lActive = false;
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
        fRedImage.SetActive(true);
        fGreenImage.SetActive(false);
        pRedImage.SetActive(true);
        pGreenImage.SetActive(false);
        lRedImage.SetActive(true);
        lGreenImage.SetActive(false);

        eActive = false;
        fActive = false;
        pActive = false;
        lActive = false;
    }
    public void FButtonActivate()
    {
        eRedImage.SetActive(false);
        if (!fActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            fActive = true;
            fGreenImage.SetActive(true);
            fRedImage.SetActive(false);
        }
        else if (fActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            fActive = false;
            fGreenImage.SetActive(false);
            fRedImage.SetActive(true);
        }
        eRedImage.SetActive(true);
        eGreenImage.SetActive(false);
        bRedImage.SetActive(true);
        bGreenImage.SetActive(false);
        pRedImage.SetActive(true);
        pGreenImage.SetActive(false);
        lRedImage.SetActive(true);
        lGreenImage.SetActive(false);

        eActive = false;
        bActive = false;
        pActive = false;
        lActive = false;
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
        fRedImage.SetActive(true);
        fGreenImage.SetActive(false);
        lRedImage.SetActive(true);
        lGreenImage.SetActive(false);

        eActive = false;
        bActive = false;
        fActive = false;
        lActive = false;
    }
    public void LButtonActive()
    {
        if (!lActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            lActive = true;
            lGreenImage.SetActive(true);
            lRedImage.SetActive(false);
        }
        else if (lActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            lActive = false;
            lGreenImage.SetActive(false);
            lRedImage.SetActive(true);
        }
        eRedImage.SetActive(true);
        eGreenImage.SetActive(false);
        bRedImage.SetActive(true);
        bGreenImage.SetActive(false);
        fRedImage.SetActive(true);
        eGreenImage.SetActive(false);
        pRedImage.SetActive(true);
        pGreenImage.SetActive(false);

        eActive = false;
        bActive = false;
        fActive = false;
        pActive = false;
    }
    public void returnButton()
    {
        enterKeyActivate = true;
    }
    public void CloseApp()
    {
        Application.Quit();
    }
   
    
    public void EngineActivation()
    {
        if (pActive)
        {
            engineActivate = true;
            weaponActive = false;
            sensorActive = false;
        }
        
    }
    public void WeaponActivation()
    {
        if (pActive)
        {
            weaponActive = true;
            sensorActive = false;
            engineActivate = false;
        }
        
    }
    public void SensorActivation()
    {
        if (pActive)
        {
            sensorActive = true;
            weaponActive = false;
            engineActivate = false;
        }
        
    }
    public void BalanceBetweenSystem()
    {
        if(pActive)
        {
            power.enginePower = 3;
            power.weaponPower = 3;
            power.sensorPower = 3;
            power.reactorPower = 3;
        }
        
    }
    public void FocusOnEngine()
    {
        if (pActive)
        {
            power.enginePower = 7;
            power.weaponPower = 2;
            power.sensorPower = 2;
            power.reactorPower = 3;
        }
        
    }
    public void FocuseOnWeapons()
    {
        if (pActive)
        {
            power.enginePower = 2;
            power.weaponPower = 7;
            power.sensorPower = 2;
            power.reactorPower = 3;
        }
        
    }
    public void FocusOnSensor()
    {
        if (pActive)
        {
            power.enginePower = 2;
            power.weaponPower = 2;
            power.sensorPower = 7;
            power.reactorPower = 3;
        }
        
    }
    public void EndGame()
    {
        Time.timeScale = 0;
        gameOverPanal.SetActive(true);
    }
}