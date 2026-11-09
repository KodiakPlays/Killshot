using UnityEngine;

/// <summary>What finishes one step of a flight drill.</summary>
public enum DrillCheck { EngHeld, EngAt, Reach, SpeedAt, SlideLeft, SlideRight, ZoomOutTo, ZoomInTo, DradisOn, StopAt, ActiveOn, Scanned }

/// <summary>One step of a flight drill (owner 2026-11-01): its briefing note, what finishes it, and its ring on the plot.</summary>
[System.Serializable]
public class DrillStep
{
    [Tooltip("ENG HELD: ENG on STANDBY with 1 to Amount units. ENG AT: ENG at Amount or more. REACH: inside the ring. SPEED AT: speed at Amount or more. "
           + "SLIDE LEFT, SLIDE RIGHT: Q or E held for Amount game seconds. ZOOM OUT TO, ZOOM IN TO: the plot's zoom at Amount. DRADIS ON: DRADIS on, with SEN power. "
           + "STOP AT: inside the ring, speed at Amount or less. ACTIVE ON: ACTIVE SCAN on, with DRADIS listening. "
           + "SCANNED: Amount ships or more scanned (named).")]
    public DrillCheck check;
    [Tooltip("See the check above.")]
    public float amount;
    [Tooltip("The ring on the plot, units (x east, y north).")]
    public Vector2 ringAt;
    [Tooltip("The ring's size, units. 0: this step has no ring.")]
    public float ringUnits;
    [Tooltip("Owner 2026-11-09: when this step starts, the map grows to this size, units, square, centred on the grid's centre. 0: it stays as it is.")]
    public float mapUnits;
    [Tooltip("The briefing note for this step. It replaces the last note and opens by itself.")]
    [TextArea(3, 8)] public string note;
}

/// <summary>
/// A level's goal (owner 2026-10-29, FTUE_PLAN §0): one in each training level, added from Starwolf → Level.
/// The board's QuestSystem plays it. A level without one plays Wolf Hunt.
/// </summary>
public class Mission : MonoBehaviour
{
    [Tooltip("WOLF HUNT: find the Garuda, destroy or capture it, then reach the jump point. DESTROY: destroy or capture it. "
           + "HIT UNSEEN: hit it once without scanning it; a scan loses. CAPTURE: take it with the marines; destroying it loses. "
           + "FIND BY SIGNAL: decode one of its radio messages, then hear it on DRADIS. FLIGHT: finish the drill's steps, in order. "
           + "JUMP POINT: reach the jump point.")]
    public MissionGoal goal = MissionGoal.Destroy;
    [Tooltip("What the help bubble on the board says at the start: what to do in this level (owner 2026-10-29: a placeholder). Empty: no bubble.")]
    [TextArea(4, 16)] public string help;
    [Tooltip("FLIGHT only: the drill, in order. The help text above is not used.")]
    public DrillStep[] drill;
    [Tooltip("JUMP POINT only: the Jump Point prefab placed in this level (Assets/Prefabs/JumpPoint.prefab). It shows from the start.")]
    public Transform jumpPoint;
    [Tooltip("On: our ship has no guns, torpedoes, PDCs or flight ops in this level.")]
    public bool weaponsOff;
    [Tooltip("Owner 2026-11-09: our battle group's path, units (x east, y north), point to point. The group flies it from the start "
           + "at its own speed and waits at the last point; we follow them. Empty: it stays where it starts.")]
    public Vector2[] groupRoute;

    // Start, not Awake: the board is set up by now, whether this level was open with the board or loaded on top of it
    private void Start()
    {
        var quest = FindAnyObjectByType<QuestSystem>();
        if (!quest) return;
        quest.Goal = goal;
        if (goal == MissionGoal.JumpPoint && jumpPoint) { quest.PlaceJumpPoint(jumpPoint.position); jumpPoint.gameObject.SetActive(false); } // seen only in the Scene view, like the spawns: the plot draws the ring
        FindAnyObjectByType<BoardManager>().State.orders.weaponsHold = weaponsOff;
        if (goal == MissionGoal.Flight) quest.StartDrill(drill); else quest.ShowHelp(help);
    }
}
