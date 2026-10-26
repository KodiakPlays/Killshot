using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [Tooltip("Drop board tracks that no world contact drives (owner 2026-10-17: the mock-up contacts go).")]
    [SerializeField] private bool dropTracksWithoutContact = true;
    [Tooltip("The level loaded on top of the board at play: a scene in Assets/Scenes/Levels, by name. If a level is already open in the editor, that one plays instead.")]
    [SerializeField] private string levelScene = "Level_01";

    /// <summary>Where the level scenes live (owner 2026-10-25: each level is its own scene).</summary>
    public const string LevelFolder = "Assets/Scenes/Levels";

    private WorldContact[] contacts = new WorldContact[0];
    private EnemyBrain[] brains = new EnemyBrain[0];

    private void Awake()
    {
        if (!board) board = FindAnyObjectByType<HandoffBoardControls>();
        if (!board) { Debug.LogError("[TacticalWorld] no HandoffBoardControls in the scene"); enabled = false; return; }
        contacts = GetComponentsInChildren<WorldContact>();
        brains = System.Array.ConvertAll(contacts, c => c.GetComponent<EnemyBrain>());
        foreach (var c in contacts) StartShip(c);
        if (dropTracksWithoutContact)
        {
            var keys = new HashSet<string>();
            foreach (var c in contacts) keys.Add(c.trackKey);
            board.State.KeepTracks(keys);
        }
        // our ship starts where its model is; from here on its NavSystem moves it (Step 41)
        var us = board.State.nav;
        if (us) { us.x = us.transform.position.x; us.y = us.transform.position.y; us.PlaceModel(); }
        // Step 49: each contact's ship takes our hits; its damage numbers follow ours from the tuning window
        board.State.ShipOf = key =>
        {
            foreach (var c in contacts)
            {
                if (!c || c.trackKey != key) continue;
                var ship = c.Ship;
                ship.damage.CopyDamageTuning(board.State.damage);
                return ship;
            }
            return null;
        };
        Sync(); // before the board's first refresh (its Start), so it never shows mock numbers
        board.Ticked += OnTick;
        // the level arrives a frame later; the board starts paused, so nothing has moved by then
        if (!string.IsNullOrEmpty(levelScene) && !LevelOpen()) SceneManager.LoadSceneAsync(levelScene, LoadSceneMode.Additive);
    }

    static bool LevelOpen()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).path.StartsWith(LevelFolder + "/")) return true;
        return false;
    }

    // Step 51: a contact's ship starts where its model is, facing its way, engines powered
    static void StartShip(WorldContact c)
    {
        var ship = c.Ship;
        ship.nav.PlaceAt(c.transform.position.x, c.transform.position.y, HandoffBoardState.JsRound(((-c.transform.eulerAngles.z) % 360 + 360) % 360) % 360);
        ship.power.PowerEngines();
    }

    /// <summary>A level's spawn point (EnemySpawn): this contact's ship starts there instead, facing
    /// the spawn's way. Its patrol route goes with it.</summary>
    public void Spawn(string key, Vector3 at, float zDeg)
    {
        foreach (var c in contacts)
        {
            if (!c || c.trackKey != key) continue;
            c.transform.SetPositionAndRotation(new Vector3(at.x, at.y, c.transform.position.z), Quaternion.Euler(0f, 0f, zDeg));
            StartShip(c);
        }
        Sync();
    }

    /// <summary>Step 73: a level's Player Spawn (PlayerSpawn). The player's ship starts there, facing the
    /// box's way, and the plot opens on it.</summary>
    public void SpawnPlayer(Vector3 at, float zDeg)
    {
        var us = board.State.nav;
        if (!us) return;
        us.PlaceAt(at.x, at.y, HandoffBoardState.JsRound(((-zDeg) % 360 + 360) % 360) % 360);
        var plot = FindAnyObjectByType<HandoffPlotPanel>();
        if (plot) plot.CentreOnOurShip();
        Sync();
    }

    private void OnDestroy() { if (board) board.Ticked -= OnTick; }

    // One board tick: each contact with a brain takes its orders and moves (the same maths as our ship),
    // then everything is placed and measured. Paused, no tick, so nothing moves.
    void OnTick()
    {
        for (int i = 0; i < contacts.Length; i++)
        {
            var c = contacts[i];
            var brain = brains[i];
            if (!c) continue;
            var ship = c.Ship;
            ship.missionNow = board.State.missionNow;
            // EMP (Step 53): power is out until it wears off; then its engines come back
            if (ship.empUntil > 0f && ship.missionNow >= ship.empUntil) { ship.empUntil = 0f; ship.power.PowerEngines(); }
            ship.damage.CopyDamageTuning(board.State.damage);
            ship.damage.TickConditions(board.Rate); // Step 59: its fires, vacuum and radiation, held still or not
            if (!brain || !brain.enabled) continue;
            if (ship.destroyed || ship.captured) ship.nav.throttle = 0; // a wreck, or a prize our marines hold (Step 60), makes no way of its own
            else brain.Think(ship, board.State);
            ship.nav.Tick(board.Rate); // it steers, moves, and puts its model where it is
        }
        Sync();
    }

    /// <summary>Write each contact's true bearing and range from our ship into its board track.
    /// (Each ship's NavSystem puts its own model in place.)</summary>
    public void Sync()
    {
        var s = board.State;
        foreach (var c in contacts)
        {
            if (!c) continue;
            var t = board.State.TrackOf(c.trackKey);
            if (t == null) continue;
            t.hasWorld = true;
            // its true spot from navigation, not the model: the model glides ahead between ticks (Step 75)
            var nav = c.Ship.nav;
            t.nav = nav;
            t.worldX = nav.x;
            t.worldY = nav.y;
            t.worldHdg = nav.heading; // its bow, clockwise from north (Step 48)
            t.speed = (float)c.Ship.nav.SpeedKms; // its real speed (Step 51); TRACK reads it
            t.course = c.Ship.nav.CourseDeg;       // and which way it is really going (Step 52)
            var d = new Vector2((float)(t.worldX - s.nav.x), (float)(t.worldY - s.nav.y));
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
