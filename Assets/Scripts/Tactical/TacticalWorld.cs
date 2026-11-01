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
    // Owner 2026-11-01: every ship is a solid box round its model, and every piece is solid too. Nothing passes
    // through anything: a ship that touches a piece or another ship bounces off and takes damage by its speed.
    struct Ruin { public Renderer r; public Vector2 at; public float half; public Vector2[] outline; public float reach; }
    private readonly List<Ruin> ruins = new List<Ruin>();
    private readonly Dictionary<NavSystem, float> nextCrash = new Dictionary<NavSystem, float>();
    private readonly List<ShipState> bodies = new List<ShipState>();
    private readonly List<Ruin> nearby = new List<Ruin>();
    private readonly Vector2[] boxA = new Vector2[4], boxB = new Vector2[4];
    /// <summary>The level's gas clouds (owner 2026-11-01). They have no collider, so Collide never sees them.</summary>
    public readonly List<GasCloud> Clouds = new List<GasCloud>();

    [Header("Crashes (owner 2026-11-01)")]
    [Tooltip("Share of its speed a ship keeps after it bounces off something.")]
    [Tune("Crashes")] [Range(0f, 1f)] [SerializeField] private float crashKeepSpeed = 0.3f;
    [Tooltip("A ship takes crash damage at most once in this many game seconds, however long it grinds against something.")]
    [Tune("Crashes")] [SerializeField] private float crashRepeatSeconds = 3f;
    [Tooltip("Owner 2026-11-01: our ship on course to hit a rock within this many game seconds raises the warning on the alarm strip, with the alert tone.")]
    [Tune("Crashes")] [SerializeField] private float rockWarnSeconds = 30f;
    [Tooltip("A moving ship is checked every this many units along its way this tick, so it can't skip through at x10.")]
    [Tune("Crashes")] [SerializeField] private float crashCheckUnits = 20f;
    /// <summary>Off: ships pass through everything (the playtest's stand-in for flying through debris).</summary>
    [System.NonSerialized] public bool solid = true;
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
                var outline = Outline(r);
                float reach = 0f;
                foreach (var p in outline) reach = Mathf.Max(reach, (p - (Vector2)b.center).magnitude);
                ruins.Add(new Ruin { r = r, at = b.center, half = Mathf.Max(b.extents.x, b.extents.y), outline = outline, reach = reach });
                r.enabled = false;
            }
        Clouds.Clear();
        Clouds.AddRange(FindObjectsByType<GasCloud>(FindObjectsSortMode.None));
    }

    // each piece in range shows; the rest hide (see ruins)
    void SeeRuins()
    {
        var s = board.State;
        if (!s.sensors) return;
        var us = new Vector2((float)s.nav.x, (float)s.nav.y);
        float sight = Mathf.Max(s.sensors.stats.visualRangeUnits, s.sensors.Listening ? s.sensors.PassiveRangeUnits : 0f); // the plot's DRADIS circle is PassiveRangeUnits
        bool InSight(Vector2 at, float half) => (at - us).magnitude <= sight + half
            || (s.hangar && s.hangar.Sees(at.x, at.y, s.sensors.stats.visualRangeUnits + half)); // Step 101: what our fighters see
        int n = 0;
        foreach (var p in ruins)
        {
            if (!p.r) continue;
            bool show = InSight(p.at, p.half);
            if (p.r.enabled != show) p.r.enabled = show;
            if (show) n++;
        }
        RuinsInSight = n;
        foreach (var c in Clouds) if (c) c.seen = InSight(c.At, Mathf.Max(c.widthUnits, c.heightUnits) / 2);
    }

    // Our ship is in a gas cloud when enough of its box is in one (owner 2026-11-01). Enemies don't hide in clouds yet.
    void InClouds()
    {
        var us = board.State;
        var at = new Vector2((float)us.nav.x, (float)us.nav.y);
        us.sensors.inCloud = false;
        foreach (var c in Clouds)
            if (c && c.ShareInside(at, us.nav.heading, us.nav.BoxHalf) >= us.sensors.stats.cloudInsideShare) us.sensors.inCloud = true;
    }

    // A piece's box seen from above: the outline of its eight corners on the plot's plane (some pieces are tilted).
    static Vector2[] Outline(Renderer r)
    {
        var b = r.localBounds;
        var points = new List<Vector2>();
        for (int i = 0; i < 8; i++)
        {
            var c = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
            points.Add(r.transform.TransformPoint(c));
        }
        points.Sort((p, q) => p.x != q.x ? p.x.CompareTo(q.x) : p.y.CompareTo(q.y));
        // round the points: the lower side left to right, then the upper side back
        var hull = new List<Vector2>();
        for (int pass = 0; pass < 2; pass++)
        {
            int start = hull.Count;
            for (int k = 0; k < points.Count; k++)
            {
                var p = points[pass == 0 ? k : points.Count - 1 - k];
                while (hull.Count >= start + 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0f) hull.RemoveAt(hull.Count - 1);
                hull.Add(p);
            }
            hull.RemoveAt(hull.Count - 1);
        }
        return hull.ToArray();
    }

    static float Cross(Vector2 o, Vector2 a, Vector2 b) => (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);

    // A ship's box at a spot, its corners in turn.
    static void Corners(Vector2 at, double heading, Vector2 half, Vector2[] into)
    {
        float rad = (float)heading * Mathf.Deg2Rad;
        Vector2 fore = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * half.y, right = new Vector2(Mathf.Cos(rad), -Mathf.Sin(rad)) * half.x;
        into[0] = at + fore + right; into[1] = at + fore - right; into[2] = at - fore - right; into[3] = at - fore + right;
    }

    static void Span(Vector2[] shape, Vector2 axis, out float min, out float max)
    {
        min = max = Vector2.Dot(shape[0], axis);
        for (int i = 1; i < shape.Length; i++) { float d = Vector2.Dot(shape[i], axis); min = Mathf.Min(min, d); max = Mathf.Max(max, d); }
    }

    // Do two outlines overlap? If so: the shortest way to push a clear of b. A gap along any edge's square means no.
    static bool Overlap(Vector2[] a, Vector2[] b, out Vector2 way, out float depth)
    {
        way = Vector2.zero; depth = float.MaxValue;
        for (int pass = 0; pass < 2; pass++)
        {
            var shape = pass == 0 ? a : b;
            for (int i = 0; i < shape.Length; i++)
            {
                Vector2 edge = shape[(i + 1) % shape.Length] - shape[i];
                if (edge.sqrMagnitude < 1e-6f) continue;
                Vector2 axis = new Vector2(-edge.y, edge.x).normalized;
                float minA, maxA, minB, maxB;
                Span(a, axis, out minA, out maxA); Span(b, axis, out minB, out maxB);
                float up = maxB - minA, down = maxA - minB;
                if (up <= 0f || down <= 0f) return false;
                if (Mathf.Min(up, down) < depth) { depth = Mathf.Min(up, down); way = up < down ? axis : -axis; }
            }
        }
        return true;
    }

    // Every tick, after everything has moved: each ship against the pieces, then against each other.
    void Collide()
    {
        var us = board.State;
        bodies.Clear();
        if (!us.destroyed) bodies.Add(us);
        foreach (var c in contacts) if (c) bodies.Add(c.Ship);
        if (solid)
        {
            foreach (var ship in bodies) AgainstPieces(ship);
            for (int i = 0; i < bodies.Count; i++)
                for (int j = i + 1; j < bodies.Count; j++) AgainstShip(bodies[i], bodies[j]);
        }
        foreach (var ship in bodies) { ship.nav.wasX = ship.nav.x; ship.nav.wasY = ship.nav.y; }
    }

    // Owner 2026-11-01: rocks are solid and x10 is fast, so our ship's way ahead is watched. On course to hit one soon: a
    // line on the alarm strip, and the alert tone as it starts.
    void WarnRocks()
    {
        var s = board.State;
        float seconds = solid && !s.destroyed ? SecondsToRock(s.nav) : -1f;
        bool warned = board.rockAlarm != null, warn = seconds >= 0f && seconds <= rockWarnSeconds;
        board.rockAlarm = warn ? "ROCK AHEAD · " + Mathf.CeilToInt(seconds) + " S" : null;
        if (warn && !warned) board.Alert();
    }

    // Game seconds until our box reaches the nearest rock on its way, or -1 if it reaches none it is looking for.
    float SecondsToRock(NavSystem nav)
    {
        float speed = (float)nav.SpeedUnits;
        if (speed <= 0.5f) return -1f;
        Vector2 at = new Vector2((float)nav.x, (float)nav.y), half = nav.BoxHalf;
        Vector2 ahead = new Vector2((float)nav.vx, (float)nav.vy) / speed, side = new Vector2(ahead.y, -ahead.x);
        float rad = nav.heading * Mathf.Deg2Rad;
        Vector2 fore = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * half.y, right = new Vector2(Mathf.Cos(rad), -Mathf.Sin(rad)) * half.x;
        // how far the box reaches sideways of its way, and forward along it
        float wide = Mathf.Abs(Vector2.Dot(fore, side)) + Mathf.Abs(Vector2.Dot(right, side));
        float front = Mathf.Abs(Vector2.Dot(fore, ahead)) + Mathf.Abs(Vector2.Dot(right, ahead));
        float look = speed * rockWarnSeconds, across = Vector2.Dot(at, side), along = Vector2.Dot(at, ahead), nearest = float.MaxValue;
        foreach (var p in ruins)
        {
            if (!p.r || (p.at - at).magnitude > look + p.reach + front) continue;
            float minS, maxS, minA, maxA;
            Span(p.outline, side, out minS, out maxS);
            if (minS > across + wide || maxS < across - wide) continue; // clear to one side
            Span(p.outline, ahead, out minA, out maxA);
            if (maxA < along) continue; // behind us
            nearest = Mathf.Min(nearest, Mathf.Max(0f, minA - along - front));
        }
        return nearest == float.MaxValue ? -1f : nearest / speed;
    }

    // Where a ship was at the last check, or where it is now if it jumped there (a level's spawn point).
    Vector2 Was(NavSystem nav)
    {
        var now = new Vector2((float)nav.x, (float)nav.y);
        var was = new Vector2((float)nav.wasX, (float)nav.wasY);
        return (now - was).magnitude > nav.stats.topSpeedUnits * board.TickGameSeconds * 2.0 ? now : was;
    }

    void AgainstPieces(ShipState ship)
    {
        var nav = ship.nav;
        Vector2 half = nav.BoxHalf, to = new Vector2((float)nav.x, (float)nav.y), from = Was(nav);
        float moved = (to - from).magnitude, size = half.magnitude;
        nearby.Clear();
        foreach (var p in ruins) if (p.r && (p.at - to).magnitude <= p.reach + size + moved) nearby.Add(p);
        if (nearby.Count == 0) return;
        int steps = Mathf.Max(1, Mathf.CeilToInt(moved / Mathf.Max(1f, crashCheckUnits)));
        for (int i = 1; i <= steps; i++)
        {
            var at = Vector2.Lerp(from, to, (float)i / steps);
            Corners(at, nav.heading, half, boxA);
            foreach (var p in nearby)
            {
                Vector2 way; float depth;
                if (!Overlap(boxA, p.outline, out way, out depth)) continue;
                nav.x = at.x + way.x * depth; nav.y = at.y + way.y * depth;
                double into = nav.vx * way.x + nav.vy * way.y; // less than 0: coming at it
                if (into < 0)
                {
                    nav.vx = (nav.vx - 2 * into * way.x) * crashKeepSpeed;
                    nav.vy = (nav.vy - 2 * into * way.y) * crashKeepSpeed;
                    Crash(ship, -way, -into);
                }
                nav.PlaceModel();
                return;
            }
        }
    }

    void AgainstShip(ShipState a, ShipState b)
    {
        NavSystem na = a.nav, nb = b.nav;
        Vector2 halfA = na.BoxHalf, halfB = nb.BoxHalf;
        Vector2 toA = new Vector2((float)na.x, (float)na.y), toB = new Vector2((float)nb.x, (float)nb.y), fromA = Was(na), fromB = Was(nb);
        float moved = Mathf.Max((toA - fromA).magnitude, (toB - fromB).magnitude);
        if ((toA - toB).magnitude > halfA.magnitude + halfB.magnitude + 2f * moved) return;
        int steps = Mathf.Max(1, Mathf.CeilToInt(moved / Mathf.Max(1f, crashCheckUnits)));
        for (int i = 1; i <= steps; i++)
        {
            Vector2 atA = Vector2.Lerp(fromA, toA, (float)i / steps), atB = Vector2.Lerp(fromB, toB, (float)i / steps);
            Corners(atA, na.heading, halfA, boxA);
            Corners(atB, nb.heading, halfB, boxB);
            Vector2 way; float depth;
            if (!Overlap(boxA, boxB, out way, out depth)) continue;
            // each pushed half the way clear, and the two bounce off each other
            na.x = atA.x + way.x * depth / 2f; na.y = atA.y + way.y * depth / 2f;
            nb.x = atB.x - way.x * depth / 2f; nb.y = atB.y - way.y * depth / 2f;
            double into = (na.vx - nb.vx) * way.x + (na.vy - nb.vy) * way.y; // less than 0: coming together
            if (into < 0)
            {
                na.vx = (na.vx - into * way.x) * crashKeepSpeed; na.vy = (na.vy - into * way.y) * crashKeepSpeed;
                nb.vx = (nb.vx + into * way.x) * crashKeepSpeed; nb.vy = (nb.vy + into * way.y) * crashKeepSpeed;
                Crash(a, -way, -into);
                Crash(b, way, -into);
            }
            na.PlaceModel(); nb.PlaceModel();
            return;
        }
    }

    // A ship takes a crash on the side of its box toward what it hit, by the speed it hit at.
    void Crash(ShipState ship, Vector2 toward, double speed)
    {
        float now, again;
        now = board.State.missionNow;
        if (nextCrash.TryGetValue(ship.nav, out again) && now < again) return;
        nextCrash[ship.nav] = now + crashRepeatSeconds;
        string tail = ship.damage.TakeCollision(ShipState.SideAt(Bearing(toward), ship.nav.heading), speed);
        if (ship != board.State || tail == "") return;
        collisionReport = "COLLISION ·" + tail; // " PROW ARMOUR 75%"
        collisions++;
        board.Struck(collisionReport, Mathf.Clamp((float)speed / Mathf.Max(0.01f, ship.damage.stats.crashFullSpeed), 0.3f, 1f)); // owner 2026-10-28: a crash must not go unnoticed
    }

    /// <summary>Owner 2026-11-01: rocks and ships are solid, so an enemy steers round them. The heading nearest the one
    /// it wants with its way clear for `look` units. All blocked: the one it wants.</summary>
    public int ClearHeading(NavSystem nav, int wanted, float look)
    {
        var at = new Vector2((float)nav.x, (float)nav.y);
        float clear = nav.BoxHalf.magnitude;
        // try the side it is already turning to first, or it dithers between the two
        int first = Mathf.DeltaAngle(wanted, nav.heading) >= 0f ? 1 : -1;
        for (int off = 0; off <= 150; off += 30)
            foreach (int side in new[] { first, -first })
            {
                int heading = ((wanted + side * off) % 360 + 360) % 360;
                float rad = heading * Mathf.Deg2Rad;
                var ahead = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
                if (WayClear(nav, at, ahead, look, clear)) return heading;
            }
        return wanted;
    }

    bool WayClear(NavSystem nav, Vector2 at, Vector2 ahead, float look, float clear)
    {
        foreach (var p in ruins)
            if (p.r && InTheWay(p.at - at, ahead, look, p.reach + clear)) return false;
        foreach (var ship in bodies)
            if (ship.nav != nav && InTheWay(new Vector2((float)ship.nav.x, (float)ship.nav.y) - at, ahead, look, ship.nav.BoxHalf.magnitude + clear)) return false;
        return true;
    }

    // Is something this far off, this wide, in a corridor straight ahead?
    static bool InTheWay(Vector2 off, Vector2 ahead, float look, float width)
    {
        float along = Vector2.Dot(off, ahead);
        return along > -width && along < look + width && Mathf.Abs(off.x * ahead.y - off.y * ahead.x) < width;
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
            ship.damage.TickConditions(board.TickGameSeconds); // Step 59: its fires, vacuum and radiation, held still or not
            if (!brain || !brain.enabled) continue;
            if (ship.destroyed || ship.captured) ship.nav.throttle = 0; // a wreck, or a prize our marines hold (Step 60), makes no way of its own
            else brain.Think(ship, board.State);
            ship.nav.Tick(board.TickGameSeconds); // it steers, moves, and puts its model where it is
        }
        Collide();
        WarnRocks();
        InClouds();
        Sync();
        SeeRuins();
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
