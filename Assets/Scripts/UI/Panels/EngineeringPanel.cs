// =================================================================================================
// EngineeringPanel.cs
//
// What it does:
// The Engineering drawer on the battle board. It shows the damage to our ship: a top-down picture
// coloured by armour, a row for each room (compartment), and the repair rockers. The player picks
// a room and throws a rocker to repair it. It is on the Engineering prefab.
//
// How it works:
// - BoardManager passes it clicks. The ACTING ON selector steps to the next room, and clicking a
//   room picks it. A repair rocker asks DamageSystem to run that repair on the chosen room.
//   DamageSystem refuses a blocked repair.
// - BoardManager redraws it after every click and every 0.42 seconds. It reads DamageSystem,
//   PowerSystem and OrdersSystem through ShipState.
// - The ship picture has four parts: prow, starboard, aft and port. Each is white while its
//   side's armour is full, amber once hit and red once gone. The picture's size is in the Tuning
//   window, under "Engineering & orders".
// - Each room row shows a ring of five segments for its health. It shows SEAL or VENT, its
//   condition (NOM, FIRE, RAD) and its crew. Lit segments are white at 4 or 5, amber at 2 or 3, red
//   below.
// - The hazard line counts rooms with a condition or open to space, and adds up the crew.
// - The next line lists what is broken: reactor power below full, the hangar, the magazine, the
//   engine's thrust. With nothing broken it says ALL SYSTEMS NOMINAL. The line is cut short with
//   dots if it does not fit beside the damage control party line.
// - The party line shows the room the damage control party is working in. It shows the party's
//   time so far against the time it needs, in game seconds. Otherwise it says STANDBY.
// - The ACTING ON pointer turns to the chosen room. Its name shows, with a lamp in its health
//   colour.
// - Each rocker shows thrown, ready or locked. A note lists the repairs ready, or the first reason
//   a repair is blocked, or NO WORK AVAILABLE. A rocker marked not fitted stays dark.
// - The playtest runner reads its rows, ship picture and rockers.
//
// Technical:
// - Data: rows and repairs are Inspector lists on the Engineering prefab. Each row has a room key.
//   The fitted repair ids are party, seal, vent, reboot and recall. A sixth, dum2, is not fitted.
// - BoardManager calls Click and Refresh. The actions are "eng.cycle", "eng.pick" (arg = room key)
//   and "eng.run" (arg = repair id).
// - It calls DamageSystem.CycleComp, PickComp and EngRun. EngRun spends AUX through OrdersSystem
//   and throws the rocker.
// - It reads DamageSystem: Comp, CompHealth, compartments, engSel, SideArmour, EngBlock and the
//   party. It reads PowerSystem.ReactorCap and OrdersSystem.dcThrown.
// - A rocker is thrown while OrdersSystem.dcThrown has "eng_" plus its id. BoardManager clears it
//   when the flick time runs out.
// - Each ring segment is a Wedge. The ship picture is four RawImages in hullSides.
// - Numbers: the picture size is a [Tune] field under "Engineering & orders". Ring angles and row
//   edges are in the Inspector. Party time and armour are in the damage part of the stats file.
// - Trap: rowLeft and rowRight are board px. The degraded line's box is resized on every redraw.
// - Trap: PlaytestRunner reads the private fields hullSides, rows and repairs by name.
// =================================================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EngineeringPanel : Panel
{
    [System.Serializable]
    public class Row
    {
        public string key;
        public Image cell;
        public Image rowBg;
        [Tooltip("Five integrity segments, the first from 9 o'clock, clockwise.")]
        public Wedge[] ring;
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
    [SerializeField] private BoardManager.Dial actingOn;
    [SerializeField] private TMP_Text compName, engNote;
    [SerializeField] private Image compLamp, compGlow;
    [SerializeField] private Repair[] repairs;

    [Header("Ring — conic segments: 72° each with a 7° gap, from -90° (compartments[].ring)")]
    [SerializeField] private float segmentDeg = 72f, gapDeg = 7f, fromDeg = -90f;
    [SerializeField] private float ringDimAlpha = 0.72f;

    [Header("Colours (design sRGB)")]
    [Header("Ship diagram (Step 74, owner 2026-10-26): our ship bow up, each side coloured by its armour")]
    [Tooltip("Top-down pictures of the four parts of our ship (UpdatedStarships model), stacked, in the order "
           + "PROW, STBD, AFT, PORT. Each is white while its side's armour is full, amber once hit, red once open.")]
    [SerializeField] private RawImage[] hullSides;
    [Tooltip("The ship's length in the diagram, px.")]
    [Tune("Engineering & orders")] [SerializeField] private float diagramShipPx = 190f;
    [Tooltip("The ship's length as a share of each picture (they were saved at 0.8, like the plot's ship view).")]
    [SerializeField] private float pictureFill = 0.8f;
    Color shipInk => Palette.data;

    Color nominal => Palette.nominal;
    Color caution => Palette.caution;
    Color alarm => Palette.alarm;
    Color data => Palette.data;
    Color secondary => Palette.secondary;
    Color label => Palette.label;
    Color stencil => Palette.stencil;
    Color wellStencil => Palette.wellStencil;
    Color ownForce => Palette.ownForce;
    Color stencilDim => Palette.stencilDim;
    Color lampOffPanel => Palette.lampOffPanel;
    Color ringUnlit => Palette.ringUnlit;

    public override bool Click(string action, string arg)
    {
        var s = State;
        switch (action)
        {
            case "eng.cycle": s.damage.CycleComp(); return true;
            case "eng.pick": s.damage.PickComp(arg); return true;
            case "eng.run": s.damage.EngRun(arg, Time.unscaledTime); return true;
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
        ShowDiagram(s);

        // compartment screen and schematic
        foreach (var r in rows)
        {
            var c = s.damage.Comp(r.key);
            if (c == null) continue;
            bool on = r.key == s.damage.engSel;
            Fill(r.cell, data, on ? 0.16f : 0.05f);
            Fill(r.rowBg, data, on ? 0.07f : 0f);
            // Step 74: lit segments white whole (4–5), amber hurt (2–3), red nearly gone (0–1)
            int health = s.damage.CompHealth(r.key);
            Color ringLit = health >= 4 ? data : health >= 2 ? caution : alarm;
            for (int i = 0; i < r.ring.Length; i++)
            {
                var w = r.ring[i];
                if (!w) continue;
                w.Set(fromDeg + i * segmentDeg, segmentDeg - gapDeg);
                w.color = (c.integ >= i + 1 ? ringLit : ringUnlit).WithAlpha(on ? 1f : ringDimAlpha);
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
        foreach (var c in s.damage.compartments)
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
        if (s.power.ReactorCap < s.power.stats.reactorUnits) parts.Add("RCT ▸ CORE " + s.power.ReactorCap + " OF " + s.power.stats.reactorUnits);
        if (!s.damage.HgrOk) parts.Add("HGR ▸ FLIGHT OPS DOWN");
        if (!s.damage.MagOk) parts.Add("MAG ▸ NO ORDNANCE");
        if (s.damage.CompHealth("eng") < 5) parts.Add("ENG ▸ THRUST " + ShipState.JsRound(s.damage.EngFactor * 100f) + "%");
        if (degradedLine)
        {
            degradedLine.text = Html(parts.Count > 0 ? string.Join("   ", parts.ToArray()) : "ALL SYSTEMS NOMINAL");
            Paint(degradedLine, parts.Count > 0 ? caution : nominal);
        }

        // partyLine
        if (partyLine)
        {
            var pc = s.damage.PartyOut ? s.damage.Comp(s.damage.partyComp) : null;
            partyLine.text = Html(s.damage.PartyOut ? "DC PARTY ▸ " + (pc != null ? pc.name : s.damage.partyComp) + "   " + Mathf.FloorToInt(s.damage.partyT) + " OF " + Mathf.RoundToInt(s.damage.stats.partySeconds) : "DC PARTY ▸ STANDBY"); // game seconds (Step 57)
            Paint(partyLine, s.damage.PartyOut ? ownForce : wellStencil);
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
        int idx = s.damage.CompIndex(s.damage.engSel), n = s.damage.compartments.Length;
        actingOn.Set(40f + idx * (100f / Mathf.Max(1, n - 1)));
        var sel = s.damage.Comp(s.damage.engSel);
        if (compName && sel != null) compName.text = sel.name;
        int h = s.damage.CompHealth(s.damage.engSel);
        Color hInk = h >= 4 ? nominal : h >= 2 ? caution : alarm;
        Paint(compLamp, hInk); Paint(compGlow, hInk);

        // repairs
        string firstBlock = null;
        var open = new System.Collections.Generic.List<string>();
        foreach (var r in repairs)
        {
            bool thrown = s.orders.dcThrown.Contains("eng_" + r.id);
            string block = r.fitted ? s.damage.EngBlock(r.id) : "NOT FITTED";
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

    // Step 74: our ship, bow up, each side coloured by its armour: white full, amber hit, red open.
    // The rooms behind are the rings beside it.
    void ShowDiagram(ShipState s)
    {
        if (hullSides == null) return;
        var armour = s.damage.SideArmour;
        float side = diagramShipPx / pictureFill;
        for (int i = 0; i < hullSides.Length && i < armour.Length; i++)
        {
            var pic = hullSides[i];
            if (!pic) continue;
            pic.rectTransform.sizeDelta = new Vector2(side, side);
            Paint(pic, armour[i] >= s.damage.stats.sideArmourMax[i] ? shipInk : armour[i] > 0f ? caution : alarm);
        }
    }
}
