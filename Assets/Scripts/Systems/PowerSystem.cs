// =================================================================================================
// PowerSystem.cs
//
// What it does:
// One ship's reactor and its four power buses: ENG, WEP, SEN and AUX. Every ship has one. The
// Power panel on the board shows our ship's buses and lets the player switch them.
//
// How it works:
// - Each bus has a number of power units and a switch with three settings: VENT, STANDBY and DRAW.
//   A click steps the switch one setting, and turns back at each end.
// - Every whole second of game time, a bus on VENT loses one unit. Then a bus on DRAW gains one
//   unit. It needs room on the bus and free units in the reactor. STANDBY keeps its units.
// - The reactor's units and each bus's top come from the ship's stats file (the Tuning window).
// - Reactor damage lowers the units it can give, never below 2. If the buses have more than that,
//   the last buses lose units first, at once.
// - The emergency load shed switch puts every bus on VENT and locks the switches. Released, every
//   bus goes back to STANDBY.
// - An EMP hit empties every bus, and they stay empty until the EMP wears off. A destroyed ship's
//   buses stay empty.
// - At the start of a level, TacticalWorld puts each other ship's ENG bus on DRAW and full. It
//   does the same for them after an EMP wears off.
// - Other systems read the units on each bus: speed and turning, weapons, sensors, the AUX charge
//   and the ship's signal.
//
// Technical:
// - Data: buses in the order ENG, WEP, SEN, AUX. fill is whole units. mode is Vent, Standby or
//   Draw. dir is the way the switch steps next. shed is the load shed switch.
// - PowerPanel calls CycleBus and ToggleShed. It reads ReactorCap, TotalFill and the buses.
// - BusUnits(key) gives a bus's units. NavSystem, WeaponSystem, SensorSystem, HangarSystem and
//   OrdersSystem read it.
// - Tick(dt) runs each step, from BoardManager, for our ship only. bank keeps part seconds. A bus
//   then moves one unit per whole game second at any rate.
// - HoldToCap runs every step. Over the cap, the last bus in the list loses units first.
// - ReactorCap reads the reactor room in DamageSystem. DamageSystem calls BlackOut on an EMP hit.
//   TacticalWorld calls PowerEngines for the other ships.
// - SensorSystem reads busSignalPerUnit for the ship's signal.
// - Numbers: PowerSystem.Stats, the power part of each ship's stats file: reactorUnits, busMax and
//   busSignalPerUnit.
// - Trap: PowerEngines and QuestSystem take buses[0] as ENG. Keep the bus order.
// =================================================================================================

using UnityEngine;

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
