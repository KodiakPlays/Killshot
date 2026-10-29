using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Step 34. The real world behind the tactical board: our ship and the contacts, in units
/// (1 unit = 1 units, x east, y north). Once per board tick it writes each contact's true bearing and
/// range into the board's track list, so every board rule reads real numbers. It never runs per
/// frame; it steps inside the board's single timer (UIStateSpec "One timer").
/// Owner decision 2026-10-17: a new world feeds the board; the old Radar / EnemyShip / PlayerShip
/// scripts are not used.
/// </summary>
public class TacticalWorld : MonoBehaviour
{
    [SerializeField] private BoardManager board;
    [Tooltip("Drop board tracks that no world contact drives (owner 2026-10-17: the mock-up contacts go).")]
    [SerializeField] private bool dropTracksWithoutContact = true;
    [Tooltip("The level loaded on top of the board at play: a scene in Assets/Scenes/Levels, by name. If a level is already open in the editor, that one plays instead.")]
    [SerializeField] private string levelScene = "Level_01";

    /// <summary>Where the level scenes live (owner 2026-10-25: each level is its own scene).</summary>
    public const string LevelFolder = "Assets/Scenes/Levels";
    /// <summary>The level picked on the level list (owner 2026-10-29). It plays instead of levelScene, and again on RESTART.</summary>
    public static string chosenLevel;

    private WorldContact[] contacts = new WorldContact[0];
    private EnemyBrain[] brains = new EnemyBrain[0];

    // Ruins (owner 2026-10-27): a piece of the level's terrain shows only while it is in range of our ship
    // (its centre within range plus half its size): inside the DRADIS circle while DRADIS listens, else
    // within visual range. Out of range it hides again.
    // Our ship running into a piece takes damage, once each time it goes in; it is not pushed out
    // (owner 2026-10-26).
    struct Ruin { public Renderer r; public Vector2 at; public float half; }
    private readonly List<Ruin> ruins = new List<Ruin>();
    private readonly HashSet<Renderer> inside = new HashSet<Renderer>();
    /// <summary>How many pieces of the level's terrain show now (within visual range).</summary>
    public int RuinsInSight { get; private set; }
    /// <summary>The last time our ship ran into a piece ("COLLISION · PROW ARMOUR 75%"), and how many times.</summary>
    [System.NonSerialized] public string collisionReport;
    [System.NonSerialized] public int collisions;

    private void Awake()
    {
        if (!board) board = FindAnyObjectByType<BoardManager>();
        if (!board) { Debug.LogError("[TacticalWorld] no BoardManager in the scene"); enabled = false; return; }
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
                return ship;
            }
            return null;
        };
        Sync(); // before the board's first refresh (its Start), so it never shows mock numbers
        board.Ticked += OnTick;
        // the level arrives a frame later; the board starts paused, so nothing has moved by then
        SceneManager.sceneLoaded += OnSceneLoaded;
        for (int i = 0; i < SceneManager.sceneCount; i++) HideRuins(SceneManager.GetSceneAt(i)); // a level already open in the editor
        string level = string.IsNullOrEmpty(chosenLevel) ? levelScene : chosenLevel;
        if (!string.IsNullOrEmpty(level) && !LevelOpen()) SceneManager.LoadSceneAsync(level, LoadSceneMode.Additive);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) { HideRuins(scene); SeeRuins(); }

    // every piece of a level's terrain starts hidden
    void HideRuins(Scene scene)
    {
        if (!scene.isLoaded || !scene.path.StartsWith(LevelFolder + "/")) return;
        int terrain = LayerMask.NameToLayer("WorldTerrain");
        foreach (var root in scene.GetRootGameObjects())
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (r.gameObject.layer != terrain || !r.enabled) continue;
                var b = r.bounds;
                ruins.Add(new Ruin { r = r, at = b.center, half = Mathf.Max(b.extents.x, b.extents.y) });
                r.enabled = false;
            }
    }

    // each piece in range shows; the rest hide (see ruins)
    void SeeRuins()
    {
        var s = board.State;
        if (!s.sensors) return;
        var us = new Vector2((float)s.nav.x, (float)s.nav.y);
        float sight = Mathf.Max(s.sensors.stats.visualRangeUnits, s.sensors.Listening ? s.sensors.PassiveRangeUnits : 0f); // the plot's DRADIS circle is PassiveRangeUnits
        int n = 0;
        foreach (var p in ruins)
        {
            if (!p.r) continue;
            bool show = (p.at - us).magnitude <= sight + p.half
                     || (s.hangar && s.hangar.Sees(p.at.x, p.at.y, s.sensors.stats.visualRangeUnits + p.half)); // Step 101: what our fighters see
            if (p.r.enabled != show) p.r.enabled = show;
            if (show) n++;
        }
        RuinsInSight = n;
    }

    // Our ship's centre inside a piece (the piece's box, level with the piece): the side facing it takes
    // the damage, once per time in. Our ship's real size (100 to 500 m) is too small to count.
    void Collide()
    {
        var s = board.State;
        if (s.destroyed || !s.damage) return;
        foreach (var p in ruins)
        {
            if (!p.r) continue;
            if ((p.at - new Vector2((float)s.nav.x, (float)s.nav.y)).magnitude > p.half * 1.8f) { inside.Remove(p.r); continue; } // nowhere near (a box's corner is at most ~1.7 half-widths out)
            var local = p.r.transform.InverseTransformPoint(new Vector3((float)s.nav.x, (float)s.nav.y, p.r.bounds.center.z));
            var lb = p.r.localBounds;
            var d = local - lb.center;
            bool hit = Mathf.Abs(d.x) <= lb.extents.x && Mathf.Abs(d.y) <= lb.extents.y && Mathf.Abs(d.z) <= lb.extents.z;
            if (!hit) { inside.Remove(p.r); continue; }
            if (!inside.Add(p.r)) continue; // still inside from before
            string side = ShipState.SideAt(Bearing(p.at - new Vector2((float)s.nav.x, (float)s.nav.y)), s.nav.heading);
            collisionReport = "COLLISION ·" + s.damage.TakeCollision(side); // " PROW ARMOUR 75%"
            collisions++;
            board.Struck(collisionReport); // owner 2026-10-28: running into debris must not go unnoticed
        }
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
        ship.nav.PlaceAt(c.transform.position.x, c.transform.position.y, ShipState.JsRound(((-c.transform.eulerAngles.z) % 360 + 360) % 360) % 360);
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

    /// <summary>A level's spawn sets whether this contact's brain hunts us and fires (owner 2026-10-29: in the training
    /// levels it does neither; hit, it no longer answers with missiles).</summary>
    public void Hunts(string key, bool on)
    {
        for (int i = 0; i < contacts.Length; i++) if (contacts[i] && contacts[i].trackKey == key && brains[i]) brains[i].hunts = brains[i].firesBack = on;
    }

    /// <summary>Step 73: a level's Player Spawn (PlayerSpawn). The player's ship starts there, facing the
    /// box's way, and the plot opens on it.</summary>
    public void SpawnPlayer(Vector3 at, float zDeg)
    {
        var us = board.State.nav;
        if (!us) return;
        us.PlaceAt(at.x, at.y, ShipState.JsRound(((-zDeg) % 360 + 360) % 360) % 360);
        var plot = FindAnyObjectByType<PlotPanel>();
        if (plot) plot.CentreOnOurShip();
        Sync();
    }

    /// <summary>Wolf Hunt (Step 99): an enemy brain has found us (ALERT). QuestSystem starts the reinforcements' countdown.</summary>
    public bool AnyAlert
    {
        get { foreach (var b in brains) if (b && b.enabled && b.hunts && b.Level == EnemyBrain.Awareness.Alert) return true; return false; }
    }

    /// <summary>Enemy ships, still alive, that hear or see our ship now: our sensors' "n HOSTILE SETS HAVE YOUR RANGE".</summary>
    public int HearingUs
    {
        get { int n = 0; for (int i = 0; i < brains.Length; i++) if (brains[i] && brains[i].enabled && brains[i].HearsUs && !contacts[i].Ship.destroyed) n++; return n; }
    }

    /// <summary>Wolf Hunt (Step 99): a ship arrives during play. A copy of the switched-off template, with its own
    /// track (key), at `at` facing `hdg`. Its brain is told roughly where we are (`brief`). From the next tick it is
    /// run and measured like the rest.</summary>
    public WorldContact AddContact(WorldContact template, string key, Vector2 at, int hdg, Vector2 brief, float now)
    {
        var c = Instantiate(template, transform);
        c.name = template.name.Replace(" (template)", "") + " " + key.ToUpperInvariant();
        c.trackKey = key;
        c.transform.SetPositionAndRotation(new Vector3(at.x, at.y, template.transform.position.z), Quaternion.Euler(0f, 0f, -hdg));
        c.gameObject.SetActive(true);
        // "Naga-class frigate" -> NAGA 1, FRIGATE
        string cls = c.GetComponent<Ship>() ? c.GetComponent<Ship>().Stats.shipClass : "";
        int dash = cls.IndexOf("-class");
        string name = (dash > 0 ? cls.Substring(0, dash) : "CONTACT").ToUpperInvariant() + " " + key.Substring(1);
        string klass = cls.Contains(" ") ? cls.Substring(cls.LastIndexOf(' ') + 1).ToUpperInvariant() : "SHIP";
        board.State.AddTrack(new ShipState.Track { key = key, name = name, klass = klass, hostile = true, sig = new[] { "THERM", "RAD" } });
        System.Array.Resize(ref contacts, contacts.Length + 1); contacts[contacts.Length - 1] = c;
        System.Array.Resize(ref brains, brains.Length + 1); brains[brains.Length - 1] = c.GetComponent<EnemyBrain>();
        StartShip(c);
        if (brains[brains.Length - 1]) brains[brains.Length - 1].Brief(brief, now);
        Sync();
        return c;
    }

    private void OnDestroy() { if (board) board.Ticked -= OnTick; SceneManager.sceneLoaded -= OnSceneLoaded; }

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
            ship.damage.TickConditions(board.Rate); // Step 59: its fires, vacuum and radiation, held still or not
            if (!brain || !brain.enabled) continue;
            if (ship.destroyed || ship.captured) ship.nav.throttle = 0; // a wreck, or a prize our marines hold (Step 60), makes no way of its own
            else brain.Think(ship, board.State);
            ship.nav.Tick(board.Rate); // it steers, moves, and puts its model where it is
        }
        Sync();
        SeeRuins();
        Collide();
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
            t.speed = (float)c.Ship.nav.SpeedUnits; // its real speed (Step 51); TRACK reads it
            t.course = c.Ship.nav.CourseDeg;       // and which way it is really going (Step 52)
            var d = new Vector2((float)(t.worldX - s.nav.x), (float)(t.worldY - s.nav.y));
            t.bearing = Bearing(d);
            t.rangeUnits = ShipState.JsRound(d.magnitude); // the board's tracks hold whole units
        }
    }

    /// <summary>True bearing of a world offset: 0 = north (+y), clockwise, whole degrees 0–359.</summary>
    public static int Bearing(Vector2 d)
    {
        double deg = System.Math.Atan2(d.x, d.y) * 180.0 / System.Math.PI;
        deg = (deg % 360 + 360) % 360;
        return ShipState.JsRound(deg) % 360;
    }
}
