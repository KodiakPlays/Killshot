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
    [SerializeField]
    private List<Button> btnPowerBool = new List<Button>();

    [SerializeField]
    private List<Image> imgPowerMet = new List<Image>();

    [Header("Weapon Screen")]
    [SerializeField] private Shader weaponScreenSha;
    [SerializeField] private Image weaponScreenImage;
    //[SerializeField] private Material WeaponScreen;

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
    
    [Header("Speedometer Settings")]
    [SerializeField] private bool autoUpdateSpeedometer = true;
    [SerializeField] private float speedometerUpdateRate = 0.1f; // Update every 0.1 seconds
    private PlayerShip playerShipRef;
    private Coroutine speedometerUpdateCoroutine;

    [Header("Stability Settings")]
    [SerializeField] private Shader stabilityShader;
    [SerializeField] private Image stabilityImg;
    private float stabMax = 1;

    [Header("Scanner Hack")]
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


    [Header("Radar Screen")]
    [SerializeField] private Shader radarSha;
    [SerializeField] private GameObject radarGO;
    [SerializeField] private GameObject radarBogeyGO;
    [SerializeField] private Image radarImg;
    [SerializeField] private float radarDegreeTest;

    [Header("Weapon Screen")]
    [SerializeField] private GameObject screenWepGO;
    private RectTransform screenWepRT;

    [Header("World Grid")]
    [SerializeField] private Shader gridSha;
    [SerializeField] private Image gridImg;
    [SerializeField] private Camera sc;

    public AnimationCurve animationCurve;

    void Start()
    {
        //FrequancyTune(360f);

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
    }

    void Update()
    {

        if (Input.GetKeyDown("space"))
        {
            ScanPlaceHack();
        }

        ScanMovment();

        //testing purposes of radar
        //radarDegreeTest = radarDegreeTest + (Time.deltaTime * 10);

        //if (radarDegreeTest >= 360)
        //{
        //    radarDegreeTest = 0;
        //}
        //RadarUpdate(radarDegreeTest);

        if (Input.GetKeyDown("/"))//test change ship
        {
            BogeySpot(radarDegreeTest);
        }

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

                //Debug.Log(shipPartsString[i] + ", " + checkPlace + ", " + scanerPipsV2[i] + ", " + placable);

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

    private void StartPower()
    {
        for (int i = 0; i < btnPowerBool.Count-1; i++)
        {
            uiPowerMetClass.Add(new UIPowerClass(new Material(shaPowerMet), 6, 1, 0f, false));

            imgPowerMet[i].material = uiPowerMetClass[i].mat;
        }

        uiPowerMetClass.Add(new UIPowerClass(new Material(shaReactorMet), 15, 15, 0f, false));

        imgPowerMet[btnPowerBool.Count-1].material = uiPowerMetClass[btnPowerBool.Count-1].mat;
    }

    public void ChargeBtn(int i)
    {
        if (uiPowerMetClass[i].charge == false)
        {
            ChargeOn(i,1);
        }
        else if (uiPowerMetClass[i].charge == true)
        {
            ChargeOff(i);
        }
    }

    public void ChargeOn(int i, int speed)
    {
        uiPowerMetClass[i].Charge(true);
        //uiPowerMetClass[uiPowerMetClass.Count - 1].Charge(true);

        powerAnimCoroutine[i] = ChargeOnAnim(i, 1);

        StartCoroutine(powerAnimCoroutine[i]);

        btnPowerBoolImage[i].sprite = uiSprite[1];
    }

    public void ChargeOff(int i)
    {
        Debug.Log("off: " + (i));

        uiPowerMetClass[i].Charge(false);

        StopCoroutine(powerAnimCoroutine[i]);

        btnPowerBoolImage[i].sprite = uiSprite[0];

        //int chargeTrue = 0;

        //for (int j = 0; j < uiPowerMetClass.Count - 1; j++)
        //{
        //    if (uiPowerMetClass[j].charge)
        //    {
        //        chargeTrue++;
        //    }
        //}

        //if (chargeTrue > 0)
        //{
        //    uiPowerMetClass[uiPowerMetClass.Count - 1].Charge(true);

        //}else if (chargeTrue <= 0)
        //{
        //    uiPowerMetClass[uiPowerMetClass.Count - 1].Charge(false);
        //}

    }

    private void BoolBtn(int i)
    {

    }

    public IEnumerator ChargeOnAnim(int i, int speed)
    {
        Debug.Log("i: " + i);

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

            //shaReactorMet[btnPowerBool.Count - 1].mat.SetFloat("_PowerCur", uiPowerMetClass[i].cur);

            ChargeOn(i, speed);
            //powerAnimCoroutine.Add(StartCoroutine(ChargeOnAnim(i, speed)));
        }
    }


    public void VentBtn()
    {
        for (int i = 0; i < uiPowerMetClass.Count - 1; i++)
        {
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

                    //Mathf.MoveTowards(0, crgAmt, time);
                    //time += (Time.deltaTime) * (speed);

                    yield return new WaitForSeconds(.5f);
                }
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

        //float multi = Mathf.Pow(100, 3);

        //float roundedFrequancy = MathF.Round(frequancyFlt * multi) / multi;

        frequancyTrans.rotation = Quaternion.Euler(0f, 0f, frequancyFlt * -1);

        frequancyTMP.text = frequancyFlt.ToString();

        //frequancyFlt = roundedFrequancy;

        float normF = tunerTrans[1].localPosition.x / tunerTrans[2].localPosition.x;

        tunerTrans[0].localPosition = Vector2.Lerp(tunerTrans[1].localPosition, tunerTrans[2].localPosition, frequancyFlt/(360*5));


    }

    public void updateShipHit(float duration)
    {
        shipHitCo = StartCoroutine(ShipShake(duration));
    }

    private IEnumerator ShipShake(float duration)
    {
        Vector3 startPos = new Vector3(0f, 0f, 0f);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float strength = shipHitCurve.Evaluate(elapsedTime / duration);
            shipHit.localPosition = startPos + Random.insideUnitSphere * strength;
            yield return null;
        }

        shipHit.localPosition = startPos;
    }

    public void updateCompass(bool loaded, int icon, float port, float aft, float prow, float starboard)
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

    public void updateCompass(float angle)
    {
        compassRect.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void updateSpeedometer(float speed)
    {
        speedometer.text = speed.ToString("F1");
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
    }
    
    private IEnumerator UpdateSpeedometerContinuously()
    {
        while (playerShipRef != null && autoUpdateSpeedometer)
        {
            UpdateSpeedometerFromPlayerShip();
            yield return new WaitForSeconds(speedometerUpdateRate);
        }
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
                
                updateSpeedometer(speed);
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
    }

    public void WorldGridLocUpdate(Vector2 shipV2)
    {
        gridImg.material.SetVector("_ShipLocV2", (shipV2/100f));
    }

    public void WorldGridRotUpdate(float r)
    {
        gridImg.material.SetFloat("_ShipRotation", r);
    }

    public void WorldGridZoom(int i)
    {

        if (i == 0)//spectral zoom 10x
        {
            gridImg.material.SetInt("_WorldView", 0);
            gridImg.material.SetVector("_CellSize", new Vector2(1,1));
            gridImg.material.SetFloat("_GridAmmount", 2f);
            gridImg.material.SetFloat("_GridThickness", .02f);

            radarImg.material.SetFloat("_RadarRange", 1f);

            screenWepGO.transform.localPosition = new Vector2(0f, 0f);

            sc.orthographicSize = 50f;
        }
        else if (i == 1)//spectral zoom 100x
        {
            gridImg.material.SetInt("_WorldView", 0);
            gridImg.material.SetVector("_CellSize", new Vector2(1, 1));
            gridImg.material.SetFloat("_GridAmmount", 10f);
            gridImg.material.SetFloat("_GridThickness", .04f);

            radarImg.material.SetFloat("_RadarRange", 1f);

            screenWepGO.transform.localPosition = new Vector2(10000f, 0f);

            sc.orthographicSize = 500f;
        }
        else if (i == 2)//spectral zoom 1000x
        {
            gridImg.material.SetInt("_WorldView", 1);
            gridImg.material.SetVector("_CellSize", new Vector2(2, 2));
            gridImg.material.SetFloat("_GridAmmount", 20f);
            gridImg.material.SetFloat("_GridThickness", .025f);

            radarImg.material.SetFloat("_RadarRange", .1f);

            screenWepGO.transform.localPosition = new Vector2(10000f, 0f);

            sc.orthographicSize = 5000f;
        }

    }

    public void UIMatStart()
    {
        //weaponScreenImage = new Material(weaponScreenSha);
    }

    public void RailFire()
    {
        StartCoroutine(RailFireCo());
    }

    private IEnumerator RailFireCo()
    {
        float t = 0f;
        float railStart = .499f;
        float railEnd = .48f;
        float speed = 5f;
        float curWidth = 0f;

        weaponScreenImage.material.SetFloat("_RailWidth", railStart);
        weaponScreenImage.material.SetFloat("_FireRail", 1f);

        yield return new WaitForSeconds(1f);

        while (t < 1f)
        {
            curWidth = Mathf.Lerp(railStart, railEnd, animationCurve.Evaluate(t));

            weaponScreenImage.material.SetFloat("_RailWidth", curWidth);

            t += Time.deltaTime * speed;

            yield return null;
        }

        weaponScreenImage.material.SetFloat("_FireRail", 0f);
    }

    private void OnDestroy()
    {
        // Clean up coroutine when object is destroyed
        if (speedometerUpdateCoroutine != null)
        {
            StopCoroutine(speedometerUpdateCoroutine);
        }
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
}
