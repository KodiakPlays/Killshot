// =================================================================================================
// Mission.cs
//
// What it does:
// A level's goal. A Mission object is placed in a level scene, added from the Level window.
// It tells the board's QuestSystem what this level asks for. A level without one plays Wolf Hunt.
//
// How it works:
// - In the Inspector you pick the goal: Wolf Hunt, destroy, hit unseen, capture, find by signal,
//   flight drill or jump point. You set the help text, the drill steps and the jump point. You
//   set a weapons-off switch, a railgun-off switch and our battle group's route.
// - When the level starts it finds QuestSystem and gives it the goal.
// - For a jump point goal it passes the jump point's place to QuestSystem and hides the jump
//   point object. The plot draws the jump point instead.
// - It passes the weapons-off and railgun-off switches to OrdersSystem.
// - For a flight drill it starts the drill steps. For every other goal it shows the help text.
// - EnemyBrain reads the battle group's route. The group flies it from point to point.
// - The flight drill step is in this file too. Each step has a briefing note and a check that
//   ends it: a speed, a zoom, a ring to reach, a scan, a lock and so on. A step may name a ship
//   or station by its track key. A step may show a ring on the plot and may set a new map size.
//   A step may end the level's weapons-off when it starts. QuestSystem runs the steps in order.
// =================================================================================================

using UnityEngine;

/// <summary>What finishes one step of a flight drill.</summary>
public enum DrillCheck { EngHeld, EngAt, Reach, SpeedAt, SlideLeft, SlideRight, ZoomOutTo, ZoomInTo, DradisOn, StopAt, ActiveOn, Scanned,
                         GainAt, ConditionAt, Locked, Near, Breached, Captured, Wait } // owner 2026-11-10, MILK RUN: new ones go on the end, so saved levels keep theirs

/// <summary>One step of a flight drill (owner 2026-11-01): its briefing note, what finishes it, and its ring on the plot.</summary>
[System.Serializable]
public class DrillStep
{
    [Tooltip("ENG HELD: ENG on STANDBY with 1 to Amount units. ENG AT: ENG at Amount or more. REACH: inside the ring. SPEED AT: speed at Amount or more. "
           + "SLIDE LEFT, SLIDE RIGHT: Q or E held for Amount game seconds. ZOOM OUT TO, ZOOM IN TO: the plot's zoom at Amount. DRADIS ON: DRADIS on, with SEN power. "
           + "STOP AT: inside the ring, speed at Amount or less. ACTIVE ON: ACTIVE SCAN on, with DRADIS listening. "
           + "SCANNED: Amount ships or more scanned (named); with a Track, that one scanned. "
           + "GAIN AT: the DRADIS gain dial at Amount or more (SEN power must be at Amount to allow it). CONDITION AT: alert condition at Amount or higher (1: Condition 2, 2: Condition 1). "
           + "The rest need a Track. LOCKED: the targeting computer locked on it. NEAR: it is Amount units away or closer. "
           + "BREACHED: one side of its armour is gone. CAPTURED: our marines hold it. "
           + "WAIT: Amount game seconds after the step starts (time to read a last note before the mission ends).")]
    public DrillCheck check;
    [Tooltip("See the check above.")]
    public float amount;
    [Tooltip("Owner 2026-11-10: the ship or station this step is about, by its track key (on its WorldContact: s1 the station, p1 the corvette in MILK RUN). "
           + "Empty: none. During NEAR the alarm strip gives its bearing and range.")]
    public string track;
    [Tooltip("Owner 2026-11-10: on: when this step starts, the level's weapons-off ends. Weapons and flight ops then follow the alert condition as usual. "
           + "The railgun stays off if the Mission's Railgun Off is on.")]
    public bool weaponsFree;
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
    [Tooltip("Owner 2026-11-10 (MILK RUN): on: our railgun stays off for the whole level, even after a drill step ends the weapons-off. Torpedoes, PDCs and flight ops still work.")]
    public bool railgunOff;
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
        var orders = FindAnyObjectByType<BoardManager>().State.orders;
        orders.weaponsHold = weaponsOff;
        orders.railgunHold = railgunOff;
        if (goal == MissionGoal.Flight) quest.StartDrill(drill); else quest.ShowHelp(help);
    }
}
