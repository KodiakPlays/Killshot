using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The end screen (Step 85). When the mission is won or lost it covers the board and says how it ended:
/// MISSION COMPLETE or SHIP LOST, why, the target, our ship and the time taken. RESTART MISSION plays this
/// level again; MAIN MENU goes back to the intro and the menu. It shows the QuestSystem.
/// </summary>
public class EndScreenPanel : HandoffPanel
{
    [Tooltip("The screen itself. Hidden while the mission runs.")]
    [SerializeField] private GameObject screen;
    [SerializeField] private TMP_Text title, reason;
    [Tooltip("The right-hand column: the target, our ship, the time. The labels beside it are fixed text.")]
    [SerializeField] private TMP_Text values;
    [Tooltip("Real seconds from the end to the screen, so the last hit is seen first.")]
    [Tune("Board & time")] [SerializeField] private float delaySeconds = 2f;
    [Tooltip("MAIN MENU loads this scene: the intro, then the menu.")]
    [SerializeField] private string menuScene = "00_Start";
    private QuestSystem quest;

    public bool Showing => screen && screen.activeSelf;

    public override void Refresh()
    {
        if (!quest) quest = FindAnyObjectByType<QuestSystem>();
        bool on = quest && quest.Over && Time.unscaledTime >= quest.EndedAt + delaySeconds;
        if (screen && screen.activeSelf != on) screen.SetActive(on);
        if (!on) return;
        var s = State;
        bool won = quest.Stage == QuestStage.MissionComplete;
        var t = s.TrackOf(quest.TargetKey);
        var enemy = s.ShipOf != null ? s.ShipOf(quest.TargetKey) : null;
        string name = t != null ? t.name : quest.TargetKey.ToUpperInvariant();
        string fate = enemy == null ? "" : enemy.destroyed ? "DESTROYED" : enemy.captured ? "CAPTURED" : "AFLOAT";
        title.text = won ? "MISSION COMPLETE" : "SHIP LOST";
        Paint(title, won ? Palette.nominal : Palette.alarm);
        reason.text = won ? name + " " + fate
                    : quest.FailReason == MissionFailReason.Desertion ? "LEFT THE BATTLE SPACE" : "REACTOR DESTROYED";
        float armour = 0f, most = 0f;
        for (int i = 0; i < s.damage.SideArmour.Length; i++) { armour += s.damage.SideArmour[i]; most += s.damage.stats.sideArmourMax[i]; }
        int secs = Mathf.FloorToInt(Controls.MissionSeconds - quest.StartedAt);
        values.text = name + " · " + fate + "\n"
                    + (s.destroyed ? "LOST" : Mathf.RoundToInt(100f * armour / Mathf.Max(1f, most)) + "% ARMOUR") + "\n"
                    + (secs / 3600).ToString("00") + ":" + (secs / 60 % 60).ToString("00") + ":" + (secs % 60).ToString("00");
    }

    public override bool Click(string action, string arg)
    {
        if (action == "end.restart") { SceneManager.LoadScene(gameObject.scene.name); return true; } // the board loads its level again
        if (action == "end.menu") { SceneManager.LoadScene(menuScene); return true; }
        return false;
    }
}
