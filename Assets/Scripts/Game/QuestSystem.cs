using UnityEngine;

/// <summary>
/// The mission: what wins it and what loses it. Soham Aradhye's quest system from the GDD ("Quests"), on
/// the board's tick since Step 85: it reads the ships' own flags. No popups or journal; the alarm strip says
/// what changed, and the end screen (EndScreenPanel) shows how it ended.
///
/// Wolf Hunt (owner 2026-10-27), in order:
/// - Find the target (our sensors identify it), then destroy or capture it.
/// - Then a jump point appears at a far edge of the battle space (the GDD's escape, spec Rule 15.2.a). Our ship
///   reaching it wins.
/// - When the Garuda finds us (its awareness reaches ALERT), the alarm says REINFORCEMENTS INBOUND and a
///   countdown starts (Step 99).
/// Lose: our ship destroyed (its reactor at 0), or lost at the battle space's edge. When it ends, the board pauses.
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
    [SerializeField] private BoardManager board;
    [Tooltip("The ship to destroy, by its track key (t07 is the Garuda).")]
    [SerializeField] private string targetKey = "t07";

    [Header("The exit (owner 2026-10-27)")]
    [Tooltip("At the kill or capture, a jump point appears this far in from the battle space's edge, on the side farthest from our ship, km.")]
    [Tune("Mission")] [SerializeField] private float exitInsetKm = 5000f;
    [Tooltip("Our ship within this distance of the jump point jumps out and wins, km.")]
    [Tune("Mission")] [SerializeField] private float exitReachKm = 2000f;

    [Header("Reinforcements (owner 2026-10-27)")]
    [Tooltip("Game seconds from the Garuda's alarm (it has found us) to the first enemy ships.")]
    [Tune("Mission")] [SerializeField] private float reinforceSeconds = 180f;
    [Tooltip("Ships in the first wave.")]
    [Tune("Mission")] [SerializeField] private int firstWave = 2;
    [Tooltip("After the first wave, one more ship this often, game seconds, until we jump out.")]
    [Tune("Mission")] [SerializeField] private float reinforceEverySeconds = 120f;
    [Tooltip("They arrive this far in from a random edge of the battle space, km.")]
    [Tune("Mission")] [SerializeField] private float arriveInsetKm = 2000f;
    [Tooltip("They are told where we are, off by up to this, km. Then they hunt by their own sensors.")]
    [Tune("Mission")] [SerializeField] private float briefErrorKm = 5000f;
    [Tooltip("The ship that arrives: a switched-off contact under World, copied for each arrival (a frigate).")]
    [SerializeField] private WorldContact reinforcement;

    public QuestStage Stage { get; private set; }
    public MissionFailReason FailReason { get; private set; }
    public string TargetKey => targetKey;
    public bool Over => Stage == QuestStage.MissionComplete || Stage == QuestStage.MissionFailed;
    /// <summary>The jump point is open: the target is down. Where it is, world km.</summary>
    public bool ExitOpen => Stage == QuestStage.ExitRegion || (Stage == QuestStage.MissionComplete && exitSet);
    public Vector2 ExitAt { get; private set; }
    /// <summary>Our ship within this distance of the jump point wins, km.</summary>
    public float ExitReachKm => exitReachKm;
    /// <summary>The board's mission clock when the mission started, game seconds.</summary>
    public float StartedAt { get; private set; }
    /// <summary>Real time when it ended. The end screen waits a moment after it.</summary>
    public float EndedAt { get; private set; }

    /// <summary>Game time the Garuda raised the alarm (it found us), or -1. The countdown runs from it.</summary>
    public float AlarmAt { get; private set; } = -1f;
    /// <summary>Enemy ships that have arrived so far.</summary>
    public int Arrived { get; private set; }

    private bool exitSet;
    private float nextArrival;
    private TacticalWorld world;

    private void Awake() { if (!board) board = FindAnyObjectByType<BoardManager>(); world = FindAnyObjectByType<TacticalWorld>(); }
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
        if (us.destroyed) { End(QuestStage.MissionFailed, us.deserted ? MissionFailReason.Desertion : MissionFailReason.Destroyed); return; }
        var target = us.ShipOf != null ? us.ShipOf(targetKey) : null;
        if (Stage == QuestStage.LocateTarget && us.sensors.identified.Contains(targetKey)) Stage = QuestStage.EliminateTarget;
        Reinforce(us);
        if (Stage < QuestStage.ExitRegion && target != null && (target.destroyed || target.captured))
        {
            Stage = QuestStage.ExitRegion;
            ExitAt = FarEdge(us);
            exitSet = true;
        }
        if (Stage == QuestStage.ExitRegion)
        {
            bool first = board.missionAlarm == null || !board.missionAlarm.StartsWith("JUMP POINT");
            var d = ExitAt - new Vector2((float)us.nav.x, (float)us.nav.y);
            board.missionAlarm = "JUMP POINT · BRG " + TacticalWorld.Bearing(d).ToString("000") + "° · "
                               + Mathf.RoundToInt(d.magnitude).ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " KM";
            board.missionCount = "!";
            if (first) board.Alert(); // the alert tone as the jump point opens, with its line already on the strip
            if (d.magnitude <= exitReachKm) End(QuestStage.MissionComplete, MissionFailReason.None);
        }
    }

    // The alarm: an enemy brain ALERT (it has found us) starts the countdown. At its end the first wave arrives,
    // then one more ship every so often. The strip shows the countdown, then how many have come.
    void Reinforce(ShipState us)
    {
        float now = board.MissionSeconds;
        if (AlarmAt < 0f)
        {
            if (world == null || !reinforcement || !world.AnyAlert) return;
            AlarmAt = now;
            nextArrival = now + reinforceSeconds;
            if (Stage != QuestStage.ExitRegion) { board.missionAlarm = "REINFORCEMENTS INBOUND"; board.missionCount = (Mathf.CeilToInt(reinforceSeconds) / 60) + ":" + (Mathf.CeilToInt(reinforceSeconds) % 60).ToString("00"); }
            board.Alert();
        }
        bool arriving = now >= nextArrival;
        if (arriving)
        {
            for (int i = 0, n = Arrived == 0 ? firstWave : 1; i < n; i++) Arrive(us, now);
            nextArrival = now + reinforceEverySeconds;
        }
        if (Stage != QuestStage.ExitRegion) // the jump point's line takes the strip
        {
            int left = Mathf.Max(0, Mathf.CeilToInt(nextArrival - now));
            board.missionAlarm = Arrived == 0 ? "REINFORCEMENTS INBOUND" : "ENEMY SHIPS IN THE SECTOR: " + Arrived;
            board.missionCount = (left / 60) + ":" + (left % 60).ToString("00");
        }
        if (arriving) board.Alert();
    }

    // one ship at a random point on a random edge, facing in, told roughly where we are
    void Arrive(ShipState us, float now)
    {
        float hw = (float)us.ArenaHalfW - arriveInsetKm, hh = (float)us.ArenaHalfH - arriveInsetKm;
        var ours = new Vector2((float)us.nav.x, (float)us.nav.y);
        Vector2 at = Vector2.zero;
        for (int tries = 0; tries < 8; tries++) // not on top of us: at least 25,000 km away if it can
        {
            float r = Random.Range(-1f, 1f);
            int edge = Random.Range(0, 4);
            at = edge == 0 ? new Vector2(r * hw, hh) : edge == 1 ? new Vector2(r * hw, -hh) : edge == 2 ? new Vector2(hw, r * hh) : new Vector2(-hw, r * hh);
            if ((at - ours).magnitude >= 25000f) break;
        }
        Arrived++;
        world.AddContact(reinforcement, "r" + Arrived, at, TacticalWorld.Bearing(-at), ours + Random.insideUnitCircle * briefErrorKm, now);
    }

    // the middle of the battle space's edge farthest from our ship, inset
    Vector2 FarEdge(ShipState us)
    {
        float hw = (float)us.ArenaHalfW - exitInsetKm, hh = (float)us.ArenaHalfH - exitInsetKm;
        var ours = new Vector2((float)us.nav.x, (float)us.nav.y);
        Vector2 best = Vector2.zero;
        foreach (var p in new[] { new Vector2(0f, hh), new Vector2(0f, -hh), new Vector2(hw, 0f), new Vector2(-hw, 0f) })
            if ((p - ours).sqrMagnitude > (best - ours).sqrMagnitude) best = p;
        return best;
    }

    void End(QuestStage stage, MissionFailReason reason)
    {
        Stage = stage;
        FailReason = reason;
        EndedAt = Time.unscaledTime;
        board.missionAlarm = board.missionCount = null;
        board.Pause();
        Debug.Log("[QuestSystem] " + (stage == QuestStage.MissionComplete ? "Mission complete" : "Mission failed: " + reason));
    }
}
