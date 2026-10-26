using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Step 33b. The Power drawer: reactor core, emergency load shed and the four bus columns. It only shows
/// our ship's PowerSystem and passes the clicks to it (owner 2026-10-26). Every label and colour is from
/// board-logic.js renderVals (loadLabel, loadColor, reactorLabel, r1–r10, coreCells, coreHealthLabel,
/// shedNote, shedLamp, buses[]) and UIStateSpec §11 "Power: buses, standby and venting".
/// </summary>
public class PowerPanel : HandoffPanel
{
    [System.Serializable]
    public class BusColumn
    {
        [Tooltip("Bar cells, bottom (s1) to top (s5).")]
        public Image[] cells;
        public TMP_Text units;
        public TMP_Text mode;
        public Image lamp, lampGlow;
        public HandoffBoardControls.Dial selector;
    }

    [Header("Header and core")]
    [SerializeField] private TMP_Text loadLabel;
    [SerializeField] private TMP_Text reactorLabel;
    [Tooltip("The lit face of each of the ten reactor unit cells, left (r1) to right (r10).")]
    [SerializeField] private Graphic[] reactorCells;
    [SerializeField] private Image[] coreCells;
    [SerializeField] private Image[] coreGlows;
    [SerializeField] private TMP_Text coreHealthLabel;

    [Header("Emergency load shed")]
    [SerializeField] private Image shedRocker;
    [SerializeField] private Sprite rockerUp, rockerThrown;
    [SerializeField] private Image shedLamp, shedGlow;
    [SerializeField] private TMP_Text shedNote;

    [Header("Bus columns — ENG, WEP, SEN, AUX")]
    [SerializeField] private BusColumn[] columns;

    [Header("Colours (design sRGB)")]
    [SerializeField] private Color nominal = new Color32(0x8F, 0xC3, 0x3A, 0xFF);
    [SerializeField] private Color caution = new Color32(0xE0, 0xA0, 0x20, 0xFF);
    [SerializeField] private Color alarm = new Color32(0xE0, 0x4B, 0x3C, 0xFF);
    [SerializeField] private Color label = new Color32(0xC9, 0xD2, 0xD8, 0xFF);
    [SerializeField] private Color lampOffPanel = new Color32(0x22, 0x26, 0x2A, 0xFF);
    [SerializeField] private Color coreUnlit = new Color32(0x2A, 0x2F, 0x34, 0xFF);
    [Tooltip("tokens.json 'white': lit data-bar cells. The package has no measured lit bus cell (see HANDOFF_GAPS).")]
    [SerializeField] private Color busCellLit = Color.white;
    [Tooltip("The empty cell as measured in layout.json (sprite.latch.01).")]
    [SerializeField] private Color busCellEmpty = new Color32(0x0E, 0x13, 0x16, 0xFF);

    public override bool Click(string action, string arg)
    {
        switch (action)
        {
            case "power.shed": State.power.ToggleShed(); return true;
            case "power.bus": State.power.CycleBus(int.Parse(arg)); return true;
        }
        return false;
    }

    public override void Refresh()
    {
        var s = State;
        var p = s.power;
        int cap = p.ReactorCap, total = p.TotalFill, left = cap - total;

        if (loadLabel) { loadLabel.text = total + " / " + cap + " UNITS"; Paint(loadLabel, left <= 0 ? alarm : left <= 2 ? caution : nominal); }
        if (reactorLabel) reactorLabel.text = left + " OF " + cap + " UNITS AVAILABLE";
        for (int i = 0; i < reactorCells.Length; i++) Show(reactorCells[i], left >= i + 1);

        int health = s.CompHealth("rct");
        Color healthInk = health >= 4 ? nominal : health >= 2 ? caution : alarm;
        for (int i = 0; i < coreCells.Length; i++)
        {
            bool lit = health >= i + 1;
            Paint(coreCells[i], lit ? healthInk : coreUnlit);
            if (i < coreGlows.Length) { Show(coreGlows[i], lit); Paint(coreGlows[i], healthInk); }
        }
        if (coreHealthLabel) { coreHealthLabel.text = health + " / 5"; Paint(coreHealthLabel, healthInk); }

        if (shedRocker) shedRocker.sprite = p.shed ? rockerThrown : rockerUp;
        Paint(shedLamp, p.shed ? alarm : lampOffPanel);
        Show(shedGlow, p.shed);
        if (shedNote) { shedNote.text = p.shed ? "ALL BUSES VENTING — SELECTORS HELD" : "SHED READY"; Paint(shedNote, p.shed ? alarm : label); }

        for (int j = 0; j < columns.Length && j < p.buses.Length; j++)
        {
            var b = p.buses[j];
            var col = columns[j];
            for (int k = 0; k < col.cells.Length; k++) Paint(col.cells[k], b.fill >= k + 1 ? busCellLit : busCellEmpty);
            if (col.units) col.units.text = b.fill.ToString();
            Color ink = b.mode == PowerSystem.BusMode.Vent ? alarm : b.mode == PowerSystem.BusMode.Draw ? nominal : caution;
            if (col.mode) { col.mode.text = b.mode == PowerSystem.BusMode.Standby ? "STBY" : b.mode == PowerSystem.BusMode.Draw ? "DRAW" : "VENT"; Paint(col.mode, ink); }
            Paint(col.lamp, ink);
            Paint(col.lampGlow, ink);
            col.selector.Set(b.mode == PowerSystem.BusMode.Vent ? -45f : b.mode == PowerSystem.BusMode.Draw ? 45f : 0f);
        }
    }
}
