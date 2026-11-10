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

    [Header("Escape menu (owner 2026-11-09)")]
    [Tooltip("Optional. Escape opens and closes this menu: RESUME, MAIN MENU, QUIT GAME (the save slots, the sector map and "
           + "the ship rooms). Empty: Escape quits the game at once, as on the intro.")]
    [SerializeField] private GameObject escapeMenu;
    [Tooltip("MAIN MENU loads this scene: the title.")]
    [SerializeField] private string mainMenuScene = "00_Start";

    private bool going;
    private int card;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!escapeMenu) Application.Quit(); // owner 2026-10-29: quit at any time (does nothing in the editor)
            else if (!going && !(showFirst && showFirst.activeSelf)) ShowMenu(!escapeMenu.activeSelf); // not once a level is loading
        }
        if (going || (escapeMenu && escapeMenu.activeSelf)) return;
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
        if (Time.timeSinceLevelLoad < 0.5f) return; // owner 2026-11-02: COLD RUN's row is under NEW GAME; the click that opened this list must not pick a level
        TacticalWorld.chosenLevel = level;
        if (showFirst) showFirst.SetActive(true);
        else Advance();
    }

    /// <summary>Load this scene at once (owner 2026-11-08: the title's NEW GAME opens SAVE SLOTS, its second row the level
    /// list). Safe to call from a Button's or the title menu's event.</summary>
    public void Go(string scene)
    {
        if (going) return;
        going = true;
        SceneManager.LoadScene(scene);
    }

    /// <summary>The Escape menu's RESUME: close it.</summary>
    public void Resume() => ShowMenu(false);

    // Open: RESUME is picked, so Enter or Space can't press a button behind the menu. Closed: nothing is picked.
    void ShowMenu(bool open)
    {
        if (!escapeMenu) return;
        escapeMenu.SetActive(open);
        var events = UnityEngine.EventSystems.EventSystem.current;
        var first = open ? escapeMenu.GetComponentInChildren<UnityEngine.UI.Button>() : null;
        if (events) events.SetSelectedGameObject(first ? first.gameObject : null);
    }

    /// <summary>The Escape menu's MAIN MENU: back to the title. The save stays as it was last saved.</summary>
    public void MainMenu()
    {
        if (going) return; // a level is already loading: it keeps the campaign
        SaveGame.Leave();
        Go(mainMenuScene);
    }

    /// <summary>The Escape menu's QUIT GAME. In the editor it stops play mode.</summary>
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Application.Quit does nothing in the editor
#else
        Application.Quit();
#endif
    }

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
