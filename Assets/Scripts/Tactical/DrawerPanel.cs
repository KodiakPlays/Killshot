using System.Collections;
using UnityEngine;

/// <summary>
/// A panel that pulls out from a rail rather than popping into place.
/// Handoff section 5: 0.42s, cubic-bezier(.2,.85,.25,1).
/// </summary>
public class DrawerPanel : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private float slideSeconds = 0.42f;
    [Tooltip("How far off-screen the panel sits when shut. Positive slides right.")]
    [SerializeField] private float hiddenOffsetX = 376f;
    [SerializeField] private bool openOnStart = false;

    private float shownX;
    private Coroutine running;

    public bool IsOpen { get; private set; }

    public RectTransform Panel { get { return panel; } set { panel = value; } }
    public float SlideSeconds { get { return slideSeconds; } set { slideSeconds = value; } }
    public float HiddenOffsetX { get { return hiddenOffsetX; } set { hiddenOffsetX = value; } }

    private void Awake()
    {
        if (panel == null) return;
        shownX = panel.anchoredPosition.x;
        IsOpen = openOnStart;
        Place(IsOpen ? 1f : 0f);
    }

    public void Toggle() { Set(!IsOpen); }
    public void Open()   { Set(true); }
    public void Close()  { Set(false); }

    public void Set(bool open)
    {
        if (panel == null || IsOpen == open) return;
        IsOpen = open;
        if (!isActiveAndEnabled || !Application.isPlaying) { Place(open ? 1f : 0f); return; }
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Slide(open));
    }

    private IEnumerator Slide(bool open)
    {
        float from = Progress();
        float to = open ? 1f : 0f;
        float t = 0f;
        float dur = Mathf.Max(0.01f, slideSeconds) * Mathf.Abs(to - from);

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;               // the board can be paused
            Place(Mathf.LerpUnclamped(from, to, Ease(Mathf.Clamp01(t / dur))));
            yield return null;
        }
        Place(to);
        running = null;
    }

    private float Progress()
    {
        if (Mathf.Approximately(hiddenOffsetX, 0f)) return IsOpen ? 1f : 0f;
        return 1f - (panel.anchoredPosition.x - shownX) / hiddenOffsetX;
    }

    private void Place(float p)
    {
        var pos = panel.anchoredPosition;
        pos.x = shownX + hiddenOffsetX * (1f - p);
        panel.anchoredPosition = pos;

        var cg = panel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = Mathf.Clamp01(p * 1.4f);        // opacity settles before the slide does
            cg.blocksRaycasts = p > 0.5f;
            cg.interactable = p > 0.5f;
        }
    }

    /// <summary>cubic-bezier(.2,.85,.25,1) from the handoff, solved for y at x.</summary>
    private static float Ease(float x)
    {
        const float x1 = 0.2f, y1 = 0.85f, x2 = 0.25f, y2 = 1f;
        float t = x;
        for (int i = 0; i < 6; i++)                     // Newton, plenty for a UI curve
        {
            float bx = Bezier(t, x1, x2) - x;
            float d = BezierSlope(t, x1, x2);
            if (Mathf.Abs(d) < 1e-5f) break;
            t -= bx / d;
            t = Mathf.Clamp01(t);
        }
        return Bezier(t, y1, y2);
    }

    private static float Bezier(float t, float a, float b)
    {
        float u = 1f - t;
        return 3f * u * u * t * a + 3f * u * t * t * b + t * t * t;
    }

    private static float BezierSlope(float t, float a, float b)
    {
        float u = 1f - t;
        return 3f * u * u * a + 6f * u * t * (b - a) + 3f * t * t * (1f - b);
    }
}
