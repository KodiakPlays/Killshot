using UnityEngine;
using System.Collections.Generic;

public class Radar : MonoBehaviour
{
    public static Radar Instance;

    [Header("References")]
    [SerializeField] private Transform playerTransform;

    [Header("Settings")]
    [SerializeField] private float[] rangeSteps = new float[] { 500f, 1000f, 2000f, 4000f, 8000f };
    [SerializeField] private KeyCode toggleKey = KeyCode.R;

    private int currentRangeIndex = 0;
    private List<RadarTarget> targets = new List<RadarTarget>();

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
        var ui = UIController.Instance;
        if (ui == null) return;

        float currentRange = GetCurrentRange();
        float radarRadius = ui.RadarBlipRadius;

        List<RadarTarget> toRemove = new List<RadarTarget>();

        foreach (var target in targets)
        {
            if (target == null)
            {
                toRemove.Add(target);
                continue;
            }

            RectTransform blipRect = ui.CreateOrGetRadarBlip(target);
            if (blipRect == null) continue;

            Vector3 relativePos = playerTransform.InverseTransformPoint(target.transform.position);
            Vector2 radarPos = new Vector2(relativePos.x, relativePos.y);

            float distance = radarPos.magnitude;
            float scale = distance / currentRange;

            if (scale > 1.0f)
            {
                if (blipRect.gameObject.activeSelf) blipRect.gameObject.SetActive(false);
            }
            else
            {
                if (!blipRect.gameObject.activeSelf) blipRect.gameObject.SetActive(true);

                Vector2 uiPos = radarPos.normalized * scale * radarRadius;
                blipRect.anchoredPosition = uiPos;

                if (target.trackRotation)
                {
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
            UIController.Instance?.DestroyRadarBlip(target);
        }
    }
}
