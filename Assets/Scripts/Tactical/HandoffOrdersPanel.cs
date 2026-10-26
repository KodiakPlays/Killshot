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
public class HandoffOrdersPanel : HandoffPanel
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

    [Header("Colours (design sRGB)")]
    [SerializeField] private Color nominal = new Color32(0x8F, 0xC3, 0x3A, 0xFF);
    [SerializeField] private Color caution = new Color32(0xE0, 0xA0, 0x20, 0xFF);
    [SerializeField] private Color alarm = new Color32(0xE0, 0x4B, 0x3C, 0xFF);
    [SerializeField] private Color data = new Color32(0xE8, 0xED, 0xF0, 0xFF);
    [SerializeField] private Color label = new Color32(0xC9, 0xD2, 0xD8, 0xFF);
    [SerializeField] private Color stencil = new Color32(0x77, 0x82, 0x8A, 0xFF);
    [SerializeField] private Color stencilDim = new Color32(0x5E, 0x68, 0x70, 0xFF);
    [SerializeField] private Color phosphor = new Color32(0xB6, 0xF0, 0x4A, 0xFF);
    [SerializeField] private Color noteInk = new Color32(0x93, 0x9E, 0xA6, 0xFF);
    [SerializeField] private Color lampOffSelector = new Color32(0x1B, 0x20, 0x24, 0xFF);
    [SerializeField] private Color lampOffPanel = new Color32(0x22, 0x26, 0x2A, 0xFF);

    public override bool Click(string action, string arg)
    {
        switch (action)
        {
            case "orders.alert": State.DoAlert(); return true;
            case "orders.run": State.RunOrder(arg, Time.unscaledTime); return true;
        }
        return false;
    }

    public override void Refresh()
    {
        var s = State;
        string[] alerts = { "CONDITION 3", "CONDITION 2", "CONDITION 1" };
        Color alertInk = s.alert == 2 ? alarm : s.alert == 1 ? caution : nominal;
        bool released = s.WeaponsReleased;
        foreach (var a in new[] { stowed, open })
        {
            if (a == null) continue;
            a.pointer.Set(60f + s.alert * 30f); // alertAngle
            for (int i = 0; i < a.lamps.Length; i++)
            {
                bool on = i == s.alert;
                Paint(a.lamps[i], on ? data : lampOffSelector);
                if (i < a.glows.Length) Show(a.glows[i], on);
                if (i < a.labels.Length) Paint(a.labels[i], on ? data : stencil);
            }
            if (a.condition) { a.condition.text = alerts[s.alert]; Paint(a.condition, alertInk); }
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
            auxPct.text = HandoffBoardState.JsRound(s.auxCharge) + "%";
            Paint(auxPct, s.auxCharge >= 30 ? nominal : aux > 0 ? caution : alarm);
        }
        if (auxRate) auxRate.text = aux > 0 ? "+" + HandoffBoardState.JsToFixed(aux * s.auxPerUnit, 1) + "/S" : "NO FEED";
        int lit = HandoffBoardState.JsRound(s.auxCharge / 100.0 * 10);
        for (int k = 0; k < auxCells.Length; k++) Show(auxCells[k], k + 1 <= lit);

        // order rockers
        foreach (var o in orders)
        {
            bool thrown = !o.blank && s.dcThrown.Contains(o.id);
            bool failed = !o.blank && s.orderFail == o.id;
            bool isOpen = !o.blank && s.auxCharge >= s.OrderCost(o.id) && s.OrderBlock(o.id) == null;
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
            else if (!string.IsNullOrEmpty(s.orderFail))
                note = s.auxCharge < s.OrderCost(s.orderFail) ? "INSUFFICIENT AUX — " + s.OrderCost(s.orderFail) + "% REQUIRED"
                     : (s.OrderBlock(s.orderFail) ?? "ORDER REFUSED").ToUpperInvariant();
            else note = "ALLOCATE AUX UNITS TO CHARGE FASTER";
            orderNote.text = note;
            Paint(orderNote, aux < 1 || !string.IsNullOrEmpty(s.orderFail) ? caution : noteInk);
        }
    }
}
