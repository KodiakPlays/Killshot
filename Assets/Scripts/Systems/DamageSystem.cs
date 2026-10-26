using UnityEngine;

/// <summary>
/// Damage: one ship's compartments and hull, and what happens to them. It holds the six compartments (their
/// integrity, crew, air and conditions), the armour on each side, what a slug, a warhead or a fighter run does,
/// fire, vacuum and radiation spreading, the crew, and damage control (the selected compartment, the repair
/// orders and the team out). Our ship and every enemy carry one (the enemy is our ship with other stats).
/// Owner 2026-10-26: damage lives in one place, here. Whether the ship is destroyed or captured, its EMP clock
/// and its dice stay on the ship state (HandoffBoardState). Rules from board-logic.js (COMPARTMENTS, engBlock,
/// engRun, applyOrder, the party tick), DAMAGE_README.md and Steps 49, 53, 58 and 59.
/// </summary>
public class DamageSystem : MonoBehaviour
{
    /// <summary>The ship this belongs to. Damage reads and sets whether it is destroyed, its EMP clock, its
    /// alert condition and its dice, and empties its power on an EMP.</summary>
    [System.NonSerialized] public HandoffBoardState ship;

    [System.Serializable]
    public class Compartment
    {
        public string key, tag, name;
        [Range(0, 5)] public int integ;
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
        new Compartment { key = "mag", tag = "MAG", name = "MAGAZINE", integ = 5, crew = 4 },
        new Compartment { key = "bri", tag = "BRI", name = "BRIDGE", integ = 5, crew = 9 },
        new Compartment { key = "hab", tag = "HAB", name = "HABITATION", integ = 5, crew = 18 },
        new Compartment { key = "hgr", tag = "HGR", name = "HANGAR BAY", integ = 5, crew = 7 },
        new Compartment { key = "rct", tag = "RCT", name = "REACTOR", integ = 5, crew = 6 },
        new Compartment { key = "eng", tag = "ENG", name = "ENGINE ROOM", integ = 5, crew = 8 },
    };


    public string engSel = "rct";
    [Tooltip("PARTY_TICKS: a damage-control party works this many game seconds (the design's 6 ticks at x1), then the repair lands.")]
    [Tune("Engineering & orders")]
    [UnityEngine.Serialization.FormerlySerializedAs("partyTicks")]
    public float partySeconds = 6f;
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
    public int RepairRate => ship.alert == 2 ? 2 : 1;

    /// <summary>engBlock: why an engineering repair is refused, or null. Step 59 (DAMAGE_README.md): the team
    /// comes from elsewhere, so it needs no crew in the room; it fights a fire, restores the air once the doors
    /// are shut, or repairs. SEAL shuts an airless room's doors. RECALL (the design's DUMMY 1) calls the team back.</summary>
    public string EngBlock(string id)
    {
        var c = Comp(engSel);
        switch (id)
        {
            case "party": return PartyOut ? "DC PARTY COMMITTED \u2014 " + Mathf.CeilToInt(partySeconds - partyT) + " TO RUN"
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
        ApplyOrder(id);
        ship.FlickBack("eng_" + id, now);
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
    // Step 58 (owner 2026-10-24, design audit §09.4): one fixed room behind each face — bow magazine,
    // starboard hangar, stern engines, port reactor. Bridge and habitation sit inboard and are never hit
    // directly. Replaces the 18 Oct map (FTUE_PLAN §4.4). DAMAGE_README.md.
    static readonly string[] BehindSide = { "mag", "hgr", "eng", "rct" };
    /// <summary>The room behind face i (PROW, STBD, AFT, PORT).</summary>
    public static string RoomBehind(int i) => BehindSide[i];
    [Tooltip("Armour per side: PROW, STBD, AFT, PORT. At 0 the side is open (owner: prow and flanks take 2 slugs, the aft 1).")]
    [Tune("Damage")] public float[] sideArmourMax = { 100f, 100f, 50f, 100f };
    [Tooltip("Armour one railgun slug takes off the side it hits.")]
    [Tune("Damage")] public float slugArmourDamage = 50f;
    [Tooltip("Integrity one slug through an open side takes off one compartment behind it (owner: 2 of 5).")]
    [Tune("Damage")] public int slugCompartmentDamage = 2;
    [System.NonSerialized] private float[] sideArmour;
    public float[] SideArmour => sideArmour ?? (sideArmour = (float[])sideArmourMax.Clone());

    /// <summary>A slug strikes this ship's side. Armour first; once the side is open, the room behind it
    /// loses integrity. Returns the report's tail (" OPEN", " · RCT −2").</summary>
    public string TakeSlug(string side)
    {
        int i = System.Array.IndexOf(Sides, side);
        if (i < 0 || ship.destroyed) return "";
        var armour = SideArmour;
        if (armour[i] > 0f)
        {
            armour[i] = Mathf.Max(0f, armour[i] - slugArmourDamage);
            return armour[i] <= 0f ? " OPEN" : " ARMOUR " + HandoffBoardState.JsRound(armour[i] / sideArmourMax[i] * 100f) + "%";
        }
        return Behind(i);
    }

    /// <summary>One fighter's attack run on this ship (Step 54). DESTROY: the side facing it, then the room
    /// behind. DISABLE: always the stern, so the engine room (Step 58).</summary>
    public string TakeFighterRun(string side, bool disable, float armourDamage, int compartmentDamage)
    {
        if (ship.destroyed) return "";
        int i = disable ? 2 : System.Array.IndexOf(Sides, side);
        if (i < 0) return "";
        var armour = SideArmour;
        if (armour[i] > 0f)
        {
            armour[i] = Mathf.Max(0f, armour[i] - armourDamage);
            return " " + Sides[i] + (armour[i] <= 0f ? " OPEN" : " ARMOUR " + HandoffBoardState.JsRound(armour[i] / sideArmourMax[i] * 100f) + "%");
        }
        return " " + Sides[i] + Behind(i, compartmentDamage, "fighter");
    }

    // The room behind side i loses the damage given. A wrecked room takes no more, but the hit can still start
    // a condition there (Step 59). The ship is destroyed when its reactor's structure reaches 0 (any ship, ours
    // too: owner 2026-10-24).
    string Behind(int i) => Behind(i, slugCompartmentDamage, "slug");
    string Behind(int i, int damage, string weapon)
    {
        var hit = Comp(BehindSide[i]);
        if (hit == null) return "";
        string started = Strike(hit, weapon);
        if (hit.integ <= 0) return " · " + hit.tag + " WRECKED" + started;
        hit.integ = Mathf.Max(0, hit.integ - damage);
        if (Comp("rct").integ <= 0) ship.destroyed = true;
        return " · " + hit.tag + " −" + damage + started;
    }


    // ---- conditions: fire, vacuum, radiation (Step 59, owner 2026-10-24, DAMAGE_README.md) -------

    [Header("Conditions (Step 59, DAMAGE_README.md): every rate per game second")]
    [Tooltip("Chance a hit through open armour starts a fire: an HE warhead, a penetrator.")]
    [Tune("Damage control")] [Range(0f, 1f)] public float fireChanceHE = 0.6f, fireChancePEN = 0.4f;
    [Tooltip("Chance a railgun slug through open armour breaches the room (it loses its air).")]
    [Tune("Damage control")] [Range(0f, 1f)] public float breachChanceSlug = 0.5f;
    [Tooltip("Chance a hit on the reactor room (not a fighter's) leaks radiation.")]
    [Tune("Damage control")] [Range(0f, 1f)] public float radChanceReactor = 0.5f;
    [Tooltip("A fire takes 1 integrity off its room this often, game seconds.")]
    [Tune("Damage control")] public float fireDamageSeconds = 30f;
    [Tooltip("Chance per second that a fire, a vacuum or radiation spreads into each neighbouring room.")]
    [Tune("Damage control")] [Range(0f, 1f)] public float fireSpreadPerSecond = 0.01f, vacuumSpreadPerSecond = 0.01f, radSpreadPerSecond = 0.01f;
    [Tooltip("Share of a room's crew lost when it is breached (VENT loses all of them).")]
    [Tune("Damage control")] [Range(0f, 1f)] public float breachCrewLoss = 0.5f;
    [Tooltip("Radiation kills one crew member in its room this often, game seconds.")]
    [Tune("Damage control")] public float radCrewLossSeconds = 20f;
    [Tooltip("The reactor stops leaking once repaired to this integrity (or vented); then each room's radiation fades after radFadeSeconds.")]
    [Tune("Damage control")] public int reactorContainedAt = 3;
    [Tune("Damage control")] public float radFadeSeconds = 90f;
    /// <summary>Teams lost when their room vented around them.</summary>
    [System.NonSerialized] public int partiesLost;

    // The deck plan (owner 2026-10-24): which rooms touch which.
    static readonly System.Collections.Generic.Dictionary<string, string[]> Neighbours = new System.Collections.Generic.Dictionary<string, string[]>
    {
        { "mag", new[] { "bri", "rct", "hgr" } },        { "bri", new[] { "mag", "hab", "rct", "hgr" } },
        { "hab", new[] { "bri", "eng", "rct", "hgr" } }, { "eng", new[] { "hab", "rct", "hgr" } },
        { "rct", new[] { "mag", "bri", "hab", "eng" } }, { "hgr", new[] { "mag", "bri", "hab", "eng" } },
    };

    // What a hit through open armour starts: the weapon and the room decide (owner).
    string Strike(Compartment c, string weapon)
    {
        string started = "";
        float fireChance = weapon == "HE" ? fireChanceHE : weapon == "PEN" ? fireChancePEN : 0f;
        if (!c.vented && !c.fire && ship.Roll(fireChance)) { c.fire = true; c.fireT = 0; started += " · FIRE"; }
        if (weapon == "slug" && !c.vented && ship.Roll(breachChanceSlug)) { Breach(c, breachCrewLoss); started += " · BREACH"; }
        if (c.key == "rct" && weapon != "fighter" && !c.rad && ship.Roll(radChanceReactor)) { c.rad = true; c.fadeT = 0; started += " · RADIATION"; }
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
        bool leaking = rct != null && rct.rad && rct.integ < reactorContainedAt && !rct.vented;
        // spread from what stood at the start of the tick, so a condition moves one room per tick at most
        var burning = System.Array.FindAll(compartments, c => c.fire);
        var airless = System.Array.FindAll(compartments, c => c.vented && !c.doorsShut);
        var hot = leaking ? System.Array.FindAll(compartments, c => c.rad) : new Compartment[0];
        double pf = 1 - System.Math.Pow(1 - fireSpreadPerSecond, dt), pv = 1 - System.Math.Pow(1 - vacuumSpreadPerSecond, dt),
               pr = 1 - System.Math.Pow(1 - radSpreadPerSecond, dt);
        foreach (var c in burning)
            foreach (var k in Neighbours[c.key]) { var m = Comp(k); if (m != null && !m.fire && !m.vented && ship.Roll(pf)) { m.fire = true; m.fireT = 0; } }
        foreach (var c in airless)
            foreach (var k in Neighbours[c.key]) { var m = Comp(k); if (m != null && !m.vented && ship.Roll(pv)) Breach(m, breachCrewLoss); }
        foreach (var c in hot)
            foreach (var k in Neighbours[c.key]) { var m = Comp(k); if (m != null && !m.rad && ship.Roll(pr)) { m.rad = true; m.fadeT = 0; } }
        foreach (var c in compartments)
        {
            if (c.fire && c.vented) c.fire = false;
            if (c.fire)
                for (c.fireT += dt; c.fireT >= fireDamageSeconds; c.fireT -= fireDamageSeconds) c.integ = Mathf.Max(0, c.integ - 1);
            if (!c.rad) continue;
            if (c.crew > 0)
                for (c.radT += dt; c.radT >= radCrewLossSeconds && c.crew > 0; c.radT -= radCrewLossSeconds) c.crew--;
            if (leaking) c.fadeT = 0;
            else if ((c.fadeT += dt) >= radFadeSeconds) { c.rad = false; c.radT = 0; c.fadeT = 0; }
        }
        if (rct != null && rct.integ <= 0) ship.destroyed = true;
        SyncCond();
    }

    /// <summary>A contact's ship takes damage by our numbers: one Tuning window for every ship (the enemy is our
    /// ship with other stats; these are not among them).</summary>
    public void CopyDamageTuning(DamageSystem from)
    {
        slugArmourDamage = from.slugArmourDamage; slugCompartmentDamage = from.slugCompartmentDamage;
        heArmourDamage = from.heArmourDamage; empSeconds = from.empSeconds;
        fireChanceHE = from.fireChanceHE; fireChancePEN = from.fireChancePEN; breachChanceSlug = from.breachChanceSlug;
        radChanceReactor = from.radChanceReactor; fireDamageSeconds = from.fireDamageSeconds;
        fireSpreadPerSecond = from.fireSpreadPerSecond; vacuumSpreadPerSecond = from.vacuumSpreadPerSecond;
        radSpreadPerSecond = from.radSpreadPerSecond; breachCrewLoss = from.breachCrewLoss;
        radCrewLossSeconds = from.radCrewLossSeconds; reactorContainedAt = from.reactorContainedAt; radFadeSeconds = from.radFadeSeconds;
    }


    [Tooltip("Armour an HE warhead takes off the side it hits (owner: it opens a side fast).")]
    [Tune("Damage")] public float heArmourDamage = 100f;
    [Tooltip("An EMP knocks a ship's power out for this many mission seconds.")]
    [Tune("Damage")] public float empSeconds = 60f;

    /// <summary>A missile's warhead strikes this ship (Step 53, owner 2026-10-23). HE hits the armour hard,
    /// then what is behind it; PENETRATOR goes straight to a compartment behind the side; EMP does no
    /// hull damage but knocks the power out.</summary>
    public string TakeWarhead(string side, string warheadName)
    {
        if (ship.destroyed) return "";
        if (warheadName == "EMP")
        {
            ship.power.BlackOut();
            ship.empUntil = ship.missionNow + empSeconds;
            return " · EMP · POWER DOWN";
        }
        int i = System.Array.IndexOf(Sides, side);
        if (i < 0) return "";
        var armour = SideArmour;
        if (warheadName == "HE" && armour[i] > 0f)
        {
            armour[i] = Mathf.Max(0f, armour[i] - heArmourDamage);
            return " · HE" + (armour[i] <= 0f ? " OPEN" : " ARMOUR " + HandoffBoardState.JsRound(armour[i] / sideArmourMax[i] * 100f) + "%");
        }
        string w = warheadName == "PENETRATOR" ? "PEN" : warheadName;
        return " · " + w + Behind(i, slugCompartmentDamage, w);
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
        if (partyT < partySeconds - 1e-4f) return;
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
