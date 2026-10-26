using UnityEngine;

/// <summary>
/// Orders: one ship's orders station. It holds the alert condition, the AUX order pool, what each order
/// costs and why one is refused, and the repair and order rockers that flick back after a throw. Our ship
/// and every enemy carry one (the enemy is our ship with other stats). Owner 2026-10-26: orders live in one
/// place, here. The repairs an order sends are damage control's (DamageSystem). Rules from board-logic.js
/// (ALERTS, ENG_ORDERS, doAlert, orderBlock, runOrder, flickBack) and Steps 33h-2, 40 and 59.
/// </summary>
public class OrdersSystem : MonoBehaviour
{
    /// <summary>The ship this belongs to. Orders read its AUX bus and send repairs to its damage control.</summary>
    [System.NonSerialized] public HandoffBoardState ship;

    [Header("Alert condition, and the rockers that flick back")]
    [Tooltip("ALERTS index: 0 = CONDITION 3, 1 = CONDITION 2, 2 = CONDITION 1 (set on the orders station).")]
    [Range(0, 2)] public int alert;
    [Tooltip("A thrown repair or order rocker flicks back after this long (flickBack).")]
    [Tune("Engineering & orders")]
    public float flickBackSeconds = 0.4f;
    public System.Collections.Generic.List<string> dcThrown = new System.Collections.Generic.List<string>();
    [Header("Orders station — AUX order pool (ENG_ORDERS)")]
    [Tooltip("AUX order pool, 0..100 (auxCharge).")]
    public double auxCharge;
    [Tooltip("Pool gained per AUX unit per game second.")]
    [Tune("Power & signal")]
    public double auxPerUnit = 3.2;
    [Tune("Engineering & orders")]
    public int orderCostParty = 30, orderCostSeal = 25, orderCostVent = 15;
    [Tooltip("The refused order (orderFail) shows this long.")]
    [Tune("Engineering & orders")]
    public float orderFailSeconds = 0.42f;
    public string orderFail;
    /// <summary>Orders refused so far (Step 40: the board sounds a refusal).</summary>
    [System.NonSerialized] public int orderRefusals;
    [System.NonSerialized] float orderFailUntil;
    [System.NonSerialized] readonly System.Collections.Generic.Dictionary<string, float> flickUntil = new System.Collections.Generic.Dictionary<string, float>();


    public void FlickBack(string id, float now)
    {
        if (!dcThrown.Contains(id)) dcThrown.Add(id);
        flickUntil[id] = now + flickBackSeconds;
    }

    /// <summary>Let thrown rockers fall back; true if any did.</summary>
    public bool ExpireFlicks(float now)
    {
        bool changed = false;
        if (!string.IsNullOrEmpty(orderFail) && now >= orderFailUntil) { orderFail = null; changed = true; }
        for (int i = dcThrown.Count - 1; i >= 0; i--)
        {
            float until;
            if (!flickUntil.TryGetValue(dcThrown[i], out until) || now >= until) { flickUntil.Remove(dcThrown[i]); dcThrown.RemoveAt(i); changed = true; }
        }
        return changed;
    }

    // ---- orders station (Step 33h-2) -------------------------------------------------------

    /// <summary>doAlert: CONDITION 3 → 2 → 1 → 3.</summary>
    public void DoAlert() { alert = (alert + 1) % 3; }

    /// <summary>ENG_ORDERS cost for a fitted order, or -1 (FIRE, BLKHD, SCRUB are not fitted).</summary>
    public int OrderCost(string id) => id == "party" ? orderCostParty : id == "seal" ? orderCostSeal : id == "vent" ? orderCostVent : -1;

    /// <summary>orderBlock: the orders station's refusals. Step 59: the same rules as engineering's rockers
    /// (the design's two sets disagreed on crew; DAMAGE_README.md has one set of rules).</summary>
    public string OrderBlock(string id) => id == "party" || id == "seal" || id == "vent" ? ship.damage.EngBlock(id) : null;

    /// <summary>runOrder: refused (too little AUX, or orderBlock) → the failure shows for 420 ms;
    /// otherwise spend the charge and apply. The rocker flicks back either way.</summary>
    public void RunOrder(string id, float now)
    {
        int cost = OrderCost(id);
        if (cost < 0) return;
        if (auxCharge < cost || OrderBlock(id) != null) { orderFail = id; orderFailUntil = now + orderFailSeconds; orderRefusals++; }
        else { auxCharge = System.Math.Max(0, auxCharge - cost); ship.damage.ApplyOrder(id); }
        FlickBack(id, now);
    }

    /// <summary>The tick: AUX charges by units x 3.2 per game second to 100 (owner 2026-10-24: every rate in game time).</summary>
    public void TickAux(double dt)
    {
        double rate = ship.power.BusUnits("aux") * auxPerUnit * dt;
        if (rate > 0 && auxCharge < 100) auxCharge = System.Math.Min(100, auxCharge + rate);
    }

    public bool FlightOpsReleased => alert >= 1;
    public bool WeaponsReleased => alert >= 1;
}
