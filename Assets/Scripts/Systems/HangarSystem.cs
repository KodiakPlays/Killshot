using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Hangar: one ship's squadrons and what they do once launched. It holds the squadrons and their craft,
/// the launch stages, the bay doors, the launch bearing and lever, the squadrons in flight, the fighters'
/// attack runs, and the assault boats with the marines they carry. Our ship and every enemy carry one (the
/// enemy is our ship with other stats). Owner 2026-10-26: the hangar lives in one place, here. Contacts,
/// damage, crew and kills stay on the ship state (ShipState); this reads them. Rules from
/// board-logic.js (SQUADS, LAUNCH_STAGES, stageBlock, launchBlock, pressStage, launch) and Steps 54 and 60.
/// </summary>
public class HangarSystem : MonoBehaviour
{
    /// <summary>The ship this belongs to. The hangar reads its contacts, AUX bus, alert state, compartments,
    /// where it is, fire control's target, and the damage model of the ships it attacks.</summary>
    [System.NonSerialized] public ShipState ship;

    /// <summary>This ship's numbers for fighters and boats: its section of the ship's stats file (Assets/Ships,
    /// Starwolf → Tuning).</summary>
    [System.Serializable]
    public class Stats
    {
        [Tooltip("Fighter speed, units/s (owner: three times our top speed; Step 63: x5).")]
        [FormerlySerializedAs("fighterSpeedKms")] public double fighterSpeedUnits = 40;
        [Tooltip("Inside this distance of the target the squadron makes its attack runs, units.")]
        [FormerlySerializedAs("fighterAttackKm")] public double fighterAttackUnits = 200;
        [Tooltip("One attack run by every fighter this often, mission seconds.")]
        public float fighterRunSeconds = 30f;
        [Tooltip("Armour each fighter takes off the side it attacks, per run.")]
        public float fighterArmourDamage = 10f;
        [Tooltip("Integrity each fighter takes off a compartment through an open side, per run.")]
        public int fighterCompartmentDamage = 1;
        [Header("Assault boats (Step 60, BOARDING_README.md)")]
        [Tooltip("Assault boat speed, units/s (Step 63: x5).")]
        [FormerlySerializedAs("boatSpeedKms")] public double boatSpeedUnits = 30;
        [Tooltip("Marines aboard our ship, carried by the assault boats; those lost aboard an enemy are gone.")]
        public int marines = 18;
        [Tooltip("Marines each assault boat carries.")]
        public int marinesPerBoat = 6;
        [Tooltip("Boats dock inside this distance of the target, units.")]
        [FormerlySerializedAs("dockKm")] public double dockUnits = 50;
        [Tooltip("A ship can be boarded only with a face open and slower than this, units/s (owner: fight it down first).")]
        [FormerlySerializedAs("boardMaxSpeedKms")] public float boardMaxSpeedUnits = 0.5f;
        [Tooltip("How long the marines fight aboard, game seconds.")]
        public float boardSeconds = 60f;
        [Tooltip("Share of the enemy's crew that fights the boarders. The odds are marines ÷ (marines + defenders).")]
        [Range(0f, 1f)] public float defenderShare = 0.25f;
        [Tooltip("Marines lost per defender, when they win.")]
        public float marineLossPerDefender = 0.5f;
        [Tooltip("Assault boats go no further than this from our ship, units (owner 2026-10-27). Outbound past it, they turn home; "
               + "coming home past it, their clock runs.")]
        [FormerlySerializedAs("boatRangeKm")] public double boatRangeUnits = 15000;
        [Tooltip("Game seconds the boats last out of range on the way home; then they and their marines are lost (owner 2026-10-27). "
               + "The clock stops while we are in range, but does not go back.")]
        public float boatStrandSeconds = 120f;
    }
    /// <summary>The numbers this part plays by: its section of the ship's stats file (a copy while the game runs).</summary>
    public Stats stats => shipStats ? shipStats.Stats.hangar : (shipStats = GetComponent<Ship>()).Stats.hangar;
    [System.NonSerialized] Ship shipStats;
    /// <summary>The marines now: it starts from the stats file (Ship), then changes as the ship plays.</summary>
    [System.NonSerialized] public int marines = 18;

    [System.Serializable]
    public class Craft { public string tag, pilot, state; [Range(0, 100)] public int fuel; }

    [System.Serializable]
    public class Squad
    {
        public string key, name, shortName, type, role;
        public string[] orders, orderLabels;
        public Craft[] craft;
    }

    [System.Serializable]
    public class Stage { public string id, label; public float rate; public bool on; public float progress; }

    [Header("Hangar — SQUADS, LAUNCH_STAGES and the board's initial state")]
    public Squad[] squads =
    {
        new Squad { key = "gw", name = "INTERCEPTOR", shortName = "INTCP", type = "STARFIGHTER", role = "INTERCEPT / ESCORT",
            orders = new[] { "destroy", "disable", "escort" }, orderLabels = new[] { "DESTROY", "DISABLE", "ESCORT" },
            craft = new[] {
                new Craft { tag = "01", pilot = "HALLORAN", fuel = 100, state = "READY" }, new Craft { tag = "02", pilot = "VOSS", fuel = 100, state = "READY" },
                new Craft { tag = "03", pilot = "IDE", fuel = 96, state = "READY" }, new Craft { tag = "04", pilot = "MARCHETTI", fuel = 88, state = "READY" },
                new Craft { tag = "05", pilot = "REYES", fuel = 41, state = "REARM" }, new Craft { tag = "06", pilot = "OKONKWO", fuel = 0, state = "DOWN" } } },
        new Squad { key = "ih", name = "ASSAULT BOAT", shortName = "ASSLT", type = "ASSAULT BOAT", role = "BOARDING / SECURE",
            orders = new[] { "destroy", "disable", "secure" }, orderLabels = new[] { "DESTROY", "DISABLE", "SECURE" },
            craft = new[] {
                new Craft { tag = "11", pilot = "DRAKOS", fuel = 100, state = "READY" }, new Craft { tag = "12", pilot = "SANTOS", fuel = 100, state = "READY" },
                new Craft { tag = "13", pilot = "ELLERY", fuel = 74, state = "READY" }, new Craft { tag = "14", pilot = "BRENNAN", fuel = 62, state = "REARM" } } },
    };
    public int squad;                 // index into squads ("gw")
    public string order = "destroy";
    [Tooltip("Squads that are out, and the order each launched on (launched).")]
    public System.Collections.Generic.List<string> launchedSquads = new System.Collections.Generic.List<string>();
    public System.Collections.Generic.List<string> launchedOrders = new System.Collections.Generic.List<string>();
    public Stage[] stages =
    {
        new Stage { id = "crew", label = "CREW", rate = 22 }, new Stage { id = "fuel", label = "FUEL", rate = 18 },
        new Stage { id = "ord", label = "ORDNANCE", rate = 14 }, new Stage { id = "evac", label = "EVAC", rate = 20 },
        new Stage { id = "doors", label = "DOORS", rate = 26 }, new Stage { id = "cat", label = "CATAPULT", rate = 12 },
    };
    public bool doorsOpen;
    [Range(0, 359)] public int lbrg = 62;
    [Tooltip("Launch refused while the launch bearing is more than this from the heading (launchBlock).")]
    public float launchConeDeg = 60f;
    [Tooltip("The launch lever (llever), 0..1; launches once past leverFireAt.")]
    [Range(0f, 1f)] public float launchLever;
    public float leverFireAt = 0.9f;

    // ---- fighters in flight (Step 54, owner 2026-10-23) ----------------------------------------

    [System.Serializable]
    public class Flight
    {
        public string squad, order, targetKey;
        public int craft;
        public double x, y, hdg;
        public bool returning, attacking;
        public float nextRun;
        [Tooltip("Step 60: assault boats — marines carried, whether they are aboard the target and for how long, why they are "
               + "holding off (null while not), and how it ended.")]
        public int marines;
        public bool aboard;
        public float boardT;
        public string holding, outcome;
        [Tooltip("Owner 2026-10-27: boats coming home out of range of our ship, and for how many game seconds so far.")]
        public bool stranded;
        public float strandT;
    }
    [System.NonSerialized] private System.Collections.Generic.List<Flight> flights;
    public System.Collections.Generic.List<Flight> Flights => flights ?? (flights = new System.Collections.Generic.List<Flight>());
    /// <summary>How far out a squadron takes off, and lands, units: the plot sets both each frame from the small circle
    /// round our ship at the current zoom, so the whole V takes off clear of it and lands as its leader reaches it.</summary>
    [System.NonSerialized] public double launchClearUnits, landClearUnits;
    double LandUnits => System.Math.Max(50, landClearUnits);
    [System.NonSerialized] public int boardings;
    /// <summary>Goes up when boats are stranded and when they are lost: each sounds the alert tone.</summary>
    [System.NonSerialized] public int boatWarnings;

    /// <summary>What the squadron is doing, for the hangar's status line.</summary>
    public string FlightStatus(string squadKey)
    {
        var f = Flights.Find(k => k.squad == squadKey);
        if (f == null) return null;
        if (f.stranded) return (f.outcome != null ? f.outcome + " · " : "") + "STRANDED · CLOSE IN · " + Mathf.CeilToInt(stats.boatStrandSeconds - f.strandT) + " S";
        if (f.returning) return (f.outcome != null ? f.outcome + " · " : "") + "RETURNING · " + ShipState.JsRound(System.Math.Max(0, System.Math.Sqrt((f.x - ship.nav.x) * (f.x - ship.nav.x) + (f.y - ship.nav.y) * (f.y - ship.nav.y)) - LandUnits)).ToString("N0") + " KM"; // to the circle, where they land
        if (f.order == "escort") return "ESCORTING";
        var t = ship.TrackOf(f.targetKey);
        string name = t != null ? ship.NameOf(t) : "TARGET";
        if (f.aboard) return "BOARDING " + name + " · " + Mathf.CeilToInt(stats.boardSeconds - f.boardT) + " S";
        if (f.holding != null) return "HOLDING OFF " + name + " · " + f.holding;
        if (f.attacking) return (f.order == "disable" ? "DISABLING " : "ATTACKING ") + name + (ship.Scanned(t) && flightReport != null ? " ·" + flightReport : ""); // Step 61: detail once scanned
        double d = t != null && t.hasWorld ? System.Math.Sqrt((t.worldX - f.x) * (t.worldX - f.x) + (t.worldY - f.y) * (t.worldY - f.y)) : 0;
        return "OUTBOUND TO " + name + " · " + ShipState.JsRound(d).ToString("N0") + " KM";
    }
    [System.NonSerialized] public string flightReport;
    [System.NonSerialized] public int flightRuns;

    /// <summary>Step 101 (owner 2026-10-27): fighters scout. A flight sees within this distance of itself, units.</summary>
    public bool Sees(double x, double y, double withinUnits)
    {
        foreach (var f in Flights) { double dx = x - f.x, dy = y - f.y; if (dx * dx + dy * dy <= withinUnits * withinUnits) return true; }
        return false;
    }

    /// <summary>Each tick: fly to the target (or home, or with us on ESCORT); inside attack range, every fighter
    /// makes a run every stats.fighterRunSeconds; home and close, they land and the squadron is ready again.</summary>
    public void TickFlights(double dt)
    {
        var list = Flights;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var f = list[i];
            bool boats = f.squad == "ih"; // the assault boats carry marines (Step 60)
            var t = f.returning || f.order == "escort" ? null : ship.TrackOf(f.targetKey);
            if (!f.returning && f.order != "escort" && (t == null || !t.hasWorld || t.wreck || t.captured)) { f.returning = true; f.attacking = false; f.aboard = false; t = null; }
            // owner 2026-10-27: boats chase only while the target is in their range of our ship; marines aboard fight on
            if (boats && t != null && !f.aboard && System.Math.Sqrt((t.worldX - ship.nav.x) * (t.worldX - ship.nav.x) + (t.worldY - ship.nav.y) * (t.worldY - ship.nav.y)) > stats.boatRangeUnits)
            { f.returning = true; f.holding = null; f.outcome = "OUT OF RANGE"; t = null; }
            double gx = t != null ? t.worldX : ship.nav.x, gy = t != null ? t.worldY : ship.nav.y;
            // an escort holds where it took off, outside the small circle round our ship (owner 2026-10-26), not on the hull
            double reach = f.returning ? LandUnits : f.order == "escort" ? System.Math.Max(300, launchClearUnits) : boats ? stats.dockUnits : stats.fighterAttackUnits;
            double dx = gx - f.x, dy = gy - f.y, d = System.Math.Sqrt(dx * dx + dy * dy);
            // coming home with our ship out of their range, the boats' clock runs; at its end they and their marines are lost
            bool lost = false;
            if (boats && f.returning)
            {
                if (d > stats.boatRangeUnits && !f.stranded) boatWarnings++;
                f.stranded = d > stats.boatRangeUnits;
                if (f.stranded) f.strandT += (float)dt;
                lost = f.strandT >= stats.boatStrandSeconds;
                if (lost)
                {
                    int n = f.craft;
                    foreach (var c in System.Array.Find(squads, q => q.key == f.squad).craft) if (n > 0 && c.state == "READY") { c.state = "LOST"; n--; }
                    f.marines = 0;
                    boatWarnings++;
                }
            }
            if (d > reach && !lost)
            {
                double step = System.Math.Min((boats ? stats.boatSpeedUnits : stats.fighterSpeedUnits) * dt, d - reach * 0.9);
                f.x += dx / d * step; f.y += dy / d * step;
                f.hdg = (System.Math.Atan2(dx, dy) * 180 / System.Math.PI + 360) % 360;
                f.holding = null;
                if (!f.aboard) continue; // owner 2026-10-27: aboard, the boats ride with the ship and the fight goes on
            }
            if (f.returning)
            {
                list.RemoveAt(i);
                int k = launchedSquads.IndexOf(f.squad);
                if (k >= 0) { launchedSquads.RemoveAt(k); launchedOrders.RemoveAt(k); }
                marines += f.marines; // the survivors are home
                ClearStages();
                continue;
            }
            if (t == null) continue; // escorting: stay with us
            if (boats)
            {
                var target = ship.ShipOf != null ? ship.ShipOf(t.key) : null;
                if (target != null) TickBoarding(f, t, target, dt);
                continue;
            }
            if (!f.attacking) { f.attacking = true; f.nextRun = ship.missionNow; }
            if (ship.missionNow < f.nextRun) continue;
            f.nextRun = ship.missionNow + stats.fighterRunSeconds;
            var enemy = ship.ShipOf != null ? ship.ShipOf(t.key) : null;
            if (enemy == null) continue;
            string side = SideFrom(t, f.x, f.y), result = "";
            for (int c = 0; c < f.craft && !enemy.destroyed; c++)
                result = enemy.damage.TakeFighterRun(side, f.order == "disable", stats.fighterArmourDamage, stats.fighterCompartmentDamage);
            flightRuns++;
            flightReport = result;
            if (enemy.destroyed && !t.wreck) ship.Kill(t);
        }
    }

    // Step 60 (owner 2026-10-24, BOARDING_README.md): the boats hold off until a face is open and the ship is nearly
    // stopped, then dock; the marines fight for stats.boardSeconds; then they carry out the order, or are repelled.
    void TickBoarding(Flight f, ShipState.Track t, ShipState enemy, double dt)
    {
        if (!f.aboard)
        {
            bool open = false;
            foreach (var a in enemy.damage.SideArmour) if (a <= 0f) open = true;
            f.holding = !open ? "NO OPEN SIDE" : t.speed > stats.boardMaxSpeedUnits ? "TARGET UNDER WAY" : null;
            if (f.holding != null) return;
            f.aboard = true; f.boardT = 0;
        }
        f.boardT += (float)dt;
        if (f.boardT < stats.boardSeconds) return;
        int defenders = Mathf.CeilToInt(enemy.damage.TotalCrew * stats.defenderShare);
        bool won = f.marines > 0 && (defenders == 0 || ship.Roll(f.marines / (double)(f.marines + defenders)));
        if (won)
        {
            enemy.damage.KillCrew(defenders);
            f.marines = Mathf.Max(0, f.marines - Mathf.RoundToInt(defenders * stats.marineLossPerDefender));
            if (f.order == "destroy") { enemy.damage.Comp("rct").integ = 0; enemy.destroyed = true; ship.Kill(t); f.outcome = ship.NameOf(t) + " SCUTTLED"; }
            else if (f.order == "disable") { enemy.damage.Comp("eng").integ = 0; enemy.damage.Comp("mag").integ = 0; f.outcome = ship.NameOf(t) + " DISABLED"; }
            else { enemy.captured = true; t.captured = true; t.hostile = false; f.outcome = ship.NameOf(t) + " SECURED"; }
        }
        else { enemy.damage.KillCrew(f.marines / 2); f.marines = 0; f.outcome = "BOARDING REPELLED"; }
        flightReport = f.outcome;
        boardings++;
        f.aboard = false;
        f.returning = true;
    }

    public Squad CurrentSquad => squads[squad];
    public bool Launched(Squad q) => launchedSquads.Contains(q.key);
    public string LaunchedOrder(Squad q) { int i = launchedSquads.IndexOf(q.key); return i >= 0 ? launchedOrders[i] : null; }
    public int SquadReady(Squad q) { int n = 0; foreach (var c in q.craft) if (c.state == "READY") n++; return n; }
    public bool AllStagesDone { get { foreach (var g in stages) if (g.progress < 100f) return false; return true; } }
    /// <summary>launchDev: how far the launch bearing is from the heading.</summary>
    public int LaunchDev => Mathf.Abs(((lbrg - ship.nav.heading + 540) % 360) - 180);

    public string StageBlock(string id)
    {
        if (id == "crew" && !ship.orders.FlightOpsReleased) return "FLIGHT OPS NOT RELEASED \u2014 HELD AT DC-1 ALERT CONDITION 3";
        if (id == "fuel" && !ship.damage.HgrOk) return "HANGAR BAY DAMAGED \u2014 FUEL LINES DOWN";
        if (id == "ord" && !ship.damage.MagOk) return "MAGAZINE HIT \u2014 NO ORDNANCE";
        if (id == "doors" && !doorsOpen) return "BAY DOOR MASTER SHUT";
        if (id == "cat" && ship.power.BusUnits("aux") < 1) return "NO AUX UNITS \u2014 CATAPULT WILL NOT CHARGE";
        return null;
    }

    public string LaunchBlock()
    {
        var q = CurrentSquad;
        if (q == null) return "NO SQUADRON SELECTED";
        if (Launched(q)) return null;
        if (!ship.orders.FlightOpsReleased) return "FLIGHT OPS NOT RELEASED \u2014 HELD AT DC-1 ALERT CONDITION 3";
        if (!ship.damage.HgrOk) return "HANGAR BAY DAMAGED \u2014 CANNOT SPOT CRAFT";
        if (SquadReady(q) == 0) return "NO CRAFT READY";
        if (q.key == "ih" && marines < 1) return "NO MARINES ABOARD"; // Step 60
        if (LaunchDev > launchConeDeg) return "BEARING OUTSIDE LAUNCH CONE";
        return null;
    }

    public void ToggleDoors() { doorsOpen = !doorsOpen; }

    /// <summary>setLbrg: non-digits stripped, wrapped to 0–359.</summary>
    public void SetLbrg(string text)
    {
        var digits = System.Text.RegularExpressions.Regex.Replace(text ?? "", "[^0-9]", "");
        int n;
        lbrg = int.TryParse(digits, out n) ? ((n % 360) + 360) % 360 : 0;
    }

    void ClearStages() { foreach (var g in stages) { g.on = false; g.progress = 0f; } }

    /// <summary>cycleSquad: next squadron, its first order, and every launch stage cleared.</summary>
    public void CycleSquad()
    {
        squad = (squad + 1) % squads.Length;
        order = CurrentSquad.orders[0];
        ClearStages();
    }

    public void CycleOrder()
    {
        var list = CurrentSquad.orders;
        int i = System.Array.IndexOf(list, order);
        order = list[(i + 1) % list.Length];
    }

    /// <summary>pressStage: releasing a stage releases it and every stage after; throwing needs the
    /// one before at 100% and no stage block; refused once the squadron is out.</summary>
    public void PressStage(int i)
    {
        if (Launched(CurrentSquad)) return;
        if (stages[i].on)
        {
            for (int k = i; k < stages.Length; k++) { stages[k].on = false; stages[k].progress = 0f; }
            return;
        }
        if (i > 0 && stages[i - 1].progress < 100f) return;
        if (StageBlock(stages[i].id) != null) return;
        stages[i].on = true;
    }

    public void LaunchSquad()
    {
        var q = CurrentSquad;
        if (Launched(q) || LaunchBlock() != null || !AllStagesDone) return;
        launchedSquads.Add(q.key);
        launchedOrders.Add(order);
        // Step 54: the squadron's ready craft take off from our ship, for fire control's target;
        // owner 2026-10-26: on the launch bearing, just outside the small circle round our ship, never on the hull
        double lb = lbrg * System.Math.PI / 180.0;
        var f = new Flight { squad = q.key, order = order, targetKey = ship.weapons.target, craft = SquadReady(q),
                             x = ship.nav.x + System.Math.Sin(lb) * launchClearUnits, y = ship.nav.y + System.Math.Cos(lb) * launchClearUnits, hdg = lbrg };
        if (q.key == "ih") { f.marines = Mathf.Min(marines, f.craft * stats.marinesPerBoat); marines -= f.marines; } // Step 60: the boats take marines
        Flights.Add(f);
    }

    public void RecallSquad()
    {
        // Step 54 (owner 2026-10-23): RECALL turns the squadron home; it is back once it lands (TickFlights)
        var f = Flights.Find(k => k.squad == CurrentSquad.key);
        if (f != null) { f.returning = true; f.aboard = false; f.holding = null; return; } // marines aboard withdraw (Step 60)
        int i = launchedSquads.IndexOf(CurrentSquad.key);
        if (i >= 0) { launchedSquads.RemoveAt(i); launchedOrders.RemoveAt(i); }
        ClearStages();
    }

    static string SideFrom(ShipState.Track t, double x, double y) => ShipState.SideAt(System.Math.Atan2(x - t.worldX, y - t.worldY) * 180 / System.Math.PI, t.worldHdg);

    /// <summary>Step 61 (owner 2026-10-24): a contact is named as the sensors board names it — "BOGEY T07" until an
    /// active scan identifies it, then "GARUDA" — and what our hits do to it is reported only once it is scanned.</summary>

    // ---- the tick — launch telemetry: a thrown stage runs out over several game seconds --------

    public void TickLaunch(double dt)
    {
        foreach (var g in stages) g.progress = g.on ? Mathf.Min(100f, g.progress + g.rate * (float)dt) : 0f;
    }
}
