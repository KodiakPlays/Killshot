using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Step 34. The real world behind the tactical board: our ship and the contacts, in km
/// (1 unit = 1 km, x east, y north). Once per board tick it writes each contact's true bearing and
/// range into the board's track list, so every board rule reads real numbers. It never runs per
/// frame; it steps inside the board's single timer (UIStateSpec "One timer").
/// Owner decision 2026-10-17: a new world feeds the board; the old Radar / EnemyShip / PlayerShip
/// scripts are not used.
/// </summary>
public class TacticalWorld : MonoBehaviour
{
    [SerializeField] private HandoffBoardControls board;
    [SerializeField] private Transform ownShip;
    [Tooltip("Drop board tracks that no world contact drives (owner 2026-10-17: the mock-up contacts go).")]
    [SerializeField] private bool dropTracksWithoutContact = true;

    private WorldContact[] contacts = new WorldContact[0];

    private void Awake()
    {
        if (!board) board = FindAnyObjectByType<HandoffBoardControls>();
        if (!board) { Debug.LogError("[TacticalWorld] no HandoffBoardControls in the scene"); enabled = false; return; }
        contacts = GetComponentsInChildren<WorldContact>();
        if (dropTracksWithoutContact)
        {
            var keys = new HashSet<string>();
            foreach (var c in contacts) keys.Add(c.trackKey);
            board.State.KeepTracks(keys);
        }
        Sync(); // before the board's first refresh (its Start), so it never shows mock numbers
        board.Ticked += Sync;
    }

    private void OnDestroy() { if (board) board.Ticked -= Sync; }

    /// <summary>Write each contact's true bearing and range from our ship into its board track.</summary>
    public void Sync()
    {
        Vector2 own = ownShip ? (Vector2)ownShip.position : Vector2.zero;
        board.State.ownX = own.x; // the plot keeps marks in world km (Step 36)
        board.State.ownY = own.y;
        // the 3D ship faces our heading (clockwise from north; about +z in the x/y plane that is −hdg)
        if (ownShip) ownShip.rotation = Quaternion.Euler(0f, 0f, -board.State.hdg);
        foreach (var c in contacts)
        {
            if (!c) continue;
            var t = board.State.TrackOf(c.trackKey);
            if (t == null) continue;
            Vector2 d = (Vector2)c.transform.position - own;
            t.hasWorld = true;
            t.worldX = c.transform.position.x;
            t.worldY = c.transform.position.y;
            t.bearing = Bearing(d);
            t.rangeKm = HandoffBoardState.JsRound(d.magnitude); // the board's tracks hold whole km
        }
    }

    /// <summary>True bearing of a world offset: 0 = north (+y), clockwise, whole degrees 0–359.</summary>
    public static int Bearing(Vector2 d)
    {
        double deg = System.Math.Atan2(d.x, d.y) * 180.0 / System.Math.PI;
        deg = (deg % 360 + 360) % 360;
        return HandoffBoardState.JsRound(deg) % 360;
    }
}
