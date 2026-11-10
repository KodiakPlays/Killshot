// =================================================================================================
// OrdersPanel.cs
//
// What it does:
// The Orders station on the battle board. The player sets the alert condition and throws order
// rockers that spend the AUX charge: a damage control party, SEAL and VENT. Each order acts on the
// room picked in the Engineering drawer. It is on the Orders prefab.
//
// How it works:
// - BoardManager passes it clicks. The alert switch steps from CONDITION 3 to 2 to 1, then back
//   to 3. An order rocker asks OrdersSystem to run that order. OrdersSystem refuses it when the
//   AUX charge is too low or the room cannot take it.
// - BoardManager redraws it after every click and every 0.42 seconds. It reads OrdersSystem,
//   PowerSystem and DamageSystem through ShipState.
// - The alert selector, in its stowed and open forms, shows the condition: green at 3, amber at 2,
//   red at 1. A line says if weapons and flight ops are released. They are released at
//   condition 2 or 1, unless WEAPONS HOLD is on.
// - It shows the name of the room the orders act on.
// - The AUX charge shows as a percent and ten cells. It is green from 30 percent. Below that it is
//   amber, or red with no power on the AUX bus. Beside it is the charge per second, or NO FEED.
// - Each rocker shows thrown, ready or locked. Its lamp lights while thrown and turns red when the
//   order is refused. FIRE, BLKHD and SCRUB are blank plates with no rocker.
// - The note under the rockers says AUX BUS DEAD with no power. Or it gives the reason for the
//   last refused order. Or it asks for more power on AUX to charge faster.
//
// Technical:
// - Data: the order state is in OrdersSystem. Its alert is 0 for CONDITION 3, 1 for CONDITION 2
//   and 2 for CONDITION 1.
// - BoardManager calls Click and Refresh. The actions are "orders.alert" and "orders.run" (arg =
//   party, seal or vent).
// - It calls OrdersSystem.DoAlert and RunOrder. It reads auxCharge, OrderCost, OrderBlock,
//   orderFail, dcThrown and WeaponsReleased.
// - OrderBlock asks DamageSystem.EngBlock about the room picked in Engineering (engSel).
// - A rocker is thrown while OrdersSystem.dcThrown has its plain id. Engineering's rockers use
//   "eng_" plus the id.
// - Numbers: order costs, the AUX charge per unit and the flick times are in the stats file's
//   orders part.
// - Blank plates are orders ticked blank in the Inspector. They get no rocker picture and no glow.
// - Trap: the 30 percent green line for the AUX charge is in the code.
// =================================================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrdersPanel : Panel
{
    [System.Serializable]
    public class AlertSet
    {
        public BoardManager.Dial pointer;
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
            auxPct.text = ShipState.JsRound(s.orders.auxCharge) + "%";
            Paint(auxPct, s.orders.auxCharge >= 30 ? nominal : aux > 0 ? caution : alarm);
        }
        if (auxRate) auxRate.text = aux > 0 ? "+" + ShipState.JsToFixed(aux * s.orders.stats.auxPerUnit, 1) + "/S" : "NO FEED";
        int lit = ShipState.JsRound(s.orders.auxCharge / 100.0 * 10);
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
