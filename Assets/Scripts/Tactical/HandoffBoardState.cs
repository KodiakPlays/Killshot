using UnityEngine;

/// <summary>
/// The handoff board's own state and rules, ported from reference/board-logic.js — what the
/// board's controls do before any game system is wired in. Plain data: one instance lives on
/// HandoffBoardControls and every panel reads it. Grows one drawer at a time (Step 33b on).
/// Every starting value and constant is from the board's initial state and data tables.
/// </summary>
[System.Serializable]
public class HandoffBoardState
{
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

    [System.Serializable]
    public class Track
    {
        public string key, name, klass;
        public int bearing;
        public float rangeKm, speed;
        public bool hostile, own;
        [Tooltip("Bands it emits in: THERM, RAD, UV, EM.")]
        public string[] sig;
        [Tooltip("Exact world position (km), written by TacticalWorld. The plot draws the contact here; "
               + "bearing and range above are the rounded readout values.")]
        [System.NonSerialized] public bool hasWorld;
        [Tooltip("Step 60: taken by our marines (SECURE).")]
        [System.NonSerialized] public bool captured;
        [System.NonSerialized] public double worldX, worldY;
        [Tooltip("Step 48: which way the contact's bow points, degrees (from its model), for the side a slug strikes.")]
        [System.NonSerialized] public double worldHdg;
        [Tooltip("Step 52: which way it is actually going, degrees (its course over the ground).")]
        [System.NonSerialized] public double course;
        [Tooltip("Step 49: destroyed; cold, no longer hostile or heard, a wreck where it died.")]
        [System.NonSerialized] public bool wreck;
        [Tooltip("Step 75: where the sweep last touched it (See). Out of sight, the plot shows it here.")]
        [System.NonSerialized] public bool seen;
        [System.NonSerialized] public double seenX, seenY, seenBrg;
        [System.NonSerialized] public float seenRangeKm;
        [Tooltip("Step 75: its navigation, written by TacticalWorld; in sight, the plot draws it where its model glides.")]
        [System.NonSerialized] public NavSystem nav;
    }

    [Header("Contacts — TRACKS and CLASSES (the board's fixed mock-up contacts)")]
    public Track[] tracks =
    {
        new Track { key = "t07", name = "GARUDA", klass = "CRUISER", bearing = 48, rangeKm = 24300, speed = 1.10f, hostile = true, sig = new[] { "THERM", "RAD" } },
        new Track { key = "c17", name = "CONTACT 17", klass = "CORVETTE", bearing = 325, rangeKm = 31000, speed = 0.82f, hostile = true, sig = new[] { "RAD", "EM" } },
        new Track { key = "c22", name = "CONTACT 22", klass = "FRIGATE", bearing = 149, rangeKm = 16800, speed = 1.44f, hostile = true, sig = new[] { "THERM", "EM" } },
        new Track { key = "ftr", name = "FTR-2", klass = "FIGHTER", bearing = 241, rangeKm = 12200, speed = 2.60f, own = true, sig = new[] { "THERM", "UV" } },
        new Track { key = "bc1", name = "BC-01", klass = "BOAT", bearing = 71, rangeKm = 9500, speed = 0.41f, own = true, sig = new[] { "UV", "EM" } },
    };

    [Tooltip("Classified by the periscope (cls). Identified is on the SensorSystem.")]
    public System.Collections.Generic.List<string> classified = new System.Collections.Generic.List<string> { "bc1" };

    /// <summary>JS Math.round: halves round up. Mathf.RoundToInt rounds halves to even
    /// (4.5 → 4), which differs from the board at exact halves (Step 33c verification).</summary>
    public static int JsRound(float x) => Mathf.FloorToInt(x + 0.5f);

    [Header("Engineering and damage control — the board's initial state")]
    public string engSel = "rct";
    [Tooltip("ALERTS index: 0 = CONDITION 3, 1 = CONDITION 2, 2 = CONDITION 1 (set on the orders station).")]
    [Range(0, 2)] public int alert;
    [Tooltip("PARTY_TICKS: a damage-control party works this many game seconds (the design's 6 ticks at x1), then the repair lands.")]
    [Tune("Engineering & orders")]
    [UnityEngine.Serialization.FormerlySerializedAs("partyTicks")]
    public float partySeconds = 6f;
    [Tooltip("The party's compartment and how long it has worked, game seconds; -1 when no party is out.")]
    public string partyComp;
    public float partyT = -1;
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

    /// <summary>The mission clock as of the last tick (HandoffBoardControls writes it).</summary>
    [System.NonSerialized] public float missionNow;

    [Header("Periscope (Step 33h-3)")]
    [Tooltip("Periscope bearing, degrees 0..360 (scopeBrg); the board reads an unset bearing as 62.")]
    public double scopeBrg = 62;
    [Tooltip("Sight magnification, 1..8 in 0.1 steps (scopeZoom).")]
    public double scopeZoom = 1.4;
    [Tooltip("Degrees per px of slew drag (startSlew).")]
    [Tune("Periscope & fire control")]
    public double slewDegPerPx = 0.34;
    [Tooltip("A slew drag ignores the first few px.")]
    [Tune("Periscope & fire control")]
    public double slewDeadPx = 3;
    [Tooltip("px of MAG knob drag per 1x of zoom (magDown).")]
    [Tune("Periscope & fire control")]
    public double magPxPerX = 40;
    public bool slewing;
    public double slewDx;
    [Tooltip("BEARING needs the periscope within this of the target (onBearing).")]
    [Tune("Periscope & fire control")]
    public float onBearingDeg = 4f;

    /// <summary>This ship's navigation: its position, heading, throttle and movement (owner 2026-10-26).</summary>
    [System.NonSerialized] public NavSystem nav;
    /// <summary>Give this ship its navigation. It reads this ship's ENG bus and engine room.</summary>
    public void UseNav(NavSystem n) { nav = n; if (n) n.ship = this; }
    /// <summary>This ship's power: its reactor, buses and load shed (owner 2026-10-26).</summary>
    [System.NonSerialized] public PowerSystem power;
    /// <summary>Give this ship its power. It reads this ship's reactor room, EMP and death.</summary>
    public void UsePower(PowerSystem p) { power = p; if (p) p.ship = this; }
    /// <summary>This ship's sensors: DRADIS, active scan, its signal, what it has identified (owner 2026-10-26).</summary>
    [System.NonSerialized] public SensorSystem sensors;
    /// <summary>Give this ship its sensors. They read this ship's contacts, SEN bus and position.</summary>
    public void UseSensors(SensorSystem n) { sensors = n; if (n) n.ship = this; }
    /// <summary>This ship's comms: its radio, the carrier lock and the cipher dials (owner 2026-10-26).</summary>
    [System.NonSerialized] public CommsSystem comms;
    public void UseComms(CommsSystem c) { comms = c; }
    /// <summary>This ship's weapons: fire control, rounds in flight and point defence (owner 2026-10-26).</summary>
    [System.NonSerialized] public WeaponSystem weapons;
    /// <summary>Give this ship its weapons. They read this ship's contacts, WEP bus, periscope and position.</summary>
    public void UseWeapons(WeaponSystem w) { weapons = w; if (w) w.ship = this; }
    /// <summary>This ship's hangar: squadrons, launches, fighters and assault boats (owner 2026-10-26).</summary>
    [System.NonSerialized] public HangarSystem hangar;
    /// <summary>Give this ship its hangar. It reads this ship's contacts, AUX bus, compartments and position.</summary>
    public void UseHangar(HangarSystem h) { hangar = h; if (h) h.ship = this; }
    // The ship's position and heading, read by the rules below. Only its NavSystem changes them.
    double ownX => nav.x;
    double ownY => nav.y;
    int hdg => nav.heading;

    // ---- gates ---------------------------------------------------------------------------

    /// <summary>compHealth: a vented compartment counts as integrity 0.</summary>
    public int CompHealth(string key)
    {
        foreach (var c in compartments) if (c.key == key) return c.vented ? 0 : c.integ;
        return 0;
    }

    // A bus's units, read by the rules below. Only this ship's PowerSystem changes them.
    int BusUnits(string key) => power.BusUnits(key);

    public static int AngDiff(int a, int b) => Mathf.Abs(((a - b) % 360 + 540) % 360 - 180);

    // What this ship's sensors hear and scan, read by the rules below. Only its SensorSystem changes them.
    bool Heard(Track t) => sensors.Heard(t);
    bool ConeCovers(Track t) => sensors.ConeCovers(t);
    System.Collections.Generic.List<string> identified => sensors.identified;

    public Compartment Comp(string key) { foreach (var c in compartments) if (c.key == key) return c; return null; }
    public int CompIndex(string key) { for (int i = 0; i < compartments.Length; i++) if (compartments[i].key == key) return i; return 0; }
    public bool PartyOut => partyT >= 0;
    public bool MagOk => CompHealth("mag") >= 3;   // shells and warheads are struck up from here
    public bool HgrOk => CompHealth("hgr") >= 3;   // craft cannot be spotted in a wrecked bay
    public bool BriOk => CompHealth("bri") >= 3;   // the plot computer lives on the bridge
    public float EngFactor => CompHealth("eng") / 5f;
    /// <summary>repairRate: Condition 1 doubles repair.</summary>
    public int RepairRate => alert == 2 ? 2 : 1;

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

    // ---- handlers ------------------------------------------------------------------------

    public void CycleComp() { engSel = compartments[(CompIndex(engSel) + 1) % compartments.Length].key; }
    public void PickComp(string key) { if (Comp(key) != null) engSel = key; }

    /// <summary>engRun: refused silently when blocked; otherwise apply and flick back.</summary>
    public bool EngRun(string id, float now)
    {
        if (EngBlock(id) != null) return false;
        ApplyOrder(id);
        FlickBack("eng_" + id, now);
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


    // ---- plot tools (Step 33i) ---------------------------------------------------------------

    [System.Serializable]
    public class Mark
    {
        public int id;
        [Tooltip("point, ruler or ring.")]
        public string kind;
        [Tooltip("World km, x east, y north (Step 36: marks stay put in space). A ring is centred on our "
               + "ship wherever it is, so its x, y are unused; x2, y2 is its far point.")]
        public double x, y, x2, y2;
        [Tooltip("Mission clock at placement (points).")]
        public float t;
        [Tooltip("Step 46: the POINT ids a RULER's ends snapped to (0 = none).")]
        public int ptA, ptB;
    }

    // ---- the battle space (Step 65, owner 2026-10-25) --------------------------------------------
    // A rectangle centred on the world origin, 100,000 x 100,000 km to start (20 x 20 big squares of 5,000 km at x1).
    // Near the edge a warning; past it, a countdown to get back inside, or the ship is lost (the GDD's desertion rule;
    // Assets/Scripts/WorldBoundary.cs in the old build). Crossing drops the time rate to x1 (HandoffBoardControls).
    [Header("Battle space (Step 65)")]
    [Tooltip("East to west, km, centred on the world origin. Zooming out stops when it fills the plot's width. A multiple of 50,000 keeps the edge on a grid line at every zoom.")]
    [Tune("Arena")] public double arenaWidthKm = 100000;
    [Tooltip("North to south, km. Less than about half the width (the plot's shape) leaves black above and below it at full zoom-out. A multiple of 50,000 keeps the edge on a grid line.")]
    [Tune("Arena")] public double arenaHeightKm = 100000;
    public double ArenaHalfW => arenaWidthKm / 2;
    public double ArenaHalfH => arenaHeightKm / 2;
    [Tooltip("The edge warning shows this far inside the edge, km (half a big square).")]
    [Tune("Arena")] public double arenaWarnKm = 2500;
    [Tooltip("Game seconds to get back inside once out (owner: 30). Straight out at full speed the fastest return takes about 75 (brake, then turn); 30 is enough below about 4.4 km/s or at a shallow crossing. The warning is the time to turn.")]
    [Tune("Arena")] public float arenaReturnSeconds = 30f;
    /// <summary>Game seconds spent outside the battle space, or -1 while inside.</summary>
    [System.NonSerialized] public float outsideFor = -1f;
    /// <summary>Lost for leaving the battle space and not coming back in time.</summary>
    [System.NonSerialized] public bool deserted;
    public bool OutsideArena => System.Math.Abs(ownX) > ArenaHalfW || System.Math.Abs(ownY) > ArenaHalfH;
    public bool NearArenaEdge => !OutsideArena && System.Math.Min(ArenaHalfW - System.Math.Abs(ownX), ArenaHalfH - System.Math.Abs(ownY)) <= arenaWarnKm;

    /// <summary>Each tick, after we move: count the time outside, and lose the ship when it runs out.
    /// True on the tick we cross out.</summary>
    public bool TickArena(double dt)
    {
        if (destroyed) return false;
        if (!OutsideArena) { outsideFor = -1f; return false; }
        bool crossed = outsideFor < 0f;
        outsideFor = crossed ? 0f : outsideFor + (float)dt;
        if (outsideFor >= arenaReturnSeconds) { destroyed = true; deserted = true; }
        return crossed;
    }

    [Header("Plot marks (§8.4)")]
    public System.Collections.Generic.List<Mark> marks = new System.Collections.Generic.List<Mark>();
    [System.NonSerialized] public Mark draft;
    public int markSeq;
    [Tooltip("A RULER or COMPASS drag shorter than this (layer px) is discarded.")]
    [Tune("Plot tools")]
    public double markMinLen = 4;

    /// <summary>plotDown: POINT stamps a mark; RULER starts a line at the press; COMPASS a ring on own ship.</summary>
    public bool MarkDown(string tool, double x, double y, float missionT)
    {
        if (tool == "POINT") { marks.Add(new Mark { id = ++markSeq, kind = "point", x = x, y = y, t = missionT }); return false; }
        if (tool == "RULER") draft = new Mark { kind = "ruler", x = x, y = y, x2 = x, y2 = y };
        else if (tool == "COMPASS") draft = new Mark { kind = "ring", x = ownX, y = ownY, x2 = x, y2 = y };
        else return false;
        return true;
    }
    public void MarkMove(double x, double y) { if (draft != null) { draft.x2 = x; draft.y2 = y; } }
    /// <summary>plotUp: a draft shorter than 4 layer px (given in km by the plot) is dropped; otherwise it becomes a mark.</summary>
    public void MarkUp(double minLenKm)
    {
        var d = draft;
        draft = null;
        if (d == null || MarkLen(d) < minLenKm - 1e-6) return; // tolerance: the km conversion can land a hair under an exact 4 px (Step 36 verification)
        d.id = ++markSeq;
        marks.Add(d);
        if (weapons.Sol.set && weapons.Sol.manual) weapons.ApplyTrack(weapons.Sol); // Step 52: a new track feeds the hand-dialled solution
    }
    /// <summary>A mark's start in world km: a ring starts at our ship, wherever it is now.</summary>
    public double MarkX(Mark m) => m.kind == "ring" ? ownX : m.x;
    public double MarkY(Mark m) => m.kind == "ring" ? ownY : m.y;
    /// <summary>Length in km.</summary>
    public double MarkLen(Mark m) { double dx = m.x2 - MarkX(m), dy = m.y2 - MarkY(m); return System.Math.Sqrt(dx * dx + dy * dy); }
    public void EraseMark(int id) => marks.RemoveAll(m => m.id == id);

    /// <summary>Step 46: the nearest POINT within reach of a spot (km), or null — a RULER snaps to it.</summary>
    public Mark PointNear(double x, double y, double withinKm)
    {
        Mark best = null;
        double bestD = withinKm;
        foreach (var m in marks)
        {
            if (m.kind != "point") continue;
            double d = System.Math.Sqrt((m.x - x) * (m.x - x) + (m.y - y) * (m.y - y));
            if (d <= bestD) { best = m; bestD = d; }
        }
        return best;
    }

    /// <summary>Step 46 (owner 2026-10-23): a RULER from one POINT to another, with different times, is a
    /// target's track. Course runs from the earlier POINT to the later; speed is distance over time.</summary>
    public bool TrackLeg(Mark m, out Mark from, out Mark to, out double courseDeg, out double speedKms)
    {
        from = to = null; courseDeg = speedKms = 0;
        if (m.kind != "ruler" || m.ptA == 0 || m.ptB == 0) return false;
        var a = marks.Find(k => k.id == m.ptA);
        var b = marks.Find(k => k.id == m.ptB);
        if (a == null || b == null || a.t == b.t) return false;
        if (a.t > b.t) { var swap = a; a = b; b = swap; }
        double dx = b.x - a.x, dy = b.y - a.y;
        from = a; to = b;
        courseDeg = (System.Math.Atan2(dx, dy) * 180 / System.Math.PI + 360) % 360;
        speedKms = System.Math.Sqrt(dx * dx + dy * dy) / (b.t - a.t);
        return true;
    }

    /// <summary>lookAt: lower the scope (by the caller) and put it on the contact's bearing.</summary>
    public bool LookAt(string key) { var t = TrackOf(key); if (t == null) return false; scopeBrg = t.bearing; slewDx = 0; return true; }

    // ---- orders station (Step 33h-2) -------------------------------------------------------

    /// <summary>doAlert: CONDITION 3 → 2 → 1 → 3.</summary>
    public void DoAlert() { alert = (alert + 1) % 3; }

    /// <summary>ENG_ORDERS cost for a fitted order, or -1 (FIRE, BLKHD, SCRUB are not fitted).</summary>
    public int OrderCost(string id) => id == "party" ? orderCostParty : id == "seal" ? orderCostSeal : id == "vent" ? orderCostVent : -1;

    /// <summary>orderBlock: the orders station's refusals. Step 59: the same rules as engineering's rockers
    /// (the design's two sets disagreed on crew; DAMAGE_README.md has one set of rules).</summary>
    public string OrderBlock(string id) => id == "party" || id == "seal" || id == "vent" ? EngBlock(id) : null;

    /// <summary>runOrder: refused (too little AUX, or orderBlock) → the failure shows for 420 ms;
    /// otherwise spend the charge and apply. The rocker flicks back either way.</summary>
    public void RunOrder(string id, float now)
    {
        int cost = OrderCost(id);
        if (cost < 0) return;
        if (auxCharge < cost || OrderBlock(id) != null) { orderFail = id; orderFailUntil = now + orderFailSeconds; orderRefusals++; }
        else { auxCharge = System.Math.Max(0, auxCharge - cost); ApplyOrder(id); }
        FlickBack(id, now);
    }

    /// <summary>The tick: AUX charges by units x 3.2 per game second to 100 (owner 2026-10-24: every rate in game time).</summary>
    public void TickAux(double dt)
    {
        double rate = BusUnits("aux") * auxPerUnit * dt;
        if (rate > 0 && auxCharge < 100) auxCharge = System.Math.Min(100, auxCharge + rate);
    }

    public bool FlightOpsReleased => alert >= 1;
    public bool WeaponsReleased => alert >= 1;
    // ---- fire control ----------------------------------------------------------------------

    public Track TrackOf(string key) { foreach (var t in tracks) if (t.key == key) return t; return null; }
    /// <summary>Keep only the tracks a world contact drives (Step 34, TacticalWorld).</summary>
    public void KeepTracks(System.Collections.Generic.ICollection<string> keys)
    {
        tracks = System.Array.FindAll(tracks, t => keys.Contains(t.key));
        identified.RemoveAll(k => !keys.Contains(k));
        classified.RemoveAll(k => !keys.Contains(k));
    }
    // ---- periscope -----------------------------------------------------------------------

    public static double AngDiffD(double a, double b) => System.Math.Abs(((a - b) % 360 + 540) % 360 - 180);
    /// <summary>scopeOn: the scope is down and within 4° of the contact.</summary>
    public bool ScopeOn(Track t, bool scopeDown) => scopeDown && AngDiffD(t.bearing, scopeBrg) <= 4;
    /// <summary>scopeOffDeg: degrees from the target to the sight; 999 with no target.</summary>
    public double ScopeOffDeg { get { var t = TrackOf(weapons.target); return t == null ? 999 : AngDiffD(t.bearing, scopeBrg); } }
    public bool ScopeOnAxis => ScopeOffDeg <= 4;
    /// <summary>scopeSubject: the first heard contact in the sight.</summary>
    public Track ScopeSubject(bool scopeDown) { foreach (var t in tracks) if (Heard(t) && ScopeOn(t, scopeDown)) return t; return null; }
    public void Classify(bool scopeDown) { var t = ScopeSubject(scopeDown); if (t != null && !classified.Contains(t.key)) classified.Add(t.key); }

    /// <summary>moveSlew: bearing = start + dx x 0.34, wrapped; ignores the first 3 px. Any slew
    /// releases BEARING and ARM (not the rungs after them), rests the lever and clears fired.</summary>
    public void Slew(double brg0, double dx)
    {
        if (System.Math.Abs(dx) < slewDeadPx) return;
        scopeBrg = ((brg0 + dx * slewDegPerPx) % 360 + 360) % 360;
        slewDx = dx;
        weapons.sw[2] = false; weapons.sw[3] = false;
        weapons.fireLever = 0f;
        weapons.fired = false;
    }
    public void EndSlew() { slewing = false; slewDx = 0; }
    /// <summary>slewTo: jump to the target's bearing; the switches are left as they are.</summary>
    public void SlewTo()
    {
        if (weapons.Sol.set && weapons.Sol.manual) { scopeBrg = ((hdg + weapons.Sol.brg) % 360 + 360) % 360; slewing = false; slewDx = 0; return; } // onto the dialled bearing
        var t = TrackOf(weapons.target); if (t == null) return; scopeBrg = t.bearing; slewing = false; slewDx = 0;
    }
    /// <summary>magDown: zoom = start + dx / 40, 1..8, rounded to 0.1.</summary>
    public void SetZoom(double z0, double dx) => scopeZoom = JsRound(System.Math.Max(1, System.Math.Min(8, z0 + dx / magPxPerX)) * 10) / 10.0;
    /// <summary>The damage model of the ship behind a track (TacticalWorld supplies it), or null.</summary>
    [System.NonSerialized] public System.Func<string, HandoffBoardState> ShipOf;
    [System.NonSerialized] public int kills;

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
    /// <summary>Destroyed: its reactor is at zero (the kill condition, FTUE §4.4), or it stayed
    /// outside the battle space too long (deserted).</summary>
    [System.NonSerialized] public bool destroyed;
    /// <summary>Taken by our marines (SECURE): it stops fighting and is a prize.</summary>
    [System.NonSerialized] public bool captured;
    [System.NonSerialized] private System.Random dice;

    /// <summary>A slug strikes this ship's side. Armour first; once the side is open, the room behind it
    /// loses integrity. Returns the report's tail (" OPEN", " · RCT −2").</summary>
    public string TakeSlug(string side)
    {
        int i = System.Array.IndexOf(Sides, side);
        if (i < 0 || destroyed) return "";
        var armour = SideArmour;
        if (armour[i] > 0f)
        {
            armour[i] = Mathf.Max(0f, armour[i] - slugArmourDamage);
            return armour[i] <= 0f ? " OPEN" : " ARMOUR " + JsRound(armour[i] / sideArmourMax[i] * 100f) + "%";
        }
        return Behind(i);
    }

    /// <summary>One fighter's attack run on this ship (Step 54). DESTROY: the side facing it, then the room
    /// behind. DISABLE: always the stern, so the engine room (Step 58).</summary>
    public string TakeFighterRun(string side, bool disable, float armourDamage, int compartmentDamage)
    {
        if (destroyed) return "";
        int i = disable ? 2 : System.Array.IndexOf(Sides, side);
        if (i < 0) return "";
        var armour = SideArmour;
        if (armour[i] > 0f)
        {
            armour[i] = Mathf.Max(0f, armour[i] - armourDamage);
            return " " + Sides[i] + (armour[i] <= 0f ? " OPEN" : " ARMOUR " + JsRound(armour[i] / sideArmourMax[i] * 100f) + "%");
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
        if (Comp("rct").integ <= 0) destroyed = true;
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
        if (!c.vented && !c.fire && Roll(fireChance)) { c.fire = true; c.fireT = 0; started += " · FIRE"; }
        if (weapon == "slug" && !c.vented && Roll(breachChanceSlug)) { Breach(c, breachCrewLoss); started += " · BREACH"; }
        if (c.key == "rct" && weapon != "fighter" && !c.rad && Roll(radChanceReactor)) { c.rad = true; c.fadeT = 0; started += " · RADIATION"; }
        SyncCond();
        return started;
    }

    public bool Roll(double p)
    {
        if (p <= 0) return false;
        return NextDie() < p;
    }

    /// <summary>The next roll of this ship's dice, 0..1. Damage and the PDCs share one sequence, seeded 49.</summary>
    public double NextDie()
    {
        if (dice == null) dice = new System.Random(49);
        return dice.NextDouble();
    }

    // A room loses its air: the fire goes out, that share of its crew die, and a team working there is lost.
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
        if (destroyed) return;
        var rct = Comp("rct");
        bool leaking = rct != null && rct.rad && rct.integ < reactorContainedAt && !rct.vented;
        // spread from what stood at the start of the tick, so a condition moves one room per tick at most
        var burning = System.Array.FindAll(compartments, c => c.fire);
        var airless = System.Array.FindAll(compartments, c => c.vented && !c.doorsShut);
        var hot = leaking ? System.Array.FindAll(compartments, c => c.rad) : new Compartment[0];
        double pf = 1 - System.Math.Pow(1 - fireSpreadPerSecond, dt), pv = 1 - System.Math.Pow(1 - vacuumSpreadPerSecond, dt),
               pr = 1 - System.Math.Pow(1 - radSpreadPerSecond, dt);
        foreach (var c in burning)
            foreach (var k in Neighbours[c.key]) { var m = Comp(k); if (m != null && !m.fire && !m.vented && Roll(pf)) { m.fire = true; m.fireT = 0; } }
        foreach (var c in airless)
            foreach (var k in Neighbours[c.key]) { var m = Comp(k); if (m != null && !m.vented && Roll(pv)) Breach(m, breachCrewLoss); }
        foreach (var c in hot)
            foreach (var k in Neighbours[c.key]) { var m = Comp(k); if (m != null && !m.rad && Roll(pr)) { m.rad = true; m.fadeT = 0; } }
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
        if (rct != null && rct.integ <= 0) destroyed = true;
        SyncCond();
    }

    /// <summary>A contact's ship takes damage by our numbers: one Tuning window for every ship (the enemy is our
    /// ship with other stats; these are not among them).</summary>
    public void CopyDamageTuning(HandoffBoardState from)
    {
        slugArmourDamage = from.slugArmourDamage; slugCompartmentDamage = from.slugCompartmentDamage;
        heArmourDamage = from.heArmourDamage; empSeconds = from.empSeconds;
        fireChanceHE = from.fireChanceHE; fireChancePEN = from.fireChancePEN; breachChanceSlug = from.breachChanceSlug;
        radChanceReactor = from.radChanceReactor; fireDamageSeconds = from.fireDamageSeconds;
        fireSpreadPerSecond = from.fireSpreadPerSecond; vacuumSpreadPerSecond = from.vacuumSpreadPerSecond;
        radSpreadPerSecond = from.radSpreadPerSecond; breachCrewLoss = from.breachCrewLoss;
        radCrewLossSeconds = from.radCrewLossSeconds; reactorContainedAt = from.reactorContainedAt; radFadeSeconds = from.radFadeSeconds;
    }

    /// <summary>A contact's ship destroyed without a hit landing (its reactor burned out) becomes a wreck too.</summary>
    public void NoteWrecks()
    {
        if (ShipOf == null) return;
        foreach (var t in tracks)
        {
            if (t.own || t.wreck) continue;
            var ship = ShipOf(t.key);
            if (ship != null && ship.destroyed) Kill(t);
        }
    }

    [Tooltip("Armour an HE warhead takes off the side it hits (owner: it opens a side fast).")]
    [Tune("Damage")] public float heArmourDamage = 100f;
    [Tooltip("An EMP knocks a ship's power out for this many mission seconds.")]
    [Tune("Damage")] public float empSeconds = 60f;
    /// <summary>Power is out until this mission time (EMP).</summary>
    [System.NonSerialized] public float empUntil;

    /// <summary>A missile's warhead strikes this ship (Step 53, owner 2026-10-23). HE hits the armour hard,
    /// then what is behind it; PENETRATOR goes straight to a compartment behind the side; EMP does no
    /// hull damage but knocks the power out.</summary>
    public string TakeWarhead(string side, string warheadName)
    {
        if (destroyed) return "";
        if (warheadName == "EMP")
        {
            power.BlackOut();
            empUntil = missionNow + empSeconds;
            return " · EMP · POWER DOWN";
        }
        int i = System.Array.IndexOf(Sides, side);
        if (i < 0) return "";
        var armour = SideArmour;
        if (warheadName == "HE" && armour[i] > 0f)
        {
            armour[i] = Mathf.Max(0f, armour[i] - heArmourDamage);
            return " · HE" + (armour[i] <= 0f ? " OPEN" : " ARMOUR " + JsRound(armour[i] / sideArmourMax[i] * 100f) + "%");
        }
        string w = warheadName == "PENETRATOR" ? "PEN" : warheadName;
        return " · " + w + Behind(i, slugCompartmentDamage, w);
    }

    // The side of a ship whose bow points bowDeg that faces a bearing fromDeg (both clockwise from north).
    public static string SideAt(double fromDeg, double bowDeg)
    {
        double rel = ((fromDeg - bowDeg) % 360 + 360) % 360;
        return rel < 45 || rel >= 315 ? "PROW" : rel < 135 ? "STBD" : rel < 225 ? "AFT" : "PORT";
    }

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

    // The side of a ship facing a point (where the attack comes from), against its bow.
    public string NameOf(Track t) => Scanned(t) ? t.name : "BOGEY " + t.key.ToUpperInvariant();
    public bool Scanned(Track t) => t != null && identified.Contains(t.key);

    public void Kill(Track t)
    {
        t.wreck = true; t.hostile = false;
        weapons.shotReport = NameOf(t) + " DESTROYED";
        kills++; weapons.shotResults++; weapons.lastShotHit = true;
    }

    // ---- the tick (tickPower, every 420 ms). The buses fill in PowerSystem.Tick (owner 2026-10-26). --

    // Owner 2026-10-24: every rate runs in game time. The design's per-tick amounts were per game second
    // at x1 (a tick is `rate` game seconds), so they are now multiplied by the tick's game seconds; x1 is
    // unchanged.

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

    // ---- number helpers (JS rounding, as the browser board does) ----------------------------

    /// <summary>JS Math.round on a double: half rounds up.</summary>
    public static int JsRound(double x) => (int)System.Math.Floor(x + 0.5);

    /// <summary>JS toFixed for a positive number: rounds on the double's exact binary value
    /// (1.45 is 1.4499…, so "1.4"); a true half rounds up, as toFixed does.</summary>
    public static string JsToFixed(double x, int digits)
    {
        long bits = System.BitConverter.DoubleToInt64Bits(x);
        int e = (int)((bits >> 52) & 0x7FF);
        long m = bits & ((1L << 52) - 1);
        if (e == 0) e = 1; else m |= 1L << 52;
        int exp = e - 1075; // x = m * 2^exp
        var num = new System.Numerics.BigInteger(m) * System.Numerics.BigInteger.Pow(10, digits);
        System.Numerics.BigInteger q;
        if (exp >= 0) q = num << exp;
        else
        {
            var den = System.Numerics.BigInteger.One << -exp;
            var r = System.Numerics.BigInteger.Remainder(num, den);
            q = System.Numerics.BigInteger.Divide(num, den);
            if (r * 2 >= den) q += 1;
        }
        string t = q.ToString().PadLeft(digits + 1, '0');
        return digits == 0 ? t : t.Substring(0, t.Length - digits) + "." + t.Substring(t.Length - digits);
    }

    /// <summary>offBearingLabel: "+1.5°" / "-14.0°" / "—" (toFixed(1), sign kept on a negative).</summary>
    public static string OffLabel(double? o) => o == null ? "\u2014" : (o.Value > 0 ? "+" : o.Value < 0 ? "-" : "") + JsToFixed(System.Math.Abs(o.Value), 1) + "\u00B0";
}
