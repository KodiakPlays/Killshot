using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using TMPro;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [SerializeField]
    private List<Button> btnPowerBool = new List<Button>();

    [SerializeField]
    private List<Image> imgPowerMet = new List<Image>();

    private List<UIPowerClass> uiPowerMetClass = new List<UIPowerClass>();

    public Dictionary<int, IEnumerator> powerAnimCoroutine = new Dictionary<int, IEnumerator>();

    [SerializeField]
    private Shader shaPowerMet;
    [SerializeField] private Shader shaReactorMet;
    private int reactorCharges = 0;

    [SerializeField]
    private List<Sprite> uiSprite = new List<Sprite>();

    [SerializeField]
    private List<Image> btnPowerBoolImage = new List<Image>();

    [SerializeField] TextMeshProUGUI frequancyTMP;
    [SerializeField] Transform frequancyTrans;
    private float frequancyFlt = 0f;

    [SerializeField] List<Transform> tunerTrans = new List<Transform>();

    [SerializeField] private Transform shipHit;
    public AnimationCurve shipHitCurve;
    private Coroutine shipHitCo;


    [SerializeField] private Transform sensorLoad;
    [SerializeField] private List<TextMeshProUGUI> sensorLoadInfo = new List<TextMeshProUGUI>();
    [SerializeField] private Image sensorIcon;
    [SerializeField] private List<Sprite> sensorLoadSprites = new List<Sprite>();

    [SerializeField] private RectTransform compassRect;
    [SerializeField] private TextMeshProUGUI speedometer;

    [Header("PlayerShip")]
    [SerializeField] private Transform shipPlayer;
    [SerializeField] private GameObject shipiconGO;

    [Header("Speedometer Settings")]
    [SerializeField] private bool autoUpdateSpeedometer = true;
    [SerializeField] private float speedometerUpdateRate = 0.1f; // Update every 0.1 seconds
    private PlayerShip playerShipRef;
    private Coroutine speedometerUpdateCoroutine;
    [SerializeField] private Shader velocityMeterSha;
    [SerializeField] private Image[] velocityMeterImg;
    [SerializeField] private TextMeshProUGUI[] velocityMeterTMP;

    [Header("Stability Settings")]
    [SerializeField] private Shader stabilityShader;
    [SerializeField] private Image stabilityImg;
    private float stabMax = 1;

    [Header("Scanner Hack")]
    [SerializeField] private Transform[] bogieTabTran;
    private int bogieTabInt = 1;
    [SerializeField] private Transform bogieTabBtn;
    [SerializeField] private Slider scannerSlider;
    private float scanValue = 0;
    private float placeValue = 0;
    [SerializeField] private int scanSpeed = 10;//higher the slower
    private bool rightScanner = true;
    [SerializeField] private Shader scannerSha;
    [SerializeField] private Image scannerImg;
    private float scanerMax = 1;
    private List<Vector2> scanerPipsV2 = new List<Vector2>();
    [SerializeField] private List<int> scanPips = new List<int>();
    [SerializeField] private string[] shipPartsString = { "_Port", "_Aft", "_Prow", "_Star" };
    private Color[] pipCol = { new Color(1f, 0.8086438f, 0f), new Color(1f, 0f, 0f), new Color(0f, 1f, 0f) };
    [SerializeField] private GameObject targetGO;
    [SerializeField] public List<BogieClass> bogieList = new List<BogieClass>();//also use with weapons
    [SerializeField] public GameObject currentBogieTarget;//also use with weapons
    [SerializeField] private MeshFilter bogieMesh;//also use with weapons

    [Header("Radar Screen")]
    [SerializeField] private Shader radarSha;
    [SerializeField] private GameObject radarGO;
    [SerializeField] private GameObject radarBogeyGO;
    [SerializeField] private Image radarImg;
    [SerializeField] private float radarDegreeTest;

    [Header("Weapon Screen")]
    [SerializeField] private Shader weaponScreenSha;
    [SerializeField] private Image weaponScreenImage;
    [SerializeField] private GameObject screenWepGO;
    private RectTransform screenWepRT;
    [SerializeField] private Transform shipAngleTarget;
    public float testFloat = 0f;
    private bool fire = false;
    [SerializeField] private RectTransform screenEnemyWeapon;
    private float screenDist;
    private float scaleSize;
    [SerializeField] private float testFloat2;

    [Header("World Grid")]
    [SerializeField] private Shader gridSha;
    [SerializeField] private Image gridImg;
    [SerializeField] private Camera[] sc;
    private int worldZoom;
    [SerializeField] private Texture[] viewTex;
    [SerializeField] private Canvas[] canvasUI;
    [SerializeField] private RawImage screenWorld;

    [Header("Power Screen")]
    [SerializeField] private Shader powerNodeSha;
    [SerializeField] private Image[] powerNodeImg;
    [SerializeField] private List<int> powerState = new List<int>();


    public AnimationCurve animationCurve;

    [Header("Comms UI")]
    [SerializeField] private GameObject signalInterceptPanel;
    [SerializeField] private Slider commsFrequencySlider;
    [SerializeField] private Image commsSignalStrengthIndicator;
    [SerializeField] private TextMeshProUGUI commsBandDisplay;
    [SerializeField] private TextMeshProUGUI commsFrequencyDisplay;
    [SerializeField] private Button commsLockButton;
    [SerializeField] private Image[] commsBandIndicators;
    [SerializeField] private TextMeshProUGUI commsLogText;

    [Header("Radar Blips")]
    [SerializeField] private RectTransform radarBlipContainer;
    [SerializeField] private GameObject radarBlipPrefab;
    [SerializeField] private float radarBlipRadius = 100f;
    private Dictionary<RadarTarget, RectTransform> radarBlips = new Dictionary<RadarTarget, RectTransform>();

    [Header("Weapon Display UI")]
    [SerializeField] private WeaponManager weaponManager;
    private int btnTabCur = 0;
    [SerializeField] private Transform btnTabTran;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI weaponTypeText;
    [SerializeField] private TextMeshProUGUI weaponAmmoText;
    [SerializeField] private TextMeshProUGUI weaponStatusText;
    [SerializeField] private Image weaponRechargeBar;
    [SerializeField] private Image weaponIconImage;
    [SerializeField] private Sprite laserIcon;
    [SerializeField] private Sprite macrocannonIcon;
    [SerializeField] private Sprite missileIcon;
    [SerializeField] private Sprite pointDefenseIcon;
    [SerializeField] private Sprite boardingPodIcon;
    [SerializeField] private Sprite railgunIcon;
    [SerializeField] private Sprite[] tabSprite;
    [SerializeField] private TextMeshProUGUI[] tabWepName;
    [SerializeField] private Image[] tabFrame;

    [Header("Screen Static")]
    [SerializeField] private Shader staticSha;
    [SerializeField] private Image[] staticImg;

    void Start()
    {
        // Auto-find player ship if not assigned in inspector
        if (shipPlayer == null)
        {
            GameObject player = this.gameObject;//GameObject.FindGameObjectWithTag("Player");
            if (player != null) shipPlayer = player.transform;
        }

        //FrequancyTune(360f);

        //NewBogie();//test

        StartPower();
        WorldGridStart();
        RadarStart();
        WorldGridZoom(0);

        // Find PlayerShip reference and start automatic speedometer updates
        if (autoUpdateSpeedometer)
        {
            StartSpeedometerUpdates();
        }

        NewScan();

        ScanTargetLoc(0);
        ScanTargetSize(0);
        BtnBogieTab();

        GlitchStart();

        if (weaponManager == null)
            weaponManager = FindFirstObjectByType<WeaponManager>();
    }

    void Update()
    {

        if (Input.GetKeyDown("space"))
        {
            ScanPlaceHack();
        }

        ScanMovment();

        //testing purposes of radar

        RadarTest();

        if (Input.GetKeyDown("/"))//test change ship
        {
            BogeySpot(radarDegreeTest);
        }

        if (Input.GetKeyDown("0"))//test change ship
        {
            LaserFireEnemy(0);
        }
        if (Input.GetKeyDown("1"))//test change ship
        {
            LaserFireEnemy(1);
        }
        if (Input.GetKeyDown("2"))//test change ship
        {
            LaserFireEnemy(2);
        }
        if (Input.GetKeyDown("3"))//test change ship
        {
            LaserFireEnemy(3);
        }
        if (Input.GetKeyDown("4"))//test change ship
        {
            LaserFireEnemy(4);
        }

        if (Input.GetKeyDown("p"))//test change ship
        {
            LaserFire();
        }

        if (Input.GetKeyDown("r"))//test change ship
        {
            RailFire();
        }

        if (Input.GetKeyDown("]"))//test change ship
        {
            ScanNewTarget();
        }

        UpdateVelocity();

    }

    private void RadarTest()
    {
        radarDegreeTest = radarDegreeTest - (Time.deltaTime * 10);

        if (radarDegreeTest <= 0)
        {
            radarDegreeTest = 360;
        }
        RadarUpdate(radarDegreeTest);
    }

    public void NewScan()
    {
        scanerPipsV2.Clear();
        scanPips.Clear();

        scannerImg.material = new Material(scannerSha);

        int check = 50;

        int startI = 0;

        scanerMax = 30f;//base this off the size of the object?

        scannerImg.material.SetFloat("_NibMax", scanerMax);

        for (int i = 0; i < 4; i++)
        {
            scannerImg.material.SetColor(shipPartsString[i] + "Color", pipCol[0]);
        }

        for (int i = 0; i < (int)scanerMax; i++)
        {
            scanPips.Add(5);//add empty pips
        }

        List<int> size = new List<int>();

        for (int i = 0; i < 4; i++)
        {
            size.Add(Random.Range(1, 5));

            scanerPipsV2.Add(new Vector2(0, 0));
        }

        for (int i = 0; i < scanerPipsV2.Count; i++)//iterate down the list of 0:port, 1:aft, 2:prow, 3, shaft
        {
            int checkPlace = 50;//how many chances to find a placement for the pips

            while (checkPlace > 0)//check placments so that its not an endless loop
            {
                bool placable = false;

                startI = Random.Range(0, (int)scanerMax); //finds a random spot and grow right from that spot

                if (startI + size[i] <= (int)scanerMax)//make sure the size fits within the pip peramiter
                {
                    for (int j = startI; j < startI + size[i]; j++) //loop to find empty pips
                    {
                        if (scanPips[j] == 5)
                        {
                            placable = true;

                        }
                        else if (scanPips[j] != 5)
                        {
                            placable = false;
                            break; //end for loop, find a new spot
                        }

                    }
                }

                if (placable)//these spots are empty, place pips type and color and move onto the next ship side.
                {
                    for (int j = startI; j < startI + size[i]; j++)
                    {
                        scanPips[j] = i;
                    }

                    scanerPipsV2[i] = new Vector2(startI, startI + size[i]);
                    scannerImg.material.SetVector(shipPartsString[i] + "V2", scanerPipsV2[i]);
                    break;
                }
                else if (!placable)
                {
                    --checkPlace;//if for some reason no pips can be place, this prevents an endless loop. try to make sure this is never used.              
                }
            }
        }

    }

    public void ScanPlaceHack()
    {
        placeValue = scannerSlider.value;

        for (int i = 0; i < scanerPipsV2.Count; i++)
        {
            if (placeValue >= scanerPipsV2[i].x && placeValue <= scanerPipsV2[i].y)
            {
                //show relavent ship info

                scannerImg.material.SetColor(shipPartsString[i]+"Color", pipCol[1]);

            }
        }
    }

    public void ScanMovment()
    {
        if(rightScanner)
        {
            scannerSlider.value = ++scanValue / scanSpeed;
        }
        else if(!rightScanner)
        {
            scannerSlider.value = --scanValue / scanSpeed;
        }

        if(scannerSlider.value >= scanerMax)
        {
            rightScanner = false;
        }
        else if (scannerSlider.value <= 0)
        {
            rightScanner = true;
        }
    }

    public void ScanNewTarget()
    {
        StartCoroutine(GlitchEffect(0f, .25f, 2));

        int current = 0;
        //send all data to scan screen

        if (bogieList.Count == 0)
        {
            currentBogieTarget = null;

            ScanTargetLoc(0);

            return;
        }else if (bogieList.Count > 0)
        {
            for (int j = 0; j < bogieList.Count - 1; j++) //loop to find empty pips
            {
                if (currentBogieTarget == bogieList[bogieList.Count - 1].go)
                {
                    currentBogieTarget = bogieList[0].go;
                    ScanTargetLoc(1);

                    if (worldZoom == 0)
                    {
                        ScanTargetSize(1);
                    }
                    else if (worldZoom == 1)
                    {
                        ScanTargetSize(2);
                    }
                    else if (worldZoom == 2)
                    {
                        ScanTargetSize(3);
                    }

                    break;

                }
                else if (bogieList[j].go == currentBogieTarget) //need a mores spacific identifier then the game object, a dictionary or something
                {
                    currentBogieTarget = bogieList[j + 1].go;
                    ScanTargetLoc(1);

                    if (worldZoom == 0)
                    {
                        ScanTargetSize(1);
                    }
                    else if (worldZoom == 1)
                    {
                        ScanTargetSize(2);
                    }
                    else if (worldZoom == 2)
                    {
                        ScanTargetSize(3);
                    }

                    break;
                }
            }
        }

        ScanMesh(currentBogieTarget);
        NewScan();//remove later
    }//needs improvment

    private void ScanTargetSize(int i)
    {
        if (i == 0)
        {
            targetGO.transform.localScale = Vector3.zero;
        }
        else if (i == 1)
        {
            targetGO.transform.localScale = new Vector3(1f,1f,1f);
        }
        else if (i == 2)
        {
            targetGO.transform.localScale = new Vector3(4f, 4f, 4f);
        }
        else if (i == 3)
        {
            targetGO.transform.localScale = new Vector3(50f, 50f, 50f);
        }

        targetGO.transform.localPosition = Vector3.zero;
    }

    private void ScanTargetLoc(int i)
    {
        if (i == 0)//player
        {
            targetGO.transform.parent = shipPlayer;
        }
        if (i == 1)
        {
            targetGO.transform.parent = currentBogieTarget.transform;
        }
    }

    private void ScanMesh(GameObject go)
    {
        MeshFilter m = go.GetComponent<MeshFilter>();
        bogieMesh.mesh = m.mesh;
    }

    private void StartPower()
    {
        for (int i = 0; i < btnPowerBool.Count-1; i++)
        {
            uiPowerMetClass.Add(new UIPowerClass(new Material(shaPowerMet), 6, 1, 0f, false));

            imgPowerMet[i].material = uiPowerMetClass[i].mat;
        }

        uiPowerMetClass.Add(new UIPowerClass(new Material(shaReactorMet), 15, 15, 0f, false));

        imgPowerMet[btnPowerBool.Count-1].material = uiPowerMetClass[btnPowerBool.Count-1].mat;

        for (int i = 0; i < powerNodeImg.Length; i++)//power nodes
        {
            powerNodeImg[i].material = new Material(powerNodeSha);

            powerNodeImg[i].material.SetInt("_On", 0);
        }

        for (int i = 0; i < btnPowerBool.Count - 1; i++)//power nodes
        {
            powerAnimCoroutine[i] = null;
        }
    }

    private void PowerNodes(int i, int state)
    {

        if (state == 0) //turn off node
        {
            powerNodeImg[i].material.SetInt("_On", 0);
            powerNodeImg[i].material.SetInt("_Charge", 0);
            powerNodeImg[i].material.SetInt("_End", 0);
            //powerNodeImg[i].material.SetInt("_Charge", 0);

        }
        else if(state > 0)
        {
            powerNodeImg[i].material.SetInt("_On", 1);
        }

        if (state == 1) //move left
        {
            powerNodeImg[i].material.SetInt("_Charge", 0);
            powerNodeImg[i].material.SetFloat("_DegreeHor", 0);
            powerNodeImg[i].material.SetInt("_End", 0);

        }
        else if (state == 2) //move left and charge Up
        {
            powerNodeImg[i].material.SetInt("_Charge", 1);
            powerNodeImg[i].material.SetFloat("_DegreeVert", 270);
            powerNodeImg[i].material.SetFloat("_DegreeHor", 0);

            powerNodeImg[i].material.SetInt("_End", 0);
        }
        else if (state == 3) //move left and charge Up, end of chain
        {
            powerNodeImg[i].material.SetInt("_Charge", 1);
            powerNodeImg[i].material.SetFloat("_DegreeVert", 270);
            powerNodeImg[i].material.SetFloat("_DegreeHor", 0);

            powerNodeImg[i].material.SetInt("_End", 1);
        }

        if (state == 4) //move right
        {
            powerNodeImg[i].material.SetInt("_Charge", 0);
            powerNodeImg[i].material.SetFloat("_DegreeHor", 180);
            powerNodeImg[i].material.SetInt("_End", 0);
        }
        else if (state == 5) //move left and charge Down
        {
            powerNodeImg[i].material.SetInt("_Charge", 1);
            powerNodeImg[i].material.SetFloat("_DegreeVert", 90);
            powerNodeImg[i].material.SetFloat("_DegreeHor", 180);

            powerNodeImg[i].material.SetInt("_End", 0);
        }
        else if (state == 6) //move left and charge Down, end of chain
        {
            powerNodeImg[i].material.SetInt("_Charge", 1);
            powerNodeImg[i].material.SetFloat("_DegreeVert", 90);
            powerNodeImg[i].material.SetFloat("_DegreeHor", 180);

            powerNodeImg[i].material.SetInt("_End", 1);
        }

        else if (state == 7) //reactor left
        {
            powerNodeImg[i].material.SetInt("_Charge", 1);
            powerNodeImg[i].material.SetFloat("_DegreeVert", 90);
            powerNodeImg[i].material.SetFloat("_DegreeHor", 0);

            powerNodeImg[i].material.SetInt("_Reactor", 1);
        }

        else if (state == 8) //reactor right
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
            StartCoroutine(GlitchEffect(0f, .75f, 4));
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

        for (int j = 0; j < uiPowerMetClass.Count - 1; j++)//check each nod for the end of the line
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

        if (noCharge == uiPowerMetClass.Count - 1)//if there are no charges end the function
        {
            PowerNodes(5, 0);
            return;
        }

        for (int j = lastCharge; j < uiPowerMetClass.Count - 1; j++)//add charge nodes where approreate
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

        for (int j = 0; j < uiPowerMetClass.Count - 1; j++)//check each nod for the end of the line
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

        if (noVent == uiPowerMetClass.Count - 1)//if there are no charges end the function
        {
            PowerNodes(5, 0);
            return;
        }

        for (int j = lastVent; j < uiPowerMetClass.Count - 1; j++)//add charge nodes where approreate
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
        //Debug.Log("i: " + i);

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
        StartCoroutine(GlitchEffect(0f, .75f, 4));

        for (int i = 0; i < uiPowerMetClass.Count - 1; i++)
        {
            if(powerAnimCoroutine[i] == null)
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

    public void FrequancyTune(float speed)
    {
        frequancyFlt = frequancyFlt + speed; //(360 * speed);

        if (frequancyFlt >= 360 * 5)
        {
            frequancyFlt = 360 * 5;
        }
        else if (frequancyFlt <= 0)
        {
            frequancyFlt = 0;
        }

        frequancyTrans.rotation = Quaternion.Euler(0f, 0f, frequancyFlt * -1);

        frequancyTMP.text = frequancyFlt.ToString();

        float normF = tunerTrans[1].localPosition.x / tunerTrans[2].localPosition.x;

        tunerTrans[0].localPosition = Vector2.Lerp(tunerTrans[1].localPosition, tunerTrans[2].localPosition, frequancyFlt/(360*5));


    }//not in game currently

    public void updateShipHit(float duration)
    {
        shipHitCo = StartCoroutine(ShipShake(duration));
    }

    private IEnumerator ShipShake(float duration)
    {
        Vector3 startPos = new Vector3(0f, 0f, 0f);
        float elapsedTime = 0f;

        float glitchO = 0f;
        float glitchT = 0f;



        for (int i = 0; i < staticImg.Length; i++)
        {
            glitchO = Random.Range(.25f, 1f);

            glitchT = Random.Range(duration/4f, duration/2f);

            StartCoroutine(GlitchEffect(glitchT, glitchO, i));
        }
        

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float strength = shipHitCurve.Evaluate(elapsedTime / duration);
            shipHit.localPosition = startPos + Random.insideUnitSphere * strength;
            yield return null;
        }

        shipHit.localPosition = startPos;
    }

    public void UpdateCompass(bool loaded, int icon, float port, float aft, float prow, float starboard)
    {

        if (loaded)
        {
            sensorIcon.sprite = sensorLoadSprites[icon];
            sensorLoadInfo[0].text = port.ToString() + "/ 100";
            sensorLoadInfo[1].text = aft.ToString() + "/ 100";
            sensorLoadInfo[2].text = prow.ToString() + "/ 100";
            sensorLoadInfo[3].text = starboard.ToString() + "/ 100";

            sensorLoad.localPosition = new Vector2(0,0);
        }
        else if (!loaded)
        {
            sensorIcon.sprite = null;
            sensorLoadInfo[0].text = null;
            sensorLoadInfo[1].text = null;
            sensorLoadInfo[2].text = null;
            sensorLoadInfo[3].text = null;

            sensorLoad.localPosition = new Vector2(2000, 0);
        }


    }

    public void UpdateCompass(float angle)
    {
        compassRect.localRotation = Quaternion.Euler(0, 0, angle);
    }

    public void UpdateSpeedometer(float speed)
    {
        //speedometer.text = speed.ToString("F1");
    }
    
    private void StartSpeedometerUpdates()
    {
        // Find PlayerShip in scene
        playerShipRef = FindFirstObjectByType<PlayerShip>();
        
        if (playerShipRef != null)
        {
            speedometerUpdateCoroutine = StartCoroutine(UpdateSpeedometerContinuously());
        }
        else
        {
            Debug.LogWarning("UIController: PlayerShip not found in scene. Speedometer will not auto-update.");
        }

        velocityMeterImg[0].material = new Material(velocityMeterSha);//player
        velocityMeterImg[1].material = new Material(velocityMeterSha);//bogie

        velocityMeterImg[0].material.SetFloat("_PowerMax", playerShipRef.turnRate * 2);//set max speed


    }
    
    private IEnumerator UpdateSpeedometerContinuously()
    {
        while (playerShipRef != null && autoUpdateSpeedometer)
        {
            UpdateSpeedometerFromPlayerShip();
            yield return new WaitForSeconds(speedometerUpdateRate);
        }
    }

    public void UpdateVelocity()
    {
        velocityMeterImg[0].material.SetFloat("_PowerCur", Mathf.RoundToInt(playerShipRef.GetCurrentSpeed()));

        velocityMeterTMP[0].text = Mathf.RoundToInt(playerShipRef.GetCurrentSpeed()).ToString();
    }
    
    private void UpdateSpeedometerFromPlayerShip()
    {
        if (playerShipRef != null)
        {
            Rigidbody shipRigidbody = playerShipRef.GetComponent<Rigidbody>();
            if (shipRigidbody != null)
            {
                // Get velocity magnitude
                float speed = shipRigidbody.linearVelocity.magnitude;
                
                // Optional: Determine if moving forward or backward
                Vector3 forwardDirection = playerShipRef.transform.forward;
                Vector3 velocityDirection = shipRigidbody.linearVelocity.normalized;
                float forwardDot = Vector3.Dot(forwardDirection, velocityDirection);
                
                // Apply negative sign if moving backward
                if (forwardDot < -0.1f)
                {
                    speed = -speed;
                }
                
                UpdateSpeedometer(speed);
            }
        }
    }
    
    // Manual method to force speedometer update (for external calls)
    public void ForceSpeedometerUpdate()
    {
        if (playerShipRef != null)
        {
            UpdateSpeedometerFromPlayerShip();
        }
    }
    
    // Method to enable/disable automatic updates
    public void SetAutoUpdateSpeedometer(bool enabled)
    {
        autoUpdateSpeedometer = enabled;
        
        if (enabled && speedometerUpdateCoroutine == null)
        {
            StartSpeedometerUpdates();
        }
        else if (!enabled && speedometerUpdateCoroutine != null)
        {
            StopCoroutine(speedometerUpdateCoroutine);
            speedometerUpdateCoroutine = null;
        }
    }

    public void StabilityMeterStart(float cur, float max)
    {
        stabilityImg.material = new Material(stabilityShader);
        stabilityImg.material.SetColor("_OnColor", new Color(1f, 1f, 1f));

        stabilityImg.material.SetFloat("_PowerCur", cur);
        stabilityImg.material.SetFloat("_PowerMax", max);

        stabMax = max;

    }

    public void StabilityMeterUpdate(float cur)
    {
        stabilityImg.material.SetFloat("_PowerCur", cur);

        StabilityMeterColor(cur);
    }

    public void StabilityMeterColor(float cur)
    {
        Color[] col = { new Color(1f, 1f, 1f), new Color(0.8117647f, 0.6f, 0f), new Color(1f, 0f, 0f) };

        if(cur >= stabMax *.25)
        {
            stabilityImg.material.SetColor("_OnColor", col[0]);
        }
        else if (cur < stabMax * .25 && cur >= stabMax * .1)
        {
            stabilityImg.material.SetColor("_OnColor", col[1]);
        }
        else if (cur < stabMax * .1)
        {
            stabilityImg.material.SetColor("_OnColor", col[2]);
        }
    }

    private void RadarStart()
    {
        radarImg = radarGO.GetComponent<Image>();
        radarImg.material = new Material(radarSha);
        radarImg.material.SetVector("_LineSizeV2", new Vector2(0.499f, 0.501f));
        radarImg.material.SetInt("_Bogey", 0);
        radarImg.material.SetInt("_Show", 0);

    }

    public void RadarRange(float r)
    {
        radarImg.material.SetFloat("_RadarRange", r);
    }

    public void RadarUpdate(float d)
    {
        radarImg.material.SetFloat("_RangeDegree", d);
    }

    public void BogeySpot(float d) // call this when radar spots a bogey
    {
        StartCoroutine(BogeyStart(d));
    }

    //public void NewBogie()//test for now, should be used to add bogies to list once they are in range for scaning
    //{
    //    GameObject go = null;
    //    GameObject goImg = null;

    //    for (int j = 0; j < 5; j++)
    //    {
    //        go = GameObject.Find("Bogie_" + j.ToString());

    //        //bogieList.Add(new BogieClass(go, go.GetComponent<MeshFilter>().mesh, null, new Material(weaponScreenSha)));

    //        goImg = Instantiate(new GameObject(), new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);

    //        goImg.transform.parent = screenEnemyWeapon;

    //        bogieList[j].wepImage = goImg.AddComponent<Image>();

    //        bogieList[j].wepImage.material = bogieList[j].matWep;

    //        goImg.transform.localScale = new Vector3(1f, 1f, 1f);
    //        goImg.transform.localPosition = new Vector3(0f, 0f, 0f);

    //        goImg.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
    //        goImg.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);

    //        goImg.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);//new Vector2(screenEnemyWeapon.GetComponent<RectTransform>().offsetMin.x, screenEnemyWeapon.GetComponent<RectTransform>().offsetMin.y);
    //        goImg.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);//new Vector2(-screenEnemyWeapon.GetComponent<RectTransform>().offsetMax.x, -screenEnemyWeapon.GetComponent<RectTransform>().offsetMax.y);


    //        bogieList[j].WeapStart();
    //    }
    //}

    public void AddBogie(BogieClass bc)
    {
        bogieList.Add(bc);

        int j = 0;

        if (bogieList.Count != 0)
        {
            j = bogieList.Count - 1;
        }
        else if (bogieList.Count == 0)
        {
            j = 0;
        }

        if (bogieList[j].wepImageGo = null)
        {
            bogieList[j].wepImageGo = Instantiate(new GameObject(), new Vector3(0f, 0f, 0f), Quaternion.identity);
        }

        bogieList[j].wepImageGo.transform.parent = screenEnemyWeapon;

        bogieList[j].wepImageGo.AddComponent<Image>().material = bogieList[j].matWep;

        //bogieList[j].wepImageGo.image.material = bogieList[j].matWep;

        bogieList[j].wepImageGo.transform.localScale = new Vector3(1f, 1f, 1f);
        bogieList[j].wepImageGo.transform.localPosition = new Vector3(0f, 0f, 0f);

        bogieList[j].wepImageGo.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        bogieList[j].wepImageGo.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);

        bogieList[j].wepImageGo.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        bogieList[j].wepImageGo.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);

        bogieList[j].WeapStart();
    }

    public void RemoveBogie(BogieClass bc)
    {
        int j = 0;

        for (int i = 0; i < bogieList.Count; i++){if (bogieList[i] == bc) {j = i;}}

        Destroy(bogieList[j].wepImageGo);

        bogieList[j].wepImageGo = null;

        bogieList.RemoveAt(j);
    }

    private IEnumerator BogeyStart(float d)
    {
        float t = 3f;

        float speed = 1f;

        GameObject go = Instantiate(radarGO, radarGO.transform.position, Quaternion.identity);

        go.transform.parent = radarBogeyGO.transform;
        go.transform.localScale = new Vector3(1f, 1f, 1f);

        RectTransform rt = go.GetComponent<RectTransform>();

        rt.offsetMin = new Vector2(0, 0);
        rt.offsetMax = new Vector2(0, 0);

        Image img = go.GetComponent<Image>();
        img.material = new Material(radarSha);
        img.material.SetFloat("_RangeDegree", d);
        img.material.SetVector("_LineSizeV2", new Vector2(0.495f, 0.505f));
        img.material.SetColor("_RadarColor", new Color(1f, 0f, 0f));
        img.material.SetInt("_Bogey", 1);

        while (t > 0f)
        {
            t -= Time.deltaTime * speed;

            img.material.SetColor("_RadarColor", new Color(t, 0f, 0f));

            //img.material.SetFloat("_RangeDegree", d);

            yield return null;
        }

        Destroy(go);
    }

    private void WorldGridStart()
    {

        screenWepRT = screenWepGO.GetComponent<RectTransform>();
        gridImg.material = new Material(gridSha);

        gridImg.material.SetVector("_ShipLocV2", new Vector2(0f,0f));
        gridImg.material.SetFloat("_ShipRotation", 0f);

        weaponScreenImage.material = new Material(weaponScreenSha);
        weaponScreenImage.material.SetInt("_LaserFire", 0);

        screenWorld.texture = viewTex[0];
        canvasUI[0].worldCamera = sc[0];
    }

    public void WorldGridLocUpdate(Vector2 shipV2)
    {
        if (worldZoom == 0)
        {
            gridImg.material.SetVector("_ShipLocV2", (shipV2 / 100f));

            if(currentBogieTarget != null)
            {
                ScanTargetLoc(1);
                ScanTargetSize(1);
            }

            shipPlayer.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = true;
            shipiconGO.SetActive(false);
        }
        else if (worldZoom == 1)
        {
            gridImg.material.SetVector("_ShipLocV2", (shipV2 / 1000f));

            if (currentBogieTarget != null)
            {
                ScanTargetLoc(2);
                ScanTargetSize(2);
            }
        }
        else if (worldZoom == 2)
        {
            if (currentBogieTarget != null)
            {
                ScanTargetLoc(3);
                ScanTargetSize(3);
            }
        }


    }

    public void WorldGridRotUpdate(float r)
    {
        if (worldZoom < 2)
        {
            gridImg.material.SetFloat("_ShipRotation", r);
        }
    }

    public void WorldGridZoom(int i)
    {
        StartCoroutine(GlitchEffect(.5f, 1f, 0));

        if (i == 0)//spectral zoom 10x
        {
            screenWorld.texture = viewTex[0];

            gridImg.material.SetInt("_WorldView", 0);
            gridImg.material.SetVector("_CellSize", new Vector2(1,1));
            gridImg.material.SetFloat("_GridAmmount", 5f);
            gridImg.material.SetFloat("_GridThickness", .02f);
            gridImg.material.SetFloat("_ShipRotation", shipPlayer.localRotation.z);

            radarImg.material.SetFloat("_RadarRange", 1f);
            radarImg.material.SetInt("_Show", 1);
            weaponScreenImage.material.SetInt("_RangeVisOn", 1);

            scaleSize = 1f;

            canvasUI[1].worldCamera = sc[6];
            sc[6].orthographicSize = 50f;

            radarImg.material.SetFloat("_VisualRange", 1f);

            for (int j = 0; j < bogieList.Count; j++)
            {
                bogieList[j].go.GetComponent<MeshRenderer>().enabled = true;
                bogieList[j].go.transform.GetChild(0).gameObject.SetActive(false);
            }

            worldZoom = 0;
            screenDist = 50;
        }
        else if (i == 1)//spectral zoom 100x
        {
            screenWorld.texture = viewTex[0];

            gridImg.material.SetInt("_WorldView", 0);
            gridImg.material.SetVector("_CellSize", new Vector2(1, 1));
            gridImg.material.SetFloat("_GridAmmount", 20f);
            gridImg.material.SetFloat("_GridThickness", .04f);
            gridImg.material.SetFloat("_ShipRotation", shipPlayer.localRotation.z);

            radarImg.material.SetFloat("_RadarRange", 1f);
            radarImg.material.SetInt("_Show", 1);
            weaponScreenImage.material.SetInt("_RangeVisOn", 0);
            scaleSize = .5f;

            //screenWepGO.transform.localPosition = new Vector2(10000f, 0f);
            canvasUI[1].worldCamera = sc[6];
            sc[6].orthographicSize = 500f;

            radarImg.material.SetFloat("_VisualRange", .1f);

            for (int j = 0; j < bogieList.Count; j++)
            {
                bogieList[j].go.GetComponent<MeshRenderer>().enabled = false;
                bogieList[j].go.transform.GetChild(0).gameObject.SetActive(true);
                bogieList[j].go.transform.GetChild(0).gameObject.transform.localScale = new Vector3(2f, 2f, 2f);

            }

            shipiconGO.SetActive(true);
            shipiconGO.transform.localScale = new Vector3(2f, 2f, 2f);
            shipPlayer.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = false;

            worldZoom = 1;
            screenDist = 500;

        }
        else if (i == 2)//spectral zoom 1000x
        {
            screenWorld.texture = viewTex[5];

            gridImg.material.SetInt("_WorldView", 1);
            gridImg.material.SetVector("_CellSize", new Vector2(2, 2));
            gridImg.material.SetFloat("_GridAmmount", 20f);
            gridImg.material.SetFloat("_GridThickness", .025f);
            gridImg.material.SetFloat("_ShipRotation", 0);

            radarImg.material.SetFloat("_RadarRange", .1f);
            radarImg.material.SetInt("_Show", 0);
            weaponScreenImage.material.SetInt("_RangeVisOn", 0);
            scaleSize = .25f;

            //screenWepGO.transform.localPosition = new Vector2(10000f, 0f);
            canvasUI[1].worldCamera = sc[5];
            sc[5].orthographicSize = 5000f;

            radarImg.material.SetFloat("_VisualRange", .01f);

            for (int j = 0; j < bogieList.Count; j++)
            {
                bogieList[j].go.GetComponent<MeshRenderer>().enabled = false;
                bogieList[j].go.transform.GetChild(0).gameObject.SetActive(true);
                bogieList[j].go.transform.GetChild(0).gameObject.transform.localScale = new Vector3(20f, 20f, 20f);

            }

            shipiconGO.SetActive(true);
            shipiconGO.transform.localScale = new Vector3(20f, 20f, 20f);
            shipPlayer.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = false;

            worldZoom = 2;
            screenDist = 5000;
        }

    }

    public void RailFire()
    {
        StartCoroutine(RailFireCo());
    }

    private IEnumerator RailFireCo()
    {
        float t = 0f;
        float y = 1f;
        float laserWidth = 0.498f;
        float speed = 10f;
        float curWidth = 0f;
        float sign = 1f;
        float offset = 0;
        float zeroDegree = 0;
        string side = "";
        float angle = 0f;
        float distance = 0f;
        float railStart = .001f;
        float railEnd = .01f;
        float start = 0f;

        Vector3 playerShipTranLoc = Vector3.zero;

        if (worldZoom < 2)
        {
            angle = 90f;
            playerShipTranLoc = Vector3.zero;
            //bogieTranLoc = shipPlayer.InverseTransformPoint(bogieTran.position);
            start = 0f;
        }
        else if (worldZoom == 2)
        {
            angle = shipPlayer.eulerAngles.z + 90;
            playerShipTranLoc = new Vector3(shipPlayer.position.x / (screenDist * 2), shipPlayer.position.y / (screenDist * 2), 0f);
            //bogieTranLoc = new Vector3(bogieTran.position.x / (screenDist * 2), bogieTran.position.y / (screenDist * 2), 0f);
            start = 0f;
        }

        weaponScreenImage.material.SetFloat("_LaserDegree", angle);
        weaponScreenImage.material.SetVector("_GunLoc", playerShipTranLoc);

        weaponScreenImage.material.SetFloat("_LaserStart", 0f);
        weaponScreenImage.material.SetFloat("_LaserEnd", 2f);//end at first game object to come in contact with
        weaponScreenImage.material.SetFloat("_LaserSize", railStart);
        weaponScreenImage.material.SetInt("_LaserFire", 1);

        while (t < 1f)
        {
            curWidth = Mathf.Lerp(0f * scaleSize, .002f * scaleSize, animationCurve.Evaluate(t));

            weaponScreenImage.material.SetFloat("_LaserSize", curWidth);

            t += Time.deltaTime;

            yield return null;
        }

        t = 0;
        updateShipHit(.5f);


        while (t < 1f)
        {
            curWidth = Mathf.Lerp(.002f * scaleSize, .01f * scaleSize, animationCurve.Evaluate(t));

            weaponScreenImage.material.SetFloat("_LaserSize", curWidth);

            t += Time.deltaTime * speed;

            yield return null;
        }

        t = 0;

        while (t < 1f)
        {
            curWidth = Mathf.Lerp(.01f * scaleSize, 0 * scaleSize, animationCurve.Evaluate(t));

            weaponScreenImage.material.SetFloat("_LaserStart", t);

            weaponScreenImage.material.SetFloat("_LaserSize", y * curWidth);

            y -= Time.deltaTime * speed;
            t += Time.deltaTime * speed;

            yield return null;
        }

        weaponScreenImage.material.SetFloat("_LaserSize", 0f);
        weaponScreenImage.material.SetInt("_LaserFire", 0);
    }

    public void LaserFire()
    {
        if (!fire)
        {
            StartCoroutine(LaserFireCo(currentBogieTarget, -1));
        }
    }

    public void LaserFireEnemy(int i)
    {
        StartCoroutine(LaserFireCo(bogieList[i].go, i));
    }

    private IEnumerator LaserFireCo(GameObject bogie, int s)//s = -1 in player
    {
        float t = 0f;
        float laserWidth = 0.498f;
        float speed = 5f;//10
        float curWidth = 0f;
        float sign = 1f;
        float offset = 0;
        float zeroDegree = 0;
        string side = "";
        float angle = 0f;
        float distance = 0f;

        float start = 0f;

        Vector3 playerShipTranLoc = Vector3.zero;
        Vector3 bogieTranLoc = Vector3.zero;

        Transform screenTran = radarGO.GetComponent<Transform>();
        Transform bogieTran = bogie.GetComponent<Transform>();

        if (worldZoom < 2)
        {
            playerShipTranLoc = Vector3.zero;
            bogieTranLoc = shipPlayer.InverseTransformPoint(bogieTran.position);
            start = 0f;
        }
        else if (worldZoom == 2)
        {
            playerShipTranLoc = new Vector3(shipPlayer.position.x / (screenDist*2), shipPlayer.position.y / (screenDist*2), 0f);
            bogieTranLoc = new Vector3(bogieTran.position.x / (screenDist * 2), bogieTran.position.y / (screenDist * 2), 0f);
            start = 0f;
        }

        if (s < 0)
        {
            weaponScreenImage.material.SetVector("_GunLoc", playerShipTranLoc);
        }
        else if (s >= 0)
        {
            bogieList[s].matWep.SetVector("_GunLoc", playerShipTranLoc);
        }

        Vector3 direction = bogieTranLoc - playerShipTranLoc;

        sign = (direction.y >= 0) ? 1 : -1;
        offset = (sign >= 0) ? 0 : 360;

        zeroDegree = (shipPlayer.position.y / 100f) + .001f;

        if (zeroDegree < 0)
        {
            zeroDegree = zeroDegree * -1f;
        }

        Vector3 vRight = new Vector3(playerShipTranLoc.x + zeroDegree, playerShipTranLoc.y, 0f);

        angle = (Vector2.Angle(vRight, direction) * sign + offset); //shipAngleTarget.position
        distance = Vector2.Distance(playerShipTranLoc, bogieTranLoc);

        float scaleDistance = 0f;

        if (worldZoom < 2){ scaleDistance = (distance / screenDist);}
        else if (worldZoom == 2) { scaleDistance = distance * 2f; }

        if (s < 0)
        {
            fire = true;
            yield return new WaitForSeconds(.1f);

            weaponScreenImage.material.SetFloat("_LaserDegree", angle);
            weaponScreenImage.material.SetInt("_LaserFire", 1);
            weaponScreenImage.material.SetFloat("_LaserSize", .0015f);
            weaponScreenImage.material.SetFloat("_LaserStart", start);
            weaponScreenImage.material.SetFloat("_LaserEnd", scaleDistance);

            t = 0;
            while (t < 1f)
            {
                weaponScreenImage.material.SetFloat("_LaserStart", t);

                weaponScreenImage.material.SetFloat("_LaserSize", t * .0015f);

                t += Time.deltaTime * speed;

                yield return null;
            }

            weaponScreenImage.material.SetInt("_LaserFire", 0);
            weaponScreenImage.material.SetFloat("_LaserStart", 0f);

            fire = false;
        }
        else if (s>=0)
        {
            bogieList[s].matWep.SetFloat("_LaserDegree", angle);
            bogieList[s].matWep.SetInt("_LaserFire", 1);
            bogieList[s].matWep.SetFloat("_LaserSize", .0015f);

            bogieList[s].matWep.SetFloat("_LaserStart", 0f);
            bogieList[s].matWep.SetFloat("_LaserEnd", scaleDistance);

            if (scaleDistance > 1)
            {
                t = 1;
            }
            else if (scaleDistance <= 1)
            {
                t = scaleDistance;
            }

                updateShipHit(t);

            while (t > 0f)
            {
                bogieList[s].matWep.SetFloat("_LaserEnd", t);

                bogieList[s].matWep.SetFloat("_LaserSize", t * .0015f);

                t -= Time.deltaTime * speed;

                yield return null;
            }

            bogieList[s].matWep.SetInt("_LaserFire", 0);
            bogieList[s].matWep.SetFloat("_LaserStart", 0f);
        }
    }

    private void OnDestroy()
    {
        // Clean up coroutine when object is destroyed
        if (speedometerUpdateCoroutine != null)
        {
            StopCoroutine(speedometerUpdateCoroutine);
        }
    }

    public void StabilityMeter(float i)
    {
        weaponScreenImage.material.SetFloat("_Stability", i);
    }

    public void BtnBogieTab()
    {
        StartCoroutine(GlitchEffect(.25f, 1f, 2));

        if (bogieTabInt == 0)
        {
            bogieTabTran[0].localPosition = new Vector3(1000000f, 0f, 0f);
            bogieTabInt = 1;
        }
        else if (bogieTabInt == 1)
        {
            bogieTabTran[1].localPosition = new Vector3(1000000f, 0f, 0f);
            bogieTabInt = 0;
        }

        BtnTunerTab(bogieTabBtn, bogieTabInt, 2);
        RectTransform rt = bogieTabTran[bogieTabInt].GetComponent<RectTransform>();

        bogieTabTran[bogieTabInt].localPosition = new Vector3(0f, 0f, 0f);

        rt.offsetMin = new Vector2(0f, 0f);
        rt.offsetMax = new Vector2(0f, 0f);


        //for (int j = 0; j < bogieTabTran.Length; j++)
        //{
        //    if (j != i)
        //    {
        //        bogieTabTran[j].localPosition = new Vector3(1000000f, 0f, 0f);
        //    }
        //    else if (j == i)
        //    {
        //        RectTransform rt = bogieTabTran[j].GetComponent<RectTransform>();

        //        bogieTabTran[j].localPosition = new Vector3(0f, 0f, 0f);

        //        rt.offsetMin = new Vector2(0f, 0f);
        //        rt.offsetMax = new Vector2(0f, 0f);
        //    }
        //}
    }

    public void BtnScannerCloke()
    {
        //cloke the ship
    }

    public void BtnRepair()
    {
        //repair player ship
    }

    public void SignalGhost()
    {
        //create a signal elseware
    }

    // ===== Comms UI Methods =====

    public void ShowCommsPanel(bool show)
    {
        if (signalInterceptPanel != null)
            signalInterceptPanel.SetActive(show);
    }

    public void SetCommsBand(int band)
    {
        if (commsBandDisplay != null)
            commsBandDisplay.text = $"Band: {band}";

        if (commsBandIndicators != null)
        {
            for (int i = 0; i < commsBandIndicators.Length; i++)
                commsBandIndicators[i].color = (i + 1 == band) ? Color.green : Color.gray;
        }
    }

    public void SetCommsFrequency(float freq)
    {
        if (commsFrequencySlider != null)
            commsFrequencySlider.value = freq;
        if (commsFrequencyDisplay != null)
            commsFrequencyDisplay.text = $"Frequency: {freq:F1}";
    }

    public void SetCommsSignalStrength(Color color)
    {
        if (commsSignalStrengthIndicator != null)
            commsSignalStrengthIndicator.color = color;
    }

    public void AddCommsLog(string message)
    {
        if (commsLogText != null)
            commsLogText.text = message;
    }

    public void SetupCommsListeners(UnityEngine.Events.UnityAction<float> onFreqChanged, UnityEngine.Events.UnityAction onLock)
    {
        if (commsFrequencySlider != null)
        {
            commsFrequencySlider.minValue = 1.0f;
            commsFrequencySlider.maxValue = 99.9f;
            commsFrequencySlider.onValueChanged.AddListener(onFreqChanged);
        }
        if (commsLockButton != null)
            commsLockButton.onClick.AddListener(onLock);
    }

    // ===== Radar Blip Methods =====

    public float RadarBlipRadius => radarBlipRadius;

    public RectTransform CreateOrGetRadarBlip(RadarTarget target)
    {
        if (radarBlips.TryGetValue(target, out RectTransform existing))
            return existing;

        if (radarBlipPrefab == null || radarBlipContainer == null) return null;

        GameObject blipObj = Instantiate(radarBlipPrefab, radarBlipContainer);
        RectTransform blipRect = blipObj.GetComponent<RectTransform>();

        Image img = blipObj.GetComponent<Image>();
        if (img != null)
        {
            if (target.icon != null) img.sprite = target.icon;
            img.color = target.color;
        }

        radarBlips[target] = blipRect;
        return blipRect;
    }

    public void DestroyRadarBlip(RadarTarget target)
    {
        if (radarBlips.TryGetValue(target, out RectTransform rt))
        {
            if (rt != null) Destroy(rt.gameObject);
            radarBlips.Remove(target);
        }
    }

    // ===== Weapon Display Methods =====

    public void BtnWepTab()
    {
        //int tabMax = 3;

        StartCoroutine(GlitchEffect(.25f, 1f, 3));

        btnTabCur++;

        if (btnTabCur > tabWepName.Length - 1)
        {
            btnTabCur = 0;
        }

        for (int j = 0; j < tabWepName.Length; j++)
        {
            if (j != btnTabCur)
            {
                tabWepName[j].color = new Color(0.8235294f, 0.5019608f, 0f);
                tabFrame[j].sprite = tabSprite[0];
            }
            else if (j == btnTabCur)
            {
                tabWepName[j].color = new Color(0f, 0f, 0f);
                tabFrame[j].sprite = tabSprite[1];
            }
        }


        BtnTunerTab(btnTabTran, btnTabCur, tabWepName.Length);
    }

    /// <summary>
    /// Called by the fire_btn. Plays the weapon-type-specific UI animation and fires through WeaponManager.
    /// </summary>
    public void BtnWeaponFire()
    {
        if (weaponManager == null) return;

        Vector3 targetPos = currentBogieTarget != null
            ? currentBogieTarget.transform.position
            : transform.position + transform.forward * 1000f;

        // Play weapon-type-specific UI animations
        switch (weaponManager.GetActiveWeaponType())
        {
            case WeaponType.Laser:
                LaserFire();
                break;
            case WeaponType.Railgun:
                RailFire();
                break;
        }

        weaponManager.FireActiveWeapon(targetPos, 1f);
    }

    private void GlitchStart()
    {
        for (int i = 0; i < staticImg.Length; i++)
        {
            staticImg[i].material = new Material(staticSha);

            staticImg[i].material.SetInt("_Glitch", 0);

            if (i > 0)
            {
                staticImg[i].material.SetFloat("_LineSize", 50f);
            }
            else if (i == 0)
            {
                staticImg[i].material.SetFloat("_LineSize", 100f);
            }

            Debug.Log(staticImg[i].material.ToString());
        }
    }

    private IEnumerator GlitchEffect(float t, float o, int s)
    {
        float speed = 1f;

        staticImg[s].material.SetInt("_Glitch", 1);

        while (o > 0f)
        {
            staticImg[s].material.SetFloat("_GlitchOpacity", o);

            o -= Time.deltaTime * speed;

            yield return null;
        }

        while (t > 0f)
        {
            t -= Time.deltaTime * speed;

            yield return null;
        }

        staticImg[s].material.SetFloat("_Glitch", 0);
    }

        /// <summary>
        /// Called by the load_btn. Primes or targets the active weapon depending on its type.
        /// Missile: locks all tubes onto the current bogie target.
        /// Macrocannon: arms (loads) one shell into the barrel.
        /// BoardingPod: sets the current bogie as the boarding target.
        /// Other weapon types auto-manage their own loading.
        /// </summary>
        public void BtnWeaponLoad()
    {
        if (weaponManager == null) return;

        Transform target = currentBogieTarget != null ? currentBogieTarget.transform : null;
        weaponManager.LoadActiveWeapon(target);
    }

    public void UpdateWeaponDisplay(string weapName, string weapType, Sprite icon, string status, Color statusColor, string ammo, float rechargeProgress, Color rechargeColor)
    {
        if (weaponNameText != null) weaponNameText.text = weapName;
        if (weaponTypeText != null) weaponTypeText.text = weapType;
        if (weaponIconImage != null) weaponIconImage.sprite = icon;
        if (weaponStatusText != null)
        {
            weaponStatusText.text = status;
            weaponStatusText.color = statusColor;
        }
        if (weaponAmmoText != null) weaponAmmoText.text = ammo;
        if (weaponRechargeBar != null)
        {
            weaponRechargeBar.fillAmount = rechargeProgress;
            weaponRechargeBar.color = rechargeColor;
        }
    }

    public Sprite GetWeaponIcon(WeaponType type)
    {
        return type switch
        {
            WeaponType.Laser => laserIcon,
            WeaponType.Macrocannon => macrocannonIcon,
            WeaponType.Missile => missileIcon,
            WeaponType.PointDefense => pointDefenseIcon,
            WeaponType.BoardingPod => boardingPodIcon,
            WeaponType.Railgun => railgunIcon,
            _ => null
        };
    }

    private void BtnTunerTab(Transform tran, int cur, int max)
    {
        float degree = (360 / max) * cur;

        tran.eulerAngles = new Vector3(0f, 0f, degree);
    }
}
