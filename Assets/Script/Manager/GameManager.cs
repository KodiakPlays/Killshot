using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }
    public bool eActive, bActive, fActive, pActive, lActive;

    public GameObject eGreenImage, eRedImage;
    public GameObject bGreenImage, bRedImage;
    public GameObject fGreenImage, fRedImage;
    public GameObject pGreenImage, pRedImage;
    public GameObject lGreenImage, lRedImage;


    public Power power;
    //bool to get that enemy is detected or not
    [HideInInspector]
    public bool isEnemyDetect;
    [HideInInspector]
    public GameObject detectedEnemy;
    //public bool isShipInside;

    [SerializeField] GameObject gameOverPanal;
    [SerializeField] GameObject spaceship;
    public SpaceshipMovement shipMoveScript;
    public GameObject GameWinPanale;
    public GameObject EscapePoint;
    public int avgPower, minPower, maxPower;

    
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

        eRedImage.SetActive(false);
        eGreenImage.SetActive(true);
        bRedImage.SetActive(false);
        bGreenImage.SetActive(true);
        fRedImage.SetActive(true);
        fGreenImage.SetActive(false);
        pRedImage.SetActive(true);
        pGreenImage.SetActive(false);
        lRedImage.SetActive(true);
        lGreenImage.SetActive(false);

        gameOverPanal.SetActive(false);

        EscapePoint.SetActive(false);

        //Power distribution
        //minPower = 2;
       //avgPower = 3;
        //maxPower = 7;

    }
   
    private void Update()
    {
        KeyboardInputs();
    }
    /// <summary>
    ///  this function is use to controls the inputs using Hotkeys
    /// </summary>
    void KeyboardInputs()
    {
        if (!(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Application.Quit();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                shipMoveScript.isEnterPress = false;
                //EButtonActive();
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                shipMoveScript.isEnterPress = false;
                //BButtonActivate();
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                shipMoveScript.isEnterPress = false;
                pButtonActivate();
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                shipMoveScript.isEnterPress = false;
                LButtonActive();
            }
            if (pActive)
            {
                if (Input.GetKeyDown(KeyCode.G))
                {
                    shipMoveScript.isEnterPress = false;
                    BalanceBetweenSystem();
                }
                if (Input.GetKeyDown(KeyCode.H))
                {
                    shipMoveScript.isEnterPress = false;
                    FocusOnEngine();
                }
                if (Input.GetKeyDown(KeyCode.J))
                {
                    shipMoveScript.isEnterPress = false;
                    FocuseOnWeapons();
                }
                if (Input.GetKeyDown(KeyCode.K))
                {
                    shipMoveScript.isEnterPress = false;
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
   
    public void BalanceBetweenSystem()
    {
        if(pActive)
        {
            power.enginePower = avgPower;
            power.weaponPower = avgPower;
            power.sensorPower = avgPower;
            power.reactorPower = avgPower;
        }
        
    }
    public void FocusOnEngine()
    {
        if (pActive)
        {
            power.enginePower = maxPower;
            power.weaponPower = minPower;
            power.sensorPower = minPower;
            power.reactorPower = avgPower;
        }
        
    }
    public void FocuseOnWeapons()
    {
        if (pActive)
        {
            power.enginePower = minPower;
            power.weaponPower = maxPower;
            power.sensorPower = minPower;
            power.reactorPower = avgPower;
        }
        
    }
    public void FocusOnSensor()
    {
        if (pActive)
        {
            power.enginePower = minPower;
            power.weaponPower = minPower;
            power.sensorPower = maxPower;
            power.reactorPower = avgPower;
        }
        
    }
    public void EndGame()
    {
        Time.timeScale = 0;
        gameOverPanal.SetActive(true);
    }
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene3");
    }
    public void BackGame()
    {
        SceneManager.LoadScene("StartScene");
    }
}
