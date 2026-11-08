using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Weapons: one ship's fire control and what it has in flight. It holds the weapon groups (railgun, PDC,
/// torpedo), the mounts, warheads, the arming ladder, the firing solutions, the salvo, the PDC loaders and
/// AUTO groups, the capacitor and the fire lever. It fires slugs and torpedoes, flies them, and its PDCs
/// take their chances at enemy torpedoes coming in. Our ship and every enemy carry one (the enemy is our
/// ship with other stats). Owner 2026-10-26: weapons live in one place, here. The contact list, damage and
/// kills stay on the ship state (ShipState); this reads them. Rules from board-logic.js (MOUNTS,
/// SYSTEMS, WARHEADS, SWITCHES, computeSol, nudgeSol, flip, allSet, drawWep) and Steps 47–55.
/// </summary>
public class WeaponSystem : MonoBehaviour
{
    /// <summary>The ship this belongs to. Weapons read its contacts, its WEP bus, the periscope, where it is,
    /// and its damage model.</summary>
    [System.NonSerialized] public ShipState ship;

    /// <summary>This ship's numbers for weapons: its section of the ship's stats file (Assets/Ships,
    /// Starwolf → Tuning).</summary>
    [System.Serializable]
    public class Stats
    {
        [Header("Mounts: reach, arcs and ammunition at the start")]
        // Owner 2026-10-26: each weapon has its own reach. Railguns 25,000 units (a slug takes about 100 s; at that range even a
        // good aim can just miss a cruiser), torpedoes 35,000 units (slower, but they home), PDCs 10,000 units. Was 30,000 for railguns
        // and torpedoes (the DRADIS at full gain) and 4,000 for PDCs. You can dumb-fire that far; a lock still needs a scan.
        // Owner 2026-10-27: every mount starts full (the design had RG-B at 19 and PDC-B at 2180). The ammunition here is what
        // each mount has at the start; the running game counts down its own copy, never the file.
        public Mount[] mounts =
        {
            // owner 2026-11-01: a 90° firing arc straight out of each side (was 160°, skewed forward)
            new Mount { id = "RGA", kind = "RG", arc = 90, halfArc = 45, rangeUnits = 25000, label = "RAILGUN TURRET A", shortName = "RG-A", ammo = 22 },
            new Mount { id = "RGB", kind = "RG", arc = 270, halfArc = 45, rangeUnits = 25000, label = "RAILGUN TURRET B", shortName = "RG-B", ammo = 22 },
            // owner 2026-10-29: two halves, "keep it simple": PDC-A the starboard side (000–180), PDC-B the port side (180–360)
            new Mount { id = "PDA", kind = "PD", arc = 90, halfArc = 90, rangeUnits = 10000, label = "PDC GROUP A", shortName = "PDC-A", ammo = 2400 },
            new Mount { id = "PDB", kind = "PD", arc = 270, halfArc = 90, rangeUnits = 10000, label = "PDC GROUP B", shortName = "PDC-B", ammo = 2400 },
            new Mount { id = "VLS", kind = "MS", arc = 0, halfArc = 180, rangeUnits = 35000, label = "TORPEDO VLS", shortName = "VLS", ammo = 8 },
        };
        [Tooltip("Owner 2026-11-01: the places on the hull a weapon can stand. The Hard Points tab of Starwolf → Tuning drags them round the ship. A weapon fires from its dot; with no dot it can't fire.")]
        public HardPoint[] hardPoints =
        {
            new HardPoint { at = new Vector2(0f, 50f) },
            new HardPoint { at = new Vector2(32f, 0f), mount = "RGA" },
            new HardPoint { at = new Vector2(-32f, 0f), mount = "RGB" },
            new HardPoint { at = new Vector2(32f, -25f), mount = "PDA" },
            new HardPoint { at = new Vector2(-32f, -25f), mount = "PDB" },
            new HardPoint { at = new Vector2(0f, -50f), mount = "VLS" },
        };
        [Tooltip("How fast a railgun's bar turns inside its cone, degrees per game second (owner 2026-11-01: slowly, as the lock builds).")]
        public float gunTurnDegPerSecond = 3f;
        [Tooltip("Game seconds the targeting computer takes to lock on a ship kept in its sights at the right zoom (owner 2026-11-01).")]
        public float lockSeconds = 10f;
        [Tooltip("Game seconds a part-filled lock bar takes to fall back to nothing once the ship leaves the sights.")]
        public float lockFallSeconds = 5f;
        [Tooltip("Game seconds a lock stays on once the bar is full. Then it drops, with its firing solution and the switches, and the bar starts again (owner 2026-11-01: locks are short).")]
        public float lockHoldSeconds = 10f;
        [Tooltip("A knob turned with no solution starts one here, on the periscope's bearing at speed 0 (owner 2026-10-23).")]
        [FormerlySerializedAs("manualStartRangeKm")] public float manualStartRangeUnits = 10000f;
        [Tooltip("Torpedoes launched together (owner 2026-10-23: set here until the design's salvo stepper has art).")]
        [Range(1, 8)] public int salvo = 2;
        [Tooltip("PDC autoloaders. Step 55: engaged at load (the design starts them off) and set here until the design's LOADERS pushbutton has art (HANDOFF_GAPS row 73); without them PDCs cannot arm.")]
        public bool loaders = true;
        public float wepFeedPerUnit = 5f, wepBleed = 3f;
        [Tooltip("Railgun slug speed, units/s (owner 2026-10-27: the fastest thing on the board, about 10 s across its 25,000 units range).")]
        [FormerlySerializedAs("slugSpeedKms")] public double slugSpeedUnits = 2500;
        [Tooltip("A torpedo's speed with its motor on, units/s (owner 2026-11-01: always slower than a slug; a torpedo reaches further and hits harder).")]
        public double torpedoSpeedUnits = 100;
        [Tooltip("Owner 2026-11-01: a torpedo leaves its launcher straight out of the ship's side at this speed, units/s.")]
        public double torpedoLaunchSpeedUnits = 80;
        [Tooltip("Game seconds it flies straight out, slowing.")]
        public double torpedoCoastSeconds = 1.5;
        [Tooltip("Its speed at the end of that, units/s. It stays at this speed as it curves onto the target.")]
        public double torpedoCoastSpeedUnits = 30;
        [Tooltip("Owner 2026-11-02: it leaves the ship's side tilted this many degrees toward the stern.")]
        public double torpedoTiltBackDeg = 15;
        [Tooltip("The sharpest it curves onto the target before its motor lights, degrees per game second.")]
        public double torpedoCurveDegPerS = 60;
        [Tooltip("Owner 2026-11-02: game seconds a turn takes to build up to its sharpest, and to ease off as it lines up.")]
        public double torpedoTurnBuildSeconds = 0.5;
        [Tooltip("How fast its motor takes it up to full speed, units/s gained each game second.")]
        public double torpedoBurnUnits = 300;
        [Tooltip("How fast a torpedo can turn toward its target with its motor on, degrees per mission second (Step 63: x5).")]
        public double torpedoTurnDegPerS = 6.5;
        [Tooltip("A salvo leaves the launcher fanned this many degrees apart.")]
        public double torpedoSpreadDeg = 3;
        [Tooltip("Rounds one PDC group spends on each go at an enemy torpedo (owner 2026-10-27).")]
        public int pdcRoundsPerGo = 100;
        [Tooltip("A shot from this ship, slug or torpedo, hits a ship if it touches the ship's box made this much bigger, percent (owner 2026-11-01; was a 100-unit circle round the ship's centre).")]
        public float hitMarginPercent = 25f;
        /// <summary>The half size of the box a shot from this ship must touch, from a ship's own box half.</summary>
        public Vector2 HitHalf(Vector2 half) => half * (1f + hitMarginPercent / 100f);
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
        [Tooltip("The SPD knob's step, units/s (the design's 0.05, x5 with the speeds in Step 63).")]
        public float spdStep = 0.25f;
        [Tooltip("The chance a PDC group on AUTO shoots down an enemy torpedo that enters its arc (owner: a chance per torpedo).")]
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
        public string id, label, shortName;
        [Tooltip("RG railgun, PD point defence or MS torpedo launcher (owner 2026-11-01: a ship has as many of each as its stats file lists).")]
        public string kind;
        public int arc, halfArc; [FormerlySerializedAs("rangeKm")] public float rangeUnits;
        [Tooltip("What it has left: slugs, PDC rounds or torpedo cells. Owner 2026-10-27: a real count; it only goes down.")]
        public int ammo;
        /// <summary>A railgun's bar: its angle from the cone's middle, degrees, now and at the last tick.</summary>
        [System.NonSerialized] public float aimDeg, aimWas;
    }

    /// <summary>A place on the hull. Owner 2026-11-01: the ship is a rectangle, short end forward.</summary>
    [System.Serializable]
    public class HardPoint
    {
        [Tooltip("Where it is, units from the ship's centre: x right, y forward.")]
        public Vector2 at;
        [Tooltip("The gun on it, by id. Empty: nothing.")]
        public string mount;
        [Tooltip("Owner 2026-11-02: the line drawn out of it in Starwolf → Tuning → Hard Points, units from the dot (x right, y forward). "
               + "A torpedo or fighter leaving from it flies along the line, then turns onto its target. Empty: it flies straight out.")]
        public Vector2[] path = new Vector2[0];
    }

    /// <summary>Owner 2026-11-02: a dot's drawn line laid in the world from (x, y), on a ship with this heading; mirror: the
    /// other side's copy. Null with no line drawn.</summary>
    public static Vector2[] Laid(Vector2[] path, double x, double y, double heading, bool mirror = false)
    {
        if (path == null || path.Length == 0) return null;
        float rad = (float)heading * Mathf.Deg2Rad, sin = Mathf.Sin(rad), cos = Mathf.Cos(rad);
        var line = new Vector2[path.Length + 1];
        line[0] = new Vector2((float)x, (float)y);
        for (int i = 0; i < path.Length; i++)
        {
            float px = mirror ? -path[i].x : path[i].x, py = path[i].y;
            line[i + 1] = line[0] + new Vector2(px * cos + py * sin, py * cos - px * sin);
        }
        return line;
    }

    /// <summary>The spot `along` units down a laid line, and the way the line points there, degrees. False past its end,
    /// with the end and its last direction.</summary>
    public static bool Along(Vector2[] line, double along, out Vector2 at, out double heading)
    {
        heading = 0;
        for (int i = 1; i < line.Length; i++)
        {
            var piece = line[i] - line[i - 1];
            float length = piece.magnitude;
            if (length <= 0f) continue;
            heading = (Mathf.Atan2(piece.x, piece.y) * Mathf.Rad2Deg + 360f) % 360f;
            if (along <= length) { at = line[i - 1] + piece * (float)(along / length); return true; }
            along -= length;
        }
        at = line[line.Length - 1];
        return false;
    }

    /// <summary>A line's whole length, units.</summary>
    public static double LengthOf(Vector2[] line)
    {
        double length = 0;
        for (int i = 1; i < line.Length; i++) length += (line[i] - line[i - 1]).magnitude;
        return length;
    }

    /// <summary>The line drawn out of a mount's hard point. None: null.</summary>
    public Vector2[] LineOf(string id)
    {
        foreach (var h in stats.hardPoints) if (h.mount == id) return h.path;
        return null;
    }

    [System.Serializable]
    public class Group { public string id, label; public float shot; }

    [System.Serializable]
    public class Solution
    {
        public bool set; [FormerlySerializedAs("rangeKm")] public float rangeUnits; public int brg; public float spd;
        [Tooltip("Step 47: dialled by hand from the plot, not by TRACK. It aims at a point in space, not a locked contact.")]
        public bool manual;
        [Tooltip("Owner 2026-10-29: TRACK made it from the plot (the newest POINT, and the newest POINT-to-POINT RULER).")]
        public bool fromPlot;
        [Tooltip("Owner 2026-11-01: the targeting computer's lock made it, and keeps it up to date.")]
        public bool locked;
        [Tooltip("Step 52: the target's course is known (from a plotted track or a scan), so the shot is led.")]
        public bool hasCourse;
        public float course;
        [Tooltip("Owner 2026-11-01: on a lock, where its course came from: SCAN, PLOT, or empty with none.")]
        public string courseFrom;
        [Tooltip("Where the target was (world units) and when (mission seconds) as the solution was set; the fire computer reckons it forward.")]
        public double wx, wy;
        public float t;
    }

    [Header("Fire control — MOUNTS, SYSTEMS, WARHEADS, SWITCHES and the board's initial state")]
    public Group[] systems =
    {
        new Group { id = "RG", label = "RAILGUN", shot = 70 },
        new Group { id = "PD", label = "PDC", shot = 25 },
        new Group { id = "MS", label = "TORPEDO", shot = 40 },
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
    [Tooltip("Step 55: PDC groups on AUTO, in the order the stats file lists them. Arming the ladder with a group selected puts it on AUTO; it then shoots by itself at enemy torpedoes in its arc.")]
    public bool[] pdcAuto = new bool[2];
    public bool fired;
    [Range(0f, 100f)] public float wepCharge;
    [Tooltip("The fire lever, 0..1; fires once past leverFireAt with the ladder set.")]
    [Range(0f, 1f)] public float fireLever;

    // ---- rounds in flight (Step 48, owner 2026-10-23) ------------------------------------------

    [System.Serializable]
    public class Round
    {
        public double x, y, dx, dy, travelled, maxUnits;
        public double speed = 50;
        [Tooltip("Step 53: a torpedo homes on its target and carries a warhead (an index into warheads).")]
        public bool torpedo;
        public string targetKey;
        public int warhead;
        [Tooltip("Step 55: PDC groups that have had their one chance at this torpedo (bit 0 = PDC-A, bit 1 = PDC-B).")]
        public int pdcTried;
        [Tooltip("Owner 2026-11-01: game seconds a torpedo has flown straight out of the launcher, and whether its motor has lit.")]
        public double coasted;
        public bool motor;
        [Tooltip("Owner 2026-11-02: how fast it is turning now, degrees per game second.")]
        public double turn;
        [System.NonSerialized] public Vector2[] line; // owner 2026-11-02: its launcher's drawn line, laid in the world; null once flown, or with none drawn
        [System.NonSerialized] public double along; // how far down the line it is, units
        [System.NonSerialized] public Stats by; // the firing ship's numbers: how close is a hit, and what the hit does
        [System.NonSerialized] public BoardManager.Glide drawn; // owner 2026-11-01: where the plot draws it, sliding toward StepAhead
    }

    [System.NonSerialized] private System.Collections.Generic.List<Round> rounds;
    public System.Collections.Generic.List<Round> Rounds => rounds ?? (rounds = new System.Collections.Generic.List<Round>());
    /// <summary>Cells left in all this ship's launchers.</summary>
    public int vlsCells { get { int n = 0; foreach (var l in stats.mounts) if (l.kind == "MS") n += l.ammo; return n; } }
    /// <summary>The selected mount can't fire again: no slug or cell left, or too few rounds for a PDC go.</summary>
    public bool Empty { get { var m = MountOf(mount); return m == null || m.ammo < (sys == "PD" ? stats.pdcRoundsPerGo : 1); } }
    /// <summary>The last shot's outcome: "HIT · GARUDA · STBD", "MISS", or null while none has landed.</summary>
    [System.NonSerialized] public string shotReport;
    [System.NonSerialized] public bool lastShotHit;
    [System.NonSerialized] public int shotsFired, shotResults;

    public Group Sys { get { foreach (var y in systems) if (y.id == sys) return y; return systems[0]; } }
    public int SysIndex { get { for (int i = 0; i < systems.Length; i++) if (systems[i].id == sys) return i; return 0; } }
    public Mount MountOf(string id) { foreach (var m in stats.mounts) if (m.id == id) return m; return null; }
    /// <summary>The ids of this ship's weapons of one kind, in the stats file's order.</summary>
    public string[] MountIds(string kind) => System.Array.ConvertAll(System.Array.FindAll(stats.mounts, m => m.kind == kind), m => m.id);
    /// <summary>Owner 2026-11-01: a weapon fires from its hard point. With none, it can't fire.</summary>
    public bool OnDot(string id) { foreach (var h in stats.hardPoints) if (h.mount == id) return true; return false; }

    private void Start() => KeepSelection();

    /// <summary>The stats file says which weapons the ship has: keep the selection on one it has, and an AUTO switch for each PDC group.</summary>
    public void KeepSelection()
    {
        foreach (var y in systems)
        {
            if (MountIds(sys).Length > 0) break;
            sys = y.id;
        }
        var ids = MountIds(sys);
        if (ids.Length > 0 && System.Array.IndexOf(ids, mount) < 0) mount = ids[0];
        System.Array.Resize(ref pdcAuto, MountIds("PD").Length);
    }

    /// <summary>Owner 2026-11-08: a railgun with no cone (Half Arc 0) is fixed to the hull, as the spinal railgun on our nose. It
    /// fires straight along its arc, and you aim it by turning the ship. It needs no target, solution, turret or periscope bearing.</summary>
    public bool FixedGun { get { var m = MountOf(mount); return sys == "RG" && m != null && m.halfArc == 0; } }

    public Solution Sol => sol[SysIndex];
    public float ShotCost => Sys != null ? Sys.shot : 50f;
    public bool WepReady => ship.power.BusUnits("wep") >= 2 && ship.damage.MagOk && ship.orders.WeaponsReleased;
    public string TrackIdFor()
    {
        var t = ship.TrackOf(target);
        if (t == null || !(ship.sensors.identified.Contains(t.key) || LockedOn(t.key))) return null;
        return "TK-" + t.bearing.ToString("000") + "-" + t.key.ToUpperInvariant();
    }
    /// <summary>offBearing: target bearing minus periscope bearing, wrapped to ±180.</summary>
    public double? OffBearing() { var t = ship.TrackOf(target); if (t == null) return null; return ((t.bearing - ship.scopeBrg + 540) % 360) - 180; }
    public bool OnBearing
    {
        get
        {
            if (LockedOn(target)) return true; // owner 2026-11-01: the lock is still on
            // a hand-dialled solution aims at a point: BEARING wants the periscope on the dialled bearing
            if (Sol.set && Sol.manual) return ShipState.AngDiffD(ship.nav.heading + Sol.brg, ship.scopeBrg) <= ship.onBearingDeg;
            var o = OffBearing(); return o.HasValue && System.Math.Abs(o.Value) <= ship.onBearingDeg;
        }
    }

    public bool AllSet
    {
        get
        {
            if (!FixedGun && (!Sol.set || (!Sol.manual && ship.TrackOf(target) == null))) return false; // a hand-dialled shot needs no locked contact
            if (sys == "PD" && !loaders) return false;
            foreach (var b in sw) if (!b) return false;
            return true;
        }
    }

    // Step 55: a PDC group on AUTO keeps its ladder armed whenever it is the one selected
    void ClearSwitches() { bool auto = PdcGroup >= 0 && pdcAuto[PdcGroup]; for (int i = 0; i < sw.Length; i++) sw[i] = auto; }
    /// <summary>The selected PDC group, or -1 when PDC is not selected.</summary>
    int PdcGroup => sys == "PD" ? System.Array.IndexOf(MountIds("PD"), mount) : -1;

    /// <summary>pickSystem via cycleSystem: next system; keep the mount if it belongs, else its first.</summary>
    public void CycleSystem()
    {
        int from = SysIndex;
        for (int i = 1; i <= systems.Length; i++) // past a system this ship has no weapon for
        {
            var ids = MountIds(systems[(from + i) % systems.Length].id);
            if (ids.Length == 0) continue;
            sys = systems[(from + i) % systems.Length].id;
            if (System.Array.IndexOf(ids, mount) < 0) mount = System.Array.Find(ids, OnDot) ?? ids[0]; // the first that can fire: a weapon on no dot can't
            break;
        }
        PickLauncher();
        ClearSwitches(); fired = false;
    }

    // Owner 2026-11-01: torpedoes reach all round, so nothing picks a launcher. The one on the target's side fires; with none left there, the other.
    void PickLauncher()
    {
        if (sys != "MS") return;
        var t = ship.TrackOf(target);
        int rel = t != null ? t.bearing - ship.nav.heading : 0;
        Mount best = null;
        foreach (var id in MountIds("MS"))
        {
            var l = MountOf(id);
            if (l.ammo > 0 && OnDot(id) && (best == null || ShipState.AngDiff(rel, l.arc) < ShipState.AngDiff(rel, best.arc))) best = l;
        }
        if (best != null) mount = best.id;
    }

    public void CycleMount()
    {
        var ids = MountIds(sys);
        if (ids.Length < 2) return;
        mount = ids[(System.Array.IndexOf(ids, mount) + 1) % ids.Length];
        ClearSwitches(); fired = false;
    }

    public void CycleWarhead() { warhead = (warhead + 1) % warheads.Length; ClearSwitches(); }
    public void StepSalvo(int d) { salvo = Mathf.Max(1, Mathf.Min(8, salvo + d)); }
    public void ToggleLoaders() { loaders = !loaders; if (!loaders) System.Array.Clear(pdcAuto, 0, pdcAuto.Length); ClearSwitches(); }
    public void ToggleQuad(int i) { quads[i] = !quads[i]; }

    /// <summary>TRACK (computeSol), with the bridge. A scanned ship the periscope is on, else the target if it is
    /// scanned, gives a sensor lock: range, relative bearing, course and speed, and becomes the target (owner
    /// 2026-10-29: the periscope picks the target). With neither, the plot feeds the gun: the newest POINT is where it
    /// was, the newest POINT-to-POINT RULER its course and speed, reckoned forward to now. Clears the ladder.</summary>
    public void ComputeSol()
    {
        if (!ship.damage.BriOk || Locked) return; // owner 2026-11-01: a lock keeps its own solution; TRACK is for a shot from the plot
        ShipState.Mark p = null;
        foreach (var m in ship.marks) if (m.kind == "point" && (p == null || m.id > p.id)) p = m;
        if (p == null) return;
        var cur = new Solution { set = true, manual = true, fromPlot = true };
        ApplyTrack(cur);
        double px = p.x, py = p.y;
        if (cur.hasCourse)
        {
            double c = cur.course * System.Math.PI / 180, dt = ship.missionNow - p.t;
            px += System.Math.Sin(c) * cur.spd * dt; py += System.Math.Cos(c) * cur.spd * dt;
        }
        double dx = px - ship.nav.x, dy = py - ship.nav.y;
        cur.rangeUnits = (float)System.Math.Sqrt(dx * dx + dy * dy);
        cur.brg = (ShipState.JsRound((System.Math.Atan2(dx, dy) * 180 / System.Math.PI + 360) % 360) - ship.nav.heading + 540) % 360 - 180;
        cur.wx = px; cur.wy = py; cur.t = ship.missionNow;
        sol[SysIndex] = cur;
        ClearSwitches(); fired = false;
    }

    // ---- the targeting computer's lock (owner 2026-11-01) ---------------------------------------

    /// <summary>The ship being locked on, or locked; and how full the lock bar is, 0 to 1. While it is set our signal
    /// rises as ACTIVE SCAN's does.</summary>
    [System.NonSerialized] public string lockKey;
    [System.NonSerialized] public float lockFill;
    /// <summary>Game seconds left on the lock (owner 2026-11-01: locks are short).</summary>
    [System.NonSerialized] public float lockLeft;
    public bool Locked => lockKey != null && lockFill >= 1f;
    public bool LockedOn(string key) => Locked && lockKey == key;
    /// <summary>The lock bar as the sight shows it: filling as the lock comes on, then running down as it runs out.</summary>
    public float LockBar => Locked ? Mathf.Clamp01(lockLeft / Mathf.Max(0.01f, stats.lockHoldSeconds)) : lockFill;
    /// <summary>What fire control says of a lock: the seconds left, and where the shot's course comes from.</summary>
    public string LockSource => "LOCK " + Mathf.CeilToInt(lockLeft) + " S · " + (Sol.courseFrom != null ? Sol.courseFrom + " COURSE" : "NO COURSE");

    /// <summary>Each tick. With a ship in the sights at the right zoom the bar fills; out of them it falls back. Once
    /// full the ship is locked: the target, tracked on the plot, its firing solution kept up to date, for
    /// lockHoldSeconds, or until DRADIS and our eyes lose it or another ship is locked. A lock works on an unscanned
    /// ship, but it knows only where the ship is (<see cref="KeepSolution"/>).</summary>
    public void TickLock(double dt, bool scopeDown)
    {
        var seen = ship.ScopeSubject(scopeDown);
        bool fits = seen != null && ship.ScopeFits(seen);
        if (Locked && !(fits && seen.key != lockKey))
        {
            var held = ship.TrackOf(lockKey);
            lockLeft -= (float)dt;
            if (lockLeft <= 0f || held == null || held.wreck || !(ship.sensors.Heard(held) || ship.sensors.InSight(held))) DropLock();
            else KeepSolution(held);
            return;
        }
        if (fits)
        {
            if (lockKey != seen.key) { DropLock(); lockKey = seen.key; }
            lockFill = Mathf.Min(1f, lockFill + (float)dt / Mathf.Max(0.01f, stats.lockSeconds));
            if (!Locked) return;
            lockLeft = stats.lockHoldSeconds;
            if (target != seen.key) Retarget(seen.key);
            KeepSolution(seen);
            return;
        }
        if (lockKey == null) return;
        lockFill = Mathf.Max(0f, lockFill - (float)dt / Mathf.Max(0.01f, stats.lockFallSeconds));
        if (lockFill <= 0f) lockKey = null;
    }

    /// <summary>The lock is gone: its solutions and the switches go with it.</summary>
    public void DropLock()
    {
        bool had = Locked;
        lockKey = null; lockFill = 0f; lockLeft = 0f;
        if (!had) return;
        for (int i = 0; i < sol.Length; i++) if (sol[i].locked) sol[i] = new Solution();
        ClearSwitches();
    }

    // The railgun and torpedo solutions, from the locked ship as it is now. A lock knows where it is. Where it is going is
    // the newest of a scan and a plotted track (owner 2026-11-01); with neither the shot goes where the ship is now
    void KeepSolution(ShipState.Track t)
    {
        int rel = ((t.bearing - ship.nav.heading + 540) % 360) - 180;
        var plotted = new Solution();
        float plotAt = ApplyTrack(plotted);
        var scan = ship.sensors.ScanOf(t.key);
        bool scanned = scan != null && scan.t >= plotAt;
        for (int i = 0; i < systems.Length && i < sol.Length; i++)
            if (systems[i].id != "PD")
                sol[i] = new Solution { set = true, locked = true, rangeUnits = t.rangeUnits, brg = rel, wx = t.worldX, wy = t.worldY, t = ship.missionNow,
                    spd = scanned ? scan.speed : plotted.spd, hasCourse = scanned ? scan.speed > 0.01f : plotted.hasCourse,
                    course = scanned ? scan.course : plotted.course, courseFrom = scanned ? "SCAN" : plotted.hasCourse ? "PLOT" : null };
    }

    /// <summary>nudgeSol: RNG ±100 units, BRG ±1° (±180), SPD ±stats.spdStep (2 dp; the design had 0.05, x5 in Step 63); clears the ladder.</summary>
    public void NudgeSol(string field, int d)
    {
        if (Locked) return; // the lock keeps the dials
        var cur = Sol;
        double abs; // the bearing to the point now, degrees from north: exact, as BRG shows it rounded to a degree
        if (!cur.set)
        {
            // Step 47 (owner 2026-10-23): a knob starts a hand-dialled solution, from the plot
            cur.set = true; cur.manual = true;
            cur.rangeUnits = stats.manualStartRangeUnits;
            cur.brg = (ShipState.JsRound(ship.scopeBrg) - ship.nav.heading + 540) % 360 - 180; // relative, as TRACK stores it
            cur.spd = 0f;
            ApplyTrack(cur);
            abs = ship.nav.heading + cur.brg;
        }
        else { double x, y; Reckon(cur, out x, out y); abs = System.Math.Atan2(x - ship.nav.x, y - ship.nav.y) * 180 / System.Math.PI; }
        if (field == "rng") cur.rangeUnits = Mathf.Max(0f, cur.rangeUnits + d * 100f);
        if (field == "brg") { int b = Mathf.Max(-180, Mathf.Min(180, cur.brg + d)); abs += b - cur.brg; cur.brg = b; }
        if (field == "spd") cur.spd = Mathf.Max(0f, (float)System.Math.Round(cur.spd + d * stats.spdStep, 2, System.MidpointRounding.AwayFromZero));
        // the dialled point is where the target is now; on TRACK's solution RNG and BRG move its point too (owner 2026-10-29)
        if (cur.manual || field != "spd") Fix(cur, abs);
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
    /// autoloaders: on AUTO it fires by itself at what enters its arc. A fixed gun needs no solution, turret or bearing.</summary>
    public bool RungGated(int i)
    {
        bool pd = sys == "PD", free = pd || FixedGun;
        return (i == 0 && (!WepReady || Empty || !OnDot(mount) || (pd ? !loaders : !FixedGun && !Sol.set)))
            || (i == 1 && !free && !InArc) || (i == 2 && !free && !OnBearing) || (i == 3 && wepCharge < ShotCost);
    }

    /// <summary>The lever passing 0.9 with the ladder set: rounds away, charge spent (drawWep).
    /// Step 48: refused below the shot's cost; firing releases ARM, one shot per arming (spec 8.4.b);
    /// a railgun shot is a slug that flies (TickRounds); firing spikes our signal (GDD p.42).</summary>
    public void Fire()
    {
        if (sys == "PD") return; // Step 55: PDCs on AUTO fire by themselves; the lever does nothing
        if (!AllSet) return; // moveSlew fires on every new pull while allSet() holds; the panel's per-pull latch stops repeats (Step 33g verification)
        if (RungGated(0) || RungGated(1)) return; // a gate failed since its rung was thrown (TickCharge drops it on the next tick)
        if (wepCharge < ShotCost || Empty) return;
        if (sys == "MS" && !TorpedoReady) return;
        fired = true;
        wepCharge = Mathf.Max(0f, wepCharge - ShotCost);
        sw[3] = false;
        shotsFired++;
        var m = MountOf(mount);
        if (sys == "RG" && m != null)
        {
            // owner 2026-11-01: the slug flies straight down the gun's bar, from its hard point. Fired before the bar is on the target, it misses.
            // Owner 2026-11-08: the bow to the fraction of a degree, for the spinal railgun; the slug keeps the ship's motion
            Vector2 gun = HardPointOffset(m.id, ship.nav.heading);
            Rounds.Add(Slug(ship.nav.x + gun.x, ship.nav.y + gun.y, ship.nav.TrueHeading + m.arc + m.aimDeg, stats.slugSpeedUnits, ship.nav.vx, ship.nav.vy, m.rangeUnits));
            m.ammo--;
            shotReport = null;
            ship.sensors.signal = Mathf.Min(100f, ship.sensors.signal + stats.fireSignalSpike);
        }
        if (sys == "MS" && m != null)
        {
            // a salvo out of the launcher, fanned a little; each torpedo flies straight out, then homes on the target
            var tgt = ship.TrackOf(target);
            int n = Mathf.Min(salvo, m.ammo);
            Vector2 tube = HardPointOffset(m.id, ship.nav.heading);
            for (int i = 0; i < n; i++)
            {
                var r = NewTorpedo(stats, ship.nav.x + tube.x, ship.nav.y + tube.y, ship.nav.heading, m.arc, i, n, m.rangeUnits, warhead, LineOf(m.id));
                r.targetKey = tgt.key;
                Rounds.Add(r);
            }
            m.ammo -= n;
            shotReport = null;
            ship.sensors.signal = Mathf.Min(100f, ship.sensors.signal + stats.fireSignalSpike);
        }
        PickLauncher();
        if (Empty) ClearSwitches(); // the last one gone: nothing left to load
    }

    /// <summary>Step 53 (GDD p.45): torpedoes need a locked target — identified by an active scan — and a cell left.</summary>
    public bool TorpedoReady { get { var t = ship.TrackOf(target); return t != null && !t.wreck && (ship.sensors.identified.Contains(t.key) || LockedOn(t.key)) && vlsCells > 0; } }

    // ---- leading a moving target (Step 52, owner 2026-10-23) ---------------------------------

    /// <summary>The latest plotted track (a RULER from one POINT to another) gives the solution its course,
    /// and its speed into SPD. Returns when that track ends, mission seconds; -1 with no track.</summary>
    public float ApplyTrack(Solution cur)
    {
        ShipState.Mark latest = null;
        ShipState.Mark f, g; double crs, spd;
        foreach (var m in ship.marks) if (m.kind == "ruler" && (latest == null || m.id > latest.id) && ship.TrackLeg(m, out f, out g, out crs, out spd)) latest = m;
        if (latest == null) return -1f;
        ship.TrackLeg(latest, out f, out g, out crs, out spd);
        cur.hasCourse = true;
        cur.course = (float)crs;
        cur.spd = (float)System.Math.Round(spd, 2, System.MidpointRounding.AwayFromZero);
        return (float)g.t;
    }

    // the dialled range, at a bearing from north, as a world point, now. The bearing is exact: built from the rounded
    // BRG, a click on a far point would also slide it up to half a degree sideways (killshot-fb, 2026-10-29)
    void Fix(Solution cur, double absDeg)
    {
        double b = absDeg * System.Math.PI / 180;
        cur.wx = ship.nav.x + System.Math.Sin(b) * cur.rangeUnits;
        cur.wy = ship.nav.y + System.Math.Cos(b) * cur.rangeUnits;
        cur.t = ship.missionNow;
    }

    // where the solution puts the target now: its point, moved on along its course and speed since it was set
    void Reckon(Solution cur, out double x, out double y)
    {
        double c = cur.course * System.Math.PI / 180, d = cur.hasCourse ? cur.spd * System.Math.Max(0, ship.missionNow - cur.t) : 0;
        x = cur.wx + System.Math.Sin(c) * d; y = cur.wy + System.Math.Cos(c) * d;
    }

    /// <summary>Owner 2026-10-29: a solution is a point in space, so turning or moving our ship doesn't spoil it.
    /// Its range and bearing from our bow are worked out again each tick, for the knobs, ANGLE, BEARING and SLEW TO.
    /// It knows only the course and speed the target had when it was set: if the target turns, TRACK again.</summary>
    void Follow(Solution cur)
    {
        double x, y; Reckon(cur, out x, out y);
        double dx = x - ship.nav.x, dy = y - ship.nav.y;
        cur.rangeUnits = (float)System.Math.Sqrt(dx * dx + dy * dy);
        cur.brg = (ShipState.JsRound((System.Math.Atan2(dx, dy) * 180 / System.Math.PI + 360) % 360) - ship.nav.heading + 540) % 360 - 180;
    }

    /// <summary>Where to point the slug. With a course, reckon the target forward from where the solution
    /// put it to now, then aim where it will be when the slug arrives. Otherwise straight at the solution's
    /// point, wherever our bow is now.</summary>
    public double LeadBearing()
    {
        var cur = Sol;
        double tx, ty; Reckon(cur, out tx, out ty);
        tx -= ship.nav.x; ty -= ship.nav.y;
        double straight = (System.Math.Atan2(tx, ty) * 180 / System.Math.PI + 360) % 360;
        if (!cur.hasCourse || cur.spd <= 0.001f) return straight;
        // the target's motion seen from our ship: the slug keeps ours (owner 2026-11-08)
        double c = cur.course * System.Math.PI / 180, vx = cur.spd * System.Math.Sin(c) - ship.nav.vx, vy = cur.spd * System.Math.Cos(c) - ship.nav.vy;
        double a = vx * vx + vy * vy - stats.slugSpeedUnits * stats.slugSpeedUnits, bq = 2 * (tx * vx + ty * vy), cq = tx * tx + ty * ty;
        double disc = bq * bq - 4 * a * cq;
        if (a >= 0 || disc < 0) return straight; // can't catch it
        double tHit = (-bq - System.Math.Sqrt(disc)) / (2 * a);
        if (tHit <= 0) return straight;
        return (System.Math.Atan2(tx + vx * tHit, ty + vy * tHit) * 180 / System.Math.PI + 360) % 360;
    }

    /// <summary>A mount's hard point from the ship's centre at this heading, units (x east, y north). No hard point: zero.</summary>
    public Vector2 HardPointOffset(string id, double heading)
    {
        foreach (var h in stats.hardPoints)
        {
            if (h.mount != id) continue;
            float rad = (float)heading * Mathf.Deg2Rad, sin = Mathf.Sin(rad), cos = Mathf.Cos(rad);
            return new Vector2(h.at.x * cos + h.at.y * sin, h.at.y * cos - h.at.x * sin);
        }
        return Vector2.zero;
    }

    // Where a railgun's bar wants to be, as far as the cone allows: for the selected gun, on the solution, or on the
    // ship being locked as the bar fills; else the middle.
    float WantedAim(Mount m)
    {
        if (m.id != mount) return 0f;
        var locking = lockKey != null ? ship.TrackOf(lockKey) : null;
        if (!Sol.set && locking == null) return 0f;
        float want = Sol.set ? (float)LeadBearing() : (float)ship.sensors.ExactBearing(locking);
        return Mathf.Clamp(Mathf.DeltaAngle((float)ship.nav.heading + m.arc, want), -m.halfArc, m.halfArc);
    }

    /// <summary>Owner 2026-11-01: inside its soft cone a railgun is a bar. It turns slowly toward the solution.
    /// Owner 2026-11-09: a fixed gun (the spinal railgun) has no bar. On a lock the whole ship turns itself onto the lead, then you fire.</summary>
    public void TickGuns(double dt)
    {
        var spinal = FixedGun ? MountOf(mount) : null;
        ship.nav.aimHeading = spinal != null && Sol.set && Sol.locked ? ((LeadBearing() - spinal.arc) % 360 + 360) % 360 : double.NaN;
        foreach (var id in MountIds("RG"))
        {
            var m = MountOf(id);
            if (m == null) continue;
            m.aimWas = m.aimDeg;
            m.aimDeg = Mathf.MoveTowards(m.aimDeg, WantedAim(m), stats.gunTurnDegPerSecond * (float)dt);
        }
    }

    /// <summary>The selected gun's bar has reached where it is going (the playtest waits for it).</summary>
    public bool GunSettled { get { var m = MountOf(mount); return m == null || Mathf.Abs(m.aimDeg - WantedAim(m)) < 0.01f; } }

    /// <summary>Hull-fixed arcs (spec 8.7.a): the solution's bearing from our bow must lie in the mount's arc.</summary>
    public bool InArc
    {
        get
        {
            var m = MountOf(mount);
            if (m == null || !Sol.set) return false;
            return ShipState.AngDiff((Sol.brg % 360 + 360) % 360, m.arc) <= m.halfArc;
        }
    }

    /// <summary>Fly every round dt mission seconds. Its whole path this tick is tested, so a fast slug
    /// cannot skip past a ship: the first ship's box it reaches is hit, on the side it reaches. The test
    /// follows the ship's own motion through the tick too (Step 63).</summary>
    public void TickRounds(double dt)
    {
        var list = Rounds;
        for (int i = list.Count - 1; i >= 0; i--) if (!InFlight(list[i], dt, RoundStep)) list.RemoveAt(i);
    }

    const double TorpedoStepSeconds = 0.05;

    // Owner 2026-11-02: a torpedo flies in short steps, which keeps its curve the same at every game speed; anything else flies
    // the whole tick at once. step gets the round, the game seconds to fly, and the game seconds of this tick already gone. False once it has ended.
    static bool InFlight(Round r, double dt, System.Func<Round, double, double, bool> step)
    {
        for (double gone = 0; gone < dt - 1e-9;)
        {
            double h = r.torpedo ? System.Math.Min(dt - gone, TorpedoStepSeconds) : dt;
            if (!step(r, h, gone)) return false;
            gone += h;
        }
        return true;
    }

    // One step of one of our rounds. False once it has hit or run out of range.
    bool RoundStep(Round r, double dt, double gone)
    {
        if (r.torpedo && Clear(r, dt)) Steer(r, dt);
        double step = System.Math.Min(r.speed * dt, r.maxUnits - r.travelled);
        double flight = r.speed > 0 ? step / r.speed : 0; // seconds of this tick the round is flying
        string side;
        float breakAt; // where along the ship it struck, 0 the stern to 1 the nose
        var struck = FirstStruck(r, flight, out side, out _, out breakAt, gone);
        if (struck != null)
        {
            // Step 49: the struck ship takes the hit on its own model (its side, then what is behind it)
            var victim = ship.ShipOf != null ? ship.ShipOf(struck.key) : null;
            string damage = victim == null ? "" : r.torpedo ? victim.damage.TakeWarhead(side, warheads[r.warhead], stats) : victim.damage.TakeSlug(side, stats);
            if (victim != null && victim.destroyed) ship.Kill(struck, breakAt);
            else { shotReport = "HIT · " + ship.NameOf(struck) + (ship.Scanned(struck) ? " · " + side + damage : ""); lastShotHit = true; shotResults++; } // Step 61: detail once scanned
            return false;
        }
        r.x += r.dx * step; r.y += r.dy * step; r.travelled += step;
        if (r.travelled < r.maxUnits - 1e-6) return true;
        shotReport = "MISS"; lastShotHit = false; shotResults++;
        return false;
    }

    // The first ship's box round r reaches in `flight` seconds, the side it comes through, and how far along that flight.
    // gone: game seconds of this tick already flown, which ships have also moved through
    ShipState.Track FirstStruck(Round r, double flight, out string side, out double first, out float breakAt, double gone = 0)
    {
        ShipState.Track struck = null;
        side = null;
        first = double.MaxValue;
        breakAt = 0.5f;
        foreach (var t in ship.tracks)
        {
            if (t.own || !t.hasWorld || t.wreck) continue;
            // the round's path as seen from the ship: its velocity less the ship's
            double c = t.course * System.Math.PI / 180;
            double fx = r.x - t.worldX - t.speed * System.Math.Sin(c) * gone, fy = r.y - t.worldY - t.speed * System.Math.Cos(c) * gone;
            double mx = (r.dx * r.speed - t.speed * System.Math.Sin(c)) * flight, my = (r.dy * r.speed - t.speed * System.Math.Cos(c)) * flight;
            var hull = ship.ShipOf != null ? ship.ShipOf(t.key) : null;
            Vector2 half = hull != null ? stats.HitHalf(hull.nav.BoxHalf) : new Vector2(25f, 25f); // no ship behind the track: a small box
            double at; string through;
            if (ShipState.EntersBox(fx, fy, fx + mx, fy + my, t.worldHdg, half.x, half.y, out at, out through) && at < first)
            {
                struck = t; side = through; first = at;
                double bow = t.worldHdg * System.Math.PI / 180;
                breakAt = 0.5f + (float)(((fx + mx * at) * System.Math.Sin(bow) + (fy + my * at) * System.Math.Cos(bow)) / (2 * half.y));
            }
        }
        return struck;
    }

    /// <summary>Owner 2026-11-01: how far round r flies by the next tick, dt game seconds away: a whole step, or only as far
    /// as the box it reaches first, so the plot's dot touches the ship as the hit lands. Drawing only.</summary>
    public double StepAhead(Round r, double dt)
    {
        double step = System.Math.Min(r.speed * dt, r.maxUnits - r.travelled), at;
        bool hits = r.by != null ? ReachesUs(r, step, 0, out at, out _) : FirstStruck(r, step / r.speed, out _, out at, out _) != null;
        return hits ? step * at : step;
    }

    // A torpedo leaving a launcher at (x, y) on a ship with this heading: straight out of the launcher's side, tilted a little
    // toward the stern, or from one on the centre line out of each side by turns, the salvo fanned a little. by: the firing ship's numbers.
    // path: the line drawn out of the launcher (owner 2026-11-02), flown instead; from the centre line every other torpedo flies its mirror.
    Round NewTorpedo(Stats by, double x, double y, double heading, int arc, int i, int n, double maxUnits, int warheadIndex, Vector2[] path = null)
    {
        int side = arc % 180 == 0 ? (i % 2 == 0 ? 1 : -1) : arc % 360 < 180 ? 1 : -1;
        double fan = (i - (n - 1) / 2.0) * by.torpedoSpreadDeg, b = (heading + side * (90 + by.torpedoTiltBackDeg) + fan) * System.Math.PI / 180;
        return new Round { x = x, y = y, dx = System.Math.Sin(b), dy = System.Math.Cos(b), maxUnits = maxUnits, speed = by.torpedoLaunchSpeedUnits,
                           torpedo = true, warhead = warheadIndex, line = Laid(path, x, y, heading + fan, arc % 180 == 0 && side < 0) };
    }

    // Owner 2026-11-01: a torpedo flies straight out of its launcher, slowing. True once it is clear of that and can steer.
    // Owner 2026-11-02: with a line drawn out of the launcher it flies down the line instead, slowing the same way, and is clear at its end.
    bool Clear(Round r, double dt)
    {
        var by = r.by ?? stats;
        if (r.line == null && r.coasted >= by.torpedoCoastSeconds) return true;
        r.coasted = System.Math.Min(by.torpedoCoastSeconds, r.coasted + dt);
        r.speed = by.torpedoLaunchSpeedUnits + (by.torpedoCoastSpeedUnits - by.torpedoLaunchSpeedUnits) * r.coasted / by.torpedoCoastSeconds;
        if (r.line == null) return false;
        // it heads for the spot one step further down the line; past the end the line is done and it turns from there
        r.along += r.speed * dt;
        Vector2 next;
        if (!Along(r.line, r.along, out next, out _)) { r.line = null; r.coasted = by.torpedoCoastSeconds; }
        double gx = next.x - r.x, gy = next.y - r.y, d = System.Math.Sqrt(gx * gx + gy * gy);
        if (d > 1e-6) { r.dx = gx / d; r.dy = gy / d; }
        return false;
    }

    // Its motor lights, or stays on: it speeds up to full speed.
    void Burn(Round r, double dt)
    {
        var by = r.by ?? stats;
        r.motor = true;
        r.speed = System.Math.Min(by.torpedoSpeedUnits, r.speed + by.torpedoBurnUnits * dt);
    }

    // A clear torpedo curves onto its target, and then its motor lights. With no target left its motor lights anyway and it flies on.
    void Steer(Round r, double dt)
    {
        var t = ship.TrackOf(r.targetKey);
        if (t == null || !t.hasWorld || t.wreck) Burn(r, dt);
        else SteerAt(r, t.worldX, t.worldY, dt);
    }

    // Owner 2026-11-02: it turns as a fighter does. The turn builds up, then eases off as it lines up. Its motor lights once it is within this of lined up, degrees.
    const double MotorLinedUpDeg = 3;

    void SteerAt(Round r, double x, double y, double dt)
    {
        var by = r.by ?? stats;
        double now = System.Math.Atan2(r.dx, r.dy) * 180 / System.Math.PI;
        double off = ((System.Math.Atan2(x - r.x, y - r.y) * 180 / System.Math.PI - now) % 360 + 540) % 360 - 180;
        double most = r.motor ? by.torpedoTurnDegPerS : by.torpedoCurveDegPerS, build = System.Math.Max(0.01, by.torpedoTurnBuildSeconds);
        double want = System.Math.Max(-most, System.Math.Min(most, off / build)), change = most / build * dt;
        r.turn += System.Math.Max(-change, System.Math.Min(change, want - r.turn));
        double head = (now + r.turn * dt) * System.Math.PI / 180;
        r.dx = System.Math.Sin(head); r.dy = System.Math.Cos(head);
        if (r.motor || System.Math.Abs(off) <= MotorLinedUpDeg) Burn(r, dt);
    }

    // ---- enemy torpedoes and our point defence (Step 55, owner 2026-10-24) -----------------------

    [System.NonSerialized] private System.Collections.Generic.List<Round> incoming;
    /// <summary>Enemy torpedoes flying at our ship.</summary>
    public System.Collections.Generic.List<Round> Incoming => incoming ?? (incoming = new System.Collections.Generic.List<Round>());
    /// <summary>The PDCs' last result ("PDC-A · TORPEDO DOWN"), and the last enemy torpedo to hit us.</summary>
    [System.NonSerialized] public string pdcReport, incomingReport;
    [System.NonSerialized] public int pdcKills, hitsTaken;

    /// <summary>An enemy at (x, y) launches n torpedoes from a launcher facing `arc` degrees round from its heading. Each
    /// flies straight out, or down the line drawn out of its launcher, then homes on our ship. by: the enemy's numbers, which its torpedoes fly and hit by.</summary>
    public void LaunchAt(Stats by, double x, double y, double heading, int arc, int n, int warheadIndex, double maxUnits, Vector2[] path = null)
    {
        for (int i = 0; i < n; i++)
        {
            var r = NewTorpedo(by, x, y, heading, arc, i, n, maxUnits, warheadIndex, path);
            r.by = by;
            Incoming.Add(r);
        }
    }

    /// <summary>Wolf Hunt (Step 97): an enemy at (x, y), moving at (vx, vy), fires a railgun slug at us on this true bearing. It flies
    /// straight at its ship's slug speed, plus the ship's own motion (owner 2026-11-08). PDCs cannot stop a slug.</summary>
    public void ShootAt(Stats by, double x, double y, double vx, double vy, double brgDeg, double maxUnits)
    {
        var r = Slug(x, y, brgDeg, by.slugSpeedUnits, vx, vy, maxUnits);
        r.by = by;
        Incoming.Add(r);
    }

    /// <summary>Owner 2026-11-08: a slug fired from a ship moving at (vx, vy) keeps that motion. It leaves the gun on this true
    /// bearing at this speed, so a drifting ship's shot drifts off the line, more the further it flies.</summary>
    static Round Slug(double x, double y, double brgDeg, double speed, double vx, double vy, double maxUnits)
    {
        double b = brgDeg * System.Math.PI / 180, sx = System.Math.Sin(b) * speed + vx, sy = System.Math.Cos(b) * speed + vy;
        double v = System.Math.Sqrt(sx * sx + sy * sy);
        return new Round { x = x, y = y, dx = sx / v, dy = sy / v, speed = v, maxUnits = maxUnits };
    }

    /// <summary>Each tick: our PDCs take their chances, then every enemy torpedo homes on us and every enemy
    /// slug flies straight; one that reaches our box, made bigger by its firing ship's hitMarginPercent, strikes the side it reaches.</summary>
    public void TickIncoming(double dt)
    {
        var list = Incoming;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var r = list[i];
            if ((r.torpedo && PdcStops(r, r.speed * dt)) || !InFlight(r, dt, IncomingStep)) list.RemoveAt(i);
        }
    }

    // One step of an enemy round. False once it has hit us or run out of range.
    bool IncomingStep(Round r, double dt, double gone)
    {
        if (r.torpedo && Clear(r, dt)) SteerAt(r, ship.nav.x, ship.nav.y, dt);
        double step = System.Math.Min(r.speed * dt, r.maxUnits - r.travelled);
        string side;
        if (ReachesUs(r, step, dt, out _, out side))
        {
            incomingReport = r.torpedo ? "TORPEDO HIT · " + side + ship.damage.TakeWarhead(side, warheads[r.warhead], r.by)
                                       : "SLUG HIT · " + side + ship.damage.TakeSlug(side, r.by);
            hitsTaken++;
            return false;
        }
        r.x += r.dx * step; r.y += r.dy * step; r.travelled += step;
        return r.travelled < r.maxUnits - 1e-6;
    }

    // Does enemy round r reach our box on a step of `step` units? `ago`: game seconds our ship has already moved of it
    bool ReachesUs(Round r, double step, double ago, out double at, out string side)
    {
        double fx, fy, mx, my;
        if (r.torpedo) { fx = r.x - ship.nav.x; fy = r.y - ship.nav.y; mx = r.dx * step; my = r.dy * step; }
        else
        {
            // a slug against our own motion through the tick
            double flight = step / r.speed;
            fx = r.x - (ship.nav.x - ship.nav.vx * ago); fy = r.y - (ship.nav.y - ship.nav.vy * ago);
            mx = (r.dx * r.speed - ship.nav.vx) * flight; my = (r.dy * r.speed - ship.nav.vy) * flight;
        }
        Vector2 half = r.by.HitHalf(ship.nav.BoxHalf);
        return ShipState.EntersBox(fx, fy, fx + mx, fy + my, ship.nav.heading, half.x, half.y, out at, out side);
    }

    // A PDC group on AUTO, loaded and powered, gets one chance at each enemy torpedo inside its arc and range.
    // Each go spends stats.pdcRoundsPerGo; a group too low for another go drops off AUTO (owner 2026-10-27).
    bool PdcStops(Round r, double step)
    {
        if (!loaders || !WepReady) return false;
        // the point of this tick's flight closest to our ship: at x5 a torpedo can cross the PDCs' reach inside one tick
        double along = System.Math.Max(0, System.Math.Min(step, (ship.nav.x - r.x) * r.dx + (ship.nav.y - r.y) * r.dy));
        double px = r.x + r.dx * along, py = r.y + r.dy * along;
        var groups = MountIds("PD");
        for (int g = 0; g < groups.Length && g < pdcAuto.Length; g++)
        {
            var m = MountOf(groups[g]);
            if (!pdcAuto[g] || m == null || !OnDot(m.id) || (r.pdcTried & (1 << g)) != 0) continue;
            Vector2 gun = HardPointOffset(m.id, ship.nav.heading);
            double dx = px - ship.nav.x - gun.x, dy = py - ship.nav.y - gun.y;
            int rel = ShipState.JsRound(((System.Math.Atan2(dx, dy) * 180 / System.Math.PI - ship.nav.heading) % 360 + 360) % 360) % 360;
            if (dx * dx + dy * dy > (double)m.rangeUnits * m.rangeUnits || ShipState.AngDiff(rel, m.arc) > m.halfArc) continue;
            r.pdcTried |= 1 << g;
            m.ammo -= stats.pdcRoundsPerGo;
            if (m.ammo < stats.pdcRoundsPerGo) { pdcAuto[g] = false; if (g == PdcGroup) ClearSwitches(); }
            bool down = ship.NextDie() < stats.pdcKillChance;
            pdcReport = m.shortName + (down ? " · TORPEDO DOWN" : " · MISSED");
            if (down) { pdcKills++; return true; }
        }
        return false;
    }

    // ---- the tick — weapons capacitor: +5 per WEP unit per game second up to 100, bleeds 3 with no WEP --

    public void TickCharge(double dt)
    {
        float feed = ship.power.BusUnits("wep") * stats.wepFeedPerUnit * (float)dt;
        wepCharge = feed > 0f ? Mathf.Min(100f, wepCharge + feed) : Mathf.Max(0f, wepCharge - stats.wepBleed * (float)dt);
        foreach (var s in sol) if (s.set) Follow(s); // our turn or move since the last tick: the bow's new range and bearing to each point
        PickLauncher();
        // Tooling's bugs 7 and 8: a thrown LOAD whose gate fails (WEP power, magazine, release, rounds) or a thrown ANGLE
        // out of arc drops, with every rung after it. BEARING is left alone: a moving target drifts off it slowly.
        for (int i = 0; i < 2; i++) if (sw[i] && RungGated(i)) { Flip(i); break; }
    }

    /// <summary>validSet: hostile contacts inside the current mount's range and arc.</summary>
    public bool ValidTarget(string key)
    {
        var m = MountOf(mount);
        var t = ship.TrackOf(key);
        return m != null && t != null && t.hostile && t.rangeUnits <= m.rangeUnits && ShipState.AngDiff(t.bearing, m.arc) <= m.halfArc;
    }
    /// <summary>Step 99: a new target, whatever its range or arc (TRACK on the ship the periscope is on): the ladder,
    /// the lever, fired and the solutions reset (a solution was worked out on the old target).</summary>
    public void Retarget(string key)
    {
        target = key;
        fired = false;
        ClearSwitches(); // a PDC group on AUTO keeps its ladder
        for (int i = 0; i < sol.Length; i++) sol[i] = new Solution();
        fireLever = 0f;
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
