using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Moves from one screen to the next. Used by the intro screens.
/// Either wire a Button's onClick to Advance(), or let it wait for Enter.
/// </summary>
public class ScreenFlow : MonoBehaviour
{
    [Header("Where to go")]
    [SerializeField] private string nextScene = "";

    [Header("How it advances")]
    [Tooltip("Tick this to advance on Enter. Leave off if a button calls Advance().")]
    [SerializeField] private bool advanceOnEnter = false;
    [Tooltip("Optional. Enter shows this first (the beta page's INITIALISING... line, owner 2026-10-24); it calls Advance() when it is done.")]
    [SerializeField] private GameObject showFirst;
    [Tooltip("Optional. Seconds after the scene opens before it moves on by itself, as Enter does. 0: it waits "
           + "(owner 2026-10-27: WOLF HUNT shows, then INITIALISING..., with no Enter press).")]
    [SerializeField] private float advanceAfterSeconds = 0f;
    [Tooltip("Optional. Title cards shown in turn, each for advanceAfterSeconds (or until Enter), before showFirst "
           + "(owner 2026-10-29: MISSION ONE, then WOLF HUNT). Switch the first on and the rest off in the scene.")]
    [SerializeField] private GameObject[] cards;

    private bool going;
    private int card;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit(); // owner 2026-10-29: quit at any time (does nothing in the editor)
        if (going) return;
        bool timeUp = advanceAfterSeconds > 0f && Time.timeSinceLevelLoad >= advanceAfterSeconds * (card + 1);
        bool enter = advanceOnEnter && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter));
        if (!timeUp && !enter) return;
        if (cards != null && card + 1 < cards.Length) { cards[card].SetActive(false); cards[++card].SetActive(true); return; }
        if (showFirst && !showFirst.activeSelf) showFirst.SetActive(true);
        else if (!showFirst) Advance();
    }

    /// <summary>The level list (owner 2026-10-29): play this level, a scene in Assets/Scenes/Levels by name. Shows
    /// showFirst, else loads the next scene at once. Safe to call from a Button's onClick.</summary>
    public void Play(string level)
    {
        if (going || (showFirst && showFirst.activeSelf)) return; // picked already
        TacticalWorld.chosenLevel = level;
        if (showFirst) showFirst.SetActive(true);
        else Advance();
    }

    /// <summary>The QUIT · ESC button (owner 2026-10-29): quits, as Escape does. Does nothing in the editor.</summary>
    public void Quit() { Application.Quit(); }

    /// <summary>Load the next scene. Safe to call from a Button's onClick.</summary>
    public void Advance()
    {
        if (going) return;
        if (string.IsNullOrEmpty(nextScene))
        {
            Debug.LogError("[ScreenFlow] No next scene set on " + name, this);
            return;
        }
        going = true;
        SceneManager.LoadScene(nextScene);
    }
}
