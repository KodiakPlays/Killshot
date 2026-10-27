using UnityEngine;

/// <summary>
/// Power: one ship's reactor and its four buses (ENG, WEP, SEN, AUX). It holds each bus's units and
/// selector and the emergency load shed, and fills or drains the buses once per board tick. Our ship and
/// every enemy carry one (the enemy is our ship with other stats). Owner 2026-10-26: power lives in one
/// place, here. Rules from board-logic.js (REACTOR_UNITS, BUS_MAX, BUSES, reactorCap, cycleBus,
/// toggleShed, the power part of tickPower) and UIStateSpec §11 "Power: buses, standby and venting".
/// </summary>
public class PowerSystem : MonoBehaviour
{
    public enum BusMode { Vent, Standby, Draw } // BUS_MODES order

    [System.Serializable]
    public class Bus
    {
        public string key, label;
        public int fill;
        public BusMode mode = BusMode.Standby;
        [Tooltip("Direction the selector steps next: +1 toward DRAW, -1 toward VENT (busDir).")]
        public int dir = 1;
    }

    public Bus[] buses =
    {
        new Bus { key = "eng", label = "ENG" },
        new Bus { key = "wep", label = "WEP" },
        new Bus { key = "sen", label = "SEN" },
        new Bus { key = "aux", label = "AUX" },
    };
    [Tooltip("The emergency load shed is thrown: every bus vents and the selectors are held.")]
    public bool shed;

    /// <summary>The ship this belongs to. Power reads its reactor room, and whether an EMP or its death
    /// has put the power out.</summary>
    [System.NonSerialized] public ShipState ship;

    /// <summary>This ship's numbers for reactor and buses: its section of the ship's stats file (Assets/Ships,
    /// Starwolf → Tuning).</summary>
    [System.Serializable]
    public class Stats
    {
        [Header("Reactor and buses — REACTOR_UNITS, BUS_MAX, BUSES")]
        public int reactorUnits = 10;
        public int busMax = 5;
        [Tooltip("Signal each unit adds, per bus (ENG, WEP, SEN, AUX), on the 0–100 scale (Step 35, owner 2026-10-17). "
               + "Default 10: a full reactor of 10 units reads 100 (prototype getSig).")]
        public float[] busSignalPerUnit = { 10f, 10f, 10f, 10f };
    }
    /// <summary>The numbers this part plays by: its section of the ship's stats file (a copy while the game runs).</summary>
    public Stats stats => shipStats ? shipStats.Stats.power : (shipStats = GetComponent<Ship>()).Stats.power;
    [System.NonSerialized] Ship shipStats;
    // Owner 2026-10-24: every rate runs in game time. A bus fills or drains one whole unit per game second;
    // fractions bank here so x0.25 is exact too.
    [System.NonSerialized] private double bank;

    /// <summary>reactorCap: reactor damage caps the units, floored at 2.</summary>
    public int ReactorCap => Mathf.Max(2, ShipState.JsRound(ship.damage.CompHealth("rct") / 5f * stats.reactorUnits));

    public int TotalFill
    {
        get { int t = 0; foreach (var b in buses) t += b.fill; return t; }
    }

    /// <summary>Units the core can still give.</summary>
    public int Available => ReactorCap - TotalFill;

    /// <summary>A bus in DRAW that will gain a unit on the next tick (the board's p1–p5).</summary>
    public bool Climbing(Bus b) => b.mode == BusMode.Draw && b.fill < stats.busMax && TotalFill < ReactorCap;

    public int BusUnits(string key) { foreach (var b in buses) if (b.key == key) return b.fill; return 0; }

    /// <summary>cycleBus: VENT ↔ STANDBY ↔ DRAW, reversing at each end; refused while shed.</summary>
    public bool CycleBus(int i)
    {
        if (shed || i < 0 || i >= buses.Length) return false;
        var b = buses[i];
        int m = (int)b.mode, d = b.dir == -1 ? -1 : 1;
        if (m + d > 2 || m + d < 0) d = -d;
        b.mode = (BusMode)(m + d);
        b.dir = d;
        return true;
    }

    /// <summary>toggleShed: every bus to VENT when thrown, back to STANDBY when released.</summary>
    public void ToggleShed()
    {
        foreach (var b in buses) b.mode = shed ? BusMode.Standby : BusMode.Vent;
        shed = !shed;
    }

    /// <summary>Every bus empties at once (an EMP, Step 53).</summary>
    public void BlackOut() { foreach (var b in buses) b.fill = 0; }

    /// <summary>The engines powered: ENG on DRAW and full (an enemy at its start, Step 51; again after an EMP).</summary>
    public void PowerEngines()
    {
        var eng = buses[0];
        eng.mode = BusMode.Draw;
        eng.fill = stats.busMax;
    }

    /// <summary>One board tick of dt game seconds: the buses fill and drain, held to the reactor's output.</summary>
    public void Tick(double dt)
    {
        // EMP: power stays down (Step 53). Destroyed (Step 58): the ship goes dark for good.
        if (ship.missionNow < ship.empUntil || ship.destroyed) { BlackOut(); return; }
        bank += dt;
        for (; bank >= 1 - 1e-9; bank -= 1) Step();
        HoldToCap(TotalFill); // the reactor cap holds at once, whatever the rate
    }

    void Step()
    {
        int total = TotalFill, cap = ReactorCap;
        // drain first, so a bus dropping out of DRAW frees its units the same tick
        foreach (var b in buses) if (b.mode == BusMode.Vent && b.fill > 0) { b.fill--; total--; }
        foreach (var b in buses) if (b.mode == BusMode.Draw && b.fill < stats.busMax && total < cap) { b.fill++; total++; }
        HoldToCap(total);
    }

    // over the cap (the reactor damaged), the last buses give their units back first
    void HoldToCap(int total)
    {
        int cap = ReactorCap;
        for (int i = buses.Length - 1; i >= 0 && total > cap; i--)
            while (buses[i].fill > 0 && total > cap) { buses[i].fill--; total--; }
    }
}
