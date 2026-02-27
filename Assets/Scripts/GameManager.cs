using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject EnemyShipPrefab;

    [Header("Mission Systems")]
    [SerializeField] private GameClock gameClock;
    [SerializeField] private QuestSystem questSystem;
    [SerializeField] private WorldBoundary worldBoundary;

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
    [SerializeField] private Camera sc;
    public int worldZoom;
    [SerializeField] private Texture[] viewTex;
    [SerializeField] private RawImage screenWorld;

    [Header("Player References")]
    [SerializeField] private Transform shipPlayer;
    [SerializeField] private GameObject shipiconGO;
    private PlayerShip playerShipRef;

    [Header("Bogie Management")]
    [SerializeField] public List<BogieClass> bogieList = new List<BogieClass>();
    [SerializeField] public GameObject currentBogieTarget;
    [SerializeField] private MeshFilter bogieMesh;

    public AnimationCurve animationCurve;

    public void SpawnEnemyShip(Vector3 position, Quaternion rotation)
    {
        Instantiate(EnemyShipPrefab, position, rotation);
    }

    void Start()
    {
        // Initialize game state
        //Spawn enemy ships randomly at start
        if (EnemyShipPrefab != null)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 randomPos = new Vector3(Random.Range(-50, 50), Random.Range(-50, 50), 0);
                SpawnEnemyShip(randomPos, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] EnemyShipPrefab is not assigned! Cannot spawn enemies.");
        }

        // Find player reference
        playerShipRef = FindFirstObjectByType<PlayerShip>();

        // Initialize mission systems
        InitializeMissionSystems();

        // Initialize world grid and weapon screen
        if (gridImg != null && gridSha != null)
        {
            WorldGridStart();
            WorldGridZoom(0);
        }

        // Initialize bogies (test)
        NewBogie();
    }

    private void InitializeMissionSystems()
    {
        // GameClock
        if (gameClock == null)
        {
            gameClock = GetComponent<GameClock>();
            if (gameClock == null) gameClock = gameObject.AddComponent<GameClock>();
        }

        // QuestSystem
        if (questSystem == null)
        {
            questSystem = GetComponent<QuestSystem>();
            if (questSystem == null) questSystem = gameObject.AddComponent<QuestSystem>();
        }

        // WorldBoundary
        if (worldBoundary == null)
        {
            worldBoundary = GetComponent<WorldBoundary>();
            if (worldBoundary == null) worldBoundary = gameObject.AddComponent<WorldBoundary>();
        }

        // Subscribe to mission events
        questSystem.OnMissionFailed += (reason) => Debug.Log($"[GameManager] Mission Failed: {reason}");
        questSystem.OnMissionComplete += () => Debug.Log("[GameManager] Mission Complete!");
    }

    void Update()
    {
        // Strategic pause per GDD: player may pause at any time, clock stops during pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameClock != null) gameClock.TogglePause();
        }

        // Weapon test inputs
        if (Input.GetKeyDown("0")) LaserFireEnemy(0);
        if (Input.GetKeyDown("1")) LaserFireEnemy(1);
        if (Input.GetKeyDown("2")) LaserFireEnemy(2);
        if (Input.GetKeyDown("3")) LaserFireEnemy(3);
        if (Input.GetKeyDown("4")) LaserFireEnemy(4);
        if (Input.GetKeyDown("p")) LaserFire();
        if (Input.GetKeyDown("r")) RailFire();
    }

    // ===== World Grid Methods (moved from UIController) =====

    private void WorldGridStart()
    {
        if (screenWepGO != null)
            screenWepRT = screenWepGO.GetComponent<RectTransform>();
        
        if (gridImg != null && gridSha != null)
        {
            gridImg.material = new Material(gridSha);
            gridImg.material.SetVector("_ShipLocV2", new Vector2(0f, 0f));
            gridImg.material.SetFloat("_ShipRotation", 0f);
        }

        if (weaponScreenImage != null && weaponScreenSha != null)
        {
            weaponScreenImage.material = new Material(weaponScreenSha);
            weaponScreenImage.material.SetInt("_LaserFire", 0);
        }

        if (screenWorld != null && viewTex != null && viewTex.Length > 0)
            screenWorld.texture = viewTex[0];
    }

    public void WorldGridLocUpdate(Vector2 shipV2)
    {
        if (gridImg == null || gridImg.material == null) return;

        if (worldZoom == 0)
        {
            gridImg.material.SetVector("_ShipLocV2", (shipV2 / 100f));

            if (currentBogieTarget != null && Radar.Instance != null)
            {
                Radar.Instance.ScanTargetLoc(1);
                Radar.Instance.ScanTargetSize(1);
            }

            if (shipPlayer != null && shipPlayer.childCount > 0)
            {
                var meshRenderer = shipPlayer.GetChild(0).gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer != null) meshRenderer.enabled = true;
            }
            if (shipiconGO != null) shipiconGO.SetActive(false);
        }
        else if (worldZoom == 1)
        {
            gridImg.material.SetVector("_ShipLocV2", (shipV2 / 1000f));

            if (currentBogieTarget != null && Radar.Instance != null)
            {
                Radar.Instance.ScanTargetLoc(2);
                Radar.Instance.ScanTargetSize(2);
            }
        }
        else if (worldZoom == 2)
        {
            if (currentBogieTarget != null && Radar.Instance != null)
            {
                Radar.Instance.ScanTargetLoc(3);
                Radar.Instance.ScanTargetSize(3);
            }
        }
    }

    public void WorldGridRotUpdate(float r)
    {
        if (gridImg == null || gridImg.material == null) return;

        if (worldZoom < 2)
        {
            gridImg.material.SetFloat("_ShipRotation", r);
        }
    }

    public void WorldGridZoom(int i)
    {
        if (gridImg == null || gridImg.material == null) return;
        
        var radar = Radar.Instance;

        if (i == 0)
        {
            if (screenWorld != null && viewTex != null && viewTex.Length > 0)
                screenWorld.texture = viewTex[0];

            gridImg.material.SetInt("_WorldView", 0);
            gridImg.material.SetVector("_CellSize", new Vector2(1, 1));
            gridImg.material.SetFloat("_GridAmmount", 5f);
            gridImg.material.SetFloat("_GridThickness", .02f);
            gridImg.material.SetFloat("_ShipRotation", shipPlayer != null ? shipPlayer.localRotation.z : 0f);

            if (radar != null && radar.radarImg != null)
            {
                radar.radarImg.material.SetFloat("_RadarRange", 1f);
                radar.radarImg.material.SetInt("_Show", 1);
                radar.radarImg.material.SetFloat("_VisualRange", 1f);
            }
            if (weaponScreenImage != null && weaponScreenImage.material != null)
                weaponScreenImage.material.SetInt("_RangeVisOn", 1);

            scaleSize = 1f;
            if (sc != null) sc.orthographicSize = 50f;

            for (int j = 0; j < bogieList.Count; j++)
            {
                bogieList[j].go.GetComponent<MeshRenderer>().enabled = true;
                bogieList[j].go.transform.GetChild(0).gameObject.SetActive(false);
            }

            worldZoom = 0;
            screenDist = 50;
        }
        else if (i == 1)
        {
            if (screenWorld != null && viewTex != null && viewTex.Length > 0)
                screenWorld.texture = viewTex[0];

            gridImg.material.SetInt("_WorldView", 0);
            gridImg.material.SetVector("_CellSize", new Vector2(1, 1));
            gridImg.material.SetFloat("_GridAmmount", 20f);
            gridImg.material.SetFloat("_GridThickness", .04f);
            gridImg.material.SetFloat("_ShipRotation", shipPlayer != null ? shipPlayer.localRotation.z : 0f);

            if (radar != null && radar.radarImg != null)
            {
                radar.radarImg.material.SetFloat("_RadarRange", 1f);
                radar.radarImg.material.SetInt("_Show", 1);
                radar.radarImg.material.SetFloat("_VisualRange", .1f);
            }
            if (weaponScreenImage != null && weaponScreenImage.material != null)
                weaponScreenImage.material.SetInt("_RangeVisOn", 0);
            scaleSize = .5f;
            if (sc != null) sc.orthographicSize = 500f;

            for (int j = 0; j < bogieList.Count; j++)
            {
                bogieList[j].go.GetComponent<MeshRenderer>().enabled = false;
                bogieList[j].go.transform.GetChild(0).gameObject.SetActive(true);
                bogieList[j].go.transform.GetChild(0).gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
            }

            if (shipiconGO != null)
            {
                shipiconGO.SetActive(true);
                shipiconGO.transform.localScale = new Vector3(2f, 2f, 2f);
            }
            if (shipPlayer != null && shipPlayer.childCount > 0)
                shipPlayer.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = false;

            worldZoom = 1;
            screenDist = 500;
        }
        else if (i == 2)
        {
            if (viewTex != null && viewTex.Length > 1 && screenWorld != null)
                screenWorld.texture = viewTex[1];

            gridImg.material.SetInt("_WorldView", 1);
            gridImg.material.SetVector("_CellSize", new Vector2(2, 2));
            gridImg.material.SetFloat("_GridAmmount", 20f);
            gridImg.material.SetFloat("_GridThickness", .025f);
            gridImg.material.SetFloat("_ShipRotation", 0);

            if (radar != null && radar.radarImg != null)
            {
                radar.radarImg.material.SetFloat("_RadarRange", .1f);
                radar.radarImg.material.SetInt("_Show", 0);
                radar.radarImg.material.SetFloat("_VisualRange", .01f);
            }
            if (weaponScreenImage != null && weaponScreenImage.material != null)
                weaponScreenImage.material.SetInt("_RangeVisOn", 0);
            scaleSize = .25f;
            if (sc != null) sc.orthographicSize = 5000f;

            for (int j = 0; j < bogieList.Count; j++)
            {
                bogieList[j].go.GetComponent<MeshRenderer>().enabled = false;
                bogieList[j].go.transform.GetChild(0).gameObject.SetActive(true);
                bogieList[j].go.transform.GetChild(0).gameObject.transform.localScale = new Vector3(20f, 20f, 20f);
            }

            if (shipiconGO != null)
            {
                shipiconGO.SetActive(true);
                shipiconGO.transform.localScale = new Vector3(20f, 20f, 20f);
            }
            if (shipPlayer != null && shipPlayer.childCount > 0)
                shipPlayer.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = false;

            worldZoom = 2;
            screenDist = 5000;
        }
    }

    // ===== Bogie Management (moved from UIController) =====

    public void NewBogie()
    {
        if (weaponScreenSha == null || screenEnemyWeapon == null) return;

        GameObject go = null;
        GameObject goImg = null;

        for (int j = 0; j < 5; j++)
        {
            go = GameObject.Find("Bogie_" + j.ToString());
            if (go == null) continue;

            int idx = bogieList.Count; // Use actual list index, not loop counter
            bogieList.Add(new BogieClass(go, go.GetComponent<MeshFilter>() != null ? go.GetComponent<MeshFilter>().mesh : null, null, new Material(weaponScreenSha)));

            // Create UI image directly (no Instantiate(new GameObject()) to avoid orphan leak)
            goImg = new GameObject("BogieImg_" + j);
            goImg.transform.SetParent(screenEnemyWeapon, false);

            bogieList[idx].wepImage = goImg.AddComponent<Image>();
            bogieList[idx].wepImage.material = bogieList[idx].matWep;

            goImg.transform.localScale = new Vector3(1f, 1f, 1f);

            RectTransform rt = goImg.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);

            bogieList[idx].WeapStart();
        }
    }

    // ===== Weapon Fire Visuals (moved from UIController) =====

    public void LaserFire()
    {
        if (!fire && currentBogieTarget != null)
        {
            StartCoroutine(LaserFireCo(currentBogieTarget, -1));
        }
    }

    public void LaserFireEnemy(int i)
    {
        if (i < bogieList.Count)
        {
            StartCoroutine(LaserFireCo(bogieList[i].go, i));
        }
    }

    private IEnumerator LaserFireCo(GameObject bogie, int s)
    {
        float t = 0f;
        float laserWidth = 0.498f;
        float speed = 5f;
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

        Transform bogieTran = bogie.GetComponent<Transform>();

        if (worldZoom < 2)
        {
            playerShipTranLoc = Vector3.zero;
            bogieTranLoc = shipPlayer.InverseTransformPoint(bogieTran.position);
            start = 0f;
        }
        else if (worldZoom == 2)
        {
            playerShipTranLoc = new Vector3(shipPlayer.position.x / (screenDist * 2), shipPlayer.position.y / (screenDist * 2), 0f);
            bogieTranLoc = new Vector3(bogieTran.position.x / (screenDist * 2), bogieTran.position.y / (screenDist * 2), 0f);
            start = 0f;
        }
        bogieTranLoc = shipPlayer.InverseTransformPoint(bogieTran.position);

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

        angle = (Vector2.Angle(vRight, direction) * sign + offset);
        distance = Vector2.Distance(vRight, bogieTranLoc);

        float scaleDistance = distance / screenDist;

        if (s < 0)
        {
            fire = true;
            yield return new WaitForSeconds(.1f);

            weaponScreenImage.material.SetFloat("_LaserDegree", angle);
            weaponScreenImage.material.SetInt("_LaserFire", 1);
            weaponScreenImage.material.SetFloat("_LaserSize", .0015f);

            weaponScreenImage.material.SetFloat("_LaserStart", start);
            weaponScreenImage.material.SetFloat("_LaserEnd", scaleDistance);

            weaponScreenImage.material.SetFloat("_LaserRange", 1f);
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
        else if (s >= 0)
        {
            bogieList[s].matWep.SetFloat("_LaserDegree", angle);
            bogieList[s].matWep.SetInt("_LaserFire", 1);
            bogieList[s].matWep.SetFloat("_LaserSize", .0015f);

            bogieList[s].matWep.SetFloat("_LaserStart", 0f);
            bogieList[s].matWep.SetFloat("_LaserEnd", scaleDistance);

            bogieList[s].matWep.SetFloat("_LaserRange", 1);

            if (scaleDistance > 1)
            {
                t = 1;
            }
            else if (scaleDistance <= 1)
            {
                t = scaleDistance;
            }

            if (playerShipRef != null) playerShipRef.updateShipHit(t);

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
            start = 0f;
        }
        else if (worldZoom == 2)
        {
            angle = shipPlayer.eulerAngles.z + 90;
            playerShipTranLoc = new Vector3(shipPlayer.position.x / (screenDist * 2), shipPlayer.position.y / (screenDist * 2), 0f);
            start = 0f;
        }

        weaponScreenImage.material.SetFloat("_LaserDegree", angle);
        weaponScreenImage.material.SetVector("_GunLoc", playerShipTranLoc);

        weaponScreenImage.material.SetFloat("_LaserStart", 0f);
        weaponScreenImage.material.SetFloat("_LaserEnd", 2f);
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
        if (playerShipRef != null) playerShipRef.updateShipHit(.5f);

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

    // ===== Stubs (moved from UIController) =====

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

    /// <summary>
    /// Called by UIController to assign UI references that live on the Canvas prefab.
    /// </summary>
    public void AssignUIReferences(
        Shader weaponScreenShaRef,
        Image weaponScreenImageRef,
        GameObject screenWepGORef,
        Transform shipAngleTargetRef,
        RectTransform screenEnemyWeaponRef,
        Shader gridShaRef,
        Image gridImgRef,
        Camera scRef,
        Texture[] viewTexRef,
        RawImage screenWorldRef,
        Transform shipPlayerRef,
        GameObject shipiconGORef,
        MeshFilter bogieMeshRef,
        AnimationCurve animationCurveRef)
    {
        weaponScreenSha = weaponScreenShaRef;
        weaponScreenImage = weaponScreenImageRef;
        screenWepGO = screenWepGORef;
        shipAngleTarget = shipAngleTargetRef;
        screenEnemyWeapon = screenEnemyWeaponRef;
        gridSha = gridShaRef;
        gridImg = gridImgRef;
        sc = scRef;
        viewTex = viewTexRef;
        screenWorld = screenWorldRef;
        shipPlayer = shipPlayerRef;
        shipiconGO = shipiconGORef;
        bogieMesh = bogieMeshRef;
        animationCurve = animationCurveRef;
    }
}
