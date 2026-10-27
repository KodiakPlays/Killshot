using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Step 33f. The Hangar drawer: bay status, the two squadron screens, the launch order (ship,
/// launch bearing, command), the six launch-stage rockers with their loading bars, the launch lever
/// and RECALL. Rules and values from board-logic.js (toggleDoors, setLbrg, launchDev, stageCells,
/// stageBlock, pressStage, cycleSquad, cycleOrder, launchBlock, launchSquad, recallSquad, llDown and
/// the lever drag, the launch part of the tick; renderVals hangarReady, door*, press*, cat*,
/// squads, squadAngle, orderAngle, devLabel/Ink, interlock/Ink, launchLamp, llLeft, llTrack,
/// llHint/Ink, recallLamp) and UIStateSpec §8.2 and §11 "Hangar: launching a squadron".
/// </summary>
public class HangarPanel : Panel
{
    [System.Serializable]
    public class SquadRow
    {
        public TMP_Text name, state, ready, role;
        public TMP_Text[] tags;
        [Tooltip("Fuel bar per craft and the cell it fills, bottom up.")]
        public RectTransform[] fuel;
        public RectTransform[] cells;
        public Image selLamp, selGlow;
    }

    [System.Serializable]
    public class StageRow
    {
        public Image rocker, lamp;
        public TMP_Text label;
        public RectTransform well, fill;
    }

    [Header("Header and bay status")]
    [SerializeField] private TMP_Text ready;
    [Tooltip("The header screen (sprite #227) grows with its readout, anchored left: measured element "
           + "1522.5 wide 106.5, the text 9 px in from each side, the sprite's shadow margin 4 px "
           + "(Step 33f verification — '7 READY / 1 AIRBORNE' ran out of it).")]
    [SerializeField] private RectTransform headerScreen;
    [SerializeField] private float headerLeft = 1522.5f, headerMinWidth = 106.5f, headerPad = 9f, headerMargin = 4f;
    [SerializeField] private BoardManager.Dial doorSelector;
    [SerializeField] private Image doorLamp, doorGlow, doorInkLamp, pressInkLamp, catInkLamp;
    [SerializeField] private TMP_Text doorLabel, pressLabel, catLabel, catPct;
    [SerializeField] private RectTransform catWell, catFill;

    [Header("Squadrons")]
    [SerializeField] private SquadRow[] squadRows;
    [Tooltip("The selected squadron's bezel (drawn #252), moved to the selected screen.")]
    [SerializeField] private RectTransform bezel;
    [SerializeField] private float squadPitch = 87f;

    [Header("Launch order")]
    [SerializeField] private BoardManager.Dial shipSelector, commandSelector;
    [SerializeField] private TMP_Text squadName, orderName, devLabel;
    [SerializeField] private TMP_InputField bearing;

    [Header("Launch sequence")]
    [SerializeField] private StageRow[] stages;
    [SerializeField] private Sprite rockerUp, rockerLocked, rockerThrown;
    [SerializeField] private TMP_Text interlock, leverHint;
    [SerializeField] private Image launchLamp;
    [SerializeField] private RectTransform leverSlot, leverCap, leverFill;
    [Tooltip("llLeft: the cap's left edge is (2 + lever x 72)% of the slot; full travel 210 px.")]
    [SerializeField] private float capLeftPct = 2f, capTravelPct = 72f, leverTravelPx = 210f;
    [SerializeField] private Image recall, recallLamp;
    [SerializeField] private Sprite recallUp, recallPressed;
    [Tooltip("RECALL's press eases over .1 s (UIStateSpec §9.1, template line 1294): the pressed art fades in over the up art.")]
    [Tune("Motion")] [SerializeField] private Eased recallPress = new Eased(0.1f, CssEase.Ease);
    [SerializeField] private Image recallOverlay;

    Color nominal => Palette.nominal;
    Color caution => Palette.caution;
    Color alarm => Palette.alarm;
    Color ownForce => Palette.ownForce;
    Color data => Palette.data;
    Color label => Palette.label;
    Color stencil => Palette.stencil;
    Color wellStencil => Palette.wellStencil;
    Color lampOffPanel => Palette.lampOffPanel;
    Color lampOffSelector => Palette.lampOffSelector;

    private float leverX0, leverV0;
    private bool leverHeld, launchedThisPull, recallHeld;
    private System.Action<float> showRecall;
    private float bezelHomeY = float.NaN;

    public override bool Click(string action, string arg)
    {
        var s = State;
        switch (action)
        {
            case "hangar.doors": s.hangar.ToggleDoors(); return true;
            case "hangar.squad": s.hangar.CycleSquad(); return true;
            case "hangar.order": s.hangar.CycleOrder(); return true;
            case "hangar.stage": s.hangar.PressStage(int.Parse(arg)); return true;
            case "hangar.recall": s.hangar.RecallSquad(); return true;
        }
        return false;
    }

    public override bool Down(string action, string arg, Vector2 board)
    {
        if (action == "hangar.recall") { recallHeld = true; return true; }
        if (action != "hangar.lever") return false;
        var s = State;
        // llDown: refused unless the launch is clear and every stage is at 100%
        if (s.hangar.LaunchBlock() != null || !s.hangar.AllStagesDone) return true;
        leverHeld = true; launchedThisPull = false;
        leverX0 = board.x; leverV0 = s.hangar.launchLever;
        return true;
    }

    public override bool Drag(string action, string arg, Vector2 board)
    {
        if (action != "hangar.lever" || !leverHeld) return false;
        var s = State;
        s.hangar.launchLever = Mathf.Clamp01(leverV0 + (board.x - leverX0) / leverTravelPx);
        if (s.hangar.launchLever > s.hangar.leverFireAt && !launchedThisPull) { launchedThisPull = true; s.hangar.LaunchSquad(); }
        return true;
    }

    public override bool Up(string action, string arg, Vector2 board)
    {
        if (action == "hangar.recall") { recallHeld = false; return true; }
        if (action != "hangar.lever") return false;
        leverHeld = false;
        State.hangar.launchLever = 0f; // springs back
        return true;
    }

    /// <summary>The launch bearing field: called by the input field as it is typed in.</summary>
    public void OnBearingTyped(string text)
    {
        if (Controls == null) return;
        State.hangar.SetLbrg(text);
        Refresh();
    }

    public void OnBearingDone(string text)
    {
        if (Controls == null) return;
        State.hangar.SetLbrg(text);
        if (bearing) bearing.SetTextWithoutNotify(State.hangar.lbrg.ToString("000"));
        Refresh();
    }

    void ShowRecall(float k)
    {
        if (recall) recall.sprite = k >= 1f ? recallPressed : recallUp;
        if (!recallOverlay) return;
        recallOverlay.enabled = k > 0f && k < 1f;
        var c = recallOverlay.color; c.a = k; recallOverlay.color = c;
    }

    public override void Refresh()
    {
        if (showRecall == null) showRecall = ShowRecall;
        recallPress.Set(recallHeld ? 1f : 0f, showRecall);
        var s = State;
        var q = s.hangar.CurrentSquad;
        bool out_ = s.hangar.Launched(q);

        // header: hangarReady
        int nReady = 0;
        foreach (var sq in s.hangar.squads) nReady += s.hangar.SquadReady(sq);
        int air = s.hangar.launchedSquads.Count;
        if (ready) ready.text = air > 0 ? nReady + " READY / " + air + " AIRBORNE" : nReady + " CRAFT READY";
        if (ready && headerScreen)
        {
            float w = Mathf.Max(headerMinWidth, ready.GetPreferredValues(ready.text).x + 2f * headerPad);
            headerScreen.sizeDelta = new Vector2(w + 2f * headerMargin, headerScreen.sizeDelta.y);
            headerScreen.anchoredPosition = new Vector2(headerLeft + w / 2f, headerScreen.anchoredPosition.y);
        }

        // bay status
        doorSelector.Set(s.hangar.doorsOpen ? 120f : 60f);
        Paint(doorLamp, s.hangar.doorsOpen ? alarm : nominal); Paint(doorGlow, s.hangar.doorsOpen ? alarm : nominal);
        if (doorLabel) doorLabel.text = s.hangar.doorsOpen ? "OPEN" : "SEALED";
        Paint(doorInkLamp, s.hangar.doorsOpen ? ownForce : stencil);
        if (pressLabel) pressLabel.text = s.hangar.doorsOpen ? "VACUUM" : "1.0 ATM";
        Paint(pressInkLamp, s.hangar.doorsOpen ? caution : nominal);
        float cat = s.hangar.stages[5].progress;
        Color catInk = cat >= 100f ? ownForce : cat > 0f ? caution : stencil;
        if (catLabel) catLabel.text = cat >= 100f ? "CHARGED" : cat > 0f ? "CHARGING" : "HOLD";
        Paint(catInkLamp, catInk);
        if (catPct) catPct.text = ShipState.JsRound(cat) + "%";
        Bar(catFill, catWell, ShipState.JsRound(cat) / 100f, catInk, catBarMotion); // Math.round(lprog.cat) + "%"

        // squadrons
        for (int r = 0; r < squadRows.Length && r < s.hangar.squads.Length; r++)
        {
            var row = squadRows[r];
            var sq = s.hangar.squads[r];
            bool sel = r == s.hangar.squad, sqOut = s.hangar.Launched(sq);
            Paint(row.name, sel ? data : label);
            if (row.state) { row.state.text = sqOut ? "AIRBORNE — " + Label(sq, s.hangar.LaunchedOrder(sq)) : "IN BAY"; Paint(row.state, sqOut ? ownForce : label); }
            if (row.ready) row.ready.text = s.hangar.SquadReady(sq) + " OF " + sq.craft.Length + " READY";
            Paint(row.selLamp, sel ? data : lampOffSelector);
            Paint(row.selGlow, data); Show(row.selGlow, sel);
            for (int c = 0; c < sq.craft.Length; c++)
            {
                var cr = sq.craft[c];
                Color ink = sqOut ? ownForce : cr.state == "DOWN" || cr.state == "LOST" ? alarm : cr.state == "REARM" ? caution : nominal;
                if (c < row.tags.Length) Paint(row.tags[c], ink);
                if (c < row.fuel.Length && c < row.cells.Length) FuelBar(row.fuel[c], row.cells[c], cr.fuel / 100f, ink);
            }
        }
        if (bezel)
        {
            if (float.IsNaN(bezelHomeY)) bezelHomeY = bezel.anchoredPosition.y;
            bezel.anchoredPosition = new Vector2(bezel.anchoredPosition.x, bezelHomeY - squadPitch * s.hangar.squad);
        }

        // launch order
        shipSelector.Set(60f + s.hangar.squad * 60f);
        int oi = Mathf.Max(0, System.Array.IndexOf(q.orders, s.hangar.order));
        commandSelector.Set(60f + oi * 30f);
        if (squadName) squadName.text = q.name;
        if (orderName) orderName.text = Label(q, s.hangar.order);
        int dev = s.hangar.LaunchDev;
        if (devLabel) { devLabel.text = (dev > s.hangar.launchConeDeg ? "+" : "") + dev + "°"; Paint(devLabel, dev > s.hangar.launchConeDeg ? alarm : dev > 40 ? caution : nominal); }
        if (bearing && !bearing.isFocused) bearing.SetTextWithoutNotify(s.hangar.lbrg.ToString("000"));

        // launch sequence: stageCells
        for (int i = 0; i < stages.Length && i < s.hangar.stages.Length; i++)
        {
            var g = s.hangar.stages[i];
            var row = stages[i];
            bool prevOk = i == 0 || s.hangar.stages[i - 1].progress >= 100f;
            bool live = prevOk && s.hangar.StageBlock(g.id) == null && !out_;
            bool done = g.progress >= 100f;
            Color ink = done ? nominal : g.on ? caution : alarm;
            if (row.rocker) row.rocker.sprite = g.on ? rockerThrown : live ? rockerUp : rockerLocked;
            Paint(row.lamp, g.on || done ? ink : lampOffPanel);
            Paint(row.label, done ? data : live ? label : stencil);
            Bar(row.fill, row.well, g.progress / 100f, ink, StageMotion(row.fill));
        }
        string block = s.hangar.LaunchBlock();
        if (interlock)
        {
            string t = null;
            if (out_) t = s.hangar.FlightStatus(q.key) ?? "SQUADRON AIRBORNE — RECALL TO RECOVER"; // Step 54: what it is doing
            else
            {
                foreach (var g in s.hangar.stages) if (g.on && g.progress < 100f) { t = g.label + " — " + ShipState.JsRound(g.progress) + "%"; break; }
                if (t == null) foreach (var g in s.hangar.stages) if (g.progress < 100f) { t = s.hangar.StageBlock(g.id) ?? g.label + " NOT SET"; break; }
                if (t == null) t = block ?? "ALL SYSTEMS GO";
            }
            interlock.text = t;
            Paint(interlock, out_ ? ownForce : block != null ? caution : s.hangar.AllStagesDone ? nominal : caution);
        }
        bool ready_ = block == null && s.hangar.AllStagesDone;
        Paint(launchLamp, out_ ? ownForce : ready_ ? nominal : lampOffPanel);

        // lever
        if (leverSlot && leverCap)
        {
            float slotLeft = leverSlot.anchoredPosition.x - leverSlot.sizeDelta.x / 2f + slotMargin;
            float slotW = leverSlot.sizeDelta.x - 2f * slotMargin;
            float capLeft = slotLeft + slotW * (capLeftPct + s.hangar.launchLever * capTravelPct) / 100f;
            leverCap.anchoredPosition = new Vector2(capLeft - capMargin + leverCap.sizeDelta.x / 2f, leverCap.anchoredPosition.y);
            if (leverFill)
            {
                leverFill.sizeDelta = new Vector2(slotW * s.hangar.launchLever, leverFill.sizeDelta.y);
                leverFill.anchoredPosition = new Vector2(slotLeft + leverFill.sizeDelta.x / 2f, leverFill.anchoredPosition.y);
            }
        }
        if (leverHint)
        {
            leverHint.text = out_ ? "AIRBORNE" : ready_ ? "READY" : "SAFE";
            Paint(leverHint, out_ ? ownForce : ready_ ? nominal : wellStencil);
        }
        Paint(recallLamp, out_ ? caution : lampOffPanel);
    }

    [Tooltip("Shadow margins of the lever slot and cap sprites as built (their rects include them).")]
    [SerializeField] private float slotMargin = 4f, capMargin = 12f;

    static string Label(HangarSystem.Squad q, string order)
    {
        int i = System.Array.IndexOf(q.orders, order);
        return i >= 0 ? q.orderLabels[i] : "—";
    }

    [Header("Bar transitions (UIStateSpec §9.1): CATAPULT .4s ease (line 1142), stages .4s linear")]
    [SerializeField] private Eased catBarMotion = new Eased(0.4f, CssEase.Ease);
    [SerializeField] private float stageBarSeconds = 0.4f;
    private readonly System.Collections.Generic.Dictionary<RectTransform, Eased> stageMotion = new System.Collections.Generic.Dictionary<RectTransform, Eased>();

    Eased StageMotion(RectTransform fill)
    {
        Eased m;
        if (!fill) return null;
        if (!stageMotion.TryGetValue(fill, out m)) stageMotion[fill] = m = new Eased(stageBarSeconds, CssEase.Linear);
        m.seconds = stageBarSeconds; // follows the inspector during play
        return m;
    }

    // A loading bar inside its well: left-anchored, width = fraction of the well, eased between ticks.
    static void Bar(RectTransform fill, RectTransform well, float f, Color design, Eased motion)
    {
        if (!fill || !well) return;
        var img = fill.GetComponent<Image>();
        if (img) { var c = design.gamma; c.a = 1f; img.color = c; }
        motion.Set(Mathf.Clamp01(f), v =>
        {
            float w = well.sizeDelta.x * v;
            float left = well.anchoredPosition.x - well.sizeDelta.x / 2f;
            fill.sizeDelta = new Vector2(w, fill.sizeDelta.y);
            fill.anchoredPosition = new Vector2(left + w / 2f, fill.anchoredPosition.y);
            fill.gameObject.SetActive(w > 0f);
        });
    }

    // A craft's fuel bar: bottom-aligned in its cell, height = fuel fraction of the cell.
    static void FuelBar(RectTransform bar, RectTransform cell, float f, Color design)
    {
        if (!bar || !cell) return;
        float h = cell.sizeDelta.y * Mathf.Clamp01(f);
        float bottom = cell.anchoredPosition.y - cell.sizeDelta.y / 2f;
        bar.sizeDelta = new Vector2(cell.sizeDelta.x, h);
        bar.anchoredPosition = new Vector2(cell.anchoredPosition.x, bottom + h / 2f);
        var img = bar.GetComponent<Image>();
        if (img) { var c = design.gamma; c.a = 1f; img.color = c; }
        bar.gameObject.SetActive(h > 0f);
    }
}
