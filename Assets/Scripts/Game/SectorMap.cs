using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The SECTOR MAP (owner 2026-11-08; Codex's console look, Step 172): the campaign's missions as dots on a star chart.
/// The left screen shows the picked mission's number, name and text. At the start one dot shows. When the map opens,
/// the newest dot is already picked. Click a dot to pick it. The MISSION ARCHIVE box has BRIEFING, DEBRIEF (a finished
/// mission only) and LAUNCH, which reads REPLAY MISSION on a finished mission. Win the mission and, back on the map,
/// the next dot fades in with a line from the last. A finished dot shows the tick picture and can be played again.
/// The missions are in the Campaign file (Assets/Campaign/Campaign.asset), in order: each opens when the one before it is won.
/// </summary>
public class SectorMap : MonoBehaviour
{
    [Tooltip("The campaign file: its missions are the dots, in order.")]
    [SerializeField] private Campaign campaign;
    private Campaign.Mission[] missions => campaign.missions;

    [Header("Parts")]
    [Tooltip("The dots and lines are made in here.")]
    [SerializeField] private RectTransform map;
    [Tooltip("Switched off; copied for each dot. Its first text is the mission's name.")]
    [SerializeField] private Button dotTemplate;
    [Tooltip("Switched off; copied for each line between two dots.")]
    [SerializeField] private Image lineTemplate;
    [Tooltip("The MISSION ARCHIVE box and the left screen's mission text. Switched off in the scene; it opens on the newest dot when the map opens.")]
    [SerializeField] private GameObject box;
    [Tooltip("On the left screen: the mission's number and name (Box Title), and its text (Box Brief).")]
    [SerializeField] private TMP_Text boxTitle, boxBrief;
    [Tooltip("The LAUNCH button's text. It reads REPLAY MISSION on a finished mission.")]
    [SerializeField] private TMP_Text launchLabel;
    [Tooltip("DEBRIEF. It works only on a finished mission.")]
    [SerializeField] private Button debriefButton;
    [Tooltip("LAUNCH: INITIALISING..., then the board with the mission's level.")]
    [SerializeField] private ScreenFlow flow;

    [Tooltip("The tick picture on a finished mission's dot. Empty: a dimmer round dot with a ✓ after its name.")]
    [SerializeField] private Sprite completedNode;

    [Header("Look")]
    [SerializeField] private Color openInk = new Color(0.635f, 0.976f, 0.627f, 1f);
    [SerializeField] private Color doneInk = new Color(0.635f, 0.976f, 0.627f, 0.45f);
    [SerializeField] private Color lineInk = new Color(0.635f, 0.976f, 0.627f, 0.35f);
    [Tooltip("With no slot picked (play mode started on this scene), this scene opens instead: the map never touches a save by itself.")]
    [SerializeField] private string slotsScene = "SaveSlots";
    [Tooltip("A new dot and its line fade in over this many seconds.")]
    [SerializeField] private float fadeInSeconds = 1.5f;

    private readonly List<Graphic> fresh = new List<Graphic>(); // the new dot, its name and its line
    private int picked = -1;

    private void Start()
    {
        SaveGame.campaign = campaign; // the end screen reads it to know the last mission
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
            if (done && completedNode)
            {
                dot.image.sprite = completedNode;
                dot.image.color = Color.white;
            }
            else dot.image.color = ink;
            var name = dot.GetComponentInChildren<TMP_Text>(true);
            name.text = (i + 1) + "  " + missions[i].title + (done && !completedNode ? "  ✓" : "");
            name.color = ink;
            int index = i;
            dot.onClick.AddListener(() => Pick(index));
            dot.gameObject.SetActive(true);
            if (newest) { fresh.Add(dot.image); fresh.Add(name); }
        }
        foreach (var g in fresh) g.canvasRenderer.SetAlpha(0f);
        if (shown > 0) Pick(shown - 1);
        else box.SetActive(false);
    }

    private void Update()
    {
        float t = Time.timeSinceLevelLoad;
        float a = Mathf.Clamp01(t / Mathf.Max(0.01f, fadeInSeconds));
        foreach (var g in fresh) g.canvasRenderer.SetAlpha(a);
    }

    /// <summary>A dot: show its mission in the box.</summary>
    public void Pick(int index)
    {
        picked = index;
        boxTitle.text = (index + 1) + "  " + missions[index].title;
        ShowBriefing();
        if (debriefButton) debriefButton.interactable = SaveGame.Current.done.Contains(missions[index].level);
        if (launchLabel) launchLabel.text = SaveGame.Current.done.Contains(missions[index].level) ? "REPLAY MISSION" : "LAUNCH";
        box.SetActive(true);
    }

    /// <summary>The box's BRIEFING: the mission's text on the left screen.</summary>
    public void ShowBriefing()
    {
        if (picked >= 0) boxBrief.text = missions[picked].brief;
    }

    /// <summary>The box's DEBRIEF: MISSION COMPLETE above the mission's text. Does nothing on an unfinished mission.</summary>
    public void ShowDebrief()
    {
        if (picked >= 0 && SaveGame.Current.done.Contains(missions[picked].level))
            boxBrief.text = "MISSION COMPLETE\n\n" + missions[picked].brief;
    }

    /// <summary>The box's LAUNCH. Safe to call from a Button's onClick.</summary>
    public void Launch()
    {
        if (picked >= 0) flow.Play(missions[picked].level);
    }
}
