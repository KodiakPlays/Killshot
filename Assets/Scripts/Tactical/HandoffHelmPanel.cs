using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Step 33h-1. The helm: the header readouts (closed and open), the throttle fader, the heading
/// dial, the attitude hold keys, approach range and mode, the stability readout, and the A and D
/// heading keys. Rules and values from board-logic.js (thrDown, setAtt, setApMode, apRangeUp/Down,
/// onKey, gLoadNum, stabNum, the heading swing in tickPower; renderVals helmHdg, turnLabel, turnInk,
/// hdgRing, helmThr, thrTrack, thrCapBottom, apAtt, apAttNote, apModes, apRangeLabel, apState,
/// apLamp, gLoad, gInk, stabPct, stabCells) and UIStateSpec §11 "Helm".
/// </summary>
public class HandoffHelmPanel : HandoffPanel
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
    [Tooltip("thrCapBottom = 7 + thr x 0.47 px; thrTrack = 7 + thr x 0.57 + 8 px, its bottom at the well bottom less 7.")]
    [SerializeField] private float capBase = 7f, capSpan = 47f, trackBase = 15f, trackSpan = 57f, capHeight = 17f;
    [SerializeField] private RectTransform hdgRing;
    [Tooltip("The heading ring turns .12s linear (UIStateSpec §9.1 line 1925).")]
    [SerializeField] private HandoffEased ringMotion = new HandoffEased(0.12f, CssEase.Linear);
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

    [Header("Keyboard — A and D step the ordered heading (onKey); held keys repeat as a browser does")]
    [SerializeField] private float repeatDelay = 0.5f, repeatInterval = 0.033f;

    [Header("Colours (design sRGB)")]
    [SerializeField] private Color nominal = new Color32(0x8F, 0xC3, 0x3A, 0xFF);
    [SerializeField] private Color caution = new Color32(0xE0, 0xA0, 0x20, 0xFF);
    [SerializeField] private Color alarm = new Color32(0xE0, 0x4B, 0x3C, 0xFF);
    [SerializeField] private Color data = new Color32(0xE8, 0xED, 0xF0, 0xFF);
    [SerializeField] private Color label = new Color32(0xC9, 0xD2, 0xD8, 0xFF);
    [SerializeField] private Color stencil = new Color32(0x77, 0x82, 0x8A, 0xFF);
    [SerializeField] private Color stencilDim = new Color32(0x5E, 0x68, 0x70, 0xFF);
    [SerializeField] private Color inactive = new Color32(0x6E, 0x7B, 0x85, 0xFF);
    [SerializeField] private Color lampOffPanel = new Color32(0x22, 0x26, 0x2A, 0xFF);
    [Tooltip("Unlit stability cell — no template value (HANDOFF_GAPS).")]
    [SerializeField] private Color stabCellOff = new Color32(0x22, 0x26, 0x2A, 0xFF);

    private int thrPressCap;
    private int heldDir;
    private float nextRepeat;

    public override bool Click(string action, string arg)
    {
        var s = State;
        switch (action)
        {
            case "helm.att": s.SetAtt(arg); return true;
            case "helm.mode": s.SetApMode(arg); return true;
            case "helm.range": s.StepApRange(int.Parse(arg)); return true;
        }
        return false;
    }

    public override bool Down(string action, string arg, Vector2 board)
    {
        if (action != "helm.thr") return false;
        thrPressCap = State.ThrCap; // thrDown reads the cap once, at the press
        SetThr(board);
        return true;
    }

    public override bool Drag(string action, string arg, Vector2 board)
    {
        if (action != "helm.thr") return false;
        SetThr(board);
        return true;
    }

    void SetThr(Vector2 board) => State.SetThr((board.y - thrWell.y) / (double)thrWell.height, thrPressCap);

    private void Update()
    {
        if (Controls == null) return;
        // onKey ignores keys while a text field has focus
        var sel = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
        if (sel && sel.GetComponent<TMP_InputField>()) { heldDir = 0; return; }
        int dir = 0;
        if (Input.GetKeyDown(KeyCode.D)) dir = 1;
        else if (Input.GetKeyDown(KeyCode.A)) dir = -1;
        if (dir != 0) { heldDir = dir; nextRepeat = Time.unscaledTime + repeatDelay; Step(dir); return; }
        if (heldDir != 0 && !Input.GetKey(heldDir > 0 ? KeyCode.D : KeyCode.A)) heldDir = 0;
        if (heldDir != 0 && Time.unscaledTime >= nextRepeat) { nextRepeat += repeatInterval; Step(heldDir); }
    }

    void Step(int dir) { State.StepOrdHdg(dir); Refresh(); }

    public override void Refresh()
    {
        var s = State;
        int turn = s.TurnLeft;
        string turnLabel = System.Math.Abs(turn) < 1 ? "STEADY"
            : (turn < 0 ? "PORT " : "STBD ") + System.Math.Abs(turn).ToString("000") + "°";
        Color turnInk = s.hdg == s.ordHdg ? nominal : caution;
        string hdg = s.hdg.ToString("000");
        double g = s.GLoad;
        string gText = HandoffBoardState.JsToFixed(g, 1) + " G";
        Color gInk = g >= 3.4 ? alarm : g >= 2.2 ? caution : data;
        string apState = string.IsNullOrEmpty(s.apAtt) ? "MANUAL" : "AUTO — " + s.apMode;
        Color apLamp = string.IsNullOrEmpty(s.apAtt) ? stencilDim : nominal;

        foreach (var h in new[] { closed, open }) PaintHeader(h, hdg, turnLabel, turnInk, s.thr.ToString(), gText, gInk, apState, apLamp);

        // 01 FLIGHT
        float bottom = thrWell.yMax;
        if (thrCap)
        {
            float capBottom = bottom - (capBase + s.thr / 100f * capSpan);
            thrCap.anchoredPosition = new Vector2(thrCap.anchoredPosition.x, -(capBottom - capHeight / 2f));
        }
        if (thrTrack)
        {
            float h = trackBase + s.thr / 100f * trackSpan;
            float trackBottom = bottom - capBase;
            thrTrack.sizeDelta = new Vector2(thrTrack.sizeDelta.x, h);
            thrTrack.anchoredPosition = new Vector2(thrTrack.anchoredPosition.x, -(trackBottom - h / 2f));
        }
        if (hdgRing) ringMotion.Set(s.hdg, z => hdgRing.localEulerAngles = new Vector3(0, 0, z)); // CSS rotate(-hdg), .12s linear
        if (dialHdg) dialHdg.text = hdg;
        if (dialTurn) { dialTurn.text = turnLabel; Paint(dialTurn, turnInk); }

        // 02 ATTITUDE HOLD
        string note = null;
        foreach (var k in attKeys)
        {
            bool on = k.key == s.apAtt;
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
        int last = s.apRanges.Length - 1;
        Paint(rangeDown, s.apRange <= 0 ? stencilDim : label);
        Paint(rangeUp, s.apRange >= last ? stencilDim : label);
        if (rangeLabel) rangeLabel.text = (s.apRanges[s.apRange] / 1000.0).ToString(System.Globalization.CultureInfo.InvariantCulture) + "k KM";
        foreach (var m in modes)
        {
            bool on = m.key == s.apMode;
            if (on) Fill(m.bg, data, 0.07f); else Fill(m.bg, Color.black, 0.4f);
            foreach (var e in m.edge) if (on) Fill(e, data, 0.42f); else Fill(e, Color.white, 0.08f);
            Color lamp = on ? (m.key == "EVASIVE" ? caution : nominal) : lampOffPanel;
            Paint(m.lamp, lamp); Paint(m.glow, lamp); Show(m.glow, on);
            Paint(m.label, on ? data : inactive);
        }

        // 04 STABILITY
        double stab = s.Stab;
        int lit = HandoffBoardState.JsRound(stab / 100.0 * 9);
        for (int i = 0; i < stabCells.Length; i++) Paint(stabCells[i], 9 - i <= lit ? data : stabCellOff);
        if (stabPct) stabPct.text = HandoffBoardState.JsRound(stab) + "%";
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
