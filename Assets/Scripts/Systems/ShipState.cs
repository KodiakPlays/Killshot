// =================================================================================================
// ShipState.cs
//
// What it does:
// The shared state of one ship in a fight. BoardManager keeps one for our ship, and every panel on
// the board reads it. WorldContact makes one for each other ship. It joins the ship's eight systems
// together. It keeps what is not in any one system: contacts, plot marks, the periscope and the
// battle space.
//
// How it works:
// - Each ship gets its systems (NavSystem, PowerSystem, SensorSystem, CommsSystem, WeaponSystem,
//   HangarSystem, DamageSystem, OrdersSystem). Most systems can then reach the others through it.
// - Contacts: the list of other ships. Each has a bearing, range, speed, the bands it gives off and
//   its place in the world. TacticalWorld keeps only the real ones and writes their places.
// - A contact shows as BOGEY plus its key until it is scanned, then by its name.
// - Plot marks: POINT drops a mark with the mission time. RULER and COMPASS draw a line or a ring
//   while dragging. A RULER from one POINT to a later POINT gives a course and a speed.
//   WeaponSystem uses that for a firing solution.
// - Periscope: its bearing, its zoom (1 to 8), and turning it by dragging. Turning it drops the
//   BEARING and ARM switches unless a lock is on. It says how full the sight's brackets are.
// - Battle space: a box round the middle of the map. Its size is set in the Tuning window. A
//   level's BattleSpace can change it. Near the edge a warning shows. Outside it, a countdown runs.
//   If the ship is still outside at the end, it is lost. Only our ship is checked.
// - A ship marked cannot-be-destroyed by its WorldContact never dies. DamageSystem and
//   HangarSystem read this.
// - It marks a contact as a wreck when its ship is destroyed, and counts the kill.
// - Each ship has a set of dice with a fixed start, for damage chances and the PDCs.
// - It works out where a straight shot path meets a ship's box, and the side it goes through.
//   Its number helpers round the same way as the web page version of the board.
//
// Technical:
// - Data: a plain class on no object. Our ship's copy is saved in the scene, on BoardManager.
//   WorldContact makes one in code for each other ship, with the code's starting values.
// - The Use methods link the eight systems and set each system's ship field. CommsSystem gets no
//   link back.
// - tracks is the contact list. TacticalWorld calls KeepTracks and AddTrack, and writes each
//   Track's world place. It sets ShipOf and RockOnLine on our ShipState only. Others have none.
// - missionNow is the mission clock in game seconds. BoardManager sets it for our ship.
//   TacticalWorld copies it to the others.
// - PlotPanel calls MarkDown, MarkMove, MarkUp, PointNear and EraseMark. WeaponSystem and PlotPanel
//   call TrackLeg.
// - PeriscopePanel calls Slew, EndSlew, SlewTo and SetZoom. It reads ScopeFill and ScopeFits.
//   Slew clears weapons.sw[2] and sw[3] unless locked.
// - BoardManager calls TickArena and NoteWrecks each step. WeaponSystem and HangarSystem call Kill.
//   WeaponSystem calls EntersBox for each shot.
// - Roll and NextDie use System.Random seeded 49. DamageSystem, WeaponSystem, HangarSystem and
//   EnemyBrain share it. The rolls start the same every game.
// - Numbers: [Tune] sections "Arena", "Periscope & fire control" and "Plot tools" on the Tuning
//   window's Board tab.
// - Trap: use JsRound for shown numbers. Mathf.RoundToInt rounds halves to even.
// =================================================================================================

using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class ShipState
{
    [System.Serializable]
    public class Track
    {
        public string key, name, klass;
        public int bearing;
        [FormerlySerializedAs("rangeKm")] public float rangeUnits;
        public float speed;
        public bool hostile, own;
        [Tooltip("Bands it emits in: THERM, RAD, UV, EM.")]
        public string[] sig;
        [Tooltip("Exact world position (units), written by TacticalWorld. The plot draws the contact here; "
               + "bearing and range above are the rounded readout values.")]
        [System.NonSerialized] public bool hasWorld;
        [Tooltip("Step 60: taken by our marines (SECURE).")]
        [System.NonSerialized] public bool captured;
        [Tooltip("Owner 2026-11-09: one of our battle group. Blue once scanned; never hostile, so never a target.")]
        [System.NonSerialized] public bool friendly;
        [System.NonSerialized] public double worldX, worldY;
        [Tooltip("Owner 2026-11-02: units the level's rocks take off our DRADIS and scan reach to this ship (TacticalWorld.Interference).")]
        [System.NonSerialized] public float interference;
        [Tooltip("Owner 2026-11-02: a rock is on the straight line to this ship, so eyes can't see it.")]
        [System.NonSerialized] public bool rockOnLine;
        [Tooltip("Step 48: which way the contact's bow points, degrees (from its model), for the side a slug strikes.")]
        [System.NonSerialized] public double worldHdg;
        [Tooltip("Step 52: which way it is actually going, degrees (its course over the ground).")]
        [System.NonSerialized] public double course;
        [Tooltip("Step 49: destroyed; cold, no longer hostile or heard, a wreck where it died.")]
        [System.NonSerialized] public bool wreck;
        [Tooltip("Owner 2026-11-01: when it was destroyed, and when it was last in sight as a wreck, game seconds. Out of sight, its dot fades from then.")]
        [System.NonSerialized] public float wreckAt, wreckSeenAt;
        [Tooltip("Owner 2026-11-01: where it broke in two, 0 its stern to 1 its nose.")]
        [System.NonSerialized] public float breakAt;
        [Tooltip("Step 75: where the sweep last touched it (See). Out of sight, the plot shows it here.")]
        [System.NonSerialized] public bool seen;
        [System.NonSerialized] public double seenX, seenY, seenBrg;
        [Tooltip("Owner 2026-11-01: when that fix was, game seconds. The plot fades its marker as it gets old.")]
        [System.NonSerialized] public float seenAt;
        [System.NonSerialized] public float seenRangeUnits;
        [Tooltip("Step 75: its navigation, written by TacticalWorld; in sight, the plot draws it where its model glides.")]
        [System.NonSerialized] public NavSystem nav;
    }

    [Header("Contacts — TRACKS and CLASSES (the board's fixed mock-up contacts)")]
    public Track[] tracks =
    {
        new Track { key = "t07", name = "GARUDA", klass = "CRUISER", bearing = 48, rangeUnits = 24300, speed = 1.10f, hostile = true, sig = new[] { "THERM", "RAD" } },
        new Track { key = "c17", name = "CONTACT 17", klass = "CORVETTE", bearing = 325, rangeUnits = 31000, speed = 0.82f, hostile = true, sig = new[] { "RAD", "EM" } },
        new Track { key = "c22", name = "CONTACT 22", klass = "FRIGATE", bearing = 149, rangeUnits = 16800, speed = 1.44f, hostile = true, sig = new[] { "THERM", "EM" } },
        new Track { key = "ftr", name = "FTR-2", klass = "FIGHTER", bearing = 241, rangeUnits = 12200, speed = 2.60f, own = true, sig = new[] { "THERM", "UV" } },
        new Track { key = "bc1", name = "BC-01", klass = "BOAT", bearing = 71, rangeUnits = 9500, speed = 0.41f, own = true, sig = new[] { "UV", "EM" } },
    };

    [Tooltip("Classified by the periscope (cls). Identified is on the SensorSystem.")]

    /// <summary>JS Math.round: halves round up. Mathf.RoundToInt rounds halves to even
    /// (4.5 → 4), which differs from the board at exact halves (Step 33c verification).</summary>
    public static int JsRound(float x) => Mathf.FloorToInt(x + 0.5f);

    /// <summary>The mission clock as of the last tick (BoardManager writes it).</summary>
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
    [Tooltip("Step 109: at 1x a ship this far away (units) fills the brackets; at 2x, one twice as far.")]
    [Tune("Periscope & fire control")]
    [FormerlySerializedAs("scopeFitKm")] public float scopeFitUnits = 3750f;
    [Tooltip("Step 109: CLASS works while the ship is within this share of the brackets' size, bigger or smaller.")]
    [Tune("Periscope & fire control")]
    public float scopeFitSlack = 0.15f;
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
    /// <summary>This ship's damage: compartments, hull, conditions, crew and damage control (owner 2026-10-26).</summary>
    [System.NonSerialized] public DamageSystem damage;
    /// <summary>Give this ship its damage model. It reads and sets this ship's status, EMP clock and dice.</summary>
    public void UseDamage(DamageSystem d) { damage = d; if (d) d.ship = this; }
    /// <summary>This ship's orders station: alert condition, AUX order pool and flicking rockers (owner 2026-10-26).</summary>
    [System.NonSerialized] public OrdersSystem orders;
    /// <summary>Give this ship its orders station. It reads this ship's AUX bus and damage control.</summary>
    public void UseOrders(OrdersSystem o) { orders = o; if (o) o.ship = this; }
    // The ship's position and heading, read by the rules below. Only its NavSystem changes them.
    double ownX => nav.x;
    double ownY => nav.y;
    int hdg => nav.heading;

    // ---- gates ---------------------------------------------------------------------------

    // A bus's units, read by the rules below. Only this ship's PowerSystem changes them.
    int BusUnits(string key) => power.BusUnits(key);

    public static int AngDiff(int a, int b) => Mathf.Abs(((a - b) % 360 + 540) % 360 - 180);

    // What this ship's sensors hear and scan, read by the rules below. Only its SensorSystem changes them.
    bool Heard(Track t) => sensors.Heard(t);
    bool ConeCovers(Track t) => sensors.ConeCovers(t);
    System.Collections.Generic.List<string> identified => sensors.identified;

    // ---- handlers ------------------------------------------------------------------------

    // ---- plot tools (Step 33i) ---------------------------------------------------------------

    [System.Serializable]
    public class Mark
    {
        public int id;
        [Tooltip("point, ruler or ring.")]
        public string kind;
        [Tooltip("World units, x east, y north (Step 36: marks stay put in space). A ring is centred on our "
               + "ship wherever it is, so its x, y are unused; x2, y2 is its far point.")]
        public double x, y, x2, y2;
        [Tooltip("Mission clock at placement (points).")]
        public float t;
        [Tooltip("Step 46: the POINT ids a RULER's ends snapped to (0 = none).")]
        public int ptA, ptB;
    }

    // ---- the battle space (Step 65, owner 2026-10-25) --------------------------------------------
    // A rectangle centred on the world origin, 100,000 x 100,000 units to start (200 x 200 big squares of 500 units).
    // Near the edge a warning; past it, a countdown to get back inside, or the ship is lost (the GDD's desertion rule;
    // Assets/Scripts/WorldBoundary.cs in the old build). Crossing drops the time rate to x1 (BoardManager).
    [Header("Battle space (Step 65)")]
    [Tooltip("East to west, units, centred on the world origin. A multiple of 1,000 keeps the edge on a big grid line.")]
    [FormerlySerializedAs("arenaWidthKm")] [Tune("Arena")] public double arenaWidthUnits = 100000;
    [Tooltip("North to south, units. A multiple of 1,000 keeps the edge on a big grid line.")]
    [FormerlySerializedAs("arenaHeightKm")] [Tune("Arena")] public double arenaHeightUnits = 100000;
    public double ArenaHalfW => arenaWidthUnits / 2;
    public double ArenaHalfH => arenaHeightUnits / 2;
    [Tooltip("The edge warning shows this far inside the edge, units.")]
    [FormerlySerializedAs("arenaWarnKm")] [Tune("Arena")] public double arenaWarnUnits = 2500;
    [Tooltip("Game seconds to get back inside once out (owner: 30). Straight out at full speed the fastest return takes about 75 (brake, then turn); 30 is enough below about 4.4 units/s or at a shallow crossing. The warning is the time to turn.")]
    [Tune("Arena")] public float arenaReturnSeconds = 30f;
    /// <summary>Game seconds spent outside the battle space, or -1 while inside.</summary>
    [System.NonSerialized] public float outsideFor = -1f;
    /// <summary>Lost for leaving the battle space and not coming back in time.</summary>
    [System.NonSerialized] public bool deserted;
    public bool OutsideArena => System.Math.Abs(ownX) > ArenaHalfW || System.Math.Abs(ownY) > ArenaHalfH;
    public bool NearArenaEdge => !OutsideArena && System.Math.Min(ArenaHalfW - System.Math.Abs(ownX), ArenaHalfH - System.Math.Abs(ownY)) <= arenaWarnUnits;

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
    /// <summary>plotUp: a draft shorter than 4 layer px (given in units by the plot) is dropped; otherwise it becomes a mark.</summary>
    public void MarkUp(double minLenUnits)
    {
        var d = draft;
        draft = null;
        if (d == null || MarkLen(d) < minLenUnits - 1e-6) return; // tolerance: the units conversion can land a hair under an exact 4 px (Step 36 verification)
        d.id = ++markSeq;
        marks.Add(d);
        if (weapons.Sol.set && weapons.Sol.manual) weapons.ApplyTrack(weapons.Sol); // Step 52: a new track feeds the hand-dialled solution
    }
    /// <summary>A mark's start in world units: a ring starts at our ship, wherever it is now.</summary>
    public double MarkX(Mark m) => m.kind == "ring" ? ownX : m.x;
    public double MarkY(Mark m) => m.kind == "ring" ? ownY : m.y;
    /// <summary>Length in units.</summary>
    public double MarkLen(Mark m) { double dx = m.x2 - MarkX(m), dy = m.y2 - MarkY(m); return System.Math.Sqrt(dx * dx + dy * dy); }
    public void EraseMark(int id) => marks.RemoveAll(m => m.id == id);

    /// <summary>Step 46: the nearest POINT within reach of a spot (units), or null — a RULER snaps to it.</summary>
    public Mark PointNear(double x, double y, double withinUnits)
    {
        Mark best = null;
        double bestD = withinUnits;
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
    public bool TrackLeg(Mark m, out Mark from, out Mark to, out double courseDeg, out double speedUnits)
    {
        from = to = null; courseDeg = speedUnits = 0;
        if (m.kind != "ruler" || m.ptA == 0 || m.ptB == 0) return false;
        var a = marks.Find(k => k.id == m.ptA);
        var b = marks.Find(k => k.id == m.ptB);
        if (a == null || b == null || a.t == b.t) return false;
        if (a.t > b.t) { var swap = a; a = b; b = swap; }
        double dx = b.x - a.x, dy = b.y - a.y;
        from = a; to = b;
        courseDeg = (System.Math.Atan2(dx, dy) * 180 / System.Math.PI + 360) % 360;
        speedUnits = System.Math.Sqrt(dx * dx + dy * dy) / (b.t - a.t);
        return true;
    }

    /// <summary>lookAt: lower the scope (by the caller) and put it on the contact's bearing.</summary>
    public bool LookAt(string key)
    {
        var t = TrackOf(key); if (t == null) return false;
        scopeBrg = t.bearing; slewDx = 0;
        return true;
    }

    // ---- fire control ----------------------------------------------------------------------

    public Track TrackOf(string key) { foreach (var t in tracks) if (t.key == key) return t; return null; }
    /// <summary>Wolf Hunt (Step 99): a ship that arrives during play gets a track.</summary>
    public void AddTrack(Track t) { System.Array.Resize(ref tracks, tracks.Length + 1); tracks[tracks.Length - 1] = t; }
    /// <summary>Keep only the tracks a world contact drives (Step 34, TacticalWorld).</summary>
    public void KeepTracks(System.Collections.Generic.ICollection<string> keys)
    {
        tracks = System.Array.FindAll(tracks, t => keys.Contains(t.key));
        identified.RemoveAll(k => !keys.Contains(k));
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
    /// <summary>Step 109: how big a ship looks in the sight against the brackets (1 fills them). Zoom in, or close in, to grow it.</summary>
    public double ScopeFill(Track t) => scopeZoom * scopeFitUnits / System.Math.Max(1.0, t.rangeUnits);
    /// <summary>Step 109 (owner 2026-10-29): a lock needs the ship to fill the brackets. At 1x a close ship may spill out: the MAG goes no lower.</summary>
    public bool ScopeFits(Track t) { double f = ScopeFill(t); return f >= 1 - scopeFitSlack && (f <= 1 + scopeFitSlack || scopeZoom <= 1); }

    /// <summary>moveSlew: bearing = start + dx x 0.34, wrapped; ignores the first 3 px. Any slew
    /// releases BEARING and ARM (not the rungs after them), rests the lever and clears fired.</summary>
    public void Slew(double brg0, double dx)
    {
        if (System.Math.Abs(dx) < slewDeadPx) return;
        scopeBrg = ((brg0 + dx * slewDegPerPx) % 360 + 360) % 360;
        slewDx = dx;
        if (weapons.Locked) return; // owner 2026-11-01: a lock stays on while the periscope moves
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
    [System.NonSerialized] public System.Func<string, ShipState> ShipOf;
    /// <summary>TacticalWorld.RockOnLine, for eyes that aren't our ship's (owner 2026-11-02).</summary>
    [System.NonSerialized] public System.Func<Vector2, Vector2, bool> RockOnLine;
    [System.NonSerialized] public int kills;

    /// <summary>Destroyed: its reactor is at zero (the kill condition, FTUE §4.4), or it stayed
    /// outside the battle space too long (deserted).</summary>
    [System.NonSerialized] public bool destroyed;
    /// <summary>Owner 2026-11-10: a mission piece that can't be destroyed (set from its WorldContact). DamageSystem keeps its reactor at 1 or more.</summary>
    [System.NonSerialized] public bool cannotBeDestroyed;
    /// <summary>Taken by our marines (SECURE): it stops fighting and is a prize.</summary>
    [System.NonSerialized] public bool captured;
    [System.NonSerialized] private System.Random dice;
    /// <summary>Power is out until this mission time (EMP).</summary>
    [System.NonSerialized] public float empUntil;

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

    /// <summary>Owner 2026-11-01: a straight path against a ship's box. The path's two ends are given from the box's
    /// centre, and the box is turned to bowDeg, hw and hl across its half width and half length. Does the path reach
    /// the box? If so: how far along (0 to 1), and through which side.</summary>
    public static bool EntersBox(double fromX, double fromY, double toX, double toY, double bowDeg, double hw, double hl, out double at, out string side)
    {
        double b = bowDeg * System.Math.PI / 180, sin = System.Math.Sin(b), cos = System.Math.Cos(b);
        // on the box's own axes: x to starboard, y to the bow
        double ax = fromX * cos - fromY * sin, ay = fromX * sin + fromY * cos;
        double bx = toX * cos - toY * sin, by = toX * sin + toY * cos;
        double inX, outX, inY, outY;
        Slab(ax, bx - ax, hw, out inX, out outX);
        Slab(ay, by - ay, hl, out inY, out outY);
        at = System.Math.Max(0, System.Math.Max(inX, inY));
        side = null;
        if (at > System.Math.Min(1, System.Math.Min(outX, outY))) return false;
        // the last of the two pairs of sides it crossed is the one it came through; started inside, the side it is nearest
        bool beam = inX > 0 || inY > 0 ? inX >= inY : System.Math.Abs(ax) / hw >= System.Math.Abs(ay) / hl;
        side = beam ? (ax > 0 ? "STBD" : "PORT") : (ay > 0 ? "PROW" : "AFT");
        return true;
    }

    // When a point moving along one axis is between -half and half: from `enter` to `leave`, as shares of its move
    static void Slab(double start, double move, double half, out double enter, out double leave)
    {
        if (System.Math.Abs(move) < 1e-9)
        {
            bool inside = System.Math.Abs(start) <= half;
            enter = inside ? double.NegativeInfinity : double.PositiveInfinity;
            leave = inside ? double.PositiveInfinity : double.NegativeInfinity;
            return;
        }
        double a = (-half - start) / move, b = (half - start) / move;
        enter = System.Math.Min(a, b); leave = System.Math.Max(a, b);
    }

    public static string SideAt(double fromDeg, double bowDeg)
    {
        double rel = ((fromDeg - bowDeg) % 360 + 360) % 360;
        return rel < 45 || rel >= 315 ? "PROW" : rel < 135 ? "STBD" : rel < 225 ? "AFT" : "PORT";
    }

    // The side of a ship facing a point (where the attack comes from), against its bow.
    public string NameOf(Track t) => Scanned(t) ? t.name : "BOGEY " + t.key.ToUpperInvariant();
    public bool Scanned(Track t) => t != null && identified.Contains(t.key);

    public void Kill(Track t, float breakAt = 0.5f)
    {
        t.wreck = true; t.hostile = false;
        t.wreckAt = t.wreckSeenAt = missionNow;
        t.breakAt = Mathf.Clamp(breakAt, 0.3f, 0.7f); // never a sliver
        weapons.shotReport = NameOf(t) + " DESTROYED";
        kills++; weapons.shotResults++; weapons.lastShotHit = true;
    }

    // ---- the tick (tickPower, every 420 ms). The buses fill in PowerSystem.Tick (owner 2026-10-26). --

    // Owner 2026-10-24: every rate runs in game time. The design's per-tick amounts were per game second
    // at x1 (a tick is `rate` game seconds), so they are now multiplied by the tick's game seconds; x1 is
    // unchanged.

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
