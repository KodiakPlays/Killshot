using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Step 33h-2. The orders station: the alert condition selector (stowed and open), the AUX order
/// pool, and the order rockers. Rules and values from board-logic.js (doAlert, runOrder, orderBlock,
/// flickBack, the AUX charge in tickPower; renderVals alertLabel, alertAngle, alertPositions,
/// alertInk, alertRelease(Ink), compName, auxPct, auxInk, auxRate, auxCells, orderSwitches,
/// orderNote(Ink)) and UIStateSpec §11 "Orders".
/// </summary>
public class OrdersPanel : HandoffPanel
{
    [System.Serializable]
    public class AlertSet
    {
        public HandoffBoardControls.Dial pointer;
        [Tooltip("C3, C2, C1.")]
        public Image[] lamps;
        public Graphic[] glows;
        public TMP_Text[] labels;
        public TMP_Text condition, release;
    }

    [System.Serializable]
    public class Order
    {
        public string id;
        [Tooltip("FIRE, BLKHD, SCRUB: unfitted blanking plates (tbd).")]
        public bool blank;
        public Image rocker;
        public Image lamp;
        public Graphic glow;
        public TMP_Text label;
    }

    [SerializeField] private AlertSet stowed, open;

    [Header("AUX order pool")]
    [SerializeField] private TMP_Text actingOn, auxPct, orderNote, auxRate;
    [Tooltip("Ten cells, left first. Lit look has no template — a stand-in fill (HANDOFF_GAPS).")]
    [SerializeField] private Graphic[] auxCells;

    [Header("Order rockers")]
    [SerializeField] private Order[] orders;
    [SerializeField] private Sprite rockerUp, rockerLocked, rockerThrown;

    Color nominal => Palette.nominal;
    Color caution => Palette.caution;
    Color alarm => Palette.alarm;
    Color data => Palette.data;
    Color label => Palette.label;
    Color stencil => Palette.stencil;
    Color stencilDim => Palette.stencilDim;
    Color phosphor => Palette.phosphor;
    Color noteInk => Palette.note;
    Color lampOffSelector => Palette.lampOffSelector;
    Color lampOffPanel => Palette.lampOffPanel;

    public override bool Click(string action, string arg)
    {
        switch (action)
        {
            case "orders.alert": State.orders.DoAlert(); return true;
            case "orders.run": State.orders.RunOrder(arg, Time.unscaledTime); return true;
        }
        return false;
    }

    public override void Refresh()
    {
        var s = State;
        string[] alerts = { "CONDITION 3", "CONDITION 2", "CONDITION 1" };
        Color alertInk = s.orders.alert == 2 ? alarm : s.orders.alert == 1 ? caution : nominal;
        bool released = s.orders.WeaponsReleased;
        foreach (var a in new[] { stowed, open })
        {
            if (a == null) continue;
            a.pointer.Set(60f + s.orders.alert * 30f); // alertAngle
            for (int i = 0; i < a.lamps.Length; i++)
            {
                bool on = i == s.orders.alert;
                Paint(a.lamps[i], on ? data : lampOffSelector);
                if (i < a.glows.Length) Show(a.glows[i], on);
                if (i < a.labels.Length) Paint(a.labels[i], on ? data : stencil);
            }
            if (a.condition) { a.condition.text = alerts[s.orders.alert]; Paint(a.condition, alertInk); }
            if (a.release)
            {
                a.release.text = released ? "WEAPONS AND FLIGHT OPS RELEASED" : "WEAPONS AND FLIGHT OPS NOT RELEASED";
                Paint(a.release, released ? nominal : caution);
            }
        }

        // AUX order pool
        var sel = s.damage.Comp(s.damage.engSel);
        if (actingOn && sel != null) actingOn.text = sel.name;
        int aux = s.power.BusUnits("aux");
        if (auxPct)
        {
            auxPct.text = HandoffBoardState.JsRound(s.orders.auxCharge) + "%";
            Paint(auxPct, s.orders.auxCharge >= 30 ? nominal : aux > 0 ? caution : alarm);
        }
        if (auxRate) auxRate.text = aux > 0 ? "+" + HandoffBoardState.JsToFixed(aux * s.orders.stats.auxPerUnit, 1) + "/S" : "NO FEED";
        int lit = HandoffBoardState.JsRound(s.orders.auxCharge / 100.0 * 10);
        for (int k = 0; k < auxCells.Length; k++) Show(auxCells[k], k + 1 <= lit);

        // order rockers
        foreach (var o in orders)
        {
            bool thrown = !o.blank && s.orders.dcThrown.Contains(o.id);
            bool failed = !o.blank && s.orders.orderFail == o.id;
            bool isOpen = !o.blank && s.orders.auxCharge >= s.orders.OrderCost(o.id) && s.orders.OrderBlock(o.id) == null;
            if (o.rocker && !o.blank) o.rocker.sprite = thrown ? rockerThrown : isOpen ? rockerUp : rockerLocked;
            Color lamp = failed ? alarm : thrown ? phosphor : isOpen ? stencilDim : lampOffPanel;
            Paint(o.lamp, lamp);
            // live lamps glow in their own colour ("0 0 6px <lamp>"); a blank's lamp is inset, no glow
            Paint(o.glow, lamp); Show(o.glow, !o.blank);
            Paint(o.label, label); // orderSwitches[].ink is computed but never read by the template (README, HANDOFF_GAPS row 41)
        }

        if (orderNote)
        {
            string note;
            if (aux < 1) note = "AUX BUS DEAD — NO ORDER WILL INITIATE";
            else if (!string.IsNullOrEmpty(s.orders.orderFail))
                note = s.orders.auxCharge < s.orders.OrderCost(s.orders.orderFail) ? "INSUFFICIENT AUX — " + s.orders.OrderCost(s.orders.orderFail) + "% REQUIRED"
                     : (s.orders.OrderBlock(s.orders.orderFail) ?? "ORDER REFUSED").ToUpperInvariant();
            else note = "ALLOCATE AUX UNITS TO CHARGE FASTER";
            orderNote.text = note;
            Paint(orderNote, aux < 1 || !string.IsNullOrEmpty(s.orders.orderFail) ? caution : noteInk);
        }
    }
}
