using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The SECTOR MAP (owner 2026-11-08): the campaign's missions as dots on a grid of space. At the start one dot shows.
/// Click a dot: a box opens with the mission's name, a line of briefing and LAUNCH. Win the mission and, back on the map,
/// the next dot fades in with a line from the last. Finished dots stay, dimmer, and can be played again.
/// The missions are listed on this script in order: each opens when the one before it is won. Stand-in look until the
/// owner sends a reference for the map.
/// </summary>
public class SectorMap : MonoBehaviour
{
    [System.Serializable]
    public class Mission
    {
        [Tooltip("The name on the map and in the box.")]
        public string title;
        [Tooltip("The level it plays: a scene in Assets/Scenes/Levels, by name.")]
        public string level;
        [Tooltip("The briefing line in the box.")]
        [TextArea] public string brief;
        [Tooltip("Where its dot is, px from the middle of the 1920 x 1080 screen.")]
        public Vector2 at;
    }

    [Tooltip("The campaign's missions, in order. Each opens when the one before it is won.")]
    [SerializeField] private Mission[] missions;

    [Header("Parts")]
    [Tooltip("The dots and lines are made in here.")]
    [SerializeField] private RectTransform map;
    [Tooltip("Switched off; copied for each dot. Its first text is the mission's name.")]
    [SerializeField] private Button dotTemplate;
    [Tooltip("Switched off; copied for each line between two dots.")]
    [SerializeField] private Image lineTemplate;
    [Tooltip("The mission box. Hidden until a dot is clicked.")]
    [SerializeField] private GameObject box;
    [SerializeField] private TMP_Text boxTitle, boxBrief;
    [Tooltip("LAUNCH: INITIALISING..., then the board with the mission's level.")]
    [SerializeField] private ScreenFlow flow;

    [Header("Look")]
    [SerializeField] private Color openInk = new Color(0.635f, 0.976f, 0.627f, 1f);
    [SerializeField] private Color doneInk = new Color(0.635f, 0.976f, 0.627f, 0.45f);
    [SerializeField] private Color lineInk = new Color(0.635f, 0.976f, 0.627f, 0.35f);
    [Tooltip("The open dot grows and shrinks this often, seconds.")]
    [SerializeField] private float pulseSeconds = 1.6f;
    [Tooltip("How much the open dot grows, as a share of its size.")]
    [SerializeField] private float pulseSize = 0.25f;
    [Tooltip("With no slot picked (play mode started on this scene), this scene opens instead: the map never touches a save by itself.")]
    [SerializeField] private string slotsScene = "SaveSlots";
    [Tooltip("A new dot and its line fade in over this many seconds.")]
    [SerializeField] private float fadeInSeconds = 1.5f;

    private readonly List<Graphic> fresh = new List<Graphic>(); // the new dot, its name and its line
    private RectTransform openDot;
    private Vector2 dotSize;
    private int picked = -1;

    private void Start()
    {
        if (!SaveGame.InCampaign) { UnityEngine.SceneManagement.SceneManager.LoadScene(slotsScene); return; }
        var save = SaveGame.Current;
        int open = 0;
        while (open < missions.Length && save.done.Contains(missions[open].level)) open++;
        int shown = Mathf.Min(open + 1, missions.Length);
        bool isNew = save.dotsShown < shown; // the newest dot shows for the first time
        save.dotsShown = shown;
        SaveGame.Save();

        for (int i = 0; i < shown; i++)
        {
            bool newest = isNew && i == shown - 1, done = i < open;
            var ink = done ? doneInk : openInk;
            if (i > 0)
            {
                var line = Instantiate(lineTemplate, map);
                Vector2 a = missions[i - 1].at, b = missions[i].at;
                line.rectTransform.anchoredPosition = (a + b) / 2f;
                line.rectTransform.sizeDelta = new Vector2((b - a).magnitude, line.rectTransform.sizeDelta.y);
                line.rectTransform.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
                line.color = lineInk;
                line.gameObject.SetActive(true);
                line.transform.SetAsFirstSibling(); // under the dots
                if (newest) fresh.Add(line);
            }
            var dot = Instantiate(dotTemplate, map);
            ((RectTransform)dot.transform).anchoredPosition = missions[i].at;
            dot.image.color = ink;
            var name = dot.GetComponentInChildren<TMP_Text>(true);
            name.text = (i + 1) + "  " + missions[i].title + (done ? "  ✓" : "");
            name.color = ink;
            int index = i;
            dot.onClick.AddListener(() => Pick(index));
            dot.gameObject.SetActive(true);
            if (!done) { openDot = dot.image.rectTransform; dotSize = openDot.sizeDelta; } // its picture grows; its name stays
            if (newest) { fresh.Add(dot.image); fresh.Add(name); }
        }
        foreach (var g in fresh) g.canvasRenderer.SetAlpha(0f);
        box.SetActive(false);
    }

    private void Update()
    {
        float t = Time.timeSinceLevelLoad;
        if (openDot) openDot.sizeDelta = dotSize * (1f + pulseSize * Mathf.Sin(t * Mathf.PI * 2f / pulseSeconds));
        float a = Mathf.Clamp01(t / Mathf.Max(0.01f, fadeInSeconds));
        foreach (var g in fresh) g.canvasRenderer.SetAlpha(a);
    }

    /// <summary>A dot: show its mission in the box.</summary>
    public void Pick(int index)
    {
        picked = index;
        boxTitle.text = (index + 1) + "  " + missions[index].title;
        boxBrief.text = missions[index].brief;
        box.SetActive(true);
    }

    /// <summary>The box's LAUNCH. Safe to call from a Button's onClick.</summary>
    public void Launch()
    {
        if (picked >= 0) flow.Play(missions[picked].level);
    }
}
