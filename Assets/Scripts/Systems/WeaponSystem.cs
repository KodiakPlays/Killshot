using UnityEngine;

/// <summary>
/// Weapons: one ship's fire control and what it has in flight. It holds the weapon groups (railgun, PDC,
/// missile), the mounts, warheads, the arming ladder, the firing solutions, the salvo, the PDC loaders and
/// AUTO groups, the capacitor and the fire lever. It fires slugs and missiles, flies them, and its PDCs
/// take their chances at enemy missiles coming in. Our ship and every enemy carry one (the enemy is our
/// ship with other stats). Owner 2026-10-26: weapons live in one place, here. The contact list, damage and
/// kills stay on the ship state (HandoffBoardState); this reads them. Rules from board-logic.js (MOUNTS,
/// SYSTEMS, WARHEADS, SWITCHES, computeSol, nudgeSol, flip, allSet, drawWep) and Steps 47–55.
/// </summary>
public class WeaponSystem : MonoBehaviour
{
    /// <summary>The ship this belongs to. Weapons read its contacts, its WEP bus, the periscope, where it is,
    /// and its damage model.</summary>
    [System.NonSerialized] public HandoffBoardState ship;

    /// <summary>This ship's numbers for weapons: its section of the ship's stats file (Assets/Ships,
    /// Starwolf → Tuning).</summary>
    [System.Serializable]
    public class Stats
    {
        [Header("Mounts: reach, arcs and ammunition at the start")]
        // Owner 2026-10-26: each weapon has its own reach. Railguns 25,000 km (a slug takes about 100 s; at that range even a
        // good aim can just miss a cruiser), missiles 35,000 km (slower, but they home), PDCs 10,000 km. Was 30,000 for railguns
        // and missiles (the DRADIS at full gain) and 4,000 for PDCs. You can dumb-fire that far; a lock still needs a scan.
        // Owner 2026-10-27: every mount starts full (the design had RG-B at 19 and PDC-B at 2180). The ammunition here is what
        // each mount has at the start; the running game counts down its own copy, never the file.
        public Mount[] mounts =
        {
            new Mount { id = "RGA", arc = 42, halfArc = 80, rangeKm = 25000, label = "RAILGUN TURRET A", shortName = "RG-A", ammo = 22 },
            new Mount { id = "RGB", arc = 250, halfArc = 80, rangeKm = 25000, label = "RAILGUN TURRET B", shortName = "RG-B", ammo = 22 },
            new Mount { id = "PDA", arc = 42, halfArc = 35, rangeKm = 10000, label = "PDC GROUP A", shortName = "PDC-A", ammo = 2400 },
            new Mount { id = "PDB", arc = 200, halfArc = 35, rangeKm = 10000, label = "PDC GROUP B", shortName = "PDC-B", ammo = 2400 },
            new Mount { id = "VLS", arc = 0, halfArc = 180, rangeKm = 35000, label = "MISSILE VLS", shortName = "VLS", ammo = 8 },
        };
        [Tooltip("A knob turned with no solution starts one here, on the periscope's bearing at speed 0 (owner 2026-10-23).")]
        public float manualStartRangeKm = 10000f;
        [Tooltip("Missiles launched together (owner 2026-10-23: set here until the design's salvo stepper has art).")]
        [Range(1, 8)] public int salvo = 2;
        [Tooltip("PDC autoloaders. Step 55: engaged at load (the design starts them off) and set here until the design's LOADERS pushbutton has art (HANDOFF_GAPS row 73); without them PDCs cannot arm.")]
        public bool loaders = true;
        public float wepFeedPerUnit = 5f, wepBleed = 3f;
        [Tooltip("Railgun slug speed, km/s (owner 2026-10-27: the fastest thing on the board, about 10 s across its 25,000 km range).")]
        public double slugSpeedKms = 2500;
        [Tooltip("Missile speed, km/s (owner 2026-10-23: slower than a slug, but it homes; Step 63: x5). About 2 minutes to 12,000 km.")]
        public double missileSpeedKms = 100;
        [Tooltip("How fast a missile can turn toward its target, degrees per mission second (Step 63: x5).")]
        public double missileTurnDegPerS = 6.5;
        [Tooltip("A salvo leaves the launcher fanned this many degrees apart.")]
        public double missileSpreadDeg = 3;
        [Tooltip("Rounds one PDC group spends on each go at an enemy missile (owner 2026-10-27).")]
        public int pdcRoundsPerGo = 100;
        [Tooltip("A shot from this ship, slug or missile, hits a ship it passes within this distance of, km (owner 2026-10-27: 100 km, any ship).")]
        public float hitRadiusKm = 100f;
        [Tooltip("Armour one of this ship's slugs takes off the side it hits.")]
        public float slugArmourDamage = 50f;
        [Tooltip("Integrity one of this ship's slugs, or a warhead through an open side, takes off the room behind it (owner: 2 of 5).")]
        public int slugCompartmentDamage = 2;
        [Tooltip("Armour one of this ship's HE warheads takes off the side it hits (owner: it opens a side fast).")]
        public float heArmourDamage = 100f;
        [Tooltip("One of this ship's EMP warheads knocks the target's power out for this many mission seconds.")]
        public float empSeconds = 60f;
        [Tooltip("Chance this ship's hit through open armour starts a fire: an HE warhead, a penetrator.")]
        [Range(0f, 1f)] public float fireChanceHE = 0.6f, fireChancePEN = 0.4f;
        [Tooltip("Chance this ship's slug through open armour breaches the room (it loses its air).")]
        [Range(0f, 1f)] public float breachChanceSlug = 0.5f;
        [Tooltip("Firing the railgun spikes our signal by this much (GDD p.42); it falls back at the usual rate.")]
        public float fireSignalSpike = 25f;
        [Tooltip("The SPD knob's step, km/s (the design's 0.05, x5 with the speeds in Step 63).")]
        public float spdStep = 0.25f;
        [Tooltip("The chance a PDC group on AUTO shoots down an enemy missile that enters its arc (owner: a chance per missile).")]
        [Range(0f, 1f)] public float pdcKillChance = 0.7f;
    }
    /// <summary>The numbers this part plays by: its section of the ship's stats file (a copy while the game runs).</summary>
    public Stats stats => shipStats ? shipStats.Stats.weapons : (shipStats = GetComponent<Ship>()).Stats.weapons;
    [System.NonSerialized] Ship shipStats;
    /// <summary>The salvo now: it starts from the stats file (Ship), then changes as the ship plays.</summary>
    [System.NonSerialized] public int salvo = 2;
    /// <summary>The loaders now: it starts from the stats file (Ship), then changes as the ship plays.</summary>
    [System.NonSerialized] public bool loaders = true;

    [System.Serializable]
    public class Mount
    {
        public string id, label, shortName; public int arc, halfArc; public float rangeKm;
        [Tooltip("What it has left: slugs, PDC rounds or missile cells. Owner 2026-10-27: a real count; it only goes down.")]
        public int ammo;
    }

    [System.Serializable]
    public class Group { public string id, label; public string[] mounts; public float shot; }

    [System.Serializable]
    public class Solution
    {
        public bool set; public float rangeKm; public int brg; public float spd;
        [Tooltip("Step 47: dialled by hand from the plot, not by TRACK. It aims at a point in space, not a locked contact.")]
        public bool manual;
        [Tooltip("Step 52: the target's course is known (from a plotted track, or TRACK's lock), so the shot is led.")]
        public bool hasCourse;
        public float course;
        [Tooltip("Where the target was (world km) and when (mission seconds) as the solution was set; the fire computer reckons it forward.")]
        public double wx, wy;
        public float t;
    }

    [Header("Fire control — MOUNTS, SYSTEMS, WARHEADS, SWITCHES and the board's initial state")]
    public Group[] systems =
    {
        new Group { id = "RG", label = "RAILGUN", mounts = new[] { "RGA", "RGB" }, shot = 70 },
        new Group { id = "PD", label = "PDC", mounts = new[] { "PDA", "PDB" }, shot = 25 },
        new Group { id = "MS", label = "MISSILE", mounts = new[] { "VLS" }, shot = 40 },
    };
    public string[] warheads = { "HE", "EMP", "PENETRATOR" };
    public string[] switches = { "load", "angle", "bearing", "arm" };
    public string sys = "RG";
    public string mount = "RGA";
    public string target = "t07";
    [Tooltip("The arming ladder: LOAD, ANGLE, BEARING, ARM (sw).")]
    public bool[] sw = new bool[4];
    [Tooltip("A firing solution per system, in SYSTEMS order (sol).")]
    public Solution[] sol = { new Solution(), new Solution(), new Solution() };
    public int warhead;
    [Tooltip("PDC quadrants FWD, STBD, AFT, PORT (quads).")]
    public bool[] quads = { true, true, false, false };
    [Tooltip("Step 55: PDC groups on AUTO (PDC-A, PDC-B). Arming the ladder with a group selected puts it on AUTO; it then shoots by itself at enemy missiles in its arc.")]
    public bool[] pdcAuto = new bool[2];
    public bool fired;
    [Range(0f, 100f)] public float wepCharge;
    [Tooltip("The fire lever, 0..1; fires once past leverFireAt with the ladder set.")]
    [Range(0f, 1f)] public float fireLever;

    // ---- rounds in flight (Step 48, owner 2026-10-23) ------------------------------------------

    [System.Serializable]
    public class Round
    {
        public double x, y, dx, dy, travelled, maxKm;
        public double speed = 50;
        [Tooltip("Step 53: a missile homes on its target and carries a warhead (an index into warheads).")]
        public bool missile;
        public string targetKey;
        public int warhead;
        [Tooltip("Step 55: PDC groups that have had their one chance at this missile (bit 0 = PDC-A, bit 1 = PDC-B).")]
        public int pdcTried;
        [System.NonSerialized] public Stats by; // the firing ship's numbers: how close is a hit, and what the hit does
    }

    [System.NonSerialized] private System.Collections.Generic.List<Round> rounds;
    public System.Collections.Generic.List<Round> Rounds => rounds ?? (rounds = new System.Collections.Generic.List<Round>());
    /// <summary>VLS cells left: the launcher's count.</summary>
    public int vlsCells { get { return MountOf("VLS").ammo; } set { MountOf("VLS").ammo = value; } }
    /// <summary>The selected mount can't fire again: no slug or cell left, or too few rounds for a PDC go.</summary>
    public bool Empty { get { var m = MountOf(mount); return m == null || m.ammo < (sys == "PD" ? stats.pdcRoundsPerGo : 1); } }
    /// <summary>The last shot's outcome: "HIT · GARUDA · STBD", "MISS", or null while none has landed.</summary>
    [System.NonSerialized] public string shotReport;
    [System.NonSerialized] public bool lastShotHit;
    [System.NonSerialized] public int shotsFired, shotResults;

    public Group Sys { get { foreach (var y in systems) if (y.id == sys) return y; return systems[0]; } }
    public int SysIndex { get { for (int i = 0; i < systems.Length; i++) if (systems[i].id == sys) return i; return 0; } }
    public Mount MountOf(string id) { foreach (var m in stats.mounts) if (m.id == id) return m; return null; }

    public Solution Sol => sol[SysIndex];
    public float ShotCost => Sys != null ? Sys.shot : 50f;
    public bool WepReady => ship.power.BusUnits("wep") >= 2 && ship.damage.MagOk && ship.orders.WeaponsReleased;
    public string TrackIdFor()
    {
        var t = ship.TrackOf(target);
        if (t == null || !ship.sensors.identified.Contains(t.key)) return null;
        return "TK-" + t.bearing.ToString("000") + "-" + t.key.ToUpperInvariant();
    }
    /// <summary>offBearing: target bearing minus periscope bearing, wrapped to ±180.</summary>
    public double? OffBearing() { var t = ship.TrackOf(target); if (t == null) return null; return ((t.bearing - ship.scopeBrg + 540) % 360) - 180; }
    public bool OnBearing
    {
        get
        {
            // a hand-dialled solution aims at a point: BEARING wants the periscope on the dialled bearing
            if (Sol.set && Sol.manual) return HandoffBoardState.AngDiffD(ship.nav.heading + Sol.brg, ship.scopeBrg) <= ship.onBearingDeg;
            var o = OffBearing(); return o.HasValue && System.Math.Abs(o.Value) <= ship.onBearingDeg;
        }
    }

    public bool AllSet
    {
        get
        {
            if (!Sol.set || (!Sol.manual && ship.TrackOf(target) == null)) return false; // a hand-dialled shot needs no locked contact
            if (sys == "PD" && !loaders) return false;
            foreach (var b in sw) if (!b) return false;
            return true;
        }
    }

    // Step 55: a PDC group on AUTO keeps its ladder armed whenever it is the one selected
    void ClearSwitches() { bool auto = PdcGroup >= 0 && pdcAuto[PdcGroup]; for (int i = 0; i < sw.Length; i++) sw[i] = auto; }
    /// <summary>The selected PDC group (0 = PDC-A, 1 = PDC-B), or -1 when PDC is not selected.</summary>
    int PdcGroup => sys == "PD" ? System.Array.IndexOf(Sys.mounts, mount) : -1;

    /// <summary>pickSystem via cycleSystem: next system; keep the mount if it belongs, else its first.</summary>
    public void CycleSystem()
    {
        var next = systems[(SysIndex + 1) % systems.Length];
        sys = next.id;
        if (System.Array.IndexOf(next.mounts, mount) < 0) mount = next.mounts[0];
        ClearSwitches(); fired = false;
    }

    public void CycleMount()
    {
        var y = Sys;
        if (y.mounts.Length < 2) return;
        int i = System.Array.IndexOf(y.mounts, mount);
        mount = y.mounts[(i + 1) % y.mounts.Length];
        ClearSwitches(); fired = false;
    }

    public void CycleWarhead() { warhead = (warhead + 1) % warheads.Length; ClearSwitches(); }
    public void StepSalvo(int d) { salvo = Mathf.Max(1, Mathf.Min(8, salvo + d)); }
    public void ToggleLoaders() { loaders = !loaders; if (!loaders) pdcAuto[0] = pdcAuto[1] = false; ClearSwitches(); }
    public void ToggleQuad(int i) { quads[i] = !quads[i]; }

    /// <summary>computeSol: needs an identified target and the bridge; stores range, relative
    /// bearing and speed for the current system and clears the ladder.</summary>
    public void ComputeSol()
    {
        var t = ship.TrackOf(target);
        if (t == null || !ship.sensors.identified.Contains(t.key) || !ship.damage.BriOk) return;
        int rel = ((t.bearing - ship.nav.heading + 540) % 360) - 180;
        // Step 52: an active lock knows its true course and speed, and where it is now, so TRACK's shot is led
        sol[SysIndex] = new Solution { set = true, rangeKm = t.rangeKm, brg = rel, spd = t.speed,
            hasCourse = t.speed > 0.01f, course = (float)t.course, wx = t.worldX, wy = t.worldY, t = ship.missionNow };
        ClearSwitches(); fired = false;
    }

    /// <summary>nudgeSol: RNG ±100 km, BRG ±1° (±180), SPD ±stats.spdStep (2 dp; the design had 0.05, x5 in Step 63); clears the ladder.</summary>
    public void NudgeSol(string field, int d)
    {
        var cur = Sol;
        if (!cur.set)
        {
            // Step 47 (owner 2026-10-23): a knob starts a hand-dialled solution, from the plot
            cur.set = true; cur.manual = true;
            cur.rangeKm = stats.manualStartRangeKm;
            cur.brg = (HandoffBoardState.JsRound(ship.scopeBrg) - ship.nav.heading + 540) % 360 - 180; // relative, as TRACK stores it
            cur.spd = 0f;
            ApplyTrack(cur);
        }
        if (field == "rng") cur.rangeKm = Mathf.Max(0f, cur.rangeKm + d * 100f);
        if (field == "brg") cur.brg = Mathf.Max(-180, Mathf.Min(180, cur.brg + d));
        if (field == "spd") cur.spd = Mathf.Max(0f, (float)System.Math.Round(cur.spd + d * stats.spdStep, 2, System.MidpointRounding.AwayFromZero));
        if (cur.manual) Fix(cur); // the dialled point is where the target is now
        ClearSwitches();
    }

    /// <summary>flip: releasing a switch releases it and every one after (and the lever);
    /// throwing needs the one before, and LOAD/BEARING/ARM their gates.</summary>
    public void Flip(int i)
    {
        if (sw[i])
        {
            for (int j = i; j < sw.Length; j++) sw[j] = false;
            fired = false; fireLever = 0f;
            if (PdcGroup >= 0) pdcAuto[PdcGroup] = false; // Step 55: dropping the ladder takes the group off AUTO
            return;
        }
        if (i > 0 && !sw[i - 1]) return;
        if (RungGated(i)) return;
        sw[i] = true; fired = false;
        if (i == 3 && PdcGroup >= 0) pdcAuto[PdcGroup] = true; // Step 55: ARM puts the PDC group on AUTO
    }

    /// <summary>A rung's gate: LOAD needs the WEP bus, ammunition and a solution, ANGLE the turret to bear (Step 48),
    /// BEARING the scope on axis, ARM the charge. Step 55: a PDC group needs no solution or bearing, only its
    /// autoloaders: on AUTO it fires by itself at what enters its arc.</summary>
    public bool RungGated(int i)
    {
        bool pd = sys == "PD";
        return (i == 0 && (!WepReady || Empty || (pd ? !loaders : !Sol.set)))
            || (i == 1 && !pd && !InArc) || (i == 2 && !pd && !OnBearing) || (i == 3 && wepCharge < ShotCost);
    }

    /// <summary>The lever passing 0.9 with the ladder set: rounds away, charge spent (drawWep).
    /// Step 48: refused below the shot's cost; firing releases ARM, one shot per arming (spec 8.4.b);
    /// a railgun shot is a slug that flies (TickRounds); firing spikes our signal (GDD p.42).</summary>
    public void Fire()
    {
        if (sys == "PD") return; // Step 55: PDCs on AUTO fire by themselves; the lever does nothing
        if (!AllSet) return; // moveSlew fires on every new pull while allSet() holds; the panel's per-pull latch stops repeats (Step 33g verification)
        if (wepCharge < ShotCost || Empty) return;
        if (sys == "MS" && !MissileReady) return;
        fired = true;
        wepCharge = Mathf.Max(0f, wepCharge - ShotCost);
        sw[3] = false;
        shotsFired++;
        var m = MountOf(mount);
        if (sys == "RG" && m != null)
        {
            double b = LeadBearing() * System.Math.PI / 180;
            Rounds.Add(new Round { x = ship.nav.x, y = ship.nav.y, dx = System.Math.Sin(b), dy = System.Math.Cos(b), maxKm = m.rangeKm, speed = stats.slugSpeedKms });
            m.ammo--;
            shotReport = null;
            ship.sensors.signal = Mathf.Min(100f, ship.sensors.signal + stats.fireSignalSpike);
        }
        if (sys == "MS" && m != null)
        {
            // a salvo, spread a little either side of the target's bearing; each missile then homes on it
            var tgt = ship.TrackOf(target);
            int n = Mathf.Min(salvo, vlsCells);
            for (int i = 0; i < n; i++)
            {
                double b = (tgt.bearing + (i - (n - 1) / 2.0) * stats.missileSpreadDeg) * System.Math.PI / 180;
                Rounds.Add(new Round { x = ship.nav.x, y = ship.nav.y, dx = System.Math.Sin(b), dy = System.Math.Cos(b), maxKm = m.rangeKm,
                                       speed = stats.missileSpeedKms, missile = true, targetKey = tgt.key, warhead = warhead });
            }
            vlsCells -= n;
            shotReport = null;
            ship.sensors.signal = Mathf.Min(100f, ship.sensors.signal + stats.fireSignalSpike);
        }
        if (Empty) ClearSwitches(); // the last one gone: nothing left to load
    }

    /// <summary>Step 53 (GDD p.45): missiles need a locked target — identified by an active scan — and a cell left.</summary>
    public bool MissileReady { get { var t = ship.TrackOf(target); return t != null && !t.wreck && ship.sensors.identified.Contains(t.key) && vlsCells > 0; } }

    // ---- leading a moving target (Step 52, owner 2026-10-23) ---------------------------------

    /// <summary>The latest plotted track (a RULER from one POINT to another) gives the solution its course,
    /// and its speed into SPD.</summary>
    public void ApplyTrack(Solution cur)
    {
        HandoffBoardState.Mark latest = null;
        HandoffBoardState.Mark f, g; double crs, spd;
        foreach (var m in ship.marks) if (m.kind == "ruler" && (latest == null || m.id > latest.id) && ship.TrackLeg(m, out f, out g, out crs, out spd)) latest = m;
        if (latest == null) return;
        ship.TrackLeg(latest, out f, out g, out crs, out spd);
        cur.hasCourse = true;
        cur.course = (float)crs;
        cur.spd = (float)System.Math.Round(spd, 2, System.MidpointRounding.AwayFromZero);
    }

    // the dialled range and bearing as a world point, now
    void Fix(Solution cur)
    {
        double b = (ship.nav.heading + cur.brg) * System.Math.PI / 180;
        cur.wx = ship.nav.x + System.Math.Sin(b) * cur.rangeKm;
        cur.wy = ship.nav.y + System.Math.Cos(b) * cur.rangeKm;
        cur.t = ship.missionNow;
    }

    /// <summary>Where to point the slug. With a course, reckon the target forward from where the solution
    /// put it to now, then aim where it will be when the slug arrives. Otherwise straight down the dialled
    /// bearing, as before.</summary>
    public double LeadBearing()
    {
        var cur = Sol;
        double straight = ship.nav.heading + cur.brg;
        if (!cur.hasCourse || cur.spd <= 0.001f) return straight;
        double c = cur.course * System.Math.PI / 180, vx = cur.spd * System.Math.Sin(c), vy = cur.spd * System.Math.Cos(c);
        double age = System.Math.Max(0, ship.missionNow - cur.t);
        double tx = cur.wx + vx * age - ship.nav.x, ty = cur.wy + vy * age - ship.nav.y;
        double a = vx * vx + vy * vy - stats.slugSpeedKms * stats.slugSpeedKms, bq = 2 * (tx * vx + ty * vy), cq = tx * tx + ty * ty;
        double disc = bq * bq - 4 * a * cq;
        if (a >= 0 || disc < 0) return straight; // can't catch it
        double tHit = (-bq - System.Math.Sqrt(disc)) / (2 * a);
        if (tHit <= 0) return straight;
        return (System.Math.Atan2(tx + vx * tHit, ty + vy * tHit) * 180 / System.Math.PI + 360) % 360;
    }

    /// <summary>Hull-fixed arcs (spec 8.7.a): the solution's bearing from our bow must lie in the mount's arc.</summary>
    public bool InArc
    {
        get
        {
            var m = MountOf(mount);
            if (m == null || !Sol.set) return false;
            return HandoffBoardState.AngDiff((Sol.brg % 360 + 360) % 360, m.arc) <= m.halfArc;
        }
    }

    /// <summary>Fly every round dt mission seconds. Its whole path this tick is tested, so a fast slug
    /// cannot skip past a ship: the first ship it passes within that ship's hit radius is hit. Step 63: the
    /// test follows the ship's own motion through the tick too (at 5x speeds and x10 a ship moves further
    /// in a tick than its hit radius).</summary>
    public void TickRounds(double dt)
    {
        var list = Rounds;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var r = list[i];
            if (r.missile) Steer(r, dt);
            double step = System.Math.Min(r.speed * dt, r.maxKm - r.travelled);
            double flight = r.speed > 0 ? step / r.speed : 0; // seconds of this tick the round is flying
            HandoffBoardState.Track struck = null;
            double first = double.MaxValue;
            foreach (var t in ship.tracks)
            {
                if (t.own || !t.hasWorld || t.wreck) continue;
                // the ship relative to the round, and how that changes: its velocity less the round's
                double c = t.course * System.Math.PI / 180;
                double wx = t.speed * System.Math.Sin(c) - r.dx * r.speed, wy = t.speed * System.Math.Cos(c) - r.dy * r.speed;
                double px = t.worldX - r.x, py = t.worldY - r.y, ww = wx * wx + wy * wy;
                double s = ww > 0 ? System.Math.Max(0, System.Math.Min(flight, -(px * wx + py * wy) / ww)) : 0; // closest moment
                double cx = px + wx * s, cy = py + wy * s, rad = stats.hitRadiusKm;
                if (cx * cx + cy * cy <= rad * rad && s < first) { struck = t; first = s; }
            }
            if (struck != null)
            {
                // Step 49: the struck ship takes the hit on its own model (its side, then what is behind it)
                string side = SideStruck(struck, r);
                var victim = ship.ShipOf != null ? ship.ShipOf(struck.key) : null;
                string damage = victim == null ? "" : r.missile ? victim.damage.TakeWarhead(side, warheads[r.warhead], stats) : victim.damage.TakeSlug(side, stats);
                if (victim != null && victim.destroyed) ship.Kill(struck);
                else { shotReport = "HIT · " + ship.NameOf(struck) + (ship.Scanned(struck) ? " · " + side + damage : ""); lastShotHit = true; shotResults++; } // Step 61: detail once scanned
                list.RemoveAt(i);
                continue;
            }
            r.x += r.dx * step; r.y += r.dy * step; r.travelled += step;
            if (r.travelled >= r.maxKm - 1e-6) { shotReport = "MISS"; lastShotHit = false; shotResults++; list.RemoveAt(i); }
        }
    }

    // A missile turns toward its target, no faster than stats.missileTurnDegPerS. With no target left it flies on.
    void Steer(Round r, double dt)
    {
        var t = ship.TrackOf(r.targetKey);
        if (t == null || !t.hasWorld || t.wreck) return;
        SteerAt(r, t.worldX, t.worldY, dt);
    }

    void SteerAt(Round r, double x, double y, double dt)
    {
        double want = System.Math.Atan2(x - r.x, y - r.y) * 180 / System.Math.PI;
        double now = System.Math.Atan2(r.dx, r.dy) * 180 / System.Math.PI;
        double turn = ((want - now) % 360 + 540) % 360 - 180, most = stats.missileTurnDegPerS * dt;
        double head = (now + System.Math.Max(-most, System.Math.Min(most, turn))) * System.Math.PI / 180;
        r.dx = System.Math.Sin(head); r.dy = System.Math.Cos(head);
    }

    // The side facing the shot (reference sim sideFacing): where it came from, against the ship's bow.
    static string SideStruck(HandoffBoardState.Track t, Round r) => HandoffBoardState.SideAt(System.Math.Atan2(-r.dx, -r.dy) * 180 / System.Math.PI, t.worldHdg);

    // ---- enemy missiles and our point defence (Step 55, owner 2026-10-24) -----------------------

    [System.NonSerialized] private System.Collections.Generic.List<Round> incoming;
    /// <summary>Enemy missiles flying at our ship.</summary>
    public System.Collections.Generic.List<Round> Incoming => incoming ?? (incoming = new System.Collections.Generic.List<Round>());
    /// <summary>The PDCs' last result ("PDC-A · MISSILE DOWN"), and the last enemy missile to hit us.</summary>
    [System.NonSerialized] public string pdcReport, incomingReport;
    [System.NonSerialized] public int pdcKills, hitsTaken;

    /// <summary>An enemy at (x, y) launches n missiles at us, fanned, each homing on our ship. by: the
    /// enemy's numbers, which its missiles fly and hit by.</summary>
    public void LaunchAt(Stats by, double x, double y, int n, int warheadIndex, double maxKm)
    {
        double brg = System.Math.Atan2(ship.nav.x - x, ship.nav.y - y) * 180 / System.Math.PI;
        for (int i = 0; i < n; i++)
        {
            double b = (brg + (i - (n - 1) / 2.0) * by.missileSpreadDeg) * System.Math.PI / 180;
            Incoming.Add(new Round { x = x, y = y, dx = System.Math.Sin(b), dy = System.Math.Cos(b), maxKm = maxKm,
                                     speed = by.missileSpeedKms, missile = true, warhead = warheadIndex, by = by });
        }
    }

    /// <summary>Each tick: our PDCs take their chances, then every enemy missile homes on us; one that
    /// passes within its firing ship's hitRadiusKm strikes the side facing it.</summary>
    public void TickIncoming(double dt)
    {
        var list = Incoming;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var r = list[i];
            if (PdcStops(r)) { list.RemoveAt(i); continue; }
            SteerAt(r, ship.nav.x, ship.nav.y, dt);
            double step = System.Math.Min(r.speed * dt, r.maxKm - r.travelled);
            double px = ship.nav.x - r.x, py = ship.nav.y - r.y;
            double u = System.Math.Max(0, System.Math.Min(step, px * r.dx + py * r.dy)); // closest point along the path
            double cx = px - r.dx * u, cy = py - r.dy * u;
            if (cx * cx + cy * cy <= r.by.hitRadiusKm * r.by.hitRadiusKm)
            {
                string side = HandoffBoardState.SideAt(System.Math.Atan2(-r.dx, -r.dy) * 180 / System.Math.PI, ship.nav.heading);
                incomingReport = "MISSILE HIT · " + side + ship.damage.TakeWarhead(side, warheads[r.warhead], r.by);
                hitsTaken++;
                list.RemoveAt(i);
                continue;
            }
            r.x += r.dx * step; r.y += r.dy * step; r.travelled += step;
            if (r.travelled >= r.maxKm - 1e-6) list.RemoveAt(i);
        }
    }

    // A PDC group on AUTO, loaded and powered, gets one chance at each enemy missile inside its arc and range.
    // Each go spends stats.pdcRoundsPerGo; a group too low for another go drops off AUTO (owner 2026-10-27).
    bool PdcStops(Round r)
    {
        if (!loaders || !WepReady) return false;
        double dx = r.x - ship.nav.x, dy = r.y - ship.nav.y;
        int rel = HandoffBoardState.JsRound(((System.Math.Atan2(dx, dy) * 180 / System.Math.PI - ship.nav.heading) % 360 + 360) % 360) % 360;
        var groups = System.Array.Find(systems, y => y.id == "PD").mounts;
        for (int g = 0; g < groups.Length && g < pdcAuto.Length; g++)
        {
            var m = MountOf(groups[g]);
            if (!pdcAuto[g] || m == null || (r.pdcTried & (1 << g)) != 0) continue;
            if (dx * dx + dy * dy > (double)m.rangeKm * m.rangeKm || HandoffBoardState.AngDiff(rel, m.arc) > m.halfArc) continue;
            r.pdcTried |= 1 << g;
            m.ammo -= stats.pdcRoundsPerGo;
            if (m.ammo < stats.pdcRoundsPerGo) { pdcAuto[g] = false; if (g == PdcGroup) ClearSwitches(); }
            bool down = ship.NextDie() < stats.pdcKillChance;
            pdcReport = m.shortName + (down ? " · MISSILE DOWN" : " · MISSED");
            if (down) { pdcKills++; return true; }
        }
        return false;
    }

    // ---- the tick — weapons capacitor: +5 per WEP unit per game second up to 100, bleeds 3 with no WEP --

    public void TickCharge(double dt)
    {
        float feed = ship.power.BusUnits("wep") * stats.wepFeedPerUnit * (float)dt;
        wepCharge = feed > 0f ? Mathf.Min(100f, wepCharge + feed) : Mathf.Max(0f, wepCharge - stats.wepBleed * (float)dt);
    }

    /// <summary>validSet: hostile contacts inside the current mount's range and arc.</summary>
    public bool ValidTarget(string key)
    {
        var m = MountOf(mount);
        var t = ship.TrackOf(key);
        return m != null && t != null && t.hostile && t.rangeKm <= m.rangeKm && HandoffBoardState.AngDiff(t.bearing, m.arc) <= m.halfArc;
    }
    /// <summary>pickTarget: a valid contact becomes the target; the ladder, lever and fired reset.</summary>
    public void PickTarget(string key)
    {
        if (!ValidTarget(key)) return;
        target = key;
        fired = false;
        for (int i = 0; i < sw.Length; i++) sw[i] = false;
        fireLever = 0f;
    }
}
