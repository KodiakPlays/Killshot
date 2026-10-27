using UnityEngine;

/// <summary>
/// The mission: what wins it and what loses it. Soham Aradhye's quest system from the GDD ("Quests"), moved onto
/// the board's tick in Step 85: it reads the ships' own flags, in place of tags and deleted objects. No popups or
/// journal; the board's end screen (EndScreenPanel) shows how it ended.
///
/// Stages, in order: find the target (our sensors identify it), then destroy it.
/// Win (FTUE_PLAN §1, owner 2026-10-18): the mission ends at the kill. The target is destroyed, or our marines
/// take it. The GDD's escape to a jump point (ExitRegion) comes later.
/// Lose: our ship is destroyed (its reactor at 0), or it is lost at the battle space's edge (desertion). The GDD's
/// time limit and life-support timer are not on the board yet.
/// When it ends, the board pauses.
/// </summary>
public enum QuestStage
{
    NotStarted,
    LocateTarget,
    EliminateTarget,
    ExitRegion,
    MissionComplete,
    MissionFailed
}

public enum MissionFailReason
{
    None,
    Destroyed,
    Desertion,
    TimeExpired,
    LifeSupportFailed
}

public class QuestSystem : MonoBehaviour
{
    [SerializeField] private HandoffBoardControls board;
    [Tooltip("The ship to destroy, by its track key (t07 is the Garuda).")]
    [SerializeField] private string targetKey = "t07";

    public QuestStage Stage { get; private set; }
    public MissionFailReason FailReason { get; private set; }
    public string TargetKey => targetKey;
    public bool Over => Stage == QuestStage.MissionComplete || Stage == QuestStage.MissionFailed;
    /// <summary>The board's mission clock when the mission started, game seconds.</summary>
    public float StartedAt { get; private set; }
    /// <summary>Real time when it ended. The end screen waits a moment after it.</summary>
    public float EndedAt { get; private set; }

    private void Awake() { if (!board) board = FindAnyObjectByType<HandoffBoardControls>(); }
    private void OnEnable() { if (board) board.Ticked += Tick; }
    private void OnDisable() { if (board) board.Ticked -= Tick; }

    private void Start()
    {
        StartedAt = board.MissionSeconds;
        Stage = QuestStage.LocateTarget;
    }

    // Once per board tick, after the ships have moved, fired and taken their hits.
    void Tick()
    {
        if (Over) return;
        var us = board.State;
        var target = us.ShipOf != null ? us.ShipOf(targetKey) : null;
        if (us.destroyed) End(QuestStage.MissionFailed, us.deserted ? MissionFailReason.Desertion : MissionFailReason.Destroyed);
        else if (target != null && (target.destroyed || target.captured)) End(QuestStage.MissionComplete, MissionFailReason.None);
        else if (Stage == QuestStage.LocateTarget && us.sensors.identified.Contains(targetKey)) Stage = QuestStage.EliminateTarget;
    }

    void End(QuestStage stage, MissionFailReason reason)
    {
        Stage = stage;
        FailReason = reason;
        EndedAt = Time.unscaledTime;
        board.Pause();
        Debug.Log("[QuestSystem] " + (stage == QuestStage.MissionComplete ? "Mission complete" : "Mission failed: " + reason));
    }
}
