using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Radar : MonoBehaviour
{
    public static Radar Instance;

    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private RectTransform radarContainer;
    [SerializeField] private GameObject blipPrefab;

    [Header("Settings")]
    [SerializeField] private float[] rangeSteps = new float[] { 500f, 1000f, 2000f, 4000f, 8000f };
    [SerializeField] private KeyCode toggleKey = KeyCode.R;
    [SerializeField] private float radarRadius = 100f; // Radius of the UI container in pixels

    private int currentRangeIndex = 0;
    private List<RadarTarget> targets = new List<RadarTarget>();
    private Dictionary<RadarTarget, RectTransform> blips = new Dictionary<RadarTarget, RectTransform>();

    [Header("Shader Radar Screen")]
    [SerializeField] private Shader radarSha;
    [SerializeField] private GameObject radarGO;
    [SerializeField] private GameObject radarBogeyGO;
    public Image radarImg;
    [SerializeField] private float radarDegreeTest;

    [Header("Scanner Hack")]
    [SerializeField] private Slider scannerSlider;
    private float scanValue = 0;
    private float placeValue = 0;
    [SerializeField] private int scanSpeed = 10;
    private bool rightScanner = true;
    [SerializeField] private Shader scannerSha;
    [SerializeField] private Image scannerImg;
    private float scanerMax = 1;
    private List<Vector2> scanerPipsV2 = new List<Vector2>();
    [SerializeField] private List<int> scanPips = new List<int>();
    [SerializeField] private string[] shipPartsString = { "_Port", "_Aft", "_Prow", "_Star" };
    private Color[] pipCol = { new Color(1f, 0.8086438f, 0f), new Color(1f, 0f, 0f), new Color(0f, 1f, 0f) };
    [SerializeField] private GameObject targetGO;
    [SerializeField] private MeshFilter bogieMesh;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        // Initialize shader radar screen
        if (radarGO != null)
        {
            RadarStart();
        }

        // Initialize scanner
        if (scannerImg != null)
        {
            NewScan();
            ScanTargetLoc(0);
            ScanTargetSize(0);
        }
    }

    private void Update()
    {
        HandleInput();
        UpdateBlips();

        // Shader radar test
        if (radarGO != null)
        {
            RadarTest();

            if (Input.GetKeyDown("/"))
            {
                BogeySpot(radarDegreeTest);
            }
        }

        // Scanner updates
        if (scannerImg != null)
        {
            if (Input.GetKeyDown("space"))
            {
                ScanPlaceHack();
            }

            ScanMovment();

            if (Input.GetKeyDown("]"))
            {
                ScanNewTarget();
            }
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            currentRangeIndex = (currentRangeIndex + 1) % rangeSteps.Length;
            Debug.Log($"Radar Range: {GetCurrentRange()}");
        }
    }

    private void UpdateBlips()
    {
        if (playerTransform == null) return;

        float currentRange = GetCurrentRange();
        
        // Create a list to remove null keys if any
        List<RadarTarget> toRemove = new List<RadarTarget>();

        foreach (var target in targets)
        {
            if (target == null)
            {
                toRemove.Add(target);
                continue;
            }

            RectTransform blipRect;
            if (!blips.TryGetValue(target, out blipRect))
            {
                if (blipPrefab == null || radarContainer == null) continue;

                GameObject blipObj = Instantiate(blipPrefab, radarContainer);
                blipRect = blipObj.GetComponent<RectTransform>();
                blips[target] = blipRect;
                
                Image img = blipObj.GetComponent<Image>();
                if (img != null)
                {
                    if (target.icon != null) img.sprite = target.icon;
                    img.color = target.color;
                }
            }

            Vector3 relativePos = playerTransform.InverseTransformPoint(target.transform.position);
            // x is right, y is forward (2D space). In UI, x is x, y is y.
            Vector2 radarPos = new Vector2(relativePos.x, relativePos.y);
            
            float distance = radarPos.magnitude;
            
            // Scale to radar
            float scale = distance / currentRange;
            
            if (scale > 1.0f)
            {
                // Hide if outside range
                if (blipRect.gameObject.activeSelf) blipRect.gameObject.SetActive(false);
            }
            else
            {
                if (!blipRect.gameObject.activeSelf) blipRect.gameObject.SetActive(true);
                
                // Calculate position
                // We want the blip to be at (normalizedPos * radarRadius)
                // But we need to be careful about the scale. 
                // If distance is 0, pos is 0. If distance is range, pos is radius.
                Vector2 uiPos = radarPos.normalized * scale * radarRadius;
                blipRect.anchoredPosition = uiPos;

                // Optional: Rotate blip to match target rotation relative to player
                if (target.trackRotation)
                {
                    // Calculate relative rotation for 2D (XY plane, rotation around Z)
                    float angle = target.transform.eulerAngles.z - playerTransform.eulerAngles.z;
                    blipRect.localEulerAngles = new Vector3(0, 0, angle);
                }
            }
        }

        foreach (var target in toRemove)
        {
            UnregisterTarget(target);
        }
    }

    public float GetCurrentRange()
    {
        if (rangeSteps.Length == 0) return 1000f;
        return rangeSteps[currentRangeIndex];
    }

    public static void RegisterTarget(RadarTarget target)
    {
        if (Instance != null && !Instance.targets.Contains(target))
        {
            Instance.targets.Add(target);
        }
    }

    public static void UnregisterTarget(RadarTarget target)
    {
        if (Instance != null)
        {
            Instance.targets.Remove(target);
            if (Instance.blips.ContainsKey(target))
            {
                if (Instance.blips[target] != null) Destroy(Instance.blips[target].gameObject);
                Instance.blips.Remove(target);
            }
        }
    }

    // ===== Shader Radar Screen Methods (moved from UIController) =====

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

    public void BogeySpot(float d)
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
            yield return null;
        }

        Destroy(go);
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

    // ===== Scanner Methods (moved from UIController) =====

    public void NewScan()
    {
        scanerPipsV2.Clear();
        scanPips.Clear();

        scannerImg.material = new Material(scannerSha);

        int check = 50;
        int startI = 0;

        scanerMax = 30f;

        scannerImg.material.SetFloat("_NibMax", scanerMax);

        for (int i = 0; i < 4; i++)
        {
            scannerImg.material.SetColor(shipPartsString[i] + "Color", pipCol[0]);
        }

        for (int i = 0; i < (int)scanerMax; i++)
        {
            scanPips.Add(5);
        }

        List<int> size = new List<int>();

        for (int i = 0; i < 4; i++)
        {
            size.Add(Random.Range(1, 5));
            scanerPipsV2.Add(new Vector2(0, 0));
        }

        for (int i = 0; i < scanerPipsV2.Count; i++)
        {
            int checkPlace = 50;

            while (checkPlace > 0)
            {
                bool placable = false;

                startI = Random.Range(0, (int)scanerMax);

                if (startI + size[i] <= (int)scanerMax)
                {
                    for (int j = startI; j < startI + size[i]; j++)
                    {
                        if (scanPips[j] == 5)
                        {
                            placable = true;
                        }
                        else if (scanPips[j] != 5)
                        {
                            placable = false;
                            break;
                        }
                    }
                }

                if (placable)
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
                    --checkPlace;
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
                scannerImg.material.SetColor(shipPartsString[i] + "Color", pipCol[1]);
            }
        }
    }

    public void ScanMovment()
    {
        if (rightScanner)
        {
            scannerSlider.value = ++scanValue / scanSpeed;
        }
        else if (!rightScanner)
        {
            scannerSlider.value = --scanValue / scanSpeed;
        }

        if (scannerSlider.value >= scanerMax)
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
        var gm = GameManager.Instance;
        if (gm == null) return;

        if (gm.bogieList.Count == 0)
        {
            gm.currentBogieTarget = null;
            ScanTargetLoc(0);
            return;
        }
        else if (gm.bogieList.Count > 0)
        {
            for (int j = 0; j < gm.bogieList.Count - 1; j++)
            {
                if (gm.currentBogieTarget == gm.bogieList[gm.bogieList.Count - 1].go)
                {
                    gm.currentBogieTarget = gm.bogieList[0].go;
                    ScanTargetLoc(1);

                    if (gm.worldZoom == 0) ScanTargetSize(1);
                    else if (gm.worldZoom == 1) ScanTargetSize(2);
                    else if (gm.worldZoom == 2) ScanTargetSize(3);

                    break;
                }
                else if (gm.bogieList[j].go == gm.currentBogieTarget)
                {
                    gm.currentBogieTarget = gm.bogieList[j + 1].go;
                    ScanTargetLoc(1);

                    if (gm.worldZoom == 0) ScanTargetSize(1);
                    else if (gm.worldZoom == 1) ScanTargetSize(2);
                    else if (gm.worldZoom == 2) ScanTargetSize(3);

                    break;
                }
            }
        }

        ScanMesh(gm.currentBogieTarget);
        NewScan();
    }

    public void ScanTargetSize(int i)
    {
        if (targetGO == null) return;

        if (i == 0)
        {
            targetGO.transform.localScale = Vector3.zero;
        }
        else if (i == 1)
        {
            targetGO.transform.localScale = new Vector3(1f, 1f, 1f);
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

    public void ScanTargetLoc(int i)
    {
        if (targetGO == null) return;

        if (i == 0)
        {
            targetGO.transform.parent = playerTransform;
        }
        if (i == 1)
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.currentBogieTarget != null)
                targetGO.transform.parent = gm.currentBogieTarget.transform;
        }
    }

    private void ScanMesh(GameObject go)
    {
        if (go == null || bogieMesh == null) return;
        MeshFilter m = go.GetComponent<MeshFilter>();
        if (m != null)
            bogieMesh.mesh = m.mesh;
    }
}
