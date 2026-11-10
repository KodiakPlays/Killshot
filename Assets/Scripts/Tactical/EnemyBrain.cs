// =================================================================================================
// EnemyBrain.cs
//
// What it does:
// This is the mind of a computer-run ship: the Garuda, the ships of our battle group, and a ship
// that runs from us, like the pirate corvette. It listens for our ship, decides where to go and
// fires its weapons. It only sets its ship's ordered heading and throttle. The ship then moves by
// the same rules as ours.
//
// How it works:
// - TacticalWorld calls it each time the board steps the game forward. Its numbers are in the
//   Tuning window, under Enemy and Battle group.
// - How aware it is only goes up: COLD, UNEASY, SEARCHING, ALERT. After 90 game seconds without
//   hearing us, it calms down to COLD. Then it goes back to its patrol.
// - Inside its sight range it sees us and goes ALERT. A rock in the way blinds it. A gas cloud
//   around us cuts its sight. Past that, it hears us by its DRADIS reach, less any interference
//   from rocks, times our signal. Its gain goes up as it gets more aware. The clearer it hears
//   us, the more aware it gets and the better it guesses where we are. A hit makes it ALERT and
//   tells it roughly where the shot came from. It can hear our fighters.
// - When hit, or ALERT with a recent fix on us, it fires torpedo salvos. They come from a
//   launcher in reach. With a fix its railguns fire too. Each fires when in reach and reloaded,
//   with us in its cone. It aims where we will be, a little off. Each shot pushes it back a
//   little, as ours does. A damaged magazine stops it firing and sends its repair team.
// - Steering: ALERT with a fix, it closes in, then turns a side to us and slows. Otherwise it goes
//   to where it last heard us and searches round it. It steers round rocks and ships ahead of it.
//   It keeps out of the battle space's edge band. If it gets outside, it heads for the middle.
// - When COLD, it hunts random points across the map. The points drift from its half toward ours.
//   If it has a patrol route, it follows that instead. The route comes from its level spawn, or
//   is set on the ship in the level. On a route it may turn back at any point. A ship can stay
//   still until our ship comes within a set range.
// - A level spawn can make it send radio messages to our comms. Each gives its bearing, heading
//   and speed. It sends the next only after the last is read or lost. Without a route it changes
//   course halfway between messages.
// - With the battle group setting on, it steers by the formation instead. The group waits where it
//   starts until our ship comes close. Then it flies the Mission's group route and turns smoothly
//   onto each new leg. The group waits at the route's end.
// - Each group ship keeps its place, nose forward. In flight it turns at most a set angle off the
//   group's course. It speeds up when behind its place and eases off when ahead. Far from its
//   place, it flies straight for it. Close to our ship, it keeps the group's course. It stops if
//   we are ahead of it and speeds on if we are behind. It passes us on its side of the
//   formation. When it steers round rocks and ships, it ignores our ship.
// - With the runs-from-us setting on, it stays still until our ship scans it. Then it runs straight
//   away from us, round rocks and inside the map. It runs at a set share of its top speed. Once
//   one side of its armour is gone, it stops and stays still.
//
// Technical:
// - Needs a WorldContact on the same object. Data: Level (Awareness), lastKnown and waypoint in
//   world units, lastHeardAt and lastFixAt in game seconds.
// - TacticalWorld.OnTick calls Think(ship, us) once per board step, then ship.nav.Tick. A wrecked
//   or captured ship gets throttle 0 instead of Think. A switched-off brain skips both.
// - Steering writes only ship.nav.orderedHeading and ship.nav.throttle (percent, capped by
//   ThrottleCap). Shots go through us.weapons.ShootAt and LaunchAt. Think also spends its
//   ammunition and orders its repair team. Each railgun shot calls ship.nav.Kick.
// - TacticalWorld passes EnemySpawn's settings in through CruiseUnits, Patrol() and SendSignals().
//   AddContact calls Brief() for a ship that arrives.
// - TacticalWorld.AnyAlert reads Level for QuestSystem. TacticalWorld.HearingUs reads HearsUs for
//   SensorsPanel's "DETECTED BY" count.
// - Hearing reach is senRangeBaseUnits plus gain times senRangePerStepUnits, from its ship's stats
//   file. Rocks take off TacticalWorld.Interference. Trap: every radio message names it GARUDA.
// - Every random choice uses ship.NextDie(). Steering goes through TacticalWorld.ClearHeading,
//   then KeepInside. The edge band is ShipState.arenaWarnUnits.
// - Numbers: [Tune("Enemy")] and [Tune("Battle group")], on the Board tab, per ship. Weapon
//   reach, ammunition and slug speed are in its ship's stats file.
// - Trap: escorts and flees change only the steering. It still listens and fires unless hunts and
//   firesBack are off.
// - Trap: a hit is seen as a drop in its total armour and room integrity from the last step.
// =================================================================================================

using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(WorldContact))]
public class EnemyBrain : MonoBehaviour
{
    public enum Awareness { Cold, Uneasy, Searching, Alert }

    [Tooltip("Wolf Hunt: it listens for us and hunts us. Off: it only hunts the map (the playtest's older checks).")]
    [Tune("Enemy")] public bool hunts = true;

    [Header("Moving")]
    [Tooltip("Cruising speed while it hunts the map, units/s.")]
    [FormerlySerializedAs("cruiseKms")] [Tune("Enemy")] [SerializeField] private float cruiseUnits = 5.5f;
    [Tooltip("A point counts as reached inside this distance, units.")]
    [FormerlySerializedAs("arriveKm")] [Tune("Enemy")] [SerializeField] private float arriveUnits = 1500f;
    [Tooltip("Game seconds for its hunt to drift from its own half of the map to ours.")]
    [Tune("Enemy")] [SerializeField] private float driftSeconds = 900f;
    [Tooltip("Its next hunting point is at least this far from where it is, units.")]
    [FormerlySerializedAs("legMinKm")] [Tune("Enemy")] [SerializeField] private float legMinUnits = 12000f;
    [Tooltip("SEARCHING: it searches inside this distance of where it last heard us, units.")]
    [FormerlySerializedAs("searchKm")] [Tune("Enemy")] [SerializeField] private float searchUnits = 6000f;

    [Header("Hearing")]
    [Tooltip("Its DRADIS gain (1–5) while COLD, UNEASY, SEARCHING and ALERT: more gain, further reach.")]
    [Tune("Enemy")] [SerializeField] private int[] gainByAwareness = { 3, 3, 4, 5 };
    [Tooltip("This many game seconds without hearing us and it gives up, calm, and goes back to its patrol (owner 2026-11-02).")]
    [Tune("Enemy")] [SerializeField] private float giveUpSeconds = 90f;
    [Tooltip("On a patrol route, the chance at each point that it turns back the way it came, 0 to 1. Above 0 its route can't be timed from the last pass.")]
    [Tune("Enemy")] [Range(0f, 1f)] [SerializeField] private float turnBackChance = 0.25f;
    [Tooltip("How loud our fighters are to it, 0–100, like our signal.")]
    [Tune("Enemy")] [SerializeField] private float fighterSignal = 60f;
    [Tooltip("Heard this clearly (0 at the edge of its hearing, 1 right on top of it): SEARCHING. Heard again 20 to 60 game seconds after it last heard us counts too.")]
    [Tune("Enemy")] [SerializeField] private float searchingAt = 0.25f;
    [Tooltip("Heard this clearly: ALERT.")]
    [Tune("Enemy")] [SerializeField] private float alertAt = 0.5f;
    [Tooltip("Game seconds a fix stays good. Older, it stops firing and goes to where it last had us.")]
    [Tune("Enemy")] [SerializeField] private float fixSeconds = 30f;

    [Header("Fighting")]
    [Tooltip("ALERT: it closes until we are inside this share of its railguns' reach, then turns a side to us.")]
    [Tune("Enemy")] [SerializeField] private float fightAtShare = 0.8f;
    [Tooltip("It looks this far ahead for rocks and ships, units, and steers round them (owner 2026-11-01).")]
    [Tune("Enemy")] [SerializeField] private float avoidLookUnits = 600f;
    private TacticalWorld world;
    [Tooltip("Side-on, its throttle as a share of its top speed.")]
    [Tune("Enemy")] [SerializeField] private float fightSpeedShare = 0.4f;
    [Tooltip("Game seconds between shots from one turret, plus up to the jitter below.")]
    [Tune("Enemy")] [SerializeField] private float railgunReloadSeconds = 12f;
    [Tune("Enemy")] [SerializeField] private float railgunJitterSeconds = 5f;
    [Tooltip("Its aim is off by up to this, degrees, either way: a long shot misses more often.")]
    [Tune("Enemy")] [SerializeField] private float aimErrorDeg = 0.4f;
    [Tooltip("ALERT with a fix: game seconds between torpedo salvos.")]
    [Tune("Enemy")] [SerializeField] private float salvoSeconds = 90f;

    [Header("Firing back (Step 55, owner 2026-10-24)")]
    [Tooltip("It fires at all: railguns and torpedoes. Off, it holds its fire (a playtest stand-in).")]
    [Tune("Enemy")] public bool firesBack = true;
    [Tooltip("Torpedoes in a salvo (HE).")]
    [Tune("Enemy")] [SerializeField] private int fireBackSalvo = 2;
    [Tooltip("When we hit it before it is ALERT: at most one salvo this often, game seconds.")]
    [Tune("Enemy")] [SerializeField] private float fireBackSeconds = 120f;

    [Header("Runs from us (owner 2026-11-10, MILK RUN's pirate corvette)")]
    [Tooltip("It sits still until we scan it. Then it runs straight away from us, round rocks and inside the map. "
           + "Once one side of its armour is gone it gives up and stops dead, and our marines can board it. Turn Hunts and Fires Back off with it.")]
    [Tune("Enemy")] public bool flees;
    [Tooltip("Running, its throttle as a share of its top speed. The corvette's top is 60, ours 50: at 0.6 it runs at 36 and we can catch it.")]
    [Tune("Enemy")] [SerializeField] private float fleeSpeedShare = 0.6f;
    private string ownKey; // its track key, from its WorldContact

    [Header("Battle group (owner 2026-11-09)")]
    [Tooltip("One of our battle group: it flies in a loose formation along the level's Group Route (on its Mission), at the group's "
           + "speed, and waits at the last point. It never follows us: we follow them. Our place is the formation's centre.")]
    [Tune("Battle group")] public bool escorts;
    [Tooltip("Its place in the formation, units from the formation's centre, turned to the way the group flies: x to the right, y ahead.")]
    [Tune("Battle group")] [SerializeField] private Vector2 slotUnits;
    [Tooltip("The formation's centre flies its route at this speed, units/s. Ours tops out at 50.")]
    [Tune("Battle group")] [SerializeField] private float groupSpeedUnits = 15f;
    [Tooltip("Owner 2026-11-10: the group waits where it starts until we come this close to its centre, units. Then it sets off.")]
    [Tune("Battle group")] [SerializeField] private float joinUnits = 800f;
    [Tooltip("The formation turns onto each new leg of its route this fast, degrees a game second, so its ships swing round smoothly.")]
    [Tune("Battle group")] [SerializeField] private float groupTurnDeg = 4f;
    [Tooltip("While the group moves, each ship steers at a point this far ahead of its place, units: it flies along with the group, "
           + "nose forward, instead of chasing its place.")]
    [Tune("Battle group")] [SerializeField] private float leadUnits = 400f;
    [Tooltip("While the group flies, a ship turns at most this far off the group's course to close on its place, degrees.")]
    [Tune("Battle group")] [SerializeField] private float mostOffCourseDeg = 30f;
    [Tooltip("Further than this from its place, units, a ship flies straight for it instead of keeping to the group's course.")]
    [Tune("Battle group")] [SerializeField] private float rejoinUnits = 1500f;
    [Tooltip("It keeps at least this far from our ship, units, and goes round us to reach its place.")]
    [Tune("Battle group")] [SerializeField] private float keepClearUnits = 500f;
    [Tooltip("Too close to our ship, it moves away at no less than this share of its top speed.")]
    [Tune("Battle group")] [Range(0f, 1f)] [SerializeField] private float makeRoomShare = 0.3f;
    [Tooltip("Away from its place, it flies this much faster than the formation, units/s, for each unit away.")]
    [Tune("Battle group")] [SerializeField] private float catchUpPerUnit = 0.05f;
    [Tooltip("This close to its place, units, it turns to the way the group flies.")]
    [Tune("Battle group")] [SerializeField] private float inPlaceUnits = 100f;
    private Mission mission;
    private bool lookedForMission;
    private Vector2 group;
    private float groupAt = -1f, groupHeading;
    private int leg;
    private bool joined;

    /// <summary>How aware it is of us. It goes back down only when it gives up.</summary>
    public Awareness Level { get; private set; }
    /// <summary>Its last thought heard or saw our ship (not our fighters): our sensors' "DETECTED BY 1 HOSTILE".</summary>
    public bool HearsUs { get; private set; }
    /// <summary>Where it is heading now, world units.</summary>
    public Vector2 Waypoint => waypoint;
    /// <summary>Where it last heard or saw us (or our fighters), world units; and when, game seconds.</summary>
    public Vector2 LastKnown => lastKnown;
    public float LastHeardAt => lastHeardAt;
    /// <summary>It has a good fix on us: it fires.</summary>
    public bool HasFix(float now) => now - lastFixAt <= fixSeconds;

    /// <summary>A level's spawn can set this (Step 130).</summary>
    public float CruiseUnits { set { cruiseUnits = value; } }
    /// <summary>A level's spawn makes it send radio messages (owner 2026-10-29).</summary>
    public void SendSignals(float first, float every, float onAir) { firstSignalSeconds = first; signalSeconds = every; onAirSeconds = onAir; }
    private float firstSignalSeconds, signalSeconds, onAirSeconds, nextSignal, nextCourse;
    /// <summary>A level's spawn gives it a patrol route (owner 2026-11-02).</summary>
    public void Patrol(Vector2[] points) { route = points; }
    [Tooltip("Owner 2026-11-10: a route set here in the level (a shuttle's run between the battle group and the escape pods). "
           + "It flies it point to point and back at Cruise Units. Empty: none, or the one its spawn gives it.")]
    [SerializeField] private Vector2[] patrolRoute;
    [Tooltip("Owner 2026-11-10: it stays still until we come this close, units (the shuttles start as we arrive). 0: it starts at once.")]
    [Tune("Enemy")] [SerializeField] private float waitForUsUnits;
    private bool launched;
    private Vector2[] route;
    private int routeAt, routeWay = 1;
    bool OnRoute => route != null && route.Length > 1;
    private Vector2 home, waypoint, lastKnown;
    private bool started;
    private float startedAt, waypointAt = -1e9f, lastHeardAt = -1e9f, lastFixAt = -1e9f;
    private float health = -1f, nextSalvo;
    private readonly System.Collections.Generic.Dictionary<string, float> nextShot = new System.Collections.Generic.Dictionary<string, float>();

    /// <summary>Step 99: a ship that arrives is told roughly where we are. It starts SEARCHING there.</summary>
    public void Brief(Vector2 where, float now) { Raise(Awareness.Searching); Heard(where, now, false); }

    /// <summary>Write this tick's orders: listen, fire, and steer.</summary>
    public void Think(ShipState ship, ShipState us)
    {
        var at = new Vector2((float)ship.nav.x, (float)ship.nav.y);
        var ours = new Vector2((float)us.nav.x, (float)us.nav.y);
        float now = ship.missionNow, dist = (ours - at).magnitude;
        if (!started) { started = true; home = at; startedAt = now; nextSignal = now + firstSignalSeconds; NewHuntPoint(ship, us, at); }
        if (signalSeconds > 0f) Send(ship, us, at, ours, now);

        // hit since the last tick: its armour or a compartment went down
        float hp = 0f;
        foreach (var a in ship.damage.SideArmour) hp += a;
        foreach (var c in ship.damage.compartments) hp += c.integ;
        bool hit = health >= 0f && hp < health;
        health = hp;

        HearsUs = false;
        if (hunts && !us.destroyed) Listen(ship, us, at, ours, dist, now, hit);
        bool fix = Level == Awareness.Alert && HasFix(now);

        // owner 2026-11-01: a hit magazine can't fire, so the crew go and repair it, by the same rules as ours
        if (!ship.damage.MagOk && !ship.damage.PartyOut) { ship.damage.PickComp("mag"); ship.damage.ApplyOrder("party"); }

        if (firesBack && !us.destroyed && ship.damage.MagOk && now >= ship.empUntil)
        {
            // torpedoes: when hit (Step 55), and every salvoSeconds once ALERT with a fix
            WeaponSystem.Mount vls = null;
            foreach (var m in ship.weapons.stats.mounts)
                if (vls == null && m.kind == "MS" && m.ammo > 0 && dist <= m.rangeUnits && ship.weapons.OnDot(m.id)) vls = m;
            if ((hit || fix) && now >= nextSalvo && vls != null)
            {
                int n = Mathf.Min(fireBackSalvo, vls.ammo);
                Vector2 tube = ship.weapons.HardPointOffset(vls.id, ship.nav.heading);
                us.weapons.LaunchAt(ship.weapons.stats, ship.nav.x + tube.x, ship.nav.y + tube.y, ship.nav.heading, vls.arc, n, 0, vls.rangeUnits, ship.weapons.LineOf(vls.id)); // HE
                vls.ammo -= n;
                nextSalvo = now + (fix ? salvoSeconds : fireBackSeconds);
            }
            if (fix) foreach (var id in ship.weapons.MountIds("RG")) Shoot(ship, us, id, dist, now);
        }

        Steer(ship, us, at, ours, dist, now, fix);
    }

    // Owner 2026-10-29: it sends a radio message every so often, and our comms hears it anywhere on the map. One at a
    // time: the wait for the next starts once the last has been read or lost.
    void Send(ShipState ship, ShipState us, Vector2 at, Vector2 ours, float now)
    {
        if (us.comms.Waiting(now)) { nextSignal = now + signalSeconds; return; }
        if (now < nextSignal) return;
        double speed = System.Math.Sqrt(ship.nav.vx * ship.nav.vx + ship.nav.vy * ship.nav.vy);
        us.comms.Receive(CommsSystem.Report("GARUDA", TacticalWorld.Bearing(at - ours), ship.nav.heading, speed), now + onAirSeconds);
        nextSignal = now + signalSeconds;
        nextCourse = now + signalSeconds / 2f; // owner 2026-11-01: the direction keeps changing. Halfway, so a message's heading is true when it is sent
    }

    // Our signal (0–100) sets how far it hears us: silent, not at all. Within visual range it sees us. A hit
    // tells it roughly where the shot came from. Our fighters are heard by their own loudness, and lead it to them.
    void Listen(ShipState ship, ShipState us, Vector2 at, Vector2 ours, float dist, float now, bool hit)
    {
        var st = ship.sensors.stats;
        int gain = gainByAwareness[Mathf.Clamp((int)Level, 0, gainByAwareness.Length - 1)];
        float reach = st.senRangeBaseUnits + gain * st.senRangePerStepUnits; // its DRADIS at this gain

        if (!world) world = FindAnyObjectByType<TacticalWorld>();
        bool rockOnLine = false, rockToFighter = false;
        float interference = world ? world.Interference(at, ours, out rockOnLine) : 0f; // owner 2026-11-02: rocks: the same rule as our sensors
        // owner 2026-11-01: a gas cloud blocks its eyes too
        float eyes = rockOnLine ? 0f : us.sensors.inCloud ? Mathf.Min(st.visualRangeUnits, us.sensors.stats.cloudSeenUnits) : st.visualRangeUnits;
        if (dist <= eyes)
        {
            Raise(Awareness.Alert);
            Heard(ours, now, true);
            HearsUs = true;
            return;
        }
        float clear = Clarity(dist, Mathf.Max(0f, reach - interference) * us.sensors.signal / 100f);
        HearsUs = clear > 0f;
        if (hit) Raise(Awareness.Alert); // hit, it is alert whether it heard us or not (the play-through: a faint hearing kept it UNEASY)
        if (clear > 0f)
        {
            bool twice = now - lastHeardAt > 20f && now - lastHeardAt <= 60f; // heard, lost, then heard again
            Raise(clear >= alertAt ? Awareness.Alert : clear >= searchingAt || twice ? Awareness.Searching : Awareness.Uneasy);
            Heard(ours + Off(ship, dist * (1f - clear) * 0.25f), now, Level == Awareness.Alert);
        }
        else if (hit) Heard(ours + Off(ship, dist * 0.15f), now, false); // it knows the way the shot came, not the range
        foreach (var f in us.hangar.Flights)
        {
            var p = new Vector2((float)f.x, (float)f.y);
            float c = Clarity((p - at).magnitude, Mathf.Max(0f, reach - (world ? world.Interference(at, p, out rockToFighter) : 0f)) * fighterSignal / 100f);
            if (c <= 0f || clear > 0f) continue; // the ship itself, heard, comes first
            Raise(c >= searchingAt ? Awareness.Searching : Awareness.Uneasy);
            Heard(p, now, false);
        }
    }

    static float Clarity(float dist, float hearUnits) => hearUnits <= 0f || dist > hearUnits ? 0f : 1f - dist / hearUnits;
    void Raise(Awareness to) { if (to > Level) Level = to; }
    void Heard(Vector2 where, float now, bool isFix) { lastKnown = where; lastHeardAt = now; if (isFix) lastFixAt = now; }
    static Vector2 Off(ShipState ship, float km)
    {
        double a = ship.NextDie() * 2 * System.Math.PI, r = km * System.Math.Sqrt(ship.NextDie());
        return new Vector2((float)(System.Math.Sin(a) * r), (float)(System.Math.Cos(a) * r));
    }

    // One turret: in reach, ammunition left, reloaded, and we are inside its arc. It leads us, a little off.
    void Shoot(ShipState ship, ShipState us, string id, float dist, float now)
    {
        var m = ship.weapons.MountOf(id);
        float next;
        nextShot.TryGetValue(id, out next);
        if (m == null || m.ammo <= 0 || now < next || dist > m.rangeUnits || !ship.weapons.OnDot(id)) return;
        double lead = LeadOn(ship, us);
        int rel = ShipState.JsRound(((lead - ship.nav.heading) % 360 + 360) % 360) % 360;
        if (ShipState.AngDiff(rel, m.arc) > m.halfArc) return;
        Vector2 gun = ship.weapons.HardPointOffset(id, ship.nav.heading);
        us.weapons.ShootAt(ship.weapons.stats, ship.nav.x + gun.x, ship.nav.y + gun.y, ship.nav.vx, ship.nav.vy, lead + (ship.NextDie() * 2 - 1) * aimErrorDeg, m.rangeUnits);
        m.ammo--;
        nextShot[id] = now + railgunReloadSeconds + (float)ship.NextDie() * railgunJitterSeconds;
    }

    // Where to aim so its slug meets us, from our position and velocity (WeaponSystem.LeadBearing, from its side). Its slug keeps
    // its own motion (owner 2026-11-08), so it leads on our motion less its own.
    static double LeadOn(ShipState ship, ShipState us)
    {
        double tx = us.nav.x - ship.nav.x, ty = us.nav.y - ship.nav.y, vx = us.nav.vx - ship.nav.vx, vy = us.nav.vy - ship.nav.vy;
        double straight = (System.Math.Atan2(tx, ty) * 180 / System.Math.PI + 360) % 360;
        double s = ship.weapons.stats.slugSpeedUnits, a = vx * vx + vy * vy - s * s, b = 2 * (tx * vx + ty * vy), c = tx * tx + ty * ty;
        double disc = b * b - 4 * a * c;
        if (a >= 0 || disc < 0) return straight;
        double t = (-b - System.Math.Sqrt(disc)) / (2 * a);
        if (t <= 0) return straight;
        return (System.Math.Atan2(tx + vx * t, ty + vy * t) * 180 / System.Math.PI + 360) % 360;
    }

    // Owner 2026-11-10 (MILK RUN): hidden until scanned, then it runs; a broken side and it stops dead
    void Flee(ShipState ship, ShipState us, Vector2 at, Vector2 ours)
    {
        bool open = false;
        foreach (var a in ship.damage.SideArmour) if (a <= 0f) open = true;
        if (ownKey == null) ownKey = GetComponent<WorldContact>().trackKey;
        if (open || !us.sensors.identified.Contains(ownKey)) { ship.nav.throttle = 0; return; }
        int heading = TacticalWorld.Bearing(at - ours);
        if (!world) world = FindAnyObjectByType<TacticalWorld>();
        heading = world ? world.ClearHeading(ship.nav, heading, avoidLookUnits) : heading;
        ship.nav.orderedHeading = KeepInside(us, at, heading);
        ship.nav.throttle = Mathf.Clamp(Mathf.RoundToInt(fleeSpeedShare * 100f), 0, ship.nav.ThrottleCap);
    }

    void Steer(ShipState ship, ShipState us, Vector2 at, Vector2 ours, float dist, float now, bool fix)
    {
        if (escorts) { Escort(ship, us, at, ours, now); return; }
        if (flees) { Flee(ship, us, at, ours); return; }
        if (!launched) { if (waitForUsUnits > 0f && dist > waitForUsUnits) { ship.nav.throttle = 0; return; } launched = true; }
        if (route == null && patrolRoute != null && patrolRoute.Length > 1) { route = patrolRoute; BackToRoute(at); }
        if (!hunts && cruiseUnits <= 0f) { ship.nav.throttle = 0; return; } // Step 130: a level's still target stays as it spawned, the side it shows included
        float top = (float)ship.nav.stats.topSpeedUnits, speed = cruiseUnits;
        if (Level > Awareness.Cold && now - lastHeardAt > giveUpSeconds)
        {
            Level = Awareness.Cold; fix = false; waypointAt = now;
            if (OnRoute) BackToRoute(at); else NewHuntPoint(ship, us, at);
        }
        int heading;
        if (fix)
        {
            // close to its railguns' reach, then put a turret's arc on us: heading = bearing to us minus the arc's centre
            int brg = TacticalWorld.Bearing(ours - at);
            if (dist > fightAtShare * Reach(ship)) { heading = brg; speed = top; }
            else { heading = SideOn(ship, brg); speed = top * fightSpeedShare; }
        }
        else
        {
            bool newNews = Level >= Awareness.Uneasy && lastHeardAt > waypointAt;
            bool newCourse = !OnRoute && Level == Awareness.Cold && nextCourse > 0f && now >= nextCourse;
            if (newNews) { waypoint = lastKnown; waypointAt = now; }
            else if ((waypoint - at).magnitude <= arriveUnits || newCourse)
            {
                if (Level >= Awareness.Searching) waypoint = lastKnown + Off(ship, searchUnits);
                else NewHuntPoint(ship, us, at);
                waypointAt = now;
                nextCourse = 0f;
            }
            waypoint = Inside(us, waypoint); // owner 2026-11-02: never a point past the edge band (where we heard us, a search point, a route point)
            heading = TacticalWorld.Bearing(waypoint - at);
            speed = Level == Awareness.Alert ? top : Level == Awareness.Searching ? top * 0.8f : cruiseUnits;
        }
        // owner 2026-11-01: rocks and ships are solid: it steers round them
        if (!world) world = FindAnyObjectByType<TacticalWorld>();
        heading = world ? world.ClearHeading(ship.nav, heading, avoidLookUnits) : heading;
        ship.nav.orderedHeading = KeepInside(us, at, heading);
        // a damaged engine room holds it back, as our lever is; wrecked, it drifts (DISABLE, BOARDING_README)
        ship.nav.throttle = Mathf.Clamp(Mathf.RoundToInt(speed / top * 100f), 0, ship.nav.ThrottleCap);
    }

    // Owner 2026-11-09: our battle group goes its own way and we follow them ("They pursue their path, and you have to sort
    // of follow them"). The formation's centre, which starts on our ship, flies the level's group route at the group's speed
    // and waits at its last point; each ship flies to its place round that centre, turned the way the group flies.
    void Escort(ShipState ship, ShipState us, Vector2 at, Vector2 ours, float now)
    {
        if (groupAt < 0f) { groupAt = now; groupHeading = ship.nav.heading; group = at - Turn(slotUnits, groupHeading); } // it starts in its place
        if (!lookedForMission) { lookedForMission = true; mission = FindAnyObjectByType<Mission>(); }
        var route = mission ? mission.groupRoute : null;
        float dt = Mathf.Max(0f, now - groupAt), moved = 0f;
        groupAt = now;
        if (!joined && (ours - group).magnitude <= joinUnits) joined = true; // owner 2026-11-10: we catch up with them, then they set off
        if (joined && route != null && leg < route.Length)
        {
            Vector2 to = route[leg] - group;
            moved = Mathf.Min(to.magnitude, groupSpeedUnits * dt);
            if (to.magnitude > 1f) { group += to / to.magnitude * moved; groupHeading = Mathf.MoveTowardsAngle(groupHeading, TacticalWorld.Bearing(to), groupTurnDeg * dt); }
            else leg++;
        }
        // Owner 2026-11-10: the ships fly nose forward with the group, never drifting about. Each steers at a point ahead of its
        // place while the group moves; it speeds up when behind its place and eases off when ahead (it used to speed up either
        // way, overshoot and turn back, and drifted round a moving sway point: the wobble).
        Vector2 dir = Turn(Vector2.up, groupHeading), place = group + Turn(slotUnits, groupHeading);
        float top = (float)ship.nav.stats.topSpeedUnits, off = (place - at).magnitude, behind = Vector2.Dot(place - at, dir);
        float pace = dt > 0f ? moved / dt : 0f;
        bool flying = pace > 0f && off < rejoinUnits;
        float speed = Mathf.Clamp(pace + catchUpPerUnit * (flying ? behind : off), 0f, top);
        int heading;
        if (flying)
        {
            // while the group flies it keeps near the group's course: it closes on its place sideways by turning at most
            // Most Off Course Deg (the inside ship of a turn used to wheel hard round, a playtest 2026-11-10)
            float sideways = Vector2.Dot(place - at, Turn(Vector2.right, groupHeading));
            heading = Heading(groupHeading + Mathf.Clamp(Mathf.Atan2(sideways, leadUnits) * Mathf.Rad2Deg, -mostOffCourseDeg, mostOffCourseDeg));
        }
        else heading = off < inPlaceUnits ? Heading(groupHeading) : TacticalWorld.Bearing(place - at);
        // our ship is in its way: it goes round us, on the side it is already on; too close, it makes room first
        // (a playtest: Indra crossed our bow to its new place as we turned, and the two boxes pinned each other)
        Vector2 way = off > 0f ? (place - at) / off : Vector2.zero, toUs = ours - at;
        float along = Vector2.Dot(toUs, way), across = way.x * toUs.y - way.y * toUs.x; // across > 0: we are to its left
        // owner 2026-11-10: too close, it keeps its course (turning away and back made the corvette swing about): it drops back
        // when we are ahead of it, and goes on ahead when we are behind it
        if (toUs.magnitude < keepClearUnits) { heading = Heading(groupHeading); speed = Vector2.Dot(toUs, dir) > 0f ? 0f : Mathf.Max(speed, top * makeRoomShare); }
        else if (along > 0f && along < off && Mathf.Abs(across) < keepClearUnits)
            // it passes us on its own side of the formation (choosing by where we were flipped it side to side when we were dead ahead)
            heading = TacticalWorld.Bearing(ours + new Vector2(way.y, -way.x) * (slotUnits.x < 0f ? -1f : 1f) * keepClearUnits - at);
        if (!world) world = FindAnyObjectByType<TacticalWorld>();
        heading = world ? world.ClearHeading(ship.nav, heading, avoidLookUnits, us.nav) : heading;
        ship.nav.orderedHeading = KeepInside(us, at, heading);
        ship.nav.throttle = Mathf.Clamp(Mathf.RoundToInt(speed / top * 100f), 0, ship.nav.ThrottleCap);
    }

    // v turned to a heading (0 north, clockwise): x to the right of it, y ahead
    static Vector2 Turn(Vector2 v, float heading)
    {
        float rad = heading * Mathf.Deg2Rad, sin = Mathf.Sin(rad), cos = Mathf.Cos(rad);
        return new Vector2(v.x * cos + v.y * sin, v.y * cos - v.x * sin);
    }
    static int Heading(float deg) => (Mathf.RoundToInt(deg) % 360 + 360) % 360;

    // Owner 2026-11-02: it can't leave the battle space either. Its edge band is our edge warning band (arenaWarnUnits):
    // in the band it drops the outward part of its heading and runs along the edge, even chasing us; past the edge it
    // heads straight for the middle. The board's band is 1,000 units; at full speed, at worst, the Garuda drifts about
    // 520 units into it while it turns, a frigate about 750 (worked out from Garuda.asset and Frigate.asset).
    static int KeepInside(ShipState us, Vector2 at, int heading)
    {
        var edge = Inside(us, at);
        if (edge == at) return heading;
        if (System.Math.Abs(at.x) > us.ArenaHalfW || System.Math.Abs(at.y) > us.ArenaHalfH) return TacticalWorld.Bearing(-at);
        float rad = heading * Mathf.Deg2Rad;
        var way = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
        if (edge.x != at.x && way.x * at.x > 0f) way.x = 0f;
        if (edge.y != at.y && way.y * at.y > 0f) way.y = 0f;
        return way.sqrMagnitude < 0.01f ? TacticalWorld.Bearing(-at) : TacticalWorld.Bearing(way); // straight out, or into a corner: back to the middle
    }

    // The nearest point to p inside the battle space less its edge band
    static Vector2 Inside(ShipState us, Vector2 p)
    {
        float hw = Mathf.Max(0f, (float)(us.ArenaHalfW - us.arenaWarnUnits)), hh = Mathf.Max(0f, (float)(us.ArenaHalfH - us.arenaWarnUnits));
        return new Vector2(Mathf.Clamp(p.x, -hw, hw), Mathf.Clamp(p.y, -hh, hh));
    }

    // Point to point, back at each end, and it may turn back at any point.
    void NextOnRoute(ShipState ship)
    {
        if (ship.NextDie() < turnBackChance) routeWay = -routeWay;
        if (routeAt + routeWay < 0 || routeAt + routeWay >= route.Length) routeWay = -routeWay;
        routeAt += routeWay;
        waypoint = route[routeAt];
    }

    void BackToRoute(Vector2 at)
    {
        for (int i = 0; i < route.Length; i++) if ((route[i] - at).sqrMagnitude < (route[routeAt] - at).sqrMagnitude) routeAt = i;
        waypoint = route[routeAt];
    }

    static float Reach(ShipState ship)
    {
        float r = 0f;
        foreach (var id in ship.weapons.MountIds("RG")) { var m = ship.weapons.MountOf(id); if (m != null && m.ammo > 0) r = Mathf.Max(r, m.rangeUnits); }
        return r > 0f ? r : 10000f;
    }

    // The turret whose arc it can bring onto us with the smaller turn
    static int SideOn(ShipState ship, int brg)
    {
        int best = brg, turn = 999;
        foreach (var id in ship.weapons.MountIds("RG"))
        {
            var m = ship.weapons.MountOf(id);
            if (m == null || m.ammo <= 0) continue;
            int h = ((brg - m.arc) % 360 + 360) % 360, t = ShipState.AngDiff(h, ship.nav.heading);
            if (t < turn) { turn = t; best = h; }
        }
        return best;
    }

    // COLD: a point across the battle space, at least legMinUnits away. Its band drifts from its own half (where it
    // started) toward ours over driftSeconds.
    void NewHuntPoint(ShipState ship, ShipState us, Vector2 at)
    {
        if (OnRoute) { NextOnRoute(ship); return; }
        float hw = (float)us.ArenaHalfW * 0.85f, hh = (float)us.ArenaHalfH * 0.85f;
        float drift = driftSeconds > 0f ? Mathf.Clamp01((ship.missionNow - startedAt) / driftSeconds) : 1f;
        float side = home.y >= 0f ? 1f : -1f;
        float centre = Mathf.Lerp(side * hh * 0.5f, -side * hh * 0.5f, drift);
        for (int i = 0; i < 8; i++)
        {
            waypoint = new Vector2((float)(ship.NextDie() * 2 - 1) * hw, Mathf.Clamp(centre + (float)(ship.NextDie() * 2 - 1) * hh * 0.5f, -hh, hh));
            if ((waypoint - at).magnitude >= legMinUnits) break;
        }
    }
}
