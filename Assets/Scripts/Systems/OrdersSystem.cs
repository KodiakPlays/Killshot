// =================================================================================================
// OrdersSystem.cs
//
// What it does:
// One ship's orders station: its alert condition, its AUX charge, and the repair orders paid from
// that charge. Every ship has one. The Orders panel on the board shows our ship's orders station.
//
// How it works:
// - The alert condition steps CONDITION 3, 2, 1, then back to 3, on a click.
// - Weapons and flight operations are released at CONDITION 2 or 1. A level with weapons off
//   keeps them locked at every condition. A level can keep the railgun off for the whole level.
//   Torpedoes, PDCs and flight operations still follow the alert condition.
// - The AUX charge goes from 0 to 100. It fills each board step. The fill per game second is the
//   units on the AUX bus times a rate. The rate is in the ship's stats file.
// - Three orders cost charge: REPAIR, SEAL and VENT. Their costs are in the ship's stats file.
// - Running an order checks the charge and DamageSystem's reasons first. A refused order shows
//   for a moment and is counted. BoardManager plays the refusal sound.
// - An order that runs spends the charge. DamageSystem then does the order on the picked room.
// - A thrown rocker, here or on the Engineering panel, flicks back after a short time.
//   BoardManager lets the rockers fall back each frame.
//
// Technical:
// - Data: alert is 0, 1 or 2 for CONDITION 3, 2 or 1. auxCharge is 0 to 100. dcThrown lists the
//   rockers thrown now.
// - OrdersPanel calls DoAlert and RunOrder. It reads OrderBlock, OrderCost, auxCharge, orderFail
//   and alert.
// - RunOrder refuses when the charge is short or DamageSystem.EngBlock gives a reason. Otherwise
//   it pays, then calls DamageSystem.ApplyOrder.
// - DamageSystem calls OrderCost and FlickBack for the Engineering panel's rockers. REBOOT and
//   RECALL cost nothing (OrderCost gives -1).
// - BoardManager calls TickAux each step and ExpireFlicks every frame. It watches orderRefusals.
//   Only our ship's AUX charge fills.
// - WeaponsReleased and FlightOpsReleased are the same test: CONDITION 2 or 1, and no weaponsHold.
//   WeaponSystem, FireControlPanel and HangarSystem read them.
// - Mission sets weaponsHold and railgunHold when a level starts. A drill step can clear
//   weaponsHold. WeaponSystem and FireControlPanel read railgunHold.
// - Flick-back and refusal times are real seconds (Time.unscaledTime).
// - Numbers: OrdersSystem.Stats, the orders part of each ship's stats file: the costs, the charge
//   rate and the two times.
// =================================================================================================

using UnityEngine;

public class OrdersSystem : MonoBehaviour
{
    /// <summary>The ship this belongs to. Orders read its AUX bus and send repairs to its damage control.</summary>
    [System.NonSerialized] public ShipState ship;

    /// <summary>This ship's numbers for orders station: its section of the ship's stats file (Assets/Ships,
    /// Starwolf → Tuning).</summary>
    [System.Serializable]
    public class Stats
    {
        [Tooltip("A thrown repair or order rocker flicks back after this long (flickBack).")]
        public float flickBackSeconds = 0.4f;
        [Tooltip("Pool gained per AUX unit per game second.")]
        public double auxPerUnit = 3.2;
        public int orderCostParty = 30, orderCostSeal = 25, orderCostVent = 15;
        [Tooltip("The refused order (orderFail) shows this long.")]
        public float orderFailSeconds = 0.42f;
    }
    /// <summary>The numbers this part plays by: its section of the ship's stats file (a copy while the game runs).</summary>
    public Stats stats => shipStats ? shipStats.Stats.orders : (shipStats = GetComponent<Ship>()).Stats.orders;
    [System.NonSerialized] Ship shipStats;

    [Header("Alert condition, and the rockers that flick back")]
    [Tooltip("ALERTS index: 0 = CONDITION 3, 1 = CONDITION 2, 2 = CONDITION 1 (set on the orders station).")]
    [Range(0, 2)] public int alert;
    public System.Collections.Generic.List<string> dcThrown = new System.Collections.Generic.List<string>();
    [Header("Orders station — AUX order pool (ENG_ORDERS)")]
    [Tooltip("AUX order pool, 0..100 (auxCharge).")]
    public double auxCharge;
    public string orderFail;
    /// <summary>Orders refused so far (Step 40: the board sounds a refusal).</summary>
    [System.NonSerialized] public int orderRefusals;
    [System.NonSerialized] float orderFailUntil;
    [System.NonSerialized] readonly System.Collections.Generic.Dictionary<string, float> flickUntil = new System.Collections.Generic.Dictionary<string, float>();

    public void FlickBack(string id, float now)
    {
        if (!dcThrown.Contains(id)) dcThrown.Add(id);
        flickUntil[id] = now + stats.flickBackSeconds;
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
    public int OrderCost(string id) => id == "party" ? stats.orderCostParty : id == "seal" ? stats.orderCostSeal : id == "vent" ? stats.orderCostVent : -1;

    /// <summary>orderBlock: the orders station's refusals. Step 59: the same rules as engineering's rockers
    /// (the design's two sets disagreed on crew; DAMAGE_README.md has one set of rules).</summary>
    public string OrderBlock(string id) => id == "party" || id == "seal" || id == "vent" ? ship.damage.EngBlock(id) : null;

    /// <summary>runOrder: refused (too little AUX, or orderBlock) → the failure shows for 420 ms;
    /// otherwise spend the charge and apply. The rocker flicks back either way.</summary>
    public void RunOrder(string id, float now)
    {
        int cost = OrderCost(id);
        if (cost < 0) return;
        if (auxCharge < cost || OrderBlock(id) != null) { orderFail = id; orderFailUntil = now + stats.orderFailSeconds; orderRefusals++; }
        else { auxCharge = System.Math.Max(0, auxCharge - cost); ship.damage.ApplyOrder(id); }
        FlickBack(id, now);
    }

    /// <summary>The tick: AUX charges by units x 3.2 per game second to 100 (owner 2026-10-24: every rate in game time).</summary>
    public void TickAux(double dt)
    {
        double rate = ship.power.BusUnits("aux") * stats.auxPerUnit * dt;
        if (rate > 0 && auxCharge < 100) auxCharge = System.Math.Min(100, auxCharge + rate);
    }

    /// <summary>A level with its weapons off (owner 2026-11-01): no guns, torpedoes, PDCs or flight ops, at any alert.</summary>
    [System.NonSerialized] public bool weaponsHold;
    public bool FlightOpsReleased => alert >= 1 && !weaponsHold;
    public bool WeaponsReleased => alert >= 1 && !weaponsHold;
    /// <summary>Owner 2026-11-10, MILK RUN: the railgun stays off for the whole level; torpedoes, PDCs and flight ops still follow the alert.</summary>
    [System.NonSerialized] public bool railgunHold;
}
