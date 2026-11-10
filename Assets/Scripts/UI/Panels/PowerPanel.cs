// =================================================================================================
// PowerPanel.cs
//
// What it does:
// The Power drawer on the battle board. It shows the reactor and the four power buses: ENG, WEP,
// SEN and AUX. The player sets each bus to DRAW, STBY or VENT, and can throw the emergency load
// shed. It is on the Power prefab. The power rules are in PowerSystem.
//
// How it works:
// - BoardManager passes it clicks. A bus switch steps to the next setting, back and forth across
//   VENT, STBY and DRAW. The load shed rocker sets every bus to VENT. Thrown again, it sets them
//   all to STBY. While the shed is on, the bus switches do nothing. PowerSystem makes each change.
// - BoardManager redraws it after every click and every 0.42 seconds. It reads PowerSystem and
//   DamageSystem through ShipState.
// - The top shows the units in use against the reactor's limit: green, amber with 2 or fewer left,
//   red with none left. Ten cells and a line of text show the units still free.
// - Five core cells show the health of the reactor room, as a number out of 5. They are green at 4
//   or 5, amber at 2 or 3, and red below.
// - The load shed rocker, its lamp and its note show ALL BUSES VENTING in red, or SHED READY.
// - Each bus column fills its cells to the bus's units and shows the number. The setting shows as
//   STBY in amber, DRAW in green or VENT in red. A lamp shows the same colour. The switch's
//   pointer turns left for VENT, up for STBY and right for DRAW.
// - The playtest runner finds this script to check that the drawer opens.
//
// Technical:
// - Data: the power state is in PowerSystem: buses (units, setting, step direction) and shed.
// - BoardManager calls Click and Refresh. The actions are "power.shed" and "power.bus" (arg = bus
//   0 to 3: ENG, WEP, SEN, AUX).
// - It calls PowerSystem.ToggleShed and CycleBus. It reads ReactorCap, TotalFill and buses, and
//   DamageSystem.CompHealth("rct").
// - PowerSystem.ReactorCap is the reactor room's health share of reactorUnits, never below 2.
// - Numbers: reactorUnits and the most units per bus are in the power part of the ship stats file.
// - Trap: the bus columns in the Inspector must be in the same order as PowerSystem.buses.
// - Trap: the amber line at 2 units left and the health colours are in the code.
// =================================================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerPanel : Panel
{
    [System.Serializable]
    public class BusColumn
    {
        [Tooltip("Bar cells, bottom (s1) to top (s5).")]
        public Image[] cells;
        public TMP_Text units;
        public TMP_Text mode;
        public Image lamp, lampGlow;
        public BoardManager.Dial selector;
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

    Color nominal => Palette.nominal;
    Color caution => Palette.caution;
    Color alarm => Palette.alarm;
    Color label => Palette.label;
    Color lampOffPanel => Palette.lampOffPanel;
    Color coreUnlit => Palette.coreUnlit;
    Color busCellLit => Palette.cellLit;
    Color busCellEmpty => Palette.cellUnlit;

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

        int health = s.damage.CompHealth("rct");
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
