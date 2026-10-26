using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Step 33c. The Sensors drawer: DRADIS and ACTIVE SCAN masters, own emissions, array gain and the
/// four spectrum bands, the active scan cone dial with NARROW / WIDEN, and the contact board.
/// Rules and values from board-logic.js (setToggles, sigCells, sigInk, sigLamp, sigNote, spectra,
/// gainCapTop, gainTrack, boostCost, senRangeLabel, cone*, emit*, bogeyCount, bogeys) and
/// UIStateSpec §8.2 (startGain, startCone) and §11 "Sensors: listen, then look, then ping".
/// </summary>
public class SensorsPanel : HandoffPanel
{
    [System.Serializable]
    public class Master { public Image rocker; public Image lamp, glow; }

    [System.Serializable]
    public class BandRow { public Image bg; public TMP_Text name, count; public Image lamp, glow; }

    [System.Serializable]
    public class ContactRow { public Image bg; public TMP_Text name, brg, rng, klass; public Image dot, dotGlow; }

    [Header("Masters")]
    [SerializeField] private Master dradis;
    [SerializeField] private Master active;
    [SerializeField] private Sprite rockerUp, rockerThrown;

    [Header("Own emissions")]
    [SerializeField] private TMP_Text sigPct;
    [SerializeField] private TMP_Text sigNote;
    [Tooltip("Ten cells, left (1) to right (10).")]
    [SerializeField] private Image[] sigCells;
    [SerializeField] private Image sigLamp, sigGlow;

    [Header("Signal box, top-right of the frame (Step 68): the same row, shown with the drawer shut")]
    [SerializeField] private TMP_Text boxPct;
    [SerializeField] private TMP_Text boxNote;
    [SerializeField] private Image[] boxCells;
    [SerializeField] private Image boxLamp, boxGlow;

    [Header("Array gain")]
    [SerializeField] private TMP_Text rangeHeader, rangeEnvelope, gainValue, boostCost;
    [SerializeField] private RectTransform gainCap, gainTrack;
    [Header("Plot · SENSOR RANGE label — rides on top of the envelope ring (senCapTop)")]
    [Tooltip("\"SENSOR RANGE\" and its range run (plot text #37, #38).")]
    [SerializeField] private RectTransform[] envelopeLabel;
    [SerializeField] private TMP_Text envelopeRange;
    [Tooltip("senCapTop = -(150 + gain x 48) - 8 px from the plot centre: the label rises 48 px per gain step. It was measured at this gain.")]
    [SerializeField] private int envelopeLabelHomeGain = 1;
    [SerializeField] private float envelopeLabelPxPerGain = 48f;
    private float[] envelopeLabelHomeY;

    [Tooltip("The gain cap's top eases .12s (UIStateSpec §9.1 line 699); the lit track does not.")]
    [SerializeField] private HandoffEased gainCapMotion = new HandoffEased(0.12f, CssEase.Ease);
    [Tooltip("Fader well element top and height, board px (layout sprite#152).")]
    [SerializeField] private float wellTop = 375.5f, wellHeight = 136f;
    [Tooltip("gainCapTop = 9 + (5 - gain) x 23.6, from the well top (UIStateSpec §9.1 line 699).")]
    [SerializeField] private float capTopAtFive = 9f, stepPx = 23.6f;
    [Tooltip("Lit track: height 9 + (gain - 1) x 23.6 + 10, bottom fixed (gainTrack).")]
    [SerializeField] private float trackBottom = 502.5f;
    [Tooltip("startGain ignores this many px at each end of the well.")]
    [SerializeField] private float wellStop = 19f;
    [SerializeField] private BandRow[] bands;

    [Header("Active scan")]
    [SerializeField] private HandoffBoardControls.Dial needle;
    [SerializeField] private HandoffWedge cone;
    [Tooltip("Cone dial centre, board px (gauge_housing centre).")]
    [SerializeField] private Vector2 dialCentre = new Vector2(1505f, 609f);
    [SerializeField] private TMP_Text coneBearing, coneArcLabel, emitLabel;

    [Header("Contact board")]
    [SerializeField] private TMP_Text bogeyCount;
    [SerializeField] private ContactRow[] rows;

    Color nominal => Palette.nominal;
    Color caution => Palette.caution;
    Color alarm => Palette.alarm;
    Color label => Palette.label;
    Color stencil => Palette.stencil;
    Color stencilDim => Palette.stencilDim;
    Color phosphor => Palette.phosphor;
    Color phosphorOff => Palette.phosphorOff;
    Color lampOffPanel => Palette.lampOffPanel;
    Color cellUnlit => Palette.cellUnlit;
    [SerializeField] private Color bandRowOn = new Color(143 / 255f, 195 / 255f, 58 / 255f, 0.15f);
    [SerializeField] private Color contactRowLit = new Color(143 / 255f, 195 / 255f, 58 / 255f, 0.07f);
    [Tooltip("bd-blink on the emissions lamp while tracked: period and dim alpha (§9.2 line 675).")]
    [SerializeField] private float blinkSeconds = 1.1f, blinkLitFraction = 0.52f, blinkLowAlpha = 0.2f;

    private bool dragging;
    private float sigLampAlpha = 1f;

    public override bool Click(string action, string arg)
    {
        var s = State;
        switch (action)
        {
            case "sensors.dradis": s.sensors.ToggleDradis(); return true;
            case "sensors.active": s.sensors.ToggleActive(); return true;
            case "sensors.band": s.sensors.SetSpectrum(arg); return true;
            case "sensors.narrow": s.sensors.ConeNarrow(); return true;
            case "sensors.widen": s.sensors.ConeWiden(); return true;
            case "sensors.identify":
                int i = int.Parse(arg);
                var list = Bogeys();
                if (i < list.Count) s.sensors.Identify(list[i].key);
                return true;
        }
        return false;
    }

    public override bool Down(string action, string arg, Vector2 board)
    {
        if (action == "sensors.gain") { Gain(board.y); return true; }
        if (action == "sensors.cone") { dragging = true; Cone(board); return true; }
        return false;
    }

    public override bool Drag(string action, string arg, Vector2 board)
    {
        if (action == "sensors.gain") { Gain(board.y); return true; }
        if (action == "sensors.cone" && dragging) { Cone(board); return true; }
        return false;
    }

    public override bool Up(string action, string arg, Vector2 board)
    {
        if (action == "sensors.cone" && dragging) { dragging = false; State.sensors.ResolveLit(); return true; }
        return false;
    }

    // startGain: t = 1 - (y - (top + 19)) / (height - 38); gain = round(1 + 4t), capped.
    void Gain(float y)
    {
        float t = 1f - (y - (wellTop + wellStop)) / (wellHeight - 2f * wellStop);
        State.sensors.SetGain(HandoffBoardState.JsRound(1f + t * 4f));
    }

    // moveCone: bearing of the pointer from the dial centre, 0 up, clockwise.
    void Cone(Vector2 board)
    {
        // in double, as the browser does, so angles a hair from a half round the same way
        double a = System.Math.Atan2(board.x - dialCentre.x, dialCentre.y - board.y) * 180.0 / System.Math.PI;
        State.sensors.SetCone((int)System.Math.Floor((a + 360.0) % 360.0 + 0.5));
    }

    System.Collections.Generic.List<HandoffBoardState.Track> Bogeys()
    {
        var s = State;
        var list = new System.Collections.Generic.List<HandoffBoardState.Track>();
        foreach (var t in s.tracks)
            if (!t.own && (s.sensors.Heard(t) || s.sensors.identified.Contains(t.key) || s.classified.Contains(t.key))) list.Add(t);
        return list;
    }

    private float sigGlowBase = -1f; // the glow's built strength (0.75 of the lamp)

    private void Update()
    {
        if (Controls == null || !sigLamp) return;
        bool tracked = State.sensors.signal >= State.sensors.stats.signalTrackedAt;
        bool lit = !tracked || (Time.unscaledTime % blinkSeconds) / blinkSeconds < blinkLitFraction;
        Blink(sigLamp, sigGlow, lit);
        Blink(boxLamp, boxGlow, lit);
    }

    void Blink(Image lamp, Image glow, bool lit)
    {
        if (lamp) { var c = lamp.color; c.a = sigLampAlpha * (lit ? 1f : blinkLowAlpha); lamp.color = c; }
        // opacity on the lamp element dims its box-shadow glow too
        if (glow) { if (sigGlowBase < 0f) sigGlowBase = glow.color.a; var g = glow.color; g.a = sigGlowBase * (lit ? 1f : blinkLowAlpha); glow.color = g; }
    }

    void ShowSignal(TMP_Text pctText, TMP_Text note, Image[] cells, Image lamp, Image glow)
    {
        var s = State;
        int pct = HandoffBoardState.JsRound(s.sensors.signal);
        Color sigInk = s.sensors.signal >= s.sensors.stats.signalTrackedAt ? alarm : s.sensors.signal > 0 ? caution : label;
        if (pctText) { pctText.text = pct + "%"; Paint(pctText, sigInk); }
        int litCells = HandoffBoardState.JsRound(s.sensors.signal / 100f * 10f);
        for (int k = 0; cells != null && k < cells.Length; k++)
        {
            int n = k + 1;
            Paint(cells[k], n <= litCells ? (n >= 8 ? alarm : n >= 5 ? caution : Color.white) : cellUnlit);
        }
        Color lampInk = s.sensors.signal >= s.sensors.stats.signalTrackedAt ? alarm : s.sensors.signal > 0 ? caution : lampOffPanel;
        Paint(lamp, lampInk);
        Paint(glow, lampInk);
        Show(glow, s.sensors.signal > 0);
        if (note)
        {
            int n = s.sensors.Trackers();
            note.text = s.sensors.signal >= s.sensors.stats.signalTrackedAt
                ? n + " HOSTILE " + (n == 1 ? "SET HAS" : "SETS HAVE") + " YOUR RANGE"
                : s.sensors.signal <= 0 ? "RUNNING SILENT"
                : s.sensors.signal > s.sensors.SignalTarget ? "EMISSIONS DECAYING — NOT YET RESOLVED"
                : "POWER SIGNATURE — NOT YET RESOLVED"; // stand-in: the design has no line for power-driven signal below the line (Step 35)
        }
    }

    public override void Refresh()
    {
        var s = State;

        // masters
        SetMaster(dradis, s.sensors.dradis, nominal);
        SetMaster(active, s.sensors.active, caution);

        // own emissions, on the drawer and in the signal box (Step 68)
        ShowSignal(sigPct, sigNote, sigCells, sigLamp, sigGlow);
        ShowSignal(boxPct, boxNote, boxCells, boxLamp, boxGlow);
        if (sigLamp) sigLampAlpha = 1f;

        // array gain
        string range = s.sensors.SenRangeKm.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " KM";
        if (rangeHeader) rangeHeader.text = range;
        if (rangeEnvelope) rangeEnvelope.text = range;
        if (envelopeRange) envelopeRange.text = range; // senRangeLabel
        if (envelopeLabel != null && envelopeLabel.Length > 0)
        {
            if (envelopeLabelHomeY == null) envelopeLabelHomeY = System.Array.ConvertAll(envelopeLabel, r => r ? r.anchoredPosition.y : 0f);
            float up = (s.sensors.senBoost - envelopeLabelHomeGain) * envelopeLabelPxPerGain; // board px, upward
            for (int i = 0; i < envelopeLabel.Length; i++)
                if (envelopeLabel[i]) envelopeLabel[i].anchoredPosition = new Vector2(envelopeLabel[i].anchoredPosition.x, envelopeLabelHomeY[i] + up);
        }
        if (gainValue) gainValue.text = s.sensors.senBoost.ToString();
        if (boostCost) { boostCost.text = "SEN BUS ALLOWS " + s.sensors.GainCap + " OF 5"; Paint(boostCost, s.sensors.GainCap < 5 ? caution : nominal); }
        if (gainCap)
        {
            float top = wellTop + capTopAtFive + (5 - s.sensors.senBoost) * stepPx; // element top, board px
            gainCapMotion.Set(top, t => gainCap.anchoredPosition = new Vector2(gainCap.anchoredPosition.x, -(t + capHalf)));
        }
        if (gainTrack)
        {
            float h = capTopAtFive + (s.sensors.senBoost - 1) * stepPx + 10f;
            gainTrack.sizeDelta = new Vector2(gainTrack.sizeDelta.x, h);
            gainTrack.anchoredPosition = new Vector2(gainTrack.anchoredPosition.x, -(trackBottom - h / 2f));
        }
        for (int b = 0; b < bands.Length && b < s.sensors.spectra.Length; b++)
        {
            bool on = s.sensors.spectra[b] == s.sensors.spectrum;
            var row = bands[b];
            if (row.bg) row.bg.color = (on ? bandRowOn : Color.clear).gamma.WithAlpha(on ? bandRowOn.a : 0f);
            Paint(row.name, on ? phosphor : nominal);
            if (row.count) { row.count.text = s.sensors.CountIn(s.sensors.spectra[b]).ToString(); Paint(row.count, on ? phosphor : nominal); }
            Paint(row.lamp, on ? phosphor : phosphorOff);
            Paint(row.glow, phosphor);
            Show(row.glow, on);
        }

        // active scan
        needle.Set(s.sensors.coneBrg - 180f);
        if (cone) cone.Set(s.sensors.coneBrg - s.sensors.coneArc, s.sensors.coneArc * 2f);
        if (coneBearing) coneBearing.text = s.sensors.coneBrg.ToString("000");
        // "ARC 80°": the degree sign follows the value (the label text#162 holds only "ARC")
        if (coneArcLabel) coneArcLabel.text = (s.sensors.coneArc * 2) + "\u00B0";
        if (emitLabel) { emitLabel.text = s.sensors.active ? "EMITTING" : "SET OFF"; Paint(emitLabel, s.sensors.active ? caution : stencil); }

        // contact board
        int heard = 0;
        foreach (var t in s.tracks) if (s.sensors.Heard(t)) heard++;
        if (bogeyCount) bogeyCount.text = heard + " ON " + s.sensors.spectrum;
        var list = Bogeys();
        for (int i = 0; i < rows.Length; i++)
        {
            var row = rows[i];
            bool used = i < list.Count;
            foreach (Component c in new Component[] { row.name, row.brg, row.rng, row.klass, row.dot }) Show(c, used);
            if (row.bg) row.bg.gameObject.SetActive(used);
            if (!used) { Show(row.dotGlow, false); continue; }
            var t = list[i];
            bool pinged = s.sensors.identified.Contains(t.key), seen = s.classified.Contains(t.key), audible = s.sensors.Heard(t);
            bool lit = s.sensors.ConeCovers(t) && audible;
            row.name.text = pinged ? t.name : "BOGEY " + t.key.ToUpperInvariant();
            row.brg.text = t.bearing.ToString("000");
            row.rng.text = pinged ? HandoffBoardState.JsRound(t.rangeKm / 1000f) + "k" : "—";
            row.klass.text = seen ? t.klass : lit ? "RESOLVING" : "—";
            Paint(row.klass, seen ? (t.hostile ? alarm : nominal) : lit ? caution : stencil);
            Color dot = pinged ? (t.hostile ? alarm : nominal) : audible ? caution : stencilDim;
            Paint(row.dot, dot);
            Paint(row.dotGlow, dot);
            Show(row.dotGlow, pinged || audible);
            if (row.bg) row.bg.color = lit ? contactRowLit.gamma.WithAlpha(contactRowLit.a) : new Color(0, 0, 0, 0);
        }
    }

    [Tooltip("Half the fader cap's element height (20 px), to place it by its centre.")]
    [SerializeField] private float capHalf = 10f;

    void SetMaster(Master m, bool on, Color lampOn)
    {
        if (m.rocker) m.rocker.sprite = on ? rockerThrown : rockerUp;
        Paint(m.lamp, on ? lampOn : lampOffPanel);
        Paint(m.glow, lampOn);
        Show(m.glow, on);
    }
}

static class ColorAlpha
{
    public static Color WithAlpha(this Color c, float a) { c.a = a; return c; }
}
