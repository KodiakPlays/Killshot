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
public class HandoffSensorsPanel : HandoffPanel
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

    [Header("Colours (design sRGB)")]
    [SerializeField] private Color nominal = new Color32(0x8F, 0xC3, 0x3A, 0xFF);
    [SerializeField] private Color caution = new Color32(0xE0, 0xA0, 0x20, 0xFF);
    [SerializeField] private Color alarm = new Color32(0xE0, 0x4B, 0x3C, 0xFF);
    [SerializeField] private Color label = new Color32(0xC9, 0xD2, 0xD8, 0xFF);
    [SerializeField] private Color stencil = new Color32(0x77, 0x82, 0x8A, 0xFF);
    [SerializeField] private Color stencilDim = new Color32(0x5E, 0x68, 0x70, 0xFF);
    [SerializeField] private Color phosphor = new Color32(0xB6, 0xF0, 0x4A, 0xFF);
    [SerializeField] private Color phosphorOff = new Color32(0x1B, 0x28, 0x10, 0xFF);
    [SerializeField] private Color lampOffPanel = new Color32(0x22, 0x26, 0x2A, 0xFF);
    [Tooltip("Unlit E-SIG cell, as measured (sprite.latch.01).")]
    [SerializeField] private Color cellUnlit = new Color32(0x0E, 0x13, 0x16, 0xFF);
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
            case "sensors.dradis": s.ToggleDradis(); return true;
            case "sensors.active": s.ToggleActive(); return true;
            case "sensors.band": s.SetSpectrum(arg); return true;
            case "sensors.narrow": s.ConeNarrow(); return true;
            case "sensors.widen": s.ConeWiden(); return true;
            case "sensors.identify":
                int i = int.Parse(arg);
                var list = Bogeys();
                if (i < list.Count) s.Identify(list[i].key);
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
        if (action == "sensors.cone" && dragging) { dragging = false; State.ResolveLit(); return true; }
        return false;
    }

    // startGain: t = 1 - (y - (top + 19)) / (height - 38); gain = round(1 + 4t), capped.
    void Gain(float y)
    {
        float t = 1f - (y - (wellTop + wellStop)) / (wellHeight - 2f * wellStop);
        State.SetGain(HandoffBoardState.JsRound(1f + t * 4f));
    }

    // moveCone: bearing of the pointer from the dial centre, 0 up, clockwise.
    void Cone(Vector2 board)
    {
        // in double, as the browser does, so angles a hair from a half round the same way
        double a = System.Math.Atan2(board.x - dialCentre.x, dialCentre.y - board.y) * 180.0 / System.Math.PI;
        State.SetCone((int)System.Math.Floor((a + 360.0) % 360.0 + 0.5));
    }

    System.Collections.Generic.List<HandoffBoardState.Track> Bogeys()
    {
        var s = State;
        var list = new System.Collections.Generic.List<HandoffBoardState.Track>();
        foreach (var t in s.tracks)
            if (!t.own && (s.Heard(t) || s.identified.Contains(t.key) || s.classified.Contains(t.key))) list.Add(t);
        return list;
    }

    private float sigGlowBase = -1f; // the glow's built strength (0.75 of the lamp)

    private void Update()
    {
        if (Controls == null || !sigLamp) return;
        bool tracked = State.signal >= State.signalTrackedAt;
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
        int pct = HandoffBoardState.JsRound(s.signal);
        Color sigInk = s.signal >= s.signalTrackedAt ? alarm : s.signal > 0 ? caution : label;
        if (pctText) { pctText.text = pct + "%"; Paint(pctText, sigInk); }
        int litCells = HandoffBoardState.JsRound(s.signal / 100f * 10f);
        for (int k = 0; cells != null && k < cells.Length; k++)
        {
            int n = k + 1;
            Paint(cells[k], n <= litCells ? (n >= 8 ? alarm : n >= 5 ? caution : Color.white) : cellUnlit);
        }
        Color lampInk = s.signal >= s.signalTrackedAt ? alarm : s.signal > 0 ? caution : lampOffPanel;
        Paint(lamp, lampInk);
        Paint(glow, lampInk);
        Show(glow, s.signal > 0);
        if (note)
        {
            int n = s.Trackers();
            note.text = s.signal >= s.signalTrackedAt
                ? n + " HOSTILE " + (n == 1 ? "SET HAS" : "SETS HAVE") + " YOUR RANGE"
                : s.signal <= 0 ? "RUNNING SILENT"
                : s.signal > s.SignalTarget ? "EMISSIONS DECAYING — NOT YET RESOLVED"
                : "POWER SIGNATURE — NOT YET RESOLVED"; // stand-in: the design has no line for power-driven signal below the line (Step 35)
        }
    }

    public override void Refresh()
    {
        var s = State;

        // masters
        SetMaster(dradis, s.dradis, nominal);
        SetMaster(active, s.active, caution);

        // own emissions, on the drawer and in the signal box (Step 68)
        ShowSignal(sigPct, sigNote, sigCells, sigLamp, sigGlow);
        ShowSignal(boxPct, boxNote, boxCells, boxLamp, boxGlow);
        if (sigLamp) sigLampAlpha = 1f;

        // array gain
        string range = s.SenRangeKm.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " KM";
        if (rangeHeader) rangeHeader.text = range;
        if (rangeEnvelope) rangeEnvelope.text = range;
        if (envelopeRange) envelopeRange.text = range; // senRangeLabel
        if (envelopeLabel != null && envelopeLabel.Length > 0)
        {
            if (envelopeLabelHomeY == null) envelopeLabelHomeY = System.Array.ConvertAll(envelopeLabel, r => r ? r.anchoredPosition.y : 0f);
            float up = (s.senBoost - envelopeLabelHomeGain) * envelopeLabelPxPerGain; // board px, upward
            for (int i = 0; i < envelopeLabel.Length; i++)
                if (envelopeLabel[i]) envelopeLabel[i].anchoredPosition = new Vector2(envelopeLabel[i].anchoredPosition.x, envelopeLabelHomeY[i] + up);
        }
        if (gainValue) gainValue.text = s.senBoost.ToString();
        if (boostCost) { boostCost.text = "SEN BUS ALLOWS " + s.GainCap + " OF 5"; Paint(boostCost, s.GainCap < 5 ? caution : nominal); }
        if (gainCap)
        {
            float top = wellTop + capTopAtFive + (5 - s.senBoost) * stepPx; // element top, board px
            gainCapMotion.Set(top, t => gainCap.anchoredPosition = new Vector2(gainCap.anchoredPosition.x, -(t + capHalf)));
        }
        if (gainTrack)
        {
            float h = capTopAtFive + (s.senBoost - 1) * stepPx + 10f;
            gainTrack.sizeDelta = new Vector2(gainTrack.sizeDelta.x, h);
            gainTrack.anchoredPosition = new Vector2(gainTrack.anchoredPosition.x, -(trackBottom - h / 2f));
        }
        for (int b = 0; b < bands.Length && b < s.spectra.Length; b++)
        {
            bool on = s.spectra[b] == s.spectrum;
            var row = bands[b];
            if (row.bg) row.bg.color = (on ? bandRowOn : Color.clear).gamma.WithAlpha(on ? bandRowOn.a : 0f);
            Paint(row.name, on ? phosphor : nominal);
            if (row.count) { row.count.text = s.CountIn(s.spectra[b]).ToString(); Paint(row.count, on ? phosphor : nominal); }
            Paint(row.lamp, on ? phosphor : phosphorOff);
            Paint(row.glow, phosphor);
            Show(row.glow, on);
        }

        // active scan
        needle.Set(s.coneBrg - 180f);
        if (cone) cone.Set(s.coneBrg - s.coneArc, s.coneArc * 2f);
        if (coneBearing) coneBearing.text = s.coneBrg.ToString("000");
        // "ARC 80°": the degree sign follows the value (the label text#162 holds only "ARC")
        if (coneArcLabel) coneArcLabel.text = (s.coneArc * 2) + "\u00B0";
        if (emitLabel) { emitLabel.text = s.active ? "EMITTING" : "SET OFF"; Paint(emitLabel, s.active ? caution : stencil); }

        // contact board
        int heard = 0;
        foreach (var t in s.tracks) if (s.Heard(t)) heard++;
        if (bogeyCount) bogeyCount.text = heard + " ON " + s.spectrum;
        var list = Bogeys();
        for (int i = 0; i < rows.Length; i++)
        {
            var row = rows[i];
            bool used = i < list.Count;
            foreach (Component c in new Component[] { row.name, row.brg, row.rng, row.klass, row.dot }) Show(c, used);
            if (row.bg) row.bg.gameObject.SetActive(used);
            if (!used) { Show(row.dotGlow, false); continue; }
            var t = list[i];
            bool pinged = s.identified.Contains(t.key), seen = s.classified.Contains(t.key), audible = s.Heard(t);
            bool lit = s.ConeCovers(t) && audible;
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
