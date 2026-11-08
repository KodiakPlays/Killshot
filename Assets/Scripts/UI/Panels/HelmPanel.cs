using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// The helm panel (Step 33h-1): the header readouts (closed and open), the throttle fader, the heading
/// dial, the attitude hold keys, approach range and mode, the stability readout, and the A and D
/// heading keys. Rules and values from board-logic.js (thrDown, setAtt, setApMode, apRangeUp/Down,
/// onKey, gLoadNum, stabNum, the heading swing in tickPower; renderVals helmHdg, turnLabel, turnInk,
/// hdgRing, helmThr, thrTrack, thrCapBottom, apAtt, apAttNote, apModes, apRangeLabel, apState,
/// apLamp, gLoad, gInk, stabPct, stabCells) and UIStateSpec §11 "Helm". It shows the ship's
/// NavSystem and passes the throttle, the keys and the clicks to it; the movement rules are there.
/// </summary>
public class HelmPanel : Panel
{
    [System.Serializable]
    public class Header
    {
        public TMP_Text hdg, turn, thrCaption, thr, pct, gLoad, apState;
        public Image lamp, glow;
        [HideInInspector] public float turnLeft, thrCaptionLeft, thrLeft, pctLeft, gLeft;
        [HideInInspector] public bool homed;
    }

    [System.Serializable]
    public class AttKey
    {
        public string key;
        public string note;
        [Tooltip("Stem outline, stem core, arrow outline, arrow core.")]
        public Graphic stem, stemCore, arrow, arrowCore;
        public Graphic[] edge;
    }

    [System.Serializable]
    public class ModeRow
    {
        public string key;
        public Graphic bg;
        public Graphic[] edge;
        public Image lamp, glow;
        public TMP_Text label;
    }

    [SerializeField] private Header closed, open;
    [Tooltip("Header row gaps, board px, as laid out: turn label → THR 20, THR → value 5, value → % 0, % → G 20.")]
    [SerializeField] private float gapToThr = 20f, gapToValue = 5f, gapToG = 20f;

    [Header("01 FLIGHT")]
    [SerializeField] private RectTransform thrCap, thrTrack;
    [Tooltip("Fader well element, board px (fader_thrust_well 652,941 44x78).")]
    [SerializeField] private Rect thrWell = new Rect(652, 941, 44, 78);
    [Tooltip("The cap's bottom, px above the well bottom: Cap Base at full reverse, Cap Base + Cap Span at full ahead (owner 2026-11-02: 0 in the middle). The lit track runs from the cap's middle at 0 to the cap.")]
    [SerializeField] private float capBase = 7f, capSpan = 47f, capHeight = 17f;
    [SerializeField] private RectTransform hdgRing;
    [Tooltip("The heading ring turns .12s linear (UIStateSpec §9.1 line 1925).")]
    [SerializeField] private Eased ringMotion = new Eased(0.12f, CssEase.Linear);
    // the ring's angle counted on past 360, so crossing north it turns the short way, not a full turn back
    private float ringDeg;
    [SerializeField] private TMP_Text dialHdg, dialTurn;

    [Header("02 ATTITUDE HOLD")]
    [SerializeField] private AttKey[] attKeys;
    [SerializeField] private TMP_Text attNote;

    [Header("03 APPROACH")]
    [SerializeField] private TMP_Text rangeDown, rangeUp, rangeLabel;
    [SerializeField] private ModeRow[] modes;

    [Header("04 STABILITY")]
    [Tooltip("Nine cells, top first (k = 9 … 1).")]
    [SerializeField] private Graphic[] stabCells;
    [SerializeField] private TMP_Text stabPct, gLoad;

    [Header("Keyboard — A and D step the ordered heading (onKey; held, they repeat as a browser does), W and S the throttle")]
    [Tune("Helm")]
    [SerializeField] private float repeatDelay = 0.5f, repeatInterval = 0.033f;
    [Tooltip("Owner 2026-11-09: a tap of W or S moves the throttle 1. Held, it keeps counting, slowly at first, then faster, and has moved 100 after this many seconds.")]
    [Tune("Helm")] [SerializeField] private float throttleHoldSeconds = 2f;
    [Tooltip("Owner 2026-10-29: the helm's keys. Change them on the Keys tab of Starwolf → Tuning.")]
    [Tune("Keys")] [SerializeField] private KeyCode turnPortKey = KeyCode.A, turnStarboardKey = KeyCode.D, throttleUpKey = KeyCode.W, throttleDownKey = KeyCode.S;
    [Tooltip("Owner 2026-11-01: held, these slide the ship sideways. The nose stays where it is.")]
    [Tune("Keys")] [SerializeField] private KeyCode slideLeftKey = KeyCode.Q, slideRightKey = KeyCode.E;
    [Tooltip("Owner 2026-11-08: held with the turn keys, this turns the ship slowly, for aiming the spinal railgun.")]
    [Tune("Keys")] [SerializeField] private KeyCode fineTurnKey = KeyCode.LeftShift;

    Color nominal => Palette.nominal;
    Color caution => Palette.caution;
    Color alarm => Palette.alarm;
    Color data => Palette.data;
    Color label => Palette.label;
    Color stencil => Palette.stencil;
    Color stencilDim => Palette.stencilDim;
    Color inactive => Palette.inactive;
    Color lampOffPanel => Palette.lampOffPanel;
    Color stabCellOff => Palette.lampOffPanel;

    private int thrPressCap;
    private KeyCode heldKey;
    private float nextRepeat;
    // the W or S key held now, when it went down, the throttle then, and the throttle the key last set
    private KeyCode thrKey;
    private float thrSince;
    private int thrFrom, thrLast;

    public override bool Click(string action, string arg)
    {
        var h = State.nav;
        switch (action)
        {
            case "helm.att": h.ToggleAttitude(arg); return true;
            case "helm.mode": h.SetApproachMode(arg); return true;
            case "helm.range": h.StepApproachRange(int.Parse(arg)); return true;
        }
        return false;
    }

    public override bool Down(string action, string arg, Vector2 board)
    {
        if (action != "helm.thr") return false;
        thrPressCap = State.nav.ThrottleCap; // thrDown reads the cap once, at the press
        SetThr(board);
        return true;
    }

    public override bool Drag(string action, string arg, Vector2 board)
    {
        if (action != "helm.thr") return false;
        SetThr(board);
        return true;
    }

    void SetThr(Vector2 board) => State.nav.SetThrottle((board.y - thrWell.y) / (double)thrWell.height, thrPressCap);

    private void Update()
    {
        if (Controls == null) return;
        // onKey ignores keys while a text field has focus
        var sel = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
        if (sel && sel.GetComponent<TMP_InputField>()) { heldKey = thrKey = KeyCode.None; State.nav.slide = State.nav.fineTurn = 0; return; }
        State.nav.slide = (Input.GetKey(slideRightKey) ? 1 : 0) - (Input.GetKey(slideLeftKey) ? 1 : 0);
        // owner 2026-11-09: every tap of Q or E counts, even one let go before the ship next moves
        if (Input.GetKeyDown(slideLeftKey)) State.nav.slideTaps--;
        if (Input.GetKeyDown(slideRightKey)) State.nav.slideTaps++;
        Throttle();
        // owner 2026-11-08: with Shift held, A and D turn the ship slowly instead of stepping the heading
        bool fine = Input.GetKey(fineTurnKey);
        State.nav.fineTurn = fine ? (Input.GetKey(turnStarboardKey) ? 1 : 0) - (Input.GetKey(turnPortKey) ? 1 : 0) : 0;
        foreach (var k in new[] { turnPortKey, turnStarboardKey })
            if (Input.GetKeyDown(k) && !fine) { heldKey = k; nextRepeat = Time.unscaledTime + repeatDelay; Press(k); return; }
        if (heldKey != KeyCode.None && (!Input.GetKey(heldKey) || fine)) heldKey = KeyCode.None;
        if (heldKey != KeyCode.None && Time.unscaledTime >= nextRepeat) { nextRepeat += repeatInterval; Press(heldKey); }
    }

    void Press(KeyCode k)
    {
        State.nav.StepOrderedHeading(k == turnStarboardKey ? 1 : -1);
        Refresh();
    }

    // owner 2026-11-09: a tap of W or S moves the throttle 1; held, it keeps counting, slowly at first, then faster.
    // Owner 2026-11-02: S goes on past 0 into reverse. Up to what the ENG bus allows.
    void Throttle()
    {
        var n = State.nav;
        foreach (var k in new[] { throttleUpKey, throttleDownKey })
            if (Input.GetKeyDown(k)) { thrKey = k; thrSince = Time.unscaledTime; thrFrom = thrLast = n.throttle; }
        if (thrKey == KeyCode.None) return;
        // let go, or the throttle was moved some other way (power lost, the fader dragged): the key stops
        if (!Input.GetKey(thrKey) || n.throttle != thrLast) { thrKey = KeyCode.None; return; }
        float t = (Time.unscaledTime - thrSince) / throttleHoldSeconds;
        int cap = Mathf.Min(100, n.ThrottleCap), by = 1 + Mathf.FloorToInt(99 * t * t);
        thrLast = Mathf.Clamp(thrFrom + (thrKey == throttleUpKey ? by : -by), -cap, cap);
        if (thrLast != n.throttle) { n.throttle = thrLast; Refresh(); }
    }

    public override void Refresh()
    {
        var s = State.nav;
        int turn = s.TurnLeft;
        string turnLabel = System.Math.Abs(turn) < 1 ? "STEADY"
            : (turn < 0 ? "PORT " : "STBD ") + System.Math.Abs(turn).ToString("000") + "°";
        Color turnInk = s.heading == s.orderedHeading ? nominal : caution;
        string hdg = s.heading.ToString("000");
        double g = s.GLoad;
        string gText = ShipState.JsToFixed(g, 1) + " G";
        Color gInk = g >= 3.4 ? alarm : g >= 2.2 ? caution : data;
        string apState = string.IsNullOrEmpty(s.attitude) ? "MANUAL" : "AUTO — " + s.approachMode;
        Color apLamp = string.IsNullOrEmpty(s.attitude) ? stencilDim : nominal;

        foreach (var h in new[] { closed, open }) PaintHeader(h, hdg, turnLabel, turnInk, s.throttle.ToString(), gText, gInk, apState, apLamp);

        // 01 FLIGHT
        // owner 2026-11-02: 0 in the middle of the well, full ahead at the top, full reverse at the bottom; the track lights from the middle to the cap
        float bottom = thrWell.yMax;
        float CapMid(int thr) => bottom - (capBase + (thr + 100) / 200f * capSpan) - capHeight / 2f;
        if (thrCap) thrCap.anchoredPosition = new Vector2(thrCap.anchoredPosition.x, -CapMid(s.throttle));
        if (thrTrack)
        {
            float from = CapMid(0), to = CapMid(s.throttle);
            thrTrack.sizeDelta = new Vector2(thrTrack.sizeDelta.x, Mathf.Abs(to - from));
            thrTrack.anchoredPosition = new Vector2(thrTrack.anchoredPosition.x, -(from + to) / 2f);
        }
        if (hdgRing) { ringDeg += Mathf.DeltaAngle(ringDeg, s.heading); ringMotion.Set(ringDeg, z => hdgRing.localEulerAngles = new Vector3(0, 0, z)); } // CSS rotate(-hdg), .12s linear
        if (dialHdg) dialHdg.text = hdg;
        if (dialTurn) { dialTurn.text = turnLabel; Paint(dialTurn, turnInk); }

        // 02 ATTITUDE HOLD
        string note = null;
        foreach (var k in attKeys)
        {
            bool on = k.key == s.attitude;
            if (on) note = k.note;
            Paint(k.stem, on ? data : stencil);
            Paint(k.stemCore, on ? data : Color.black);
            Paint(k.arrow, on ? nominal : stencilDim);
            Paint(k.arrowCore, on ? nominal : Color.black);
            // edge: inset 1px #8FC33A when held, else rgba(232,237,240,.26); the 9 px outer glow is not built
            foreach (var e in k.edge) Fill(e, on ? nominal : data, on ? 1f : 0.26f);
        }
        if (attNote) attNote.text = note != null ? "HOLDING " + note : "NO ATTITUDE HOLD — FREE DRIFT";

        // 03 APPROACH
        int last = s.approachRanges.Length - 1;
        Paint(rangeDown, s.approachRange <= 0 ? stencilDim : label);
        Paint(rangeUp, s.approachRange >= last ? stencilDim : label);
        if (rangeLabel) rangeLabel.text = (s.approachRanges[s.approachRange] / 1000.0).ToString(System.Globalization.CultureInfo.InvariantCulture) + "k KM";
        foreach (var m in modes)
        {
            bool on = m.key == s.approachMode;
            if (on) Fill(m.bg, data, 0.07f); else Fill(m.bg, Color.black, 0.4f);
            foreach (var e in m.edge) if (on) Fill(e, data, 0.42f); else Fill(e, Color.white, 0.08f);
            Color lamp = on ? (m.key == "EVASIVE" ? caution : nominal) : lampOffPanel;
            Paint(m.lamp, lamp); Paint(m.glow, lamp); Show(m.glow, on);
            Paint(m.label, on ? data : inactive);
        }

        // 04 STABILITY
        double stab = s.Stability;
        int lit = ShipState.JsRound(stab / 100.0 * 9);
        for (int i = 0; i < stabCells.Length; i++) Paint(stabCells[i], 9 - i <= lit ? data : stabCellOff);
        if (stabPct) stabPct.text = ShipState.JsRound(stab) + "%";
        if (gLoad) { gLoad.text = gText; Paint(gLoad, gInk); }
    }

    // The header row flows left to right (Share Tech Mono, so width follows the character count):
    // the turn label, THR, the throttle value, % and the G load move with the text before them.
    void PaintHeader(Header h, string hdg, string turn, Color turnInk, string thr, string g, Color gInk, string apState, Color apLamp)
    {
        if (h == null || !h.turn) return;
        if (!h.homed)
        {
            h.turnLeft = Left(h.turn); h.thrCaptionLeft = Left(h.thrCaption); h.thrLeft = Left(h.thr);
            h.pctLeft = Left(h.pct); h.gLeft = Left(h.gLoad); h.homed = true;
        }
        if (h.hdg) h.hdg.text = hdg;
        float turnW = SetText(h.turn, turn, h.turnLeft);
        Paint(h.turn, turnInk);
        float x = h.turnLeft + turnW + gapToThr;
        x += Place(h.thrCaption, x) + gapToValue;
        x += SetText(h.thr, thr, x);
        x += Place(h.pct, x) + gapToG;
        SetText(h.gLoad, g, x);
        if (h.apState) h.apState.text = apState;
        Paint(h.lamp, apLamp); Paint(h.glow, apLamp); Show(h.glow, apLamp == nominal);
    }

    static float Left(TMP_Text t) => t.rectTransform.anchoredPosition.x - t.rectTransform.sizeDelta.x * t.rectTransform.pivot.x;

    // Monospace: the advance per character is the built box over the built text's length.
    float SetText(TMP_Text t, string text, float left)
    {
        var rt = t.rectTransform;
        int n0 = Mathf.Max(1, t.text.Length);
        float adv = rt.sizeDelta.x / n0;
        t.text = text;
        rt.sizeDelta = new Vector2(adv * text.Length, rt.sizeDelta.y);
        return Place(t, left);
    }

    static float Place(TMP_Text t, float left)
    {
        var rt = t.rectTransform;
        rt.anchoredPosition = new Vector2(left + rt.sizeDelta.x * rt.pivot.x, rt.anchoredPosition.y);
        return rt.sizeDelta.x;
    }

    static void Fill(Graphic g, Color design, float alpha)
    {
        if (!g) return;
        var c = design.gamma; c.a = alpha; g.color = c;
    }
}
