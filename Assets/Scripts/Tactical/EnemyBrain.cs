using UnityEngine;

/// <summary>
/// Step 51 (owner 2026-10-23). The first enemy brain (FTUE_PLAN §4.5): it only writes orders — an
/// ordered heading and a throttle — into its ship's own HandoffBoardState. The ship turns and moves with
/// the same maths as ours (TickHelm, TickMove; TacticalWorld runs them each board tick). For now it
/// patrols a loop of waypoints at a cruising speed and does not react to us; hearing, hunting and
/// shooting back come in later steps.
/// </summary>
[RequireComponent(typeof(WorldContact))]
public class EnemyBrain : MonoBehaviour
{
    [Tooltip("The patrol loop, km from where the ship starts (x east, y north). It sails from point to point and round again.")]
    [Tune("Enemy")] [SerializeField] private Vector2[] route = { new Vector2(0, 0), new Vector2(-6000, -4000), new Vector2(0, -8000), new Vector2(6000, -4000) };
    [Tooltip("Cruising speed, km/s (the design's TRACK 07: 1.10).")]
    [Tune("Enemy")] [SerializeField] private float cruiseKms = 1.1f;
    [Tooltip("A waypoint counts as reached inside this distance, km.")]
    [Tune("Enemy")] [SerializeField] private float arriveKm = 300f;

    private Vector2 home;
    private int leg = 1;
    private bool started;

    /// <summary>Where it is heading now, world km.</summary>
    public Vector2 Waypoint => home + route[leg % route.Length];

    /// <summary>Write this tick's orders: head for the current waypoint at cruising speed.</summary>
    public void Think(HandoffBoardState ship)
    {
        if (route == null || route.Length < 2) { ship.thr = 0; return; }
        var at = new Vector2((float)ship.ownX, (float)ship.ownY);
        if (!started) { home = at; started = true; }
        if ((Waypoint - at).magnitude <= arriveKm) leg = (leg + 1) % route.Length;
        ship.ordHdg = TacticalWorld.Bearing(Waypoint - at);
        ship.thr = Mathf.Clamp(Mathf.RoundToInt(cruiseKms / (float)ship.topSpeedKms * 100f), 0, 100);
    }
}
