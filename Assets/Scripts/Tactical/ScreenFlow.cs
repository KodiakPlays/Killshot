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

    private bool going;

    private void Update()
    {
        if (!advanceOnEnter || going) return;
        if (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.KeypadEnter)) return;
        if (showFirst && !showFirst.activeSelf) showFirst.SetActive(true);
        else if (!showFirst) Advance();
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
