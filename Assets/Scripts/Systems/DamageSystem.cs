// =================================================================================================
// DamageSystem.cs
//
// What it does:
// One ship's damage: its armour, its rooms, its crew, fires, air loss and radiation, and its
// repairs. Our ship has one, and every other ship has one too. The Engineering panel on the board
// shows our ship's damage and gives the repair orders.
//
// How it works:
// - The ship has six rooms: magazine, bridge, habitation, hangar bay, reactor and engine room.
//   Each room has a strength from 0 to 5 and its crew. A room can have fire, radiation or no air.
// - Four armour plates make a box round the rooms: PROW, STBD, AFT and PORT. The armour and the
//   crew come from the ship's stats file (the Tuning window). So does each room's row from bow to
//   stern.
// - A hit takes armour off the side it struck. Once that side is open, the next hit damages the
//   room behind it. For PROW that is the front room, and for AFT the back room. For a side it is
//   the room at that spot along the ship. A wrecked room lets the hit through to the next one.
// - WeaponSystem sends railgun slugs and torpedoes. HE torpedoes strip armour hard. PENETRATOR
//   torpedoes go straight to a room. EMP torpedoes knock the power out for a while.
// - A torpedo hit on a room can start a fire. A slug hit can blow out its air and kill crew. A
//   hit on the reactor can leak radiation. HangarSystem sends fighter runs and marine boarding
//   losses. TacticalWorld sends crashes; a faster crash does more damage.
// - Every board step, fires wear their rooms down and radiation kills crew. Fire, air loss and
//   radiation can spread to the rooms next door. Radiation fades once the reactor is repaired
//   or vented.
// - The ship is destroyed when its reactor room reaches 0. A ship marked cannot be destroyed keeps
//   its reactor at 1 instead.
// - Repair orders, on the picked room: REPAIR sends a team. After a few game seconds the team puts
//   out a fire, or brings the air back, or repairs. At CONDITION 1 it repairs twice as much.
// - SEAL shuts an airless room's doors. VENT dumps a room's air, puts out its fire and kills
//   everyone inside. REBOOT adds 1 strength. RECALL calls the team back.
// - An order is refused, with a reason, when it cannot run or the AUX charge is too low. The
//   charge comes from OrdersSystem. SaveGame reads and writes the armour and room strength.
//
// Technical:
// - Data: compartments are the six rooms, by key: mag, bri, hab, hgr, rct, eng. integ is 0 to 5.
//   SideArmour follows Sides: PROW, STBD, AFT, PORT.
// - engSel is the picked room. partyT is the team's time out, in game seconds, or -1.
// - ship is its ShipState. Through it, damage sets destroyed and empUntil, reads the alert, rolls
//   the dice and cuts the power.
// - Hits in: WeaponSystem calls TakeSlug and TakeWarhead. HangarSystem calls TakeFighterRun and
//   KillCrew. TacticalWorld calls TakeCollision. All but KillCrew return the hit report's tail.
// - NextRoom(side, fromBow) picks the room a hit reaches. fromBow is 0 at the bow, 1 at the
//   stern. PeriscopePanel calls it too.
// - TickParty and TickConditions run each step. BoardManager calls them for our ship, TacticalWorld
//   for the others.
// - EngineeringPanel calls CycleComp, PickComp, EngBlock and EngRun. OrdersSystem asks EngBlock,
//   pays the AUX, then calls ApplyOrder. EnemyBrain calls PickComp and ApplyOrder and pays nothing.
// - Numbers: DamageSystem.Stats, the damage part of each ship's stats file (Assets/Ships). Its
//   lists use the room order MAG, BRI, HAB, HGR, RCT, ENG.
// - Chances use the ship's dice (ShipState.Roll). Spread chances are per game second, turned into
//   a chance per step.
// - Traps: room keys are used by name in many scripts and in the deck plan (Neighbours). A
//   warhead's room damage is the shooter's slugCompartmentDamage. partiesLost is never read.
//   Neighbours is fixed in the code. It does not follow roomRow from the stats file.
// =================================================================================================

using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    /// <summary>The ship this belongs to. Damage reads and sets whether it is destroyed, its EMP clock, its
    /// alert condition and its dice, and empties its power on an EMP.</summary>
    [System.NonSerialized] public ShipState ship;

    /// <summary>This ship's numbers for damage and damage control: its section of the ship's stats file (Assets/Ships,
    /// Starwolf → Tuning).</summary>
    [System.Serializable]
    public class Stats
    {
        [Tooltip("PARTY_TICKS: a damage-control party works this many game seconds (the design's 6 ticks at x1), then the repair lands.")]
        public float partySeconds = 6f;
        [Tooltip("Armour per side: PROW, STBD, AFT, PORT. At 0 the side is open (owner: prow and flanks take 2 slugs, the aft 1).")]
        public float[] sideArmourMax = { 100f, 100f, 50f, 100f };
        [Tooltip("Crew in each room at the start: MAG, BRI, HAB, HGR, RCT, ENG (owner 2026-10-29: a tuning number, per ship). "
               + "Breaches, VENT and radiation kill them.")]
        public int[] crew = { 4, 9, 18, 7, 6, 8 };
        [Tooltip("Each room's row in the ship, counted from the bow: 1 is right behind the PROW plate. In the same room "
               + "order as Crew: MAG, BRI, HAB, HGR, RCT, ENG (owner 2026-11-10). Rooms with the same number are side by "
               + "side. The rows share the ship's length equally. Set it in Starwolf → Tuning → Hard Points → ROOMS.")]
        public int[] roomRow = { 1, 2, 4, 4, 3, 5 };
        [Tooltip("A crash into debris or another ship at Crash Full Speed: armour off the side that touched.")]
        public float collisionArmourDamage = 25f;
        [Tooltip("The same crash with that side open: integrity off the room behind it.")]
        public int collisionCompartmentDamage = 1;
        [Tooltip("A crash at this speed, units/s, does the full damage above. Slower does less, faster does more (owner 2026-11-01).")]
        public float crashFullSpeed = 12.5f;
        [Tooltip("Slower than this, units/s, a crash is a nudge: no damage.")]
        public float crashNudgeSpeed = 0.5f;
        [Header("Conditions (Step 59, DAMAGE_README.md): every rate per game second")]
        [Tooltip("Chance a hit on the reactor room (not a fighter's) leaks radiation. The fire and breach chances are the shooter's (Weapons).")]
        [Range(0f, 1f)] public float radChanceReactor = 0.5f;
        [Tooltip("A fire takes 1 integrity off its room this often, game seconds.")]
        public float fireDamageSeconds = 30f;
        [Tooltip("Chance per second that a fire, a vacuum or radiation spreads into each neighbouring room.")]
        [Range(0f, 1f)] public float fireSpreadPerSecond = 0.01f, vacuumSpreadPerSecond = 0.01f, radSpreadPerSecond = 0.01f;
        [Tooltip("Share of a room's crew lost when it is breached (VENT loses all of them).")]
        [Range(0f, 1f)] public float breachCrewLoss = 0.5f;
        [Tooltip("Radiation kills one crew member in its room this often, game seconds.")]
        public float radCrewLossSeconds = 20f;
        [Tooltip("The reactor stops leaking once repaired to this integrity (or vented); then each room's radiation fades after radFadeSeconds.")]
        public int reactorContainedAt = 3;
        public float radFadeSeconds = 90f;
    }
    /// <summary>The numbers this part plays by: its section of the ship's stats file (a copy while the game runs).</summary>
    public Stats stats => shipStats ? shipStats.Stats.damage : (shipStats = GetComponent<Ship>()).Stats.damage;
    [System.NonSerialized] Ship shipStats;

    [System.Serializable]
    public class Compartment
    {
        public string key, tag, name;
        [Range(0, 5)] public int integ;
        [Tooltip("The crew in the room now. It starts from the stats file (damage → crew).")]
        public int crew;
        public bool vented;
        [Tooltip("What the design's row shows: FIRE, RAD or empty. Step 59: derived from fire and rad below.")]
        public string cond;
        [Tooltip("Step 59 (DAMAGE_README.md): burning; integrity falls, and it spreads to neighbours with air.")]
        public bool fire;
        [Tooltip("Step 59: radiation in the room; its crew die while they stay.")]
        public bool rad;
        [Tooltip("Step 59: its doors are shut (SEAL on an airless room, or VENT), so vacuum cannot spread from it.")]
        public bool doorsShut;
        [System.NonSerialized] public double fireT, radT, fadeT;
    }

    [Header("Compartments — COMPARTMENTS. Load state: all green, no damage, all sealed (owner, 2026-10-04)")]
    [Tooltip("The design loads damaged (MAG vented, HAB RAD, HGR FIRE) only to show its behaviours; the game starts undamaged.")]
    public Compartment[] compartments =
    {
        new Compartment { key = "mag", tag = "MAG", name = "MAGAZINE", integ = 5 },
        new Compartment { key = "bri", tag = "BRI", name = "BRIDGE", integ = 5 },
        new Compartment { key = "hab", tag = "HAB", name = "HABITATION", integ = 5 },
        new Compartment { key = "hgr", tag = "HGR", name = "HANGAR BAY", integ = 5 },
        new Compartment { key = "rct", tag = "RCT", name = "REACTOR", integ = 5 },
        new Compartment { key = "eng", tag = "ENG", name = "ENGINE ROOM", integ = 5 },
    };

    public string engSel = "rct";
    [Tooltip("The party's compartment and how long it has worked, game seconds; -1 when no party is out.")]
    public string partyComp;
    public float partyT = -1;

    /// <summary>compHealth: a vented compartment counts as integrity 0.</summary>
    public int CompHealth(string key)
    {
        foreach (var c in compartments) if (c.key == key) return c.vented ? 0 : c.integ;
        return 0;
    }

    public Compartment Comp(string key) { foreach (var c in compartments) if (c.key == key) return c; return null; }
    public int CompIndex(string key) { for (int i = 0; i < compartments.Length; i++) if (compartments[i].key == key) return i; return 0; }
    public bool PartyOut => partyT >= 0;
    public bool MagOk => CompHealth("mag") >= 3;   // shells and warheads are struck up from here
    public bool HgrOk => CompHealth("hgr") >= 3;   // craft cannot be spotted in a wrecked bay
    public bool BriOk => CompHealth("bri") >= 3;   // the plot computer lives on the bridge
    public float EngFactor => CompHealth("eng") / 5f;
    /// <summary>repairRate: Condition 1 doubles repair.</summary>
    public int RepairRate => ship.orders.alert == 2 ? 2 : 1;

    /// <summary>engBlock: why an engineering repair is refused, or null. Step 59 (DAMAGE_README.md): the team
    /// comes from elsewhere, so it needs no crew in the room; it fights a fire, restores the air once the doors
    /// are shut, or repairs. SEAL shuts an airless room's doors. RECALL (the design's DUMMY 1) calls the team back.
    /// Bug 6 (owner 2026-10-29): REPAIR, SEAL and VENT cost the same AUX here as at the orders station; the room's
    /// own reason comes first.</summary>
    public string EngBlock(string id) => RoomBlock(id) ?? AuxBlock(id);

    string AuxBlock(string id)
    {
        int cost = ship.orders.OrderCost(id);
        return cost > 0 && ship.orders.auxCharge < cost ? "INSUFFICIENT AUX \u2014 " + cost + "% REQUIRED" : null;
    }

    string RoomBlock(string id)
    {
        var c = Comp(engSel);
        switch (id)
        {
            case "party": return PartyOut ? "DC PARTY COMMITTED \u2014 " + Mathf.CeilToInt(stats.partySeconds - partyT) + " TO RUN"
                                          : c.vented && !c.doorsShut ? "SEAL THE DOORS FIRST"
                                          : !c.fire && !c.vented && c.integ >= 5 ? "COMPARTMENT AT FULL INTEGRITY" : null;
            case "seal": return !c.vented ? "COMPARTMENT ALREADY SEALED" : c.doorsShut ? "DOORS ALREADY SHUT" : null;
            case "vent": return c.vented ? "COMPARTMENT ALREADY VENTED" : null;
            case "reboot": return c.integ < 5 ? null : "SYSTEMS NOMINAL \u2014 NOTHING TO RESTART";
            case "recall": return PartyOut ? null : "NO TEAM OUT";
        }
        return "NOT FITTED";
    }

    public void CycleComp() { engSel = compartments[(CompIndex(engSel) + 1) % compartments.Length].key; }
    public void PickComp(string key) { if (Comp(key) != null) engSel = key; }

    /// <summary>engRun: refused silently when blocked; otherwise apply and flick back.</summary>
    public bool EngRun(string id, float now)
    {
        if (EngBlock(id) != null) return false;
        int cost = ship.orders.OrderCost(id); // bug 6: REBOOT and RECALL are free (-1)
        if (cost > 0) ship.orders.auxCharge -= cost;
        ApplyOrder(id);
        ship.orders.FlickBack("eng_" + id, now);
        return true;
    }

    /// <summary>applyOrder: a repair is dispatched, not applied. Step 59 (DAMAGE_README.md): SEAL shuts an
    /// airless room's doors (the air comes back with a repair); VENT dumps the air, so the fire goes, and the
    /// crew and any team inside die; REBOOT adds 1 and no longer clears a condition; RECALL calls the team back.</summary>
    public void ApplyOrder(string id)
    {
        var c = Comp(engSel);
        switch (id)
        {
            case "party": partyComp = engSel; partyT = 0; break;
            case "seal": c.doorsShut = true; break;
            case "vent": Breach(c, 1f); c.doorsShut = true; break;
            case "reboot": c.integ = Mathf.Min(5, c.integ + 1); break;
            case "recall": partyT = -1; partyComp = null; break;
        }
        SyncCond();
    }

    // ---- hull sides and damage (Step 49, owner 2026-10-23) --------------------------------------
    // The same model for every ship, ours included (the enemy is our ship with other stats).

    public static readonly string[] Sides = { "PROW", "STBD", "AFT", "PORT" };

    // Where the rooms are (owner 2026-11-10, ROOM_LAYOUT_PLAN.md): the four plates make a box, and the rooms are in a
    // row inside it, from bow to stern. Each room's row is in the ship's file.
    int RowOf(int i) => i < stats.roomRow.Length ? Mathf.Max(1, stats.roomRow[i]) : 1;
    int Rows { get { int n = 1; for (int i = 0; i < compartments.Length; i++) n = Mathf.Max(n, RowOf(i)); return n; } }

    /// <summary>The room the next hit through open plate s (PROW, STBD, AFT, PORT) damages, or null when every room
    /// is wrecked. PROW: the front room still standing; AFT: the back one. STBD or PORT: the room behind the spot it
    /// landed (fromBow: 0 at the bow, 1 at the stern; the rows share the length equally), else the nearest room still
    /// standing. A wrecked room lets the hit through. Ties: more integrity, then the one higher in the list.</summary>
    public Compartment NextRoom(int s, float fromBow = 0.5f)
    {
        int rows = Rows;
        Compartment best = null;
        float bestKey = 0f;
        for (int i = 0; i < compartments.Length; i++)
        {
            var c = compartments[i];
            if (c.integ <= 0) continue;
            float key = s == 0 ? RowOf(i) : s == 2 ? -RowOf(i)
                      : Mathf.Round(Mathf.Abs((RowOf(i) - 0.5f) / rows - fromBow) * 10000f); // rounded, so equal distances tie
            if (best == null || key < bestKey || (key == bestKey && c.integ > best.integ)) { best = c; bestKey = key; }
        }
        return best;
    }
    [System.NonSerialized] private float[] sideArmour;
    public float[] SideArmour => sideArmour ?? (sideArmour = (float[])stats.sideArmourMax.Clone());

    /// <summary>A slug strikes this ship's side. Armour first; once the side is open, the next room (NextRoom)
    /// loses integrity. by: the firing ship's numbers. fromBow: where along the ship it landed, 0 the bow to 1 the
    /// stern. Returns the report's tail (" OPEN", " · RCT −2").</summary>
    public string TakeSlug(string side, WeaponSystem.Stats by, float fromBow = 0.5f)
    {
        int i = System.Array.IndexOf(Sides, side);
        if (i < 0 || ship.destroyed) return "";
        var armour = SideArmour;
        if (armour[i] > 0f)
        {
            armour[i] = Mathf.Max(0f, armour[i] - by.slugArmourDamage);
            return armour[i] <= 0f ? " OPEN" : " ARMOUR " + ShipState.JsRound(armour[i] / stats.sideArmourMax[i] * 100f) + "%";
        }
        return Behind(i, fromBow, by.slugCompartmentDamage, "slug", by);
    }

    /// <summary>One fighter's attack run on this ship (Step 54). DESTROY: the side facing it, then the next room, at
    /// the middle of a side. DISABLE: the end of the ship the engine room is nearer, PROW or AFT.</summary>
    public string TakeFighterRun(string side, bool disable, float armourDamage, int compartmentDamage)
    {
        if (ship.destroyed) return "";
        int i = disable ? (RowOf(CompIndex("eng")) - 0.5f > Rows / 2f ? 2 : 0) : System.Array.IndexOf(Sides, side);
        if (i < 0) return "";
        var armour = SideArmour;
        if (armour[i] > 0f)
        {
            armour[i] = Mathf.Max(0f, armour[i] - armourDamage);
            return " " + Sides[i] + (armour[i] <= 0f ? " OPEN" : " ARMOUR " + ShipState.JsRound(armour[i] / stats.sideArmourMax[i] * 100f) + "%");
        }
        return " " + Sides[i] + Behind(i, 0.5f, compartmentDamage, "fighter", null);
    }

    /// <summary>The ship crashes into debris or another ship (TacticalWorld) at this speed: the side that touched, then
    /// the room behind, as a fighter's run does. Starts no fire, breach or radiation.</summary>
    public string TakeCollision(string side, double speed)
    {
        if (speed < stats.crashNudgeSpeed) return "";
        float share = (float)speed / Mathf.Max(0.01f, stats.crashFullSpeed);
        return TakeFighterRun(side, false, stats.collisionArmourDamage * share, Mathf.CeilToInt(stats.collisionCompartmentDamage * share));
    }

    // The next room through side i (NextRoom, at fromBow on a side) loses the damage given, and the hit can start a
    // condition there (Step 59). Damage left over does not carry on. With nothing left, the hit does nothing more. The
    // ship is destroyed when its reactor's structure reaches 0 (any ship, ours too: owner 2026-10-24). by: the firing
    // ship's numbers (none for a fighter).
    string Behind(int i, float fromBow, int damage, string weapon, WeaponSystem.Stats by)
    {
        var hit = NextRoom(i, fromBow);
        if (hit == null) return "";
        string started = Strike(hit, weapon, by);
        hit.integ = Mathf.Max(0, hit.integ - damage);
        if (Comp("rct").integ <= 0) { if (ship.cannotBeDestroyed) Comp("rct").integ = 1; else ship.destroyed = true; } // owner 2026-11-10: a mission piece keeps 1
        return " · " + hit.tag + " −" + damage + started;
    }

    // ---- conditions: fire, vacuum, radiation (Step 59, owner 2026-10-24, DAMAGE_README.md) -------

    /// <summary>Teams lost when their room vented around them.</summary>
    [System.NonSerialized] public int partiesLost;

    // The deck plan (owner 2026-10-24): which rooms touch which.
    static readonly System.Collections.Generic.Dictionary<string, string[]> Neighbours = new System.Collections.Generic.Dictionary<string, string[]>
    {
        { "mag", new[] { "bri", "rct", "hgr" } },        { "bri", new[] { "mag", "hab", "rct", "hgr" } },
        { "hab", new[] { "bri", "eng", "rct", "hgr" } }, { "eng", new[] { "hab", "rct", "hgr" } },
        { "rct", new[] { "mag", "bri", "hab", "eng" } }, { "hgr", new[] { "mag", "bri", "hab", "eng" } },
    };

    // What a hit through open armour starts: the weapon and the room decide (owner). The fire and breach
    // chances are the shooter's; radiation is the room's.
    string Strike(Compartment c, string weapon, WeaponSystem.Stats by)
    {
        string started = "";
        float fireChance = by == null ? 0f : weapon == "HE" ? by.fireChanceHE : weapon == "PEN" ? by.fireChancePEN : 0f;
        if (!c.vented && !c.fire && ship.Roll(fireChance)) { c.fire = true; c.fireT = 0; started += " · FIRE"; }
        if (weapon == "slug" && !c.vented && ship.Roll(by.breachChanceSlug)) { Breach(c, stats.breachCrewLoss); started += " · BREACH"; }
        if (c.key == "rct" && weapon != "fighter" && !c.rad && ship.Roll(stats.radChanceReactor)) { c.rad = true; c.fadeT = 0; started += " · RADIATION"; }
        SyncCond();
        return started;
    }

    void Breach(Compartment c, float crewLost)
    {
        c.vented = true;
        c.fire = false;
        c.crew -= Mathf.CeilToInt(c.crew * crewLost);
        if (PartyOut && partyComp == c.key) { partyT = -1; partyComp = null; partiesLost++; }
    }

    // The design's row shows one condition: fire before radiation.
    void SyncCond() { foreach (var c in compartments) c.cond = c.fire ? "FIRE" : c.rad ? "RAD" : null; }

    /// <summary>Each tick: fires burn their rooms, radiation kills crew, and all three spread to neighbours
    /// (fire only into rooms with air; vacuum only through open doors; radiation only while the reactor leaks).
    /// Radiation fades once the reactor is contained. A reactor burned to 0 destroys the ship.</summary>
    public void TickConditions(double dt)
    {
        if (ship.destroyed) return;
        var rct = Comp("rct");
        bool leaking = rct != null && rct.rad && rct.integ < stats.reactorContainedAt && !rct.vented;
        // spread from what stood at the start of the tick, so a condition moves one room per tick at most
        var burning = System.Array.FindAll(compartments, c => c.fire);
        var airless = System.Array.FindAll(compartments, c => c.vented && !c.doorsShut);
        var hot = leaking ? System.Array.FindAll(compartments, c => c.rad) : new Compartment[0];
        double pf = 1 - System.Math.Pow(1 - stats.fireSpreadPerSecond, dt), pv = 1 - System.Math.Pow(1 - stats.vacuumSpreadPerSecond, dt),
               pr = 1 - System.Math.Pow(1 - stats.radSpreadPerSecond, dt);
        foreach (var c in burning)
            foreach (var k in Neighbours[c.key]) { var m = Comp(k); if (m != null && !m.fire && !m.vented && ship.Roll(pf)) { m.fire = true; m.fireT = 0; } }
        foreach (var c in airless)
            foreach (var k in Neighbours[c.key]) { var m = Comp(k); if (m != null && !m.vented && ship.Roll(pv)) Breach(m, stats.breachCrewLoss); }
        foreach (var c in hot)
            foreach (var k in Neighbours[c.key]) { var m = Comp(k); if (m != null && !m.rad && ship.Roll(pr)) { m.rad = true; m.fadeT = 0; } }
        foreach (var c in compartments)
        {
            if (c.fire && c.vented) c.fire = false;
            if (c.fire)
                for (c.fireT += dt; c.fireT >= stats.fireDamageSeconds; c.fireT -= stats.fireDamageSeconds) c.integ = Mathf.Max(0, c.integ - 1);
            if (!c.rad) continue;
            if (c.crew > 0)
                for (c.radT += dt; c.radT >= stats.radCrewLossSeconds && c.crew > 0; c.radT -= stats.radCrewLossSeconds) c.crew--;
            if (leaking) c.fadeT = 0;
            else if ((c.fadeT += dt) >= stats.radFadeSeconds) { c.rad = false; c.radT = 0; c.fadeT = 0; }
        }
        if (rct != null && rct.integ <= 0) { if (ship.cannotBeDestroyed) rct.integ = 1; else ship.destroyed = true; } // owner 2026-11-10: a mission piece keeps 1
        SyncCond();
    }

    /// <summary>A torpedo's warhead strikes this ship (Step 53, owner 2026-10-23). HE hits the armour hard,
    /// then what is behind it; PENETRATOR goes straight to a compartment behind the side; EMP does no
    /// hull damage but knocks the power out. by: the firing ship's numbers. fromBow: where along the ship it landed,
    /// 0 the bow to 1 the stern.</summary>
    public string TakeWarhead(string side, string warheadName, WeaponSystem.Stats by, float fromBow = 0.5f)
    {
        if (ship.destroyed) return "";
        if (warheadName == "EMP")
        {
            ship.power.BlackOut();
            ship.empUntil = ship.missionNow + by.empSeconds;
            return " · EMP · POWER DOWN";
        }
        int i = System.Array.IndexOf(Sides, side);
        if (i < 0) return "";
        var armour = SideArmour;
        if (warheadName == "HE" && armour[i] > 0f)
        {
            armour[i] = Mathf.Max(0f, armour[i] - by.heArmourDamage);
            return " · HE" + (armour[i] <= 0f ? " OPEN" : " ARMOUR " + ShipState.JsRound(armour[i] / stats.sideArmourMax[i] * 100f) + "%");
        }
        string w = warheadName == "PENETRATOR" ? "PEN" : warheadName;
        return " · " + w + Behind(i, fromBow, by.slugCompartmentDamage, w, by);
    }

    // The side of a ship whose bow points bowDeg that faces a bearing fromDeg (both clockwise from north).

    public int TotalCrew { get { int n = 0; foreach (var c in compartments) n += c.crew; return n; } }

    /// <summary>Crew lost aboard (boarding): taken from the most crowded rooms first.</summary>
    public void KillCrew(int n)
    {
        for (; n > 0; n--)
        {
            Compartment most = null;
            foreach (var c in compartments) if (c.crew > 0 && (most == null || c.crew > most.crew)) most = c;
            if (most == null) return;
            most.crew--;
        }
    }

    // ---- the tick — damage-control party ---------------------------------------------------

    public void TickParty(double dt)
    {
        if (!PartyOut) return;
        partyT += (float)dt;
        if (partyT < stats.partySeconds - 1e-4f) return;
        var c = Comp(partyComp);
        // Step 59: the team fights a fire first; in a sealed airless room it restores the air; otherwise it repairs
        if (c != null)
        {
            if (c.fire) { c.fire = false; c.fireT = 0; }
            else if (c.vented) { c.vented = false; c.doorsShut = false; }
            else c.integ = Mathf.Min(5, c.integ + RepairRate);
        }
        partyT = -1;
        partyComp = null;
        SyncCond();
    }
}
