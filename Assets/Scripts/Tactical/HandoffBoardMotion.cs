using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Step 32. How the handoff board moves: the six drawers slide and fade in, the periscope drops,
/// the helm and orders station open with their latches, and the board's looping animations run.
/// Every timing and curve is from reference/UIStateSpec.md §9 and is tunable here. Nothing here
/// reads game state; Step 33 calls these methods from the board's controls.
/// Tweens run in play mode; in edit mode every change lands at once.
/// </summary>
public class HandoffBoardMotion : MonoBehaviour
{
    [System.Serializable]
    public class Drawer
    {
        public string name;
        public RectTransform group;
        public CanvasGroup fade;
    }

    [System.Serializable]
    public class Drift
    {
        public RectTransform[] targets;
        [Tooltip("Offset at the middle of the cycle, board px, y down (CSS translate).")]
        public Vector2 offset;
        public float seconds;
        [System.NonSerialized] public Vector2[] home;
    }

    [System.Serializable]
    public class Pulse
    {
        public Graphic[] targets;
        public float from = 0.38f, to = 0.62f, seconds = 4f;
        [System.NonSerialized] public float[] baseAlpha;
    }

    [Header("Drawers — one open at a time")]
    [SerializeField] private Drawer[] drawers;
    [Tune("Motion")]
    [SerializeField] private float drawerTravelPx = 376f;
    [Tune("Motion")]
    [SerializeField] private float drawerSlideSeconds = 0.42f;
    [SerializeField] private CssEase drawerSlideEase = new CssEase(0.2f, 0.85f, 0.25f, 1f);
    [Tune("Motion")]
    [SerializeField] private float drawerFadeSeconds = 0.3f;
    [SerializeField] private CssEase drawerFadeEase = new CssEase(0.25f, 0.1f, 0.25f, 1f);

    [Header("Periscope — the housing's clip box drops")]
    [SerializeField] private RectTransform periscopeClip;
    [SerializeField] private float scopeHousedPx = 44f;
    [SerializeField] private float scopeLoweredPx = 656f;
    [Tune("Motion")]
    [SerializeField] private float scopeSeconds = 0.5f;
    [SerializeField] private CssEase scopeEase = new CssEase(0.24f, 0.9f, 0.26f, 1f);
    [SerializeField] private GameObject scopeHousedOnly;
    [SerializeField] private GameObject scopeLoweredOnly;

    [Header("Helm — opens at once; the header latch slides")]
    [SerializeField] private GameObject helmClosed;
    [SerializeField] private GameObject helmOpen;
    [SerializeField] private RectTransform helmLatchClosed;
    [SerializeField] private RectTransform helmLatchOpen;
    [SerializeField] private float helmLatchTravelPx = 15f;
    [Tune("Motion")]
    [SerializeField] private float helmLatchSeconds = 0.3f;
    [SerializeField] private CssEase helmLatchEase = new CssEase(0.3f, 1.2f, 0.5f, 1f);

    [Header("Orders — opens at once; the header latch slides")]
    [SerializeField] private GameObject ordersClosed;
    [SerializeField] private GameObject ordersOpen;
    [SerializeField] private RectTransform ordersLatchClosed;
    [SerializeField] private RectTransform ordersLatchOpen;
    [SerializeField] private float ordersLatchTravelPx = 15f;
    [Tune("Motion")]
    [SerializeField] private float ordersLatchSeconds = 0.22f;
    [SerializeField] private CssEase ordersLatchEase = new CssEase(0.3f, 1.4f, 0.5f, 1f);

    [Header("Loops")]
    [Tooltip("bd-blink: '0%,48% opacity:1 · 52%,100% opacity:.2' with steps(1) — lit for the first 52%.")]
    [SerializeField] private Graphic[] blinkers;
    [Tune("Motion")]
    [SerializeField] private float blinkSeconds = 1.1f;
    [SerializeField, Range(0f, 1f)] private float blinkLitFraction = 0.52f;
    [SerializeField] private float blinkLowAlpha = 0.2f;
    [SerializeField] private Drift[] drifts;
    [SerializeField] private Pulse[] pulses;
    [SerializeField] private CssEase loopEase = new CssEase(0.42f, 0f, 0.58f, 1f); // ease-in-out

    public string OpenDrawerName { get; private set; }
    public bool ScopeDown { get; private set; }
    public bool HelmOpen { get; private set; }
    public bool OrdersOpen { get; private set; }
    public event System.Action Changed;

    class Tween { public float start, seconds; public CssEase ease; public System.Action<float> apply; public System.Action done; }
    readonly Dictionary<Object, Tween> tweens = new Dictionary<Object, Tween>();
    float[] blinkBase;

    private void Start()
    {
        foreach (var d in drawers) SetDrawer(d, 0f);
        SetScope(0f);
        Swap(helmClosed, helmOpen, false);
        Swap(ordersClosed, ordersOpen, false);
        if (scopeHousedOnly) scopeHousedOnly.SetActive(true);
        if (scopeLoweredOnly) scopeLoweredOnly.SetActive(false);

        blinkBase = new float[blinkers.Length];
        for (int i = 0; i < blinkers.Length; i++) blinkBase[i] = blinkers[i] ? blinkers[i].color.a : 1f;
        foreach (var d in drifts) d.home = System.Array.ConvertAll(d.targets, t => t ? t.anchoredPosition : Vector2.zero);
        foreach (var p in pulses) p.baseAlpha = System.Array.ConvertAll(p.targets, g => g ? g.color.a : 1f);
    }

    private readonly List<Object> finished = new List<Object>();

    private void Update()
    {
        float now = Time.unscaledTime; // board motion is UI; it does not stop when the game pauses
        finished.Clear(); // one list reused, no garbage per frame (final QA)
        foreach (var kv in tweens)
        {
            var tw = kv.Value;
            float k = tw.seconds <= 0f ? 1f : Mathf.Clamp01((now - tw.start) / tw.seconds);
            tw.apply(tw.ease.Evaluate(k));
            if (k >= 1f) finished.Add(kv.Key);
        }
        foreach (var key in finished)
        {
            var done = tweens[key].done;
            tweens.Remove(key);
            if (done != null) done();
        }
        Loops(now);
    }

    // ---- drawers -------------------------------------------------------------------------

    /// <summary>Open the named drawer, or close it if it is already out. Opening one closes the rest.</summary>
    public void ToggleDrawer(string name)
    {
        if (OpenDrawerName == name) CloseDrawers();
        else OpenDrawer(name);
    }

    public void OpenDrawer(string name)
    {
        foreach (var d in drawers)
        {
            if (d.name == name) AnimateDrawer(d, true);
            else if (d.group && d.group.gameObject.activeSelf) AnimateDrawer(d, false);
        }
        OpenDrawerName = name;
        Notify();
    }

    public void CloseDrawers()
    {
        foreach (var d in drawers) if (d.group && d.group.gameObject.activeSelf) AnimateDrawer(d, false);
        OpenDrawerName = null;
        Notify();
    }

    void AnimateDrawer(Drawer d, bool open)
    {
        if (open) d.group.gameObject.SetActive(true);
        float x0 = d.group.anchoredPosition.x, a0 = d.fade.alpha;
        float x1 = open ? 0f : drawerTravelPx, a1 = open ? 1f : 0f;
        Run(d.group, drawerSlideSeconds, drawerSlideEase,
            k => d.group.anchoredPosition = new Vector2(Mathf.LerpUnclamped(x0, x1, k), d.group.anchoredPosition.y),
            () => { if (!open && OpenDrawerName != d.name) d.group.gameObject.SetActive(false); });
        Run(d.fade, drawerFadeSeconds, drawerFadeEase, k => d.fade.alpha = Mathf.Lerp(a0, a1, k));
        d.fade.blocksRaycasts = open;
    }

    void SetDrawer(Drawer d, float openness)
    {
        if (!d.group) return;
        d.group.anchoredPosition = new Vector2(Mathf.Lerp(drawerTravelPx, 0f, openness), d.group.anchoredPosition.y);
        d.fade.alpha = openness;
        d.fade.blocksRaycasts = openness > 0f;
        d.group.gameObject.SetActive(openness > 0f);
    }

    // ---- periscope -----------------------------------------------------------------------

    public void ToggleScope() { SetScopeDown(!ScopeDown); }

    public void SetScopeDown(bool down)
    {
        ScopeDown = down;
        float h0 = periscopeClip.sizeDelta.y, h1 = down ? scopeLoweredPx : scopeHousedPx;
        Run(periscopeClip, scopeSeconds, scopeEase, k => periscopeClip.sizeDelta = new Vector2(periscopeClip.sizeDelta.x, Mathf.LerpUnclamped(h0, h1, k)));
        if (scopeHousedOnly) scopeHousedOnly.SetActive(!down);
        if (scopeLoweredOnly) scopeLoweredOnly.SetActive(down);
        Notify();
    }

    void SetScope(float lowered)
    {
        if (periscopeClip) periscopeClip.sizeDelta = new Vector2(periscopeClip.sizeDelta.x, Mathf.Lerp(scopeHousedPx, scopeLoweredPx, lowered));
    }

    // ---- helm and orders -----------------------------------------------------------------

    public void ToggleHelm()
    {
        HelmOpen = !HelmOpen;
        Swap(helmClosed, helmOpen, HelmOpen);
        SlideLatch(HelmOpen ? helmLatchOpen : helmLatchClosed, HelmOpen ? -helmLatchTravelPx : helmLatchTravelPx, helmLatchSeconds, helmLatchEase);
        Notify();
    }

    public void ToggleOrders()
    {
        OrdersOpen = !OrdersOpen;
        Swap(ordersClosed, ordersOpen, OrdersOpen);
        SlideLatch(OrdersOpen ? ordersLatchOpen : ordersLatchClosed, OrdersOpen ? -ordersLatchTravelPx : ordersLatchTravelPx, ordersLatchSeconds, ordersLatchEase);
        Notify();
    }

    static void Swap(GameObject closed, GameObject open, bool isOpen)
    {
        if (closed) closed.SetActive(!isOpen);
        if (open) open.SetActive(isOpen);
    }

    // The latch is built at its resting place in each layout; it arrives there from `from` px away.
    void SlideLatch(RectTransform latch, float from, float seconds, CssEase ease)
    {
        if (!latch) return;
        Vector2 home;
        if (!latchHome.TryGetValue(latch, out home)) latchHome[latch] = home = latch.anchoredPosition;
        Run(latch, seconds, ease, k => latch.anchoredPosition = home + new Vector2(Mathf.LerpUnclamped(from, 0f, k), 0f));
    }
    readonly Dictionary<RectTransform, Vector2> latchHome = new Dictionary<RectTransform, Vector2>();

    // ---- loops ---------------------------------------------------------------------------

    void Loops(float now)
    {
        if (blinkBase != null)
        {
            bool lit = (now % blinkSeconds) / blinkSeconds < blinkLitFraction;
            for (int i = 0; i < blinkers.Length; i++)
                if (blinkers[i]) SetAlpha(blinkers[i], blinkBase[i] * (lit ? 1f : blinkLowAlpha));
        }
        foreach (var d in drifts)
        {
            if (d.home == null || d.seconds <= 0f) continue;
            float k = Tri(now, d.seconds);
            for (int i = 0; i < d.targets.Length; i++)
                if (d.targets[i]) d.targets[i].anchoredPosition = d.home[i] + new Vector2(d.offset.x, -d.offset.y) * k;
        }
        foreach (var p in pulses)
        {
            if (p.baseAlpha == null || p.seconds <= 0f) continue;
            float a = Mathf.Lerp(p.from, p.to, Tri(now, p.seconds));
            for (int i = 0; i < p.targets.Length; i++)
                if (p.targets[i]) SetAlpha(p.targets[i], p.baseAlpha[i] * a);
        }
    }

    // 0 at 0% and 100% of the period, 1 at 50%, eased each half (CSS keyframes 0%,100% / 50%).
    float Tri(float now, float period)
    {
        float t = (now % period) / period;
        return loopEase.Evaluate(t < 0.5f ? t * 2f : 2f - t * 2f);
    }

    static void SetAlpha(Graphic g, float a) { var c = g.color; c.a = a; g.color = c; }

    // ---- tween runner --------------------------------------------------------------------

    void Run(Object key, float seconds, CssEase ease, System.Action<float> apply, System.Action done = null)
    {
        if (!Application.isPlaying || seconds <= 0f)
        {
            tweens.Remove(key);
            apply(1f);
            if (done != null) done();
            return;
        }
        tweens[key] = new Tween { start = Time.unscaledTime, seconds = seconds, ease = ease, apply = apply, done = done };
        apply(ease.Evaluate(0f)); // first frame already at the start, whatever runs after this Update
    }

    void Notify() { if (Changed != null) Changed(); }
}
