using UnityEngine;

/// <summary>
/// Step 51 (owner 2026-10-23). The first enemy brain (FTUE_PLAN §4.5): it only writes orders — an
/// ordered heading and a throttle — into its ship's own HandoffBoardState. The ship turns and moves with
/// the same maths as ours (TickHelm, TickMove; TacticalWorld runs them each board tick). For now it
/// patrols a loop of waypoints at a cruising speed. Step 55: when we hit it, it fires a missile salvo
/// back at us. Hearing and hunting come in later steps.
/// </summary>
[RequireComponent(typeof(WorldContact))]
public class EnemyBrain : MonoBehaviour
{
    [Tooltip("The patrol loop, km from where the ship starts (x east, y north). It sails from point to point and round again.")]
    [Tune("Enemy")] [SerializeField] private Vector2[] route = { new Vector2(0, 0), new Vector2(-6000, -4000), new Vector2(0, -8000), new Vector2(6000, -4000) };
    [Tooltip("Cruising speed, km/s (the design's TRACK 07: 1.10; Step 63: x5).")]
    [Tune("Enemy")] [SerializeField] private float cruiseKms = 5.5f;
    [Tooltip("A waypoint counts as reached inside this distance, km.")]
    [Tune("Enemy")] [SerializeField] private float arriveKm = 300f;

    [Header("Firing back (Step 55, owner 2026-10-24)")]
    [Tooltip("When we hit it and we are inside its launcher's range, it fires a missile salvo back at us.")]
    [Tune("Enemy")] public bool firesBack = true;
    [Tooltip("Missiles in its salvo (HE).")]
    [Tune("Enemy")] [SerializeField] private int fireBackSalvo = 2;
    [Tooltip("At most one salvo this often, mission seconds, however often it is hit.")]
    [Tune("Enemy")] [SerializeField] private float fireBackSeconds = 120f;

    private Vector2 home;
    private int leg = 1;
    private bool started;
    private float health = -1f, nextSalvo;

    /// <summary>Where it is heading now, world km.</summary>
    public Vector2 Waypoint => home + route[leg % route.Length];

    /// <summary>Write this tick's orders: fire back if we hit it, and head for the current waypoint at
    /// cruising speed.</summary>
    public void Think(HandoffBoardState ship, HandoffBoardState us)
    {
        // hit since the last tick: its armour or a compartment went down
        float now = 0f;
        foreach (var a in ship.SideArmour) now += a;
        foreach (var c in ship.compartments) now += c.integ;
        bool hit = health >= 0f && now < health;
        health = now;
        double dx = us.ownX - ship.ownX, dy = us.ownY - ship.ownY;
        var vls = ship.MountOf("VLS");
        if (firesBack && hit && ship.missionNow >= nextSalvo && ship.vlsCells > 0 && vls != null && dx * dx + dy * dy <= (double)vls.rangeKm * vls.rangeKm)
        {
            int n = Mathf.Min(fireBackSalvo, ship.vlsCells);
            us.LaunchAt(ship.ownX, ship.ownY, n, 0, vls.rangeKm); // HE
            ship.vlsCells -= n;
            nextSalvo = ship.missionNow + fireBackSeconds;
        }

        if (route == null || route.Length < 2) { ship.thr = 0; return; }
        var at = new Vector2((float)ship.ownX, (float)ship.ownY);
        if (!started) { home = at; started = true; }
        if ((Waypoint - at).magnitude <= arriveKm) leg = (leg + 1) % route.Length;
        ship.ordHdg = TacticalWorld.Bearing(Waypoint - at);
        ship.thr = Mathf.Clamp(Mathf.RoundToInt(cruiseKms / (float)ship.topSpeedKms * 100f), 0, 100);
    }
}
