// =================================================================================================
// EndScreenPanel.cs
//
// What it does:
// The screen that covers the battle board when a mission ends. It says MISSION COMPLETE, SHIP LOST
// or MISSION FAILED, why, and how the fight went. Escape in a level opens it as the pause menu.
// It is on the End Screen prefab.
//
// How it works:
// - BoardManager redraws it after every click and every 0.42 seconds. It finds QuestSystem in the
//   scene and asks it if the mission is over.
// - The screen shows 2 real seconds after the end. The delay is in the Tuning window, under
//   "Board & time".
// - The title is green on a win and red on a loss. MISSION FAILED means the player scanned or
//   locked the target in a level that allows neither. Destroying a ship meant for capture is
//   MISSION FAILED too. Any other loss is SHIP LOST.
// - The reason line names the target and what happened to it, or the cause of the loss: LEFT THE
//   BATTLE SPACE, REACTOR DESTROYED and so on. A won flight mission says CHECKLIST COMPLETE.
// - The right column shows the target and its fate. It shows our ship's armour left, as a percent
//   of its full side armour, or LOST. Last comes the time taken, in hours, minutes and seconds.
// - RESTART MISSION loads the same scene again. MISSION SELECT tells SaveGame to keep the damage
//   from a won campaign mission. Then it loads the Ship scene in the campaign, or 00_Start outside
//   it. After the campaign's last mission is won, it loads the EndCard scene instead. With no
//   closing line written, the end card goes straight to the title: just STARWOLF (owner 2026-11-11).
// - Escape comes in from BoardManager. It stops time and shows the screen titled PAUSED. This has
//   RESUME and QUIT GAME, and no rows. Escape or RESUME closes it and puts time back to
//   running or paused, as before. Escape does nothing once the mission is over.
// - QUIT GAME stops play mode in the editor and closes the game in a build.
// - While the pause menu is open, BoardManager ignores the board's keys. The playtest runner
//   reads this screen's texts.
//
// Technical:
// - Data: MenuOpen is true while the pause menu is up. Showing is true while the screen is on.
// - BoardManager calls Refresh and Click. The actions are "end.restart", "end.menu", "end.pause"
//   and "end.quit". BoardManager sends "end.pause" on Escape.
// - BoardManager reads MenuOpen to stop the board's keys. PlaytestRunner reads Showing.
// - It finds QuestSystem once with FindAnyObjectByType. It reads Over, EndedAt, Stage, Goal,
//   FailReason, TargetKey, StartedAt and ExitOpen.
// - It reads ShipState.TrackOf, ShipOf and destroyed. Armour is DamageSystem.SideArmour against
//   sideArmourMax in the damage part of the ship stats file.
// - The pause menu calls BoardManager.Pause. On close it puts back the pause state from before.
// - MISSION SELECT calls SaveGame.KeepWonDamage. Then SaveGame.InCampaign and FinishedWith pick
//   the scene.
// - Numbers: the delay is a [Tune] field under "Board & time". The three scene names are in the
//   Inspector on the End Screen prefab.
// - Trap: PlaytestRunner reads the private title, reason and values fields by name. Renaming them
//   breaks the playtest.
// - Trap: the end card loads only if the EndCard scene is in the build. The delay counts real
//   seconds (Time.unscaledTime).
// =================================================================================================

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenPanel : Panel
{
    [Tooltip("The screen itself. Hidden while the mission runs.")]
    [SerializeField] private GameObject screen;
    [SerializeField] private TMP_Text title, reason;
    [Tooltip("The right-hand column: the target, our ship, the time. The labels beside it are fixed text.")]
    [SerializeField] private TMP_Text values;
    [Tooltip("Real seconds from the end to the screen, so the last hit is seen first.")]
    [Tune("Board & time")] [SerializeField] private float delaySeconds = 2f;
    [Tooltip("MISSION SELECT loads this scene outside the campaign: the title (owner 2026-11-09: SKIRMISH; the level list is gone).")]
    [SerializeField] private string menuScene = "00_Start";
    [Tooltip("Owner 2026-11-08: in the campaign, MISSION SELECT loads this scene instead: the ship's rooms, then the sector map.")]
    [SerializeField] private string campaignScene = "Ship";
    [Tooltip("Owner 2026-11-09: after the campaign's last mission is won, MISSION SELECT loads this scene instead: the end card, then the title.")]
    [SerializeField] private string endCardScene = "EndCard";
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
        reason.text = won && flight ? "CHECKLIST COMPLETE" : won ? name + " " + (quest.Goal == MissionGoal.HitUnseen ? "HIT · NEVER SCANNED" : quest.Goal == MissionGoal.FindBySignal ? "FOUND BY SIGNAL" : quest.Goal == MissionGoal.JumpPoint ? "EVADED" : fate) + (quest.ExitOpen ? " · JUMPED OUT" : "")
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
        if (action == "end.menu")
        {
            SaveGame.KeepWonDamage();
            bool finished = quest != null && quest.Stage == QuestStage.MissionComplete && SaveGame.FinishedWith(TacticalWorld.chosenLevel)
                         && Application.CanStreamedLevelBeLoaded(endCardScene); // no end card scene in the build: the rooms, as before
            SceneManager.LoadScene(!SaveGame.InCampaign ? menuScene : finished ? endCardScene : campaignScene);
            return true;
        }
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
