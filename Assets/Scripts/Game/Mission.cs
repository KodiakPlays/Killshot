using UnityEngine;

/// <summary>What finishes one step of a flight drill.</summary>
public enum DrillCheck { EngHeld, EngAt, Reach, SpeedAt, SlideLeft, SlideRight, ZoomOutTo, ZoomInTo, DradisOn, StopAt }

/// <summary>One step of a flight drill (owner 2026-11-01): its briefing note, what finishes it, and its ring on the plot.</summary>
[System.Serializable]
public class DrillStep
{
    [Tooltip("ENG HELD: ENG on STANDBY with 1 to Amount units. ENG AT: ENG at Amount or more. REACH: inside the ring. SPEED AT: speed at Amount or more. "
           + "SLIDE LEFT, SLIDE RIGHT: Q or E held for Amount game seconds. ZOOM OUT TO, ZOOM IN TO: the plot's zoom at Amount. DRADIS ON: DRADIS on, with SEN power. "
           + "STOP AT: inside the ring, speed at Amount or less.")]
    public DrillCheck check;
    [Tooltip("See the check above.")]
    public float amount;
    [Tooltip("The ring on the plot, units (x east, y north).")]
    public Vector2 ringAt;
    [Tooltip("The ring's size, units. 0: this step has no ring.")]
    public float ringUnits;
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
           + "FIND BY SIGNAL: decode one of its radio messages, then hear it on DRADIS. FLIGHT: finish the drill's steps, in order.")]
    public MissionGoal goal = MissionGoal.Destroy;
    [Tooltip("What the help bubble on the board says at the start: what to do in this level (owner 2026-10-29: a placeholder). Empty: no bubble.")]
    [TextArea(4, 16)] public string help;
    [Tooltip("FLIGHT only: the drill, in order. The help text above is not used.")]
    public DrillStep[] drill;

    // Start, not Awake: the board is set up by now, whether this level was open with the board or loaded on top of it
    private void Start()
    {
        var quest = FindAnyObjectByType<QuestSystem>();
        if (!quest) return;
        quest.Goal = goal;
        if (goal == MissionGoal.Flight) quest.StartDrill(drill); else quest.ShowHelp(help);
    }
}
