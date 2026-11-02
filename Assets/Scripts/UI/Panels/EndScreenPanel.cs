using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The end screen (Step 85). When the mission is won or lost it covers the board and says how it ended:
/// MISSION COMPLETE or SHIP LOST, why, the target, our ship and the time taken. RESTART MISSION plays this
/// level again; MISSION SELECT goes back to the mission list (owner 2026-11-02). It shows the QuestSystem.
/// It is the pause menu too (owner 2026-11-02): Escape in a level stops time and shows it titled PAUSED, with
/// RESUME and QUIT GAME added and the rows hidden. Escape or RESUME closes it and time goes on as it was.
/// </summary>
public class EndScreenPanel : Panel
{
    [Tooltip("The screen itself. Hidden while the mission runs.")]
    [SerializeField] private GameObject screen;
    [SerializeField] private TMP_Text title, reason;
    [Tooltip("The right-hand column: the target, our ship, the time. The labels beside it are fixed text.")]
    [SerializeField] private TMP_Text values;
    [Tooltip("Real seconds from the end to the screen, so the last hit is seen first.")]
    [Tune("Board & time")] [SerializeField] private float delaySeconds = 2f;
    [Tooltip("MISSION SELECT loads this scene: the mission list.")]
    [SerializeField] private string menuScene = "01_Briefing";
    [Tooltip("Pause menu only: RESUME and QUIT GAME.")]
    [SerializeField] private GameObject resumeButton, quitButton;
    [Tooltip("The target, our ship and the time, with their labels. Hidden on the pause menu.")]
    [SerializeField] private GameObject rows;
    private QuestSystem quest;
    private bool pausedBefore;

    public bool Showing => screen && screen.activeSelf;
    /// <summary>The pause menu is up. The board's keys wait meanwhile.</summary>
    public bool MenuOpen { get; private set; }

    public override void Refresh()
    {
        if (!quest) quest = FindAnyObjectByType<QuestSystem>();
        bool on = MenuOpen || (quest && quest.Over && Time.unscaledTime >= quest.EndedAt + delaySeconds);
        if (screen && screen.activeSelf != on) screen.SetActive(on);
        if (!on) return;
        if (rows) rows.SetActive(!MenuOpen);
        if (resumeButton) resumeButton.SetActive(MenuOpen);
        if (quitButton) quitButton.SetActive(MenuOpen);
        if (MenuOpen)
        {
            title.text = "PAUSED";
            Paint(title, Palette.data);
            reason.text = "";
            return;
        }
        var s = State;
        bool won = quest.Stage == QuestStage.MissionComplete;
        var t = s.TrackOf(quest.TargetKey);
        var enemy = s.ShipOf != null ? s.ShipOf(quest.TargetKey) : null;
        string name = t != null ? t.name : quest.TargetKey.ToUpperInvariant();
        string fate = enemy == null ? "" : enemy.destroyed ? "DESTROYED" : enemy.captured ? "CAPTURED" : "AFLOAT";
        bool shipLost = quest.FailReason != MissionFailReason.Scanned && quest.FailReason != MissionFailReason.PrizeDestroyed;
        title.text = won ? "MISSION COMPLETE" : shipLost ? "SHIP LOST" : "MISSION FAILED";
        Paint(title, won ? Palette.nominal : Palette.alarm);
        bool flight = quest.Goal == MissionGoal.Flight;
        reason.text = won && flight ? "FLIGHT DRILL PASSED" : won ? name + " " + (quest.Goal == MissionGoal.HitUnseen ? "HIT · NEVER SCANNED" : quest.Goal == MissionGoal.FindBySignal ? "FOUND BY SIGNAL" : quest.Goal == MissionGoal.JumpPoint ? "EVADED" : fate) + (quest.ExitOpen ? " · JUMPED OUT" : "")
                    : quest.FailReason == MissionFailReason.Desertion ? "LEFT THE BATTLE SPACE"
                    : quest.FailReason == MissionFailReason.Scanned ? name + " SCANNED OR LOCKED · THIS LEVEL ALLOWS NEITHER"
                    : quest.FailReason == MissionFailReason.PrizeDestroyed ? name + " DESTROYED · IT WAS TO BE CAPTURED"
                    : "REACTOR DESTROYED";
        float armour = 0f, most = 0f;
        for (int i = 0; i < s.damage.SideArmour.Length; i++) { armour += s.damage.SideArmour[i]; most += s.damage.stats.sideArmourMax[i]; }
        int secs = Mathf.FloorToInt(Controls.MissionSeconds - quest.StartedAt);
        values.text = (flight ? "NONE" : name + " · " + fate) + "\n"
                    + (s.destroyed ? "LOST" : Mathf.RoundToInt(100f * armour / Mathf.Max(1f, most)) + "% ARMOUR") + "\n"
                    + (secs / 3600).ToString("00") + ":" + (secs / 60 % 60).ToString("00") + ":" + (secs % 60).ToString("00");
    }

    public override bool Click(string action, string arg)
    {
        if (action == "end.restart") { SceneManager.LoadScene(gameObject.scene.name); return true; } // the board loads its level again
        if (action == "end.menu") { SceneManager.LoadScene(menuScene); return true; }
        if (action == "end.pause") // Escape, and RESUME
        {
            if (quest && quest.Over) return true; // the end screen has its own buttons
            MenuOpen = !MenuOpen;
            if (MenuOpen) { pausedBefore = Controls.Paused; Controls.Pause(); }
            else Controls.Pause(pausedBefore);
            return true;
        }
        if (action == "end.quit")
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Application.Quit does nothing in the editor
#else
            Application.Quit();
#endif
            return true;
        }
        return false;
    }
}
