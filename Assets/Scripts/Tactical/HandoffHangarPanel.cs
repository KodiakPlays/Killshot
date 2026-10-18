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
public class HandoffHangarPanel : HandoffPanel
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
    [SerializeField] private HandoffBoardControls.Dial doorSelector;
    [SerializeField] private Image doorLamp, doorGlow, doorInkLamp, pressInkLamp, catInkLamp;
    [SerializeField] private TMP_Text doorLabel, pressLabel, catLabel, catPct;
    [SerializeField] private RectTransform catWell, catFill;

    [Header("Squadrons")]
    [SerializeField] private SquadRow[] squadRows;
    [Tooltip("The selected squadron's bezel (drawn #252), moved to the selected screen.")]
    [SerializeField] private RectTransform bezel;
    [SerializeField] private float squadPitch = 87f;

    [Header("Launch order")]
    [SerializeField] private HandoffBoardControls.Dial shipSelector, commandSelector;
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
    [Tune("Motion")] [SerializeField] private HandoffEased recallPress = new HandoffEased(0.1f, CssEase.Ease);
    [SerializeField] private Image recallOverlay;

    [Header("Colours (design sRGB)")]
    [SerializeField] private Color nominal = new Color32(0x8F, 0xC3, 0x3A, 0xFF);
    [SerializeField] private Color caution = new Color32(0xE0, 0xA0, 0x20, 0xFF);
    [SerializeField] private Color alarm = new Color32(0xE0, 0x4B, 0x3C, 0xFF);
    [SerializeField] private Color ownForce = new Color32(0x4A, 0x9F, 0xD8, 0xFF);
    [SerializeField] private Color data = new Color32(0xE8, 0xED, 0xF0, 0xFF);
    [SerializeField] private Color label = new Color32(0xC9, 0xD2, 0xD8, 0xFF);
    [SerializeField] private Color stencil = new Color32(0x77, 0x82, 0x8A, 0xFF);
    [SerializeField] private Color wellStencil = new Color32(0x5B, 0x66, 0x6F, 0xFF);
    [SerializeField] private Color lampOffPanel = new Color32(0x22, 0x26, 0x2A, 0xFF);
    [SerializeField] private Color lampOffSelector = new Color32(0x1B, 0x20, 0x24, 0xFF);

    private float leverX0, leverV0;
    private bool leverHeld, launchedThisPull, recallHeld;
    private System.Action<float> showRecall;
    private float bezelHomeY = float.NaN;

    public override bool Click(string action, string arg)
    {
        var s = State;
        switch (action)
        {
            case "hangar.doors": s.ToggleDoors(); return true;
            case "hangar.squad": s.CycleSquad(); return true;
            case "hangar.order": s.CycleOrder(); return true;
            case "hangar.stage": s.PressStage(int.Parse(arg)); return true;
            case "hangar.recall": s.RecallSquad(); return true;
        }
        return false;
    }

    public override bool Down(string action, string arg, Vector2 board)
    {
        if (action == "hangar.recall") { recallHeld = true; return true; }
        if (action != "hangar.lever") return false;
        var s = State;
        // llDown: refused unless the launch is clear and every stage is at 100%
        if (s.LaunchBlock() != null || !s.AllStagesDone) return true;
        leverHeld = true; launchedThisPull = false;
        leverX0 = board.x; leverV0 = s.launchLever;
        return true;
    }

    public override bool Drag(string action, string arg, Vector2 board)
    {
        if (action != "hangar.lever" || !leverHeld) return false;
        var s = State;
        s.launchLever = Mathf.Clamp01(leverV0 + (board.x - leverX0) / leverTravelPx);
        if (s.launchLever > s.leverFireAt && !launchedThisPull) { launchedThisPull = true; s.LaunchSquad(); }
        return true;
    }

    public override bool Up(string action, string arg, Vector2 board)
    {
        if (action == "hangar.recall") { recallHeld = false; return true; }
        if (action != "hangar.lever") return false;
        leverHeld = false;
        State.launchLever = 0f; // springs back
        return true;
    }

    /// <summary>The launch bearing field: called by the input field as it is typed in.</summary>
    public void OnBearingTyped(string text)
    {
        if (Controls == null) return;
        State.SetLbrg(text);
        Refresh();
    }

    public void OnBearingDone(string text)
    {
        if (Controls == null) return;
        State.SetLbrg(text);
        if (bearing) bearing.SetTextWithoutNotify(State.lbrg.ToString("000"));
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
        var q = s.CurrentSquad;
        bool out_ = s.Launched(q);

        // header: hangarReady
        int nReady = 0;
        foreach (var sq in s.squads) nReady += s.SquadReady(sq);
        int air = s.launchedSquads.Count;
        if (ready) ready.text = air > 0 ? nReady + " READY / " + air + " AIRBORNE" : nReady + " CRAFT READY";
        if (ready && headerScreen)
        {
            float w = Mathf.Max(headerMinWidth, ready.GetPreferredValues(ready.text).x + 2f * headerPad);
            headerScreen.sizeDelta = new Vector2(w + 2f * headerMargin, headerScreen.sizeDelta.y);
            headerScreen.anchoredPosition = new Vector2(headerLeft + w / 2f, headerScreen.anchoredPosition.y);
        }

        // bay status
        doorSelector.Set(s.doorsOpen ? 120f : 60f);
        Paint(doorLamp, s.doorsOpen ? alarm : nominal); Paint(doorGlow, s.doorsOpen ? alarm : nominal);
        if (doorLabel) doorLabel.text = s.doorsOpen ? "OPEN" : "SEALED";
        Paint(doorInkLamp, s.doorsOpen ? ownForce : stencil);
        if (pressLabel) pressLabel.text = s.doorsOpen ? "VACUUM" : "1.0 ATM";
        Paint(pressInkLamp, s.doorsOpen ? caution : nominal);
        float cat = s.stages[5].progress;
        Color catInk = cat >= 100f ? ownForce : cat > 0f ? caution : stencil;
        if (catLabel) catLabel.text = cat >= 100f ? "CHARGED" : cat > 0f ? "CHARGING" : "HOLD";
        Paint(catInkLamp, catInk);
        if (catPct) catPct.text = HandoffBoardState.JsRound(cat) + "%";
        Bar(catFill, catWell, HandoffBoardState.JsRound(cat) / 100f, catInk, catBarMotion); // Math.round(lprog.cat) + "%"

        // squadrons
        for (int r = 0; r < squadRows.Length && r < s.squads.Length; r++)
        {
            var row = squadRows[r];
            var sq = s.squads[r];
            bool sel = r == s.squad, sqOut = s.Launched(sq);
            Paint(row.name, sel ? data : label);
            if (row.state) { row.state.text = sqOut ? "AIRBORNE — " + Label(sq, s.LaunchedOrder(sq)) : "IN BAY"; Paint(row.state, sqOut ? ownForce : label); }
            if (row.ready) row.ready.text = s.SquadReady(sq) + " OF " + sq.craft.Length + " READY";
            Paint(row.selLamp, sel ? data : lampOffSelector);
            Paint(row.selGlow, data); Show(row.selGlow, sel);
            for (int c = 0; c < sq.craft.Length; c++)
            {
                var cr = sq.craft[c];
                Color ink = sqOut ? ownForce : cr.state == "DOWN" ? alarm : cr.state == "REARM" ? caution : nominal;
                if (c < row.tags.Length) Paint(row.tags[c], ink);
                if (c < row.fuel.Length && c < row.cells.Length) FuelBar(row.fuel[c], row.cells[c], cr.fuel / 100f, ink);
            }
        }
        if (bezel)
        {
            if (float.IsNaN(bezelHomeY)) bezelHomeY = bezel.anchoredPosition.y;
            bezel.anchoredPosition = new Vector2(bezel.anchoredPosition.x, bezelHomeY - squadPitch * s.squad);
        }

        // launch order
        shipSelector.Set(60f + s.squad * 60f);
        int oi = Mathf.Max(0, System.Array.IndexOf(q.orders, s.order));
        commandSelector.Set(60f + oi * 30f);
        if (squadName) squadName.text = q.name;
        if (orderName) orderName.text = Label(q, s.order);
        int dev = s.LaunchDev;
        if (devLabel) { devLabel.text = (dev > s.launchConeDeg ? "+" : "") + dev + "°"; Paint(devLabel, dev > s.launchConeDeg ? alarm : dev > 40 ? caution : nominal); }
        if (bearing && !bearing.isFocused) bearing.SetTextWithoutNotify(s.lbrg.ToString("000"));

        // launch sequence: stageCells
        for (int i = 0; i < stages.Length && i < s.stages.Length; i++)
        {
            var g = s.stages[i];
            var row = stages[i];
            bool prevOk = i == 0 || s.stages[i - 1].progress >= 100f;
            bool live = prevOk && s.StageBlock(g.id) == null && !out_;
            bool done = g.progress >= 100f;
            Color ink = done ? nominal : g.on ? caution : alarm;
            if (row.rocker) row.rocker.sprite = g.on ? rockerThrown : live ? rockerUp : rockerLocked;
            Paint(row.lamp, g.on || done ? ink : lampOffPanel);
            Paint(row.label, done ? data : live ? label : stencil);
            Bar(row.fill, row.well, g.progress / 100f, ink, StageMotion(row.fill));
        }
        string block = s.LaunchBlock();
        if (interlock)
        {
            string t = null;
            if (out_) t = "SQUADRON AIRBORNE — RECALL TO RECOVER";
            else
            {
                foreach (var g in s.stages) if (g.on && g.progress < 100f) { t = g.label + " — " + HandoffBoardState.JsRound(g.progress) + "%"; break; }
                if (t == null) foreach (var g in s.stages) if (g.progress < 100f) { t = s.StageBlock(g.id) ?? g.label + " NOT SET"; break; }
                if (t == null) t = block ?? "ALL SYSTEMS GO";
            }
            interlock.text = t;
            Paint(interlock, out_ ? ownForce : block != null ? caution : s.AllStagesDone ? nominal : caution);
        }
        bool ready_ = block == null && s.AllStagesDone;
        Paint(launchLamp, out_ ? ownForce : ready_ ? nominal : lampOffPanel);

        // lever
        if (leverSlot && leverCap)
        {
            float slotLeft = leverSlot.anchoredPosition.x - leverSlot.sizeDelta.x / 2f + slotMargin;
            float slotW = leverSlot.sizeDelta.x - 2f * slotMargin;
            float capLeft = slotLeft + slotW * (capLeftPct + s.launchLever * capTravelPct) / 100f;
            leverCap.anchoredPosition = new Vector2(capLeft - capMargin + leverCap.sizeDelta.x / 2f, leverCap.anchoredPosition.y);
            if (leverFill)
            {
                leverFill.sizeDelta = new Vector2(slotW * s.launchLever, leverFill.sizeDelta.y);
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

    static string Label(HandoffBoardState.Squad q, string order)
    {
        int i = System.Array.IndexOf(q.orders, order);
        return i >= 0 ? q.orderLabels[i] : "—";
    }

    [Header("Bar transitions (UIStateSpec §9.1): CATAPULT .4s ease (line 1142), stages .4s linear")]
    [SerializeField] private HandoffEased catBarMotion = new HandoffEased(0.4f, CssEase.Ease);
    [SerializeField] private float stageBarSeconds = 0.4f;
    private readonly System.Collections.Generic.Dictionary<RectTransform, HandoffEased> stageMotion = new System.Collections.Generic.Dictionary<RectTransform, HandoffEased>();

    HandoffEased StageMotion(RectTransform fill)
    {
        HandoffEased m;
        if (!fill) return null;
        if (!stageMotion.TryGetValue(fill, out m)) stageMotion[fill] = m = new HandoffEased(stageBarSeconds, CssEase.Linear);
        m.seconds = stageBarSeconds; // follows the inspector during play
        return m;
    }

    // A loading bar inside its well: left-anchored, width = fraction of the well, eased between ticks.
    static void Bar(RectTransform fill, RectTransform well, float f, Color design, HandoffEased motion)
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
