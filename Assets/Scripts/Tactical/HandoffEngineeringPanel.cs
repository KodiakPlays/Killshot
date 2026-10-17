using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Step 33e. The Engineering drawer: the ship schematic and compartment screen, the hazard,
/// degraded and party lines, the ACTING ON selector, and the repair rockers. Rules and values from
/// board-logic.js (cycleComp, pickComp, engBlock, engRun, applyOrder, flickBack, the party in the
/// tick; renderVals compartments, hazardLine/Ink, degradedLine/Ink, partyLine/Ink, compName,
/// compAngle, compLamp, engNote/Ink, engSwitches) and UIStateSpec §11 "Engineering".
/// </summary>
public class HandoffEngineeringPanel : HandoffPanel
{
    [System.Serializable]
    public class Row
    {
        public string key;
        public Image cell;
        public Image rowBg;
        [Tooltip("Five integrity segments, the first from 9 o'clock, clockwise.")]
        public HandoffWedge[] ring;
        public TMP_Text tag, atmo, cond, crew;
    }

    [System.Serializable]
    public class Repair
    {
        public string id;
        public bool fitted = true;
        public Image rocker;
        public Sprite up, locked, thrown;
        public Image lamp, glow;
        public TMP_Text label;
    }

    [SerializeField] private Row[] rows;
    [SerializeField] private TMP_Text hazardLine, degradedLine, partyLine;
    [Tooltip("Status row: the degraded line starts here and the party line ends here (board px); "
           + "the degraded line takes the room left of the party line's real width, less the gap, "
           + "and ends in an ellipsis if it does not fit (HANDOFF_GAPS — no template for this row).")]
    [SerializeField] private float rowLeft = 1474f, rowRight = 1782f, rowGap = 10f, degradedMax = 204f;
    [SerializeField] private HandoffBoardControls.Dial actingOn;
    [SerializeField] private TMP_Text compName, engNote;
    [SerializeField] private Image compLamp, compGlow;
    [SerializeField] private Repair[] repairs;

    [Header("Ring — conic segments: 72° each with a 7° gap, from -90° (compartments[].ring)")]
    [SerializeField] private float segmentDeg = 72f, gapDeg = 7f, fromDeg = -90f;
    [SerializeField] private float ringDimAlpha = 0.72f;

    [Header("Colours (design sRGB)")]
    [SerializeField] private Color nominal = new Color32(0x8F, 0xC3, 0x3A, 0xFF);
    [SerializeField] private Color caution = new Color32(0xE0, 0xA0, 0x20, 0xFF);
    [SerializeField] private Color alarm = new Color32(0xE0, 0x4B, 0x3C, 0xFF);
    [SerializeField] private Color data = new Color32(0xE8, 0xED, 0xF0, 0xFF);
    [SerializeField] private Color secondary = new Color32(0x8A, 0x95, 0x9D, 0xFF);
    [SerializeField] private Color label = new Color32(0xC9, 0xD2, 0xD8, 0xFF);
    [SerializeField] private Color stencil = new Color32(0x77, 0x82, 0x8A, 0xFF);
    [SerializeField] private Color wellStencil = new Color32(0x5B, 0x66, 0x6F, 0xFF);
    [SerializeField] private Color ownForce = new Color32(0x4A, 0x9F, 0xD8, 0xFF);
    [SerializeField] private Color stencilDim = new Color32(0x5E, 0x68, 0x70, 0xFF);
    [SerializeField] private Color lampOffPanel = new Color32(0x22, 0x26, 0x2A, 0xFF);
    [SerializeField] private Color ringUnlit = new Color32(0x1A, 0x1F, 0x23, 0xFF);

    public override bool Click(string action, string arg)
    {
        var s = State;
        switch (action)
        {
            case "eng.cycle": s.CycleComp(); return true;
            case "eng.pick": s.PickComp(arg); return true;
            case "eng.run": s.EngRun(arg, Time.unscaledTime); return true;
        }
        return false;
    }

    static void Fill(Graphic g, Color design, float alpha)
    {
        if (!g) return;
        var c = design.gamma; c.a = alpha; g.color = c;
    }

    public override void Refresh()
    {
        var s = State;

        // compartment screen and schematic
        foreach (var r in rows)
        {
            var c = s.Comp(r.key);
            if (c == null) continue;
            bool on = r.key == s.engSel;
            Fill(r.cell, data, on ? 0.16f : 0.05f);
            Fill(r.rowBg, data, on ? 0.07f : 0f);
            for (int i = 0; i < r.ring.Length; i++)
            {
                var w = r.ring[i];
                if (!w) continue;
                w.Set(fromDeg + i * segmentDeg, segmentDeg - gapDeg);
                w.color = (c.integ >= i + 1 ? data : ringUnlit).WithAlpha(on ? 1f : ringDimAlpha);
            }
            Paint(r.tag, on ? data : secondary);
            if (r.atmo) { r.atmo.text = c.vented ? "VENT" : "SEAL"; Paint(r.atmo, c.vented ? alarm : nominal); }
            string cond = string.IsNullOrEmpty(c.cond) ? "NOM" : c.cond;
            if (r.cond) { r.cond.text = cond; Paint(r.cond, cond == "FIRE" ? alarm : cond == "RAD" ? caution : nominal); }
            if (r.crew) { r.crew.text = c.crew.ToString(); Paint(r.crew, c.crew > 0 ? label : alarm); }
        }

        // hazardLine
        int hazards = 0, complement = 0;
        bool losing = false;
        foreach (var c in s.compartments)
        {
            if (!string.IsNullOrEmpty(c.cond) || c.vented) hazards++;
            if (!string.IsNullOrEmpty(c.cond)) losing = true;
            complement += c.crew;
        }
        if (hazardLine)
        {
            hazardLine.text = (hazards > 0 ? hazards + (hazards == 1 ? " COMPARTMENT IN HAZARD" : " COMPARTMENTS IN HAZARD") : "NO HAZARD") + " · COMPLEMENT " + complement;
            Paint(hazardLine, losing ? alarm : hazards > 0 ? caution : nominal);
        }

        // degradedLine
        var parts = new System.Collections.Generic.List<string>();
        if (s.ReactorCap < s.reactorUnits) parts.Add("RCT ▸ CORE " + s.ReactorCap + " OF " + s.reactorUnits);
        if (!s.HgrOk) parts.Add("HGR ▸ FLIGHT OPS DOWN");
        if (!s.MagOk) parts.Add("MAG ▸ NO ORDNANCE");
        if (s.CompHealth("eng") < 5) parts.Add("ENG ▸ THRUST " + HandoffBoardState.JsRound(s.EngFactor * 100f) + "%");
        if (degradedLine)
        {
            degradedLine.text = Html(parts.Count > 0 ? string.Join("   ", parts.ToArray()) : "ALL SYSTEMS NOMINAL");
            Paint(degradedLine, parts.Count > 0 ? caution : nominal);
        }

        // partyLine
        if (partyLine)
        {
            var pc = s.PartyOut ? s.Comp(s.partyComp) : null;
            partyLine.text = Html(s.PartyOut ? "DC PARTY ▸ " + (pc != null ? pc.name : s.partyComp) + "   " + s.partyT + " OF " + s.partyTicks : "DC PARTY ▸ STANDBY");
            Paint(partyLine, s.PartyOut ? ownForce : wellStencil);
        }

        // status row: party line at its real width on the right; degraded line gets what is left
        if (degradedLine && partyLine)
        {
            float partyW = partyLine.GetPreferredValues(partyLine.text).x;
            float w = Mathf.Clamp(rowRight - partyW - rowGap - rowLeft, 0f, degradedMax);
            var rt = degradedLine.rectTransform;
            degradedLine.overflowMode = TextOverflowModes.Ellipsis;
            // Ellipsis also cuts vertically: a 9.00 px box hides a 9.02 px line entirely (Step 33e
            // verification). Let the box reach the line's height; it is centred, so the baseline holds.
            float lineH = Mathf.Max(rt.sizeDelta.y, Mathf.Ceil(degradedLine.GetPreferredValues(degradedLine.text, 9999f, 9999f).y));
            rt.sizeDelta = new Vector2(w, lineH);
            rt.anchoredPosition = new Vector2(rowLeft + w / 2f, rt.anchoredPosition.y); // centre pivot, left edge fixed
        }

        // ACTING ON: compAngle = 40 + index x 100 / (count - 1)
        int idx = s.CompIndex(s.engSel), n = s.compartments.Length;
        actingOn.Set(40f + idx * (100f / Mathf.Max(1, n - 1)));
        var sel = s.Comp(s.engSel);
        if (compName && sel != null) compName.text = sel.name;
        int h = s.CompHealth(s.engSel);
        Color hInk = h >= 4 ? nominal : h >= 2 ? caution : alarm;
        Paint(compLamp, hInk); Paint(compGlow, hInk);

        // repairs
        string firstBlock = null;
        var open = new System.Collections.Generic.List<string>();
        foreach (var r in repairs)
        {
            bool thrown = s.dcThrown.Contains("eng_" + r.id);
            string block = r.fitted ? s.EngBlock(r.id) : "NOT FITTED";
            bool isOpen = r.fitted && block == null;
            if (r.fitted) { if (isOpen) open.Add(Label(r.id)); else if (firstBlock == null) firstBlock = block; }
            if (r.rocker && r.fitted) r.rocker.sprite = thrown ? r.thrown : isOpen ? r.up : r.locked;
            Color lamp = thrown ? nominal : isOpen ? stencilDim : lampOffPanel;
            Paint(r.lamp, lamp); Paint(r.glow, nominal); Show(r.glow, thrown);
            Paint(r.label, isOpen ? label : stencil);
        }
        if (engNote)
        {
            engNote.text = open.Count > 0 ? string.Join(" · ", open.ToArray()) + " AVAILABLE" : firstBlock ?? "NO WORK AVAILABLE";
            Paint(engNote, open.Count > 0 ? nominal : caution);
        }
    }

    // ENG_REPAIRS labels
    static string Label(string id) => id == "reboot" ? "REBOOT" : id == "party" ? "REPAIR" : id == "seal" ? "SEAL" : id == "vent" ? "VENT" : id.ToUpperInvariant();
}
