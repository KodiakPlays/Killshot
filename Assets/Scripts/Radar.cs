// =================================================================================================
// Radar.cs
//
// What it does:
// The old DRADIS screen of the older physics player ship, named Radar in the code. It places a dot
// for each nearby ship on a round screen in the older UIController panel. It is on the PlayerShip
// prefab. The battle board does not use this script. The board's DRADIS is in SensorSystem and
// PlotPanel.
//
// How it works:
// - Only one is kept. A second copy removes itself.
// - At the start it finds the object tagged "Player", if no ship is set in the Inspector.
// - Ships with RadarTarget sign up with it when they switch on. They sign off when they switch
//   off.
// - The range has five steps: 500, 1,000, 2,000, 4,000 and 8,000 units. The R key moves to the next
//   step and writes the range to the Console.
// - Every frame it works out where each ship is from the player's ship. It scales that to the
//   screen. UIController makes one dot per ship, and this script moves the dot there.
// - A ship past the range has its dot hidden. A ship set to show its heading has its dot turned to
//   match.
// - When a ship is gone, it drops the ship from the list. It asks UIController to remove its dot.
//
// Technical:
// - Data: targets (a list of RadarTarget) and currentRangeIndex into rangeSteps, in units.
// - Radar.RegisterTarget and Radar.UnregisterTarget are called by RadarTarget in OnEnable and
//   OnDisable.
// - It calls UIController.Instance: RadarBlipRadius, CreateOrGetRadarBlip(target) and
//   DestroyRadarBlip(target).
// - A dot's place is the target's position in the player ship's local space. It is scaled by the
//   range to the screen radius.
// - Numbers: rangeSteps and the key (R) are in the Inspector. The key uses the old Input Manager.
// - Trap: a second Radar removes its whole object. On the PlayerShip prefab, that object is the
//   ship.
// - Trap: a RadarTarget switched on before Radar's Awake finds no Radar.Instance. It never gets
//   a dot.
// =================================================================================================

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
        // Keyboard shortcut for dev convenience — controller X button is now handled in PlayerShip
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
