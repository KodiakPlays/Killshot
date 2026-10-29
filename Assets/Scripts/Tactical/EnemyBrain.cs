using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// The enemy's mind (Step 51; Wolf Hunt, Step 97, owner 2026-10-27). It only writes orders into its own
/// ship's ShipState (an ordered heading and a throttle) and fires by its own ship's weapon numbers. The ship
/// turns and moves with the same maths as ours (its NavSystem; TacticalWorld ticks it each board tick).
///
/// How aware it is (FTUE_PLAN §4.2) only goes up: COLD, UNEASY, SEARCHING, ALERT.
/// - Hearing (owner 2026-10-27): it hears us out to its DRADIS reach times our signal (0–100). Silent, we
///   are not heard. Within its visual range it sees us, whatever our signal. Our fighters are loud too.
/// - COLD: it hunts the map: points across the battle space, drifting from its half to ours.
/// - UNEASY: heard faintly. It heads for where it heard us.
/// - SEARCHING: heard clearly, or twice in a minute. It goes faster, then searches round that spot.
/// - ALERT: heard loud and close, seen, or hit. QuestSystem starts the reinforcements' countdown. It closes
///   to its railguns' reach, turns a side to us, and fires railguns and missile salvos while it has a fix.
/// </summary>
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
    [Tooltip("Side-on, its throttle as a share of its top speed.")]
    [Tune("Enemy")] [SerializeField] private float fightSpeedShare = 0.4f;
    [Tooltip("Game seconds between shots from one turret, plus up to the jitter below.")]
    [Tune("Enemy")] [SerializeField] private float railgunReloadSeconds = 12f;
    [Tune("Enemy")] [SerializeField] private float railgunJitterSeconds = 5f;
    [Tooltip("Its aim is off by up to this, degrees, either way: a long shot misses more often.")]
    [Tune("Enemy")] [SerializeField] private float aimErrorDeg = 0.4f;
    [Tooltip("ALERT with a fix: game seconds between missile salvos.")]
    [Tune("Enemy")] [SerializeField] private float salvoSeconds = 90f;

    [Header("Firing back (Step 55, owner 2026-10-24)")]
    [Tooltip("It fires at all: railguns and missiles. Off, it holds its fire (a playtest stand-in).")]
    [Tune("Enemy")] public bool firesBack = true;
    [Tooltip("Missiles in a salvo (HE).")]
    [Tune("Enemy")] [SerializeField] private int fireBackSalvo = 2;
    [Tooltip("When we hit it before it is ALERT: at most one salvo this often, game seconds.")]
    [Tune("Enemy")] [SerializeField] private float fireBackSeconds = 120f;

    /// <summary>How aware it is of us. It never goes down.</summary>
    public Awareness Level { get; private set; }
    /// <summary>Its last thought heard or saw our ship (not our fighters): our sensors' "HOSTILE SET HAS YOUR RANGE".</summary>
    public bool HearsUs { get; private set; }
    /// <summary>Where it is heading now, world units.</summary>
    public Vector2 Waypoint => waypoint;
    /// <summary>Where it last heard or saw us (or our fighters), world units; and when, game seconds.</summary>
    public Vector2 LastKnown => lastKnown;
    public float LastHeardAt => lastHeardAt;
    /// <summary>It has a good fix on us: it fires.</summary>
    public bool HasFix(float now) => now - lastFixAt <= fixSeconds;

    private Vector2 home, waypoint, lastKnown;
    private bool started;
    private float startedAt, waypointAt = -1e9f, lastHeardAt = -1e9f, lastFixAt = -1e9f;
    private float health = -1f, nextSalvo, nextShotA, nextShotB;

    /// <summary>Step 99: a ship that arrives is told roughly where we are. It starts SEARCHING there.</summary>
    public void Brief(Vector2 where, float now) { Raise(Awareness.Searching); Heard(where, now, false); }

    /// <summary>Write this tick's orders: listen, fire, and steer.</summary>
    public void Think(ShipState ship, ShipState us)
    {
        var at = new Vector2((float)ship.nav.x, (float)ship.nav.y);
        var ours = new Vector2((float)us.nav.x, (float)us.nav.y);
        float now = ship.missionNow, dist = (ours - at).magnitude;
        if (!started) { started = true; home = at; startedAt = now; NewHuntPoint(ship, us, at); }

        // hit since the last tick: its armour or a compartment went down
        float hp = 0f;
        foreach (var a in ship.damage.SideArmour) hp += a;
        foreach (var c in ship.damage.compartments) hp += c.integ;
        bool hit = health >= 0f && hp < health;
        health = hp;

        HearsUs = false;
        if (hunts && !us.destroyed) Listen(ship, us, at, ours, dist, now, hit);
        bool fix = Level == Awareness.Alert && HasFix(now);

        if (firesBack && !us.destroyed && ship.damage.MagOk && now >= ship.empUntil)
        {
            // missiles: when hit (Step 55), and every salvoSeconds once ALERT with a fix
            var vls = ship.weapons.MountOf("VLS");
            if ((hit || fix) && now >= nextSalvo && vls != null && vls.ammo > 0 && dist <= vls.rangeUnits)
            {
                int n = Mathf.Min(fireBackSalvo, vls.ammo);
                us.weapons.LaunchAt(ship.weapons.stats, ship.nav.x, ship.nav.y, n, 0, vls.rangeUnits); // HE
                vls.ammo -= n;
                nextSalvo = now + (fix ? salvoSeconds : fireBackSeconds);
            }
            if (fix)
            {
                Shoot(ship, us, "RGA", ref nextShotA, dist, now);
                Shoot(ship, us, "RGB", ref nextShotB, dist, now);
            }
        }

        Steer(ship, us, at, ours, dist, now, fix);
    }

    // Our signal (0–100) sets how far it hears us: silent, not at all. Within visual range it sees us. A hit
    // tells it roughly where the shot came from. Our fighters are heard by their own loudness, and lead it to them.
    void Listen(ShipState ship, ShipState us, Vector2 at, Vector2 ours, float dist, float now, bool hit)
    {
        var st = ship.sensors.stats;
        int gain = gainByAwareness[Mathf.Clamp((int)Level, 0, gainByAwareness.Length - 1)];
        float reach = st.senRangeBaseUnits + gain * st.senRangePerStepUnits; // its DRADIS at this gain

        if (dist <= st.visualRangeUnits)
        {
            Raise(Awareness.Alert);
            Heard(ours, now, true);
            HearsUs = true;
            return;
        }
        float clear = Clarity(dist, reach * us.sensors.signal / 100f);
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
            float c = Clarity((p - at).magnitude, reach * fighterSignal / 100f);
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
    void Shoot(ShipState ship, ShipState us, string id, ref float next, float dist, float now)
    {
        var m = ship.weapons.MountOf(id);
        if (m == null || m.ammo <= 0 || now < next || dist > m.rangeUnits) return;
        double lead = LeadOn(ship, us);
        int rel = ShipState.JsRound(((lead - ship.nav.heading) % 360 + 360) % 360) % 360;
        if (ShipState.AngDiff(rel, m.arc) > m.halfArc) return;
        us.weapons.ShootAt(ship.weapons.stats, ship.nav.x, ship.nav.y, lead + (ship.NextDie() * 2 - 1) * aimErrorDeg, m.rangeUnits);
        m.ammo--;
        next = now + railgunReloadSeconds + (float)ship.NextDie() * railgunJitterSeconds;
    }

    // Where to aim so its slug meets us, from our position and velocity (WeaponSystem.LeadBearing, from its side).
    static double LeadOn(ShipState ship, ShipState us)
    {
        double tx = us.nav.x - ship.nav.x, ty = us.nav.y - ship.nav.y, vx = us.nav.vx, vy = us.nav.vy;
        double straight = (System.Math.Atan2(tx, ty) * 180 / System.Math.PI + 360) % 360;
        double s = ship.weapons.stats.slugSpeedUnits, a = vx * vx + vy * vy - s * s, b = 2 * (tx * vx + ty * vy), c = tx * tx + ty * ty;
        double disc = b * b - 4 * a * c;
        if (a >= 0 || disc < 0) return straight;
        double t = (-b - System.Math.Sqrt(disc)) / (2 * a);
        if (t <= 0) return straight;
        return (System.Math.Atan2(tx + vx * t, ty + vy * t) * 180 / System.Math.PI + 360) % 360;
    }

    void Steer(ShipState ship, ShipState us, Vector2 at, Vector2 ours, float dist, float now, bool fix)
    {
        float top = (float)ship.nav.stats.topSpeedUnits, speed = cruiseUnits;
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
            if (newNews) { waypoint = lastKnown; waypointAt = now; }
            else if ((waypoint - at).magnitude <= arriveUnits)
            {
                if (Level >= Awareness.Searching) waypoint = lastKnown + Off(ship, searchUnits);
                else NewHuntPoint(ship, us, at);
                waypointAt = now;
            }
            heading = TacticalWorld.Bearing(waypoint - at);
            speed = Level == Awareness.Alert ? top : Level == Awareness.Searching ? top * 0.8f : cruiseUnits;
        }
        ship.nav.orderedHeading = heading;
        // a damaged engine room holds it back, as our lever is; wrecked, it drifts (DISABLE, BOARDING_README)
        ship.nav.throttle = Mathf.Clamp(Mathf.RoundToInt(speed / top * 100f), 0, ship.nav.ThrottleCap);
    }

    static float Reach(ShipState ship)
    {
        float r = 0f;
        foreach (var id in new[] { "RGA", "RGB" }) { var m = ship.weapons.MountOf(id); if (m != null && m.ammo > 0) r = Mathf.Max(r, m.rangeUnits); }
        return r > 0f ? r : 10000f;
    }

    // The turret whose arc it can bring onto us with the smaller turn
    static int SideOn(ShipState ship, int brg)
    {
        int best = brg, turn = 999;
        foreach (var id in new[] { "RGA", "RGB" })
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
