using UnityEngine;
using UnityEngine.UI;
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
    }

    private void Update()
    {
        HandleInput();
        UpdateBlips();
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
}
