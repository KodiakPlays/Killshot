using UnityEngine;
using UnityEngine.Serialization;

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
///
/// The training levels (owner 2026-10-29, FTUE_PLAN §0): a level's Mission sets another goal. Each is won or lost
/// the moment it is met, with no jump point.
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
    LifeSupportFailed,
    Scanned,
    PrizeDestroyed
}

/// <summary>What a level asks for (its Mission). WolfHunt: find the target, destroy or capture it, reach the jump
/// point. Destroy: destroy or capture it. HitUnseen: hit it once without scanning it; a scan loses. Capture: take it
/// with the marines; destroying it loses. Flight: finish a flight drill's steps, in order.</summary>
public enum MissionGoal { WolfHunt, Destroy, HitUnseen, Capture, FindBySignal, Flight, JumpPoint }

public class QuestSystem : MonoBehaviour
{
    [SerializeField] private BoardManager board;
    [Tooltip("The ship to destroy, by its track key (t07 is the Garuda).")]
    [SerializeField] private string targetKey = "t07";

    [Header("The exit (owner 2026-10-27)")]
    [Tooltip("At the kill or capture, a jump point appears this far in from the battle space's edge, on the side farthest from our ship, units.")]
    [FormerlySerializedAs("exitInsetKm")] [Tune("Mission")] [SerializeField] private float exitInsetUnits = 5000f;
    [Tooltip("Our ship within this distance of the jump point jumps out and wins, units.")]
    [FormerlySerializedAs("exitReachKm")] [Tune("Mission")] [SerializeField] private float exitReachUnits = 2000f;

    [Header("Reinforcements (owner 2026-10-27)")]
    [Tooltip("Game seconds from the Garuda's alarm (it has found us) to the first enemy ships.")]
    [Tune("Mission")] [SerializeField] private float reinforceSeconds = 180f;
    [Tooltip("Ships in the first wave.")]
    [Tune("Mission")] [SerializeField] private int firstWave = 2;
    [Tooltip("After the first wave, one more ship this often, game seconds, until we jump out.")]
    [Tune("Mission")] [SerializeField] private float reinforceEverySeconds = 120f;
    [Tooltip("They arrive this far in from a random edge of the battle space, units.")]
    [FormerlySerializedAs("arriveInsetKm")] [Tune("Mission")] [SerializeField] private float arriveInsetUnits = 2000f;
    [Tooltip("They are told where we are, off by up to this, units. Then they hunt by their own sensors.")]
    [FormerlySerializedAs("briefErrorKm")] [Tune("Mission")] [SerializeField] private float briefErrorUnits = 5000f;
    [Tooltip("The ship that arrives: a switched-off contact under World, copied for each arrival (a frigate).")]
    [SerializeField] private WorldContact reinforcement;

    public QuestStage Stage { get; private set; }
    public MissionFailReason FailReason { get; private set; }
    /// <summary>The level's goal, set by its Mission. A level without one plays Wolf Hunt.</summary>
    public MissionGoal Goal { get; set; } = MissionGoal.WolfHunt;
    public string TargetKey => targetKey;
    public bool Over => Stage == QuestStage.MissionComplete || Stage == QuestStage.MissionFailed;
    /// <summary>The jump point is open: the target is down. Where it is, world units.</summary>
    public bool ExitOpen => Ring != null || exitPlaced || Stage == QuestStage.ExitRegion || (Stage == QuestStage.MissionComplete && exitSet);
    public Vector2 ExitAt { get { return Ring != null ? Ring.ringAt : exitAt; } private set { exitAt = value; } }
    /// <summary>Our ship within this distance of the jump point wins, units.</summary>
    public float ExitReachUnits => Ring != null ? Ring.ringUnits : exitReachUnits;
    /// <summary>What the plot calls the ring.</summary>
    public string ExitName => Goal == MissionGoal.Flight ? "CHECKPOINT" : "JUMP POINT";
    /// <summary>The board's mission clock when the mission started, game seconds.</summary>
    public float StartedAt { get; private set; }
    /// <summary>Real time when it ended. The end screen waits a moment after it.</summary>
    public float EndedAt { get; private set; }

    /// <summary>Game time the Garuda raised the alarm (it found us), or -1. The countdown runs from it.</summary>
    public float AlarmAt { get; private set; } = -1f;
    /// <summary>Enemy ships that have arrived so far.</summary>
    public int Arrived { get; private set; }

    private bool exitSet, exitPlaced;
    private Vector2 exitAt;
    private float nextArrival;
    private TacticalWorld world;

    [Header("The help bubble (owner 2026-10-29: a placeholder that says what to do in each level)")]
    [Tooltip("The bubble on the board. Hidden until a level's Mission gives it text; a click on it hides it.")]
    [SerializeField] private GameObject helpBubble;
    [SerializeField] private TMPro.TMP_Text helpText;
    [Tooltip("The small icon that opens the bubble again (owner 2026-11-01). It stays on the board once the bubble has shown.")]
    [SerializeField] private GameObject helpIcon;

    /// <summary>A level's Mission shows what to do in it. Empty text: no bubble.</summary>
    public void ShowHelp(string text)
    {
        if (!helpBubble || !helpText) return;
        helpText.text = text;
        helpBubble.SetActive(false);
        if (helpIcon) helpIcon.SetActive(false);
        if (!string.IsNullOrEmpty(text)) StartCoroutine(HelpOnceGridFormed());
    }

    // Step 120: the bubble covers the plot, so it waits until the grid has formed.
    System.Collections.IEnumerator HelpOnceGridFormed()
    {
        var plot = FindAnyObjectByType<PlotPanel>();
        while (plot && !plot.GridFormed) yield return null;
        helpBubble.SetActive(true);
        if (helpIcon) helpIcon.SetActive(true);
    }

    /// <summary>The icon: opens the bubble, or closes it if it is open.</summary>
    public void ToggleHelp() { if (helpBubble) helpBubble.SetActive(!helpBubble.activeSelf); }

    private void Awake()
    {
        if (!board) board = FindAnyObjectByType<BoardManager>(); world = FindAnyObjectByType<TacticalWorld>();
        var close = helpBubble ? helpBubble.GetComponent<UnityEngine.UI.Button>() : null;
        if (close) close.onClick.AddListener(() => helpBubble.SetActive(false));
    }
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
        bool scanned = us.sensors.identified.Contains(targetKey);
        if (Stage == QuestStage.LocateTarget && scanned) Stage = QuestStage.EliminateTarget;
        if (Goal != MissionGoal.WolfHunt) { Train(us, target, scanned); return; }
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
            Point(d);
            if (first) board.Alert(); // the alert tone as the jump point opens, with its line already on the strip
            if (d.magnitude <= exitReachUnits) End(QuestStage.MissionComplete, MissionFailReason.None);
        }
    }

    /// <summary>A level's Mission puts the jump point on the map from the start (owner 2026-11-01).</summary>
    public void PlaceJumpPoint(Vector2 at) { ExitAt = at; exitPlaced = exitSet = true; }

    // the alarm strip's line to the ring on the plot
    void Point(Vector2 d)
    {
        board.missionAlarm = ExitName + " · BRG " + TacticalWorld.Bearing(d).ToString("000") + "° · "
                           + Mathf.RoundToInt(d.magnitude).ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " KM";
        board.missionCount = "!";
    }

    // The flight drill (owner 2026-11-01): one step at a time; each step's note replaces the last
    [Tooltip("In a flight drill the briefing box is this tall, board px.")]
    [Tune("Mission")] [SerializeField] private float drillNoteHeight = 230f;
    private DrillStep[] drill;
    private float slideHeld;
    /// <summary>The drill's step now, counted from 0.</summary>
    public int DrillAt { get; private set; }
    DrillStep Ring => drill != null && !Over && drill[DrillAt].ringUnits > 0f ? drill[DrillAt] : null;

    public void StartDrill(DrillStep[] steps)
    {
        drill = steps != null && steps.Length > 0 ? steps : null;
        DrillAt = 0;
        if (drill == null) return;
        // the notes are short and stay up while you fly: a lower box, on the same bottom edge
        var box = (RectTransform)helpBubble.transform;
        box.anchoredPosition += Vector2.down * (box.sizeDelta.y - drillNoteHeight);
        helpText.rectTransform.sizeDelta -= Vector2.up * (box.sizeDelta.y - drillNoteHeight);
        box.sizeDelta = new Vector2(box.sizeDelta.x, drillNoteHeight);
        ShowHelp(drill[0].note);
    }

    void Drill(ShipState us)
    {
        if (drill == null) return;
        var step = drill[DrillAt];
        var d = step.ringAt - new Vector2((float)us.nav.x, (float)us.nav.y);
        if (step.ringUnits > 0f) Point(d); else board.missionAlarm = board.missionCount = null;
        if (!Passed(step, us, d.magnitude)) return;
        if (DrillAt + 1 >= drill.Length) { End(QuestStage.MissionComplete, MissionFailReason.None); return; }
        slideHeld = 0f;
        ShowHelp(drill[++DrillAt].note);
        board.Chime();
    }

    bool Passed(DrillStep s, ShipState us, float toRing)
    {
        var eng = us.power.buses[0];
        switch (s.check)
        {
            case DrillCheck.EngHeld: return eng.fill > 0 && eng.fill <= s.amount && eng.mode == PowerSystem.BusMode.Standby;
            case DrillCheck.EngAt: return eng.fill >= s.amount;
            case DrillCheck.Reach: return toRing <= s.ringUnits;
            case DrillCheck.SpeedAt: return us.nav.SpeedUnits >= s.amount;
            case DrillCheck.SlideLeft:
            case DrillCheck.SlideRight:
                if (eng.fill > 0 && us.nav.slide == (s.check == DrillCheck.SlideLeft ? -1 : 1)) slideHeld += board.TickGameSeconds;
                return slideHeld >= s.amount;
            case DrillCheck.ZoomOutTo: return (float)board.ZoomExact <= s.amount;
            case DrillCheck.ZoomInTo: return (float)board.ZoomExact >= s.amount;
            case DrillCheck.DradisOn: return us.sensors.Listening;
            case DrillCheck.StopAt: return toRing <= s.ringUnits && us.nav.SpeedUnits <= s.amount;
        }
        return false;
    }

    // A training level's goal, met or lost at once (FTUE_PLAN §0)
    void Train(ShipState us, ShipState target, bool scanned)
    {
        if (Goal == MissionGoal.Flight) { Drill(us); return; }
        if (Goal == MissionGoal.FindBySignal) // owner 2026-10-29: comms tells you where to go; DRADIS finds it
        {
            var track = us.TrackOf(targetKey);
            if (us.comms.messagesRead > 0 && track != null && us.sensors.Heard(track)) End(QuestStage.MissionComplete, MissionFailReason.None);
            return;
        }
        if (Goal == MissionGoal.JumpPoint) // owner 2026-11-01: get past the enemy to the jump point
        {
            var d = ExitAt - new Vector2((float)us.nav.x, (float)us.nav.y);
            if (us.comms.Waiting(us.missionNow)) board.missionAlarm = board.missionCount = null; else Point(d); // INCOMING SIGNAL needs the strip
            if (d.magnitude <= exitReachUnits) End(QuestStage.MissionComplete, MissionFailReason.None);
            return;
        }
        if (Goal == MissionGoal.HitUnseen)
        {
            if (scanned || us.weapons.Locked) End(QuestStage.MissionFailed, MissionFailReason.Scanned); // owner 2026-11-01: a lock is as loud as a scan
            else if (us.weapons.lastShotHit) End(QuestStage.MissionComplete, MissionFailReason.None);
            return;
        }
        if (target == null) return;
        if (target.captured) End(QuestStage.MissionComplete, MissionFailReason.None);
        else if (target.destroyed && Goal == MissionGoal.Capture) End(QuestStage.MissionFailed, MissionFailReason.PrizeDestroyed);
        else if (target.destroyed) End(QuestStage.MissionComplete, MissionFailReason.None);
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
        float hw = (float)us.ArenaHalfW - arriveInsetUnits, hh = (float)us.ArenaHalfH - arriveInsetUnits;
        var ours = new Vector2((float)us.nav.x, (float)us.nav.y);
        Vector2 at = Vector2.zero;
        for (int tries = 0; tries < 8; tries++) // not on top of us: at least 8,000 units away if it can
        {
            float r = Random.Range(-1f, 1f);
            int edge = Random.Range(0, 4);
            at = edge == 0 ? new Vector2(r * hw, hh) : edge == 1 ? new Vector2(r * hw, -hh) : edge == 2 ? new Vector2(hw, r * hh) : new Vector2(-hw, r * hh);
            if ((at - ours).magnitude >= 8000f) break;
        }
        Arrived++;
        world.AddContact(reinforcement, "r" + Arrived, at, TacticalWorld.Bearing(-at), ours + Random.insideUnitCircle * briefErrorUnits, now);
    }

    // the middle of the battle space's edge farthest from our ship, inset
    Vector2 FarEdge(ShipState us)
    {
        float hw = (float)us.ArenaHalfW - exitInsetUnits, hh = (float)us.ArenaHalfH - exitInsetUnits;
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
