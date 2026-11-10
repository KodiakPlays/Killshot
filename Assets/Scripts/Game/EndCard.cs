// =================================================================================================
// EndCard.cs
//
// What it does:
// The end card. It shows after the campaign's last mission is won. The story's closing line
// types in on a black screen. Then the title loads, where STARWOLF flickers in.
//
// How it works:
// - It is on the EndCard scene. The end screen's MISSION SELECT loads that scene after the last
//   mission is won.
// - The line comes from the Campaign file, its Closing Line. An empty line skips to the title.
// - The letters appear one by one, at Letters Per Second. The whole line then stays up for Hold
//   Seconds. Both are on the Board tab of Starwolf > Tuning, with this scene open.
// - Any key or click shows the whole line at once. Another one goes on to the title.
// - It leaves the campaign first. The save already says CAMPAIGN COMPLETE.
// - It reads keys through Unity's old Input class.
//
// Technical:
// - Inspector fields: campaign (the Campaign file), line (the TMP_Text it types into), and
//   titleScene ("00_Start" by default).
// - Start calls SaveGame.Leave, copies campaign.closingLine into the text and hides every letter.
// - Update shows more letters through maxVisibleCharacters, at lettersPerSecond. It times with
//   Time.time.
// - GoOn loads titleScene. The going flag stops a second load.
// - EndScreenPanel loads this scene by name (its endCardScene, "EndCard"), after the last
//   mission is won. It does so only when the scene is in the build.
// - lettersPerSecond and holdSeconds are [Tune("End card")] numbers. They show in the Tuning
//   window only with this scene open.
// =================================================================================================

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndCard : MonoBehaviour
{
    [Tooltip("The Campaign file. Its Closing Line is the text shown.")]
    [SerializeField] private Campaign campaign;
    [Tooltip("The text the closing line types into.")]
    [SerializeField] private TMP_Text line;
    [Tooltip("Then this scene: the title.")]
    [SerializeField] private string titleScene = "00_Start";
    [Tooltip("How fast the closing line types in, in letters a second.")]
    [Tune("End card")] [SerializeField] private float lettersPerSecond = 30f;
    [Tooltip("Seconds the whole line stays up before the title.")]
    [Tune("End card")] [SerializeField] private float holdSeconds = 5f;

    private float started, fullAt = -1f;
    private bool going;

    private void Start()
    {
        SaveGame.Leave(); // the campaign is over: the save already reads CAMPAIGN COMPLETE
        line.text = campaign ? campaign.closingLine : "";
        line.maxVisibleCharacters = 0;
        started = Time.time;
        if (string.IsNullOrEmpty(line.text)) GoOn();
    }

    private void Update()
    {
        if (going) return;
        int length = line.text.Length;
        bool press = Input.anyKeyDown;
        if (fullAt < 0f)
        {
            int shown = Mathf.FloorToInt((Time.time - started) * lettersPerSecond);
            if (press || shown >= length) { line.maxVisibleCharacters = length; fullAt = Time.time; return; }
            line.maxVisibleCharacters = shown;
            return;
        }
        if (press || Time.time - fullAt >= holdSeconds) GoOn();
    }

    void GoOn()
    {
        going = true;
        SceneManager.LoadScene(titleScene);
    }
}
