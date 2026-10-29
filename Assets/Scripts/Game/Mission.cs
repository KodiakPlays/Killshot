using UnityEngine;

/// <summary>
/// A level's goal (owner 2026-10-29, FTUE_PLAN §0): one in each training level, added from Starwolf → Level.
/// The board's QuestSystem plays it. A level without one plays Wolf Hunt.
/// </summary>
public class Mission : MonoBehaviour
{
    [Tooltip("WOLF HUNT: find the Garuda, destroy or capture it, then reach the jump point. DESTROY: destroy or capture it. "
           + "HIT UNSEEN: hit it once without scanning it; a scan loses. CAPTURE: take it with the marines; destroying it loses.")]
    public MissionGoal goal = MissionGoal.Destroy;
    [Tooltip("What the help bubble on the board says at the start: what to do in this level (owner 2026-10-29: a placeholder). Empty: no bubble.")]
    [TextArea(4, 16)] public string help;

    // Start, not Awake: the board is set up by now, whether this level was open with the board or loaded on top of it
    private void Start()
    {
        var quest = FindAnyObjectByType<QuestSystem>();
        if (quest) { quest.Goal = goal; quest.ShowHelp(help); }
    }
}
