using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Starwolf → Tuning (owner, 2026-10-27): six tabs.
/// - Ship: our ship's stats file, section by section (navigation, power, sensors, weapons, hangar, damage, orders).
/// - Enemy: the same for each enemy ship in the open scene.
/// - Board: every value marked [Tune("Section")] on the open scene's other components (the plot, sounds, the
///   clock), grouped by section, as before (owner, 2026-10-17).
/// - Keys: the board's keys, the values marked [Tune("Keys")] (owner, 2026-10-29).
/// - Hard Points: a ship as a rectangle, with its hard points as dots to drag (owner, 2026-11-01).
/// - Ranges: every system's reach as a ring round a ship, to drag (owner, 2026-11-01).
/// A stats file edit is kept, in play mode too, and the running game takes it at once. A Board or Keys edit made
/// in play mode is lost when play stops, unless "Keep play-mode changes" is ticked.
/// </summary>
public class StarwolfTuningWindow : EditorWindow
{
    [MenuItem("Starwolf/Tuning")]
    static void Open() => GetWindow<StarwolfTuningWindow>("Starwolf Tuning");

    static readonly string[] Tabs = { "Ship", "Enemy", "Board", "Keys", "Hard Points", "Ranges" };
    [SerializeField] int tab;

    class Entry { public SerializedObject so; public string path; public string section; public string owner; public string label; }

    Vector2 scroll;
    readonly Dictionary<string, bool> folds = new Dictionary<string, bool>();
    List<Entry> entries;
    List<Ship> ships;
    // IMGUI draws each frame twice (Layout, then Repaint) and both passes must draw the same controls,
    // so what to draw is decided on Layout only: the lists are rebuilt there, and play mode is read there
    bool stale = true, playing;
    static bool keepPlayChanges;
    // play-mode snapshot: object id → (property path → value)
    static readonly Dictionary<string, Dictionary<string, string>> kept = new Dictionary<string, Dictionary<string, string>>();

    void OnEnable() { EditorApplication.playModeStateChanged += OnPlayMode; EditorSceneManager.sceneOpened += OnSceneOpened; }
    void OnDisable() { EditorApplication.playModeStateChanged -= OnPlayMode; EditorSceneManager.sceneOpened -= OnSceneOpened; }
    void OnSceneOpened(UnityEngine.SceneManagement.Scene s, OpenSceneMode m) { stale = true; Repaint(); }
    void OnHierarchyChange() { stale = true; Repaint(); }

    void OnGUI()
    {
        if (Event.current.type == EventType.Layout)
        {
            playing = EditorApplication.isPlaying;
            if (stale || entries == null || ships == null)
            {
                entries = Collect();
                ships = UnityEngine.Object.FindObjectsByType<Ship>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(s => s).OrderBy(s => s.name).ToList();
                stale = false;
            }
        }
        if (entries == null || ships == null) return;
        tab = GUILayout.Toolbar(tab, Tabs);
        EditorGUILayout.Space(4);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        if (tab == 4) DrawHardPoints();
        else if (tab == 5) DrawRanges();
        else if (tab >= 2) DrawBoard(tab == 3);
        else
        {
            // ours is the ship that is not a contact; the enemies are the contacts
            var list = ships.Where(s => s && (s.GetComponent<WorldContact>() == null) == (tab == 0)).ToList(); // a ship from the last play is gone
            if (list.Count == 0) EditorGUILayout.HelpBox(tab == 0 ? "No ship of ours in the open scene. Open 02.1_Grid." : "No enemy ship in the open scene. Open 02.1_Grid.", MessageType.Info);
            foreach (var ship in list) DrawShip(ship);
        }
        EditorGUILayout.EndScrollView();
    }

    // One ship's stats file: its class and file at the top, then each section (Navigation, Power, ...) folding open.
    void DrawShip(Ship ship)
    {
        if (ship == null) { stale = true; return; }
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(ship.statsFile ? ship.statsFile.shipClass : ship.name, EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            var file = (ShipStats)EditorGUILayout.ObjectField(ship.statsFile, typeof(ShipStats), false, GUILayout.Width(200));
            if (EditorGUI.EndChangeCheck() && !playing)
            {
                Undo.RecordObject(ship, "Change stats file");
                ship.statsFile = file;
                EditorSceneManager.MarkSceneDirty(ship.gameObject.scene);
            }
        }
        if (!ship.statsFile) { EditorGUILayout.HelpBox(ship.name + " has no stats file. Pick one in Assets/Ships.", MessageType.Warning); return; }
        var so = new SerializedObject(ship.statsFile);
        so.Update();
        var it = so.GetIterator();
        it.NextVisible(true); // m_Script
        EditorGUI.BeginChangeCheck();
        while (it.NextVisible(false))
        {
            if (it.name == "shipClass") { EditorGUILayout.PropertyField(it, new GUIContent("Ship class")); continue; }
            EditorGUILayout.PropertyField(it, true); // a section, with its own fold-out
        }
        if (EditorGUI.EndChangeCheck())
        {
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssetIfDirty(ship.statsFile);
            if (playing) ship.PullFromFile();
        }
        EditorGUILayout.Space(10);
    }

    // ---- the Hard Points tab (owner, 2026-11-01) ---------------------------------------------------------------

    const float HullPx = 300f, ConePx = 150f, DotPx = 7f, GrabPx = 14f, HullPad = 20f;
    static readonly Color Gun = new Color(0.88f, 0.63f, 0.13f), PdcInk = new Color(0.9f, 0.45f, 0.75f), TorpedoInk = new Color(0.88f, 0.3f, 0.24f), HullFill = new Color(0.22f, 0.25f, 0.28f), HullEdge = new Color(0.8f, 0.83f, 0.86f);
    ShipStats[] hullFiles;
    [SerializeField] int hullFile, hullDot, hullMarks = 4;

    // The ship as a rectangle, nose up, its hard points dots to drag; under it, the selected dot's gun and cone.
    void DrawHardPoints()
    {
        if (hullFiles == null) hullFiles = AssetDatabase.FindAssets("t:ShipStats").Select(g => AssetDatabase.LoadAssetAtPath<ShipStats>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
        if (hullFiles.Length == 0) { EditorGUILayout.HelpBox("No ship stats files in Assets/Ships.", MessageType.Info); return; }
        EditorGUILayout.HelpBox("The top of the picture is the nose. The rectangle is the ship's solid box: shots and crashes hit it. Drag a dot: it stays on the edge, and its weapon points straight out of that side. Dotted lines show where a weapon can fire. The solid line is a railgun at rest.", MessageType.None);
        hullFile = EditorGUILayout.Popup("Ship", hullFile, hullFiles.Select(f => f.name).ToArray());
        var file = hullFiles[hullFile];
        hullMarks = EditorGUILayout.IntSlider(new GUIContent("Marks on each side", "Each side of the ship is cut into this many parts, and dots snap to the marks. 0: no marks."), hullMarks, 0, 16);
        EditorGUI.BeginChangeCheck();
        float width = EditorGUILayout.FloatField(new GUIContent("Ship width", "Units."), file.width);
        if (EditorGUI.EndChangeCheck()) { Undo.RecordObject(file, "Ship width"); file.width = Mathf.Max(1f, width); SaveHull(file); }

        var dots = file.weapons.hardPoints;
        Rect area = GUILayoutUtility.GetRect(10f, HullPx + 2f * ConePx + 2f * HullPad, GUILayout.ExpandWidth(true));
        float scale = HullPx / file.length;
        Vector2 middle = area.center;
        Func<Vector2, Vector2> onScreen = u => middle + new Vector2(u.x, -u.y) * scale;
        int control = GUIUtility.GetControlID(FocusType.Passive);
        var e = Event.current;
        if (e.type == EventType.MouseDown && area.Contains(e.mousePosition))
        {
            var near = Enumerable.Range(0, dots.Length).Where(i => (onScreen(dots[i].at) - e.mousePosition).magnitude <= GrabPx)
                                 .OrderBy(i => (onScreen(dots[i].at) - e.mousePosition).magnitude).ToList();
            if (near.Count > 0) { hullDot = near[0]; GUIUtility.hotControl = control; e.Use(); }
        }
        else if (e.type == EventType.MouseDrag && GUIUtility.hotControl == control)
        {
            Vector2 off = (e.mousePosition - middle) / scale;
            MoveDot(file, dots[hullDot], new Vector2(off.x, -off.y), hullMarks);
            e.Use();
        }
        else if (e.type == EventType.MouseUp && GUIUtility.hotControl == control)
        {
            GUIUtility.hotControl = 0;
            SaveHull(file);
            e.Use();
        }
        if (e.type == EventType.Repaint) PaintHull(file, area, onScreen);

        EditorGUILayout.Space(6);
        if (dots.Length > 0) DrawDot(file);
        DrawNewWeapons(file);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add a dot"))
            {
                Undo.RecordObject(file, "Add hard point");
                ArrayUtility.Add(ref file.weapons.hardPoints, new WeaponSystem.HardPoint { at = new Vector2(0f, -file.length / 2f) });
                hullDot = file.weapons.hardPoints.Length - 1;
                SaveHull(file);
            }
            if (GUILayout.Button("Remove this dot") && dots.Length > 0)
            {
                Undo.RecordObject(file, "Remove hard point");
                ArrayUtility.RemoveAt(ref file.weapons.hardPoints, hullDot);
                SaveHull(file);
            }
        }
    }

    void PaintHull(ShipStats file, Rect area, Func<Vector2, Vector2> onScreen)
    {
        EditorGUI.DrawRect(area, new Color(0.1f, 0.11f, 0.12f));
        float hw = file.width / 2f, hl = file.length / 2f;
        Vector2 min = onScreen(new Vector2(-hw, hl)), max = onScreen(new Vector2(hw, -hl));
        Handles.DrawSolidRectangleWithOutline(new Rect(min, max - min), HullFill, HullEdge);
        GUI.Label(new Rect((min.x + max.x) / 2f - 16f, min.y - 26f, 40f, 16f), "NOSE", EditorStyles.whiteMiniLabel);
        if (hullMarks > 0)
        {
            Handles.color = HullEdge;
            for (int i = 0; i <= hullMarks; i++)
            {
                float across = -hw + 2f * hw * i / hullMarks, along = -hl + 2f * hl * i / hullMarks;
                foreach (float end in new[] { -1f, 1f })
                {
                    Vector2 x = onScreen(new Vector2(across, end * hl)), y = onScreen(new Vector2(end * hw, along));
                    Handles.DrawLine(x + Vector2.down * 3f, x + Vector2.up * 3f);
                    Handles.DrawLine(y + Vector2.left * 3f, y + Vector2.right * 3f);
                }
            }
        }
        var dots = file.weapons.hardPoints;
        for (int i = 0; i < dots.Length; i++)
        {
            Vector2 at = onScreen(dots[i].at);
            var m = MountOn(file, dots[i]);
            if (m != null)
            {
                Handles.color = Ink(m.kind);
                Func<float, Vector2> rim = deg => at + new Vector2(Mathf.Sin(deg * Mathf.Deg2Rad), -Mathf.Cos(deg * Mathf.Deg2Rad)) * ConePx;
                if (m.kind == "RG") Handles.DrawAAPolyLine(2f, at, rim(m.arc));
                Dots(at, rim(m.arc - m.halfArc));
                Dots(at, rim(m.arc + m.halfArc));
                for (int d = -m.halfArc; d < m.halfArc; d += 3) Handles.DrawSolidDisc(rim(m.arc + d), Vector3.forward, 1f);
                GUI.Label(new Rect(at.x + 10f, at.y + 4f, 60f, 16f), m.shortName, EditorStyles.whiteMiniLabel);
            }
            Handles.color = m != null ? Ink(m.kind) : HullEdge;
            Handles.DrawSolidDisc(at, Vector3.forward, DotPx);
            if (i != hullDot) continue;
            Handles.color = Color.white;
            Handles.DrawWireDisc(at, Vector3.forward, DotPx + 3f);
        }
    }

    // The selected dot: what is on it, and that weapon's numbers.
    void DrawDot(ShipStats file)
    {
        var dots = file.weapons.hardPoints;
        hullDot = Mathf.Clamp(hullDot, 0, dots.Length - 1);
        var dot = dots[hullDot];
        var all = file.weapons.mounts;
        var names = new[] { "Nothing" }.Concat(all.Select(m => m.shortName)).ToArray();
        int now = Array.FindIndex(all, m => m.id == dot.mount) + 1;
        EditorGUILayout.LabelField("Selected dot", dot.at.x.ToString("0.#") + " right, " + dot.at.y.ToString("0.#") + " forward of the ship's centre, units");
        int pick = EditorGUILayout.Popup("On this dot", now, names);
        if (pick != now)
        {
            Undo.RecordObject(file, "Put a weapon on a hard point");
            foreach (var other in dots) if (pick > 0 && other.mount == all[pick - 1].id) other.mount = ""; // a weapon is on one dot only
            dot.mount = pick > 0 ? all[pick - 1].id : "";
            if (pick > 0 && all[pick - 1].kind != "MS") all[pick - 1].arc = SideOf(file, dot.at) * 90;
            SaveHull(file);
        }
        var gun = MountOn(file, dot);
        if (gun == null) return;
        EditorGUI.BeginChangeCheck();
        string name = EditorGUILayout.TextField(new GUIContent("Name", "On the Fire Control drawer."), gun.label);
        string tag = EditorGUILayout.TextField(new GUIContent("Short name", "On the Fire Control knob."), gun.shortName);
        float reach = EditorGUILayout.Slider(new GUIContent("Reach", "Units."), gun.rangeUnits, 0f, MostRangeUnits);
        int ammo = EditorGUILayout.IntField(new GUIContent("Ammunition", "Slugs, rounds or torpedo cells at the start."), gun.ammo);
        int arc = EditorGUILayout.IntSlider(new GUIContent("Direction", "Degrees round from the nose: 90 is straight out of the right side."), gun.arc, 0, 359);
        int cone = EditorGUILayout.IntSlider(new GUIContent("Cone width", "Degrees, edge to edge. 360: all round."), gun.halfArc * 2, 0, 360);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(file, "Edit weapon");
            gun.label = name; gun.shortName = tag; gun.rangeUnits = reach; gun.ammo = Mathf.Max(0, ammo); gun.arc = arc; gun.halfArc = cone / 2;
            SaveHull(file);
        }
        if (all.Length > 1 && GUILayout.Button("Delete this weapon"))
        {
            Undo.RecordObject(file, "Delete weapon");
            dot.mount = "";
            ArrayUtility.Remove(ref file.weapons.mounts, gun);
            SaveHull(file);
        }
    }

    // New weapons, and the weapons with no dot: they can't fire.
    void DrawNewWeapons(ShipStats file)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("New railgun")) NewWeapon(file, "RG", "RAILGUN TURRET", "RG");
            if (GUILayout.Button("New PDC")) NewWeapon(file, "PD", "PDC GROUP", "PDC");
            if (GUILayout.Button("New torpedo launcher")) NewWeapon(file, "MS", "TORPEDO VLS", "VLS");
        }
        var loose = file.weapons.mounts.Where(m => !file.weapons.hardPoints.Any(h => h.mount == m.id)).Select(m => m.shortName).ToArray();
        if (loose.Length > 0) EditorGUILayout.HelpBox("On no dot, so they can't fire: " + string.Join(", ", loose) + ". Pick a dot, then choose it under On this dot.", MessageType.Warning);
    }

    // A copy of the first weapon of its kind, with the next free letter, on the selected dot if it is empty, else the
    // first empty one, else a new dot at the back of the ship.
    void NewWeapon(ShipStats file, string kind, string word, string tag)
    {
        Undo.RecordObject(file, "New weapon");
        var w = file.weapons;
        var model = w.mounts.FirstOrDefault(m => m.kind == kind) ?? new WeaponSystem.Stats().mounts.First(m => m.kind == kind);
        char letter = 'A';
        while (w.mounts.Any(m => m.id == kind + letter || m.shortName == tag + "-" + letter)) letter++;
        var gun = new WeaponSystem.Mount { id = kind + letter, kind = kind, label = word + " " + letter, shortName = tag + "-" + letter, arc = model.arc, halfArc = model.halfArc, rangeUnits = model.rangeUnits, ammo = model.ammo };
        ArrayUtility.Add(ref w.mounts, gun);
        int dot = hullDot < w.hardPoints.Length && string.IsNullOrEmpty(w.hardPoints[hullDot].mount) ? hullDot : Array.FindIndex(w.hardPoints, h => string.IsNullOrEmpty(h.mount));
        if (dot < 0)
        {
            ArrayUtility.Add(ref w.hardPoints, new WeaponSystem.HardPoint { at = new Vector2(0f, -file.length / 2f) });
            dot = w.hardPoints.Length - 1;
        }
        w.hardPoints[dot].mount = gun.id;
        if (kind != "MS") gun.arc = SideOf(file, w.hardPoints[dot].at) * 90;
        hullDot = dot;
        SaveHull(file);
    }

    static void Dots(Vector2 a, Vector2 b)
    {
        float length = (b - a).magnitude;
        for (float s = 0f; s < length; s += 7f) Handles.DrawSolidDisc(Vector2.Lerp(a, b, s / length), Vector3.forward, 1f);
    }

    static WeaponSystem.Mount MountOn(ShipStats file, WeaponSystem.HardPoint dot) => string.IsNullOrEmpty(dot.mount) ? null : file.weapons.mounts.FirstOrDefault(m => m.id == dot.mount);

    // The nearest side of the ship to a point: 0 front, 1 right, 2 back, 3 left.
    static int SideOf(ShipStats file, Vector2 u)
    {
        float hw = file.width / 2f, hl = file.length / 2f;
        var gap = new[] { Mathf.Abs(hl - u.y), Mathf.Abs(hw - u.x), Mathf.Abs(hl + u.y), Mathf.Abs(hw + u.x) };
        return Array.IndexOf(gap, gap.Min());
    }

    // A dot goes to the nearest spot on the ship's edge, on a mark if the sides have them, and its gun points out of that side.
    static void MoveDot(ShipStats file, WeaponSystem.HardPoint dot, Vector2 u, int marks)
    {
        float hw = file.width / 2f, hl = file.length / 2f;
        u = new Vector2(Mathf.Clamp(u.x, -hw, hw), Mathf.Clamp(u.y, -hl, hl));
        int side = SideOf(file, u);
        if (side % 2 == 0) u = new Vector2(OnMark(u.x, hw, marks), side == 0 ? hl : -hl);
        else u = new Vector2(side == 1 ? hw : -hw, OnMark(u.y, hl, marks));
        Undo.RecordObject(file, "Move hard point");
        dot.at = u;
        var gun = MountOn(file, dot);
        if (gun != null) gun.arc = side * 90;
        EditorUtility.SetDirty(file);
    }

    static float OnMark(float v, float half, int marks)
    {
        if (marks < 1) return v;
        float part = 2f * half / marks;
        return Mathf.Round((v + half) / part) * part - half;
    }

    // Keep the edit in the file, and the running game takes it at once.
    void SaveHull(ShipStats file)
    {
        EditorUtility.SetDirty(file);
        AssetDatabase.SaveAssetIfDirty(file);
        if (playing) foreach (var s in ships) if (s && s.statsFile == file) s.PullFromFile();
    }

    // ---- the Ranges tab (owner, 2026-11-01) --------------------------------------------------------------------

    class Ring
    {
        public string name; public Color ink; public Func<ShipStats, float> get; public Action<ShipStats, float> set;
        public Ring(string name, Color ink, Func<ShipStats, float> get, Action<ShipStats, float> set) { this.name = name; this.ink = ink; this.get = get; this.set = set; }
    }

    const float RangesPx = 620f, GrabRingPx = 7f, RangeSnapUnits = 100f, MostRangeUnits = 80000f;
    const int DialTop = 5, RingPieces = 128; // the DRADIS dial goes 1 to 5 (SensorSystem.senBoost)
    static float DialRange(ShipStats f, int dial) => f.sensors.senRangeBaseUnits + dial * f.sensors.senRangePerStepUnits;
    static void SetDial(ShipStats f, float atOne, float atTop)
    {
        f.sensors.senRangePerStepUnits = Mathf.Max(0f, atTop - atOne) / (DialTop - 1);
        f.sensors.senRangeBaseUnits = atOne - f.sensors.senRangePerStepUnits;
    }
    static float KindRange(ShipStats f, string kind) { var m = f.weapons.mounts.FirstOrDefault(x => x.kind == kind); return m != null ? m.rangeUnits : 0f; }
    static void SetKind(ShipStats f, float units, string kind) { foreach (var m in f.weapons.mounts) if (m.kind == kind) m.rangeUnits = units; }
    static Color Ink(string kind) => kind == "PD" ? PdcInk : kind == "MS" ? TorpedoInk : Gun;

    static readonly Ring[] Rings =
    {
        new Ring("Seen with eyes", Color.white, f => f.sensors.visualRangeUnits, (f, u) => f.sensors.visualRangeUnits = u),
        new Ring("DRADIS, dial at 1", new Color(0.45f, 0.75f, 1f), f => DialRange(f, 1), (f, u) => SetDial(f, u, DialRange(f, DialTop))),
        new Ring("DRADIS, dial at 5", new Color(0.3f, 0.5f, 1f), f => DialRange(f, DialTop), (f, u) => SetDial(f, DialRange(f, 1), u)),
        new Ring("Railguns", Gun, f => KindRange(f, "RG"), (f, u) => SetKind(f, u, "RG")),
        new Ring("PDCs", PdcInk, f => KindRange(f, "PD"), (f, u) => SetKind(f, u, "PD")),
        new Ring("Torpedoes", TorpedoInk, f => KindRange(f, "MS"), (f, u) => SetKind(f, u, "MS")),
        new Ring("Assault boats", new Color(0.4f, 0.85f, 0.5f), f => (float)f.hangar.boatRangeUnits, (f, u) => f.hangar.boatRangeUnits = u),
    };

    int ringChosen;
    float rangesShown = 40000f, rangesArena = 60000f, rangesZoom = 0.1f;

    // Every system's reach as a ring round the ship, to scale, with the plot's view and the battle space to compare.
    void DrawRanges()
    {
        if (hullFiles == null) hullFiles = AssetDatabase.FindAssets("t:ShipStats").Select(g => AssetDatabase.LoadAssetAtPath<ShipStats>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
        if (hullFiles.Length == 0) { EditorGUILayout.HelpBox("No ship stats files in Assets/Ships.", MessageType.Info); return; }
        EditorGUILayout.HelpBox("Drag a ring to change that reach. The green box is what the plot shows at the zoom below. The dotted square is the battle space with the ship in the middle.", MessageType.None);
        hullFile = EditorGUILayout.Popup("Ship", hullFile, hullFiles.Select(f => f.name).ToArray());
        var file = hullFiles[hullFile];
        rangesShown = EditorGUILayout.Slider(new GUIContent("Picture shows", "Units from the ship to the top of the picture."), rangesShown, 2000f, MostRangeUnits);
        rangesZoom = EditorGUILayout.Slider(new GUIContent("Plot zoom", "The zoom the green box is drawn at, as the board's SCALE knob."), rangesZoom, 0.01f, 1f);
        rangesArena = EditorGUILayout.FloatField(new GUIContent("Battle space width", "Units. Each level sets its own (Starwolf → Level)."), rangesArena);

        Rect area = GUILayoutUtility.GetRect(10f, RangesPx, GUILayout.ExpandWidth(true));
        float scale = RangesPx / 2f / rangesShown;
        Vector2 middle = area.center;
        int control = GUIUtility.GetControlID(FocusType.Passive);
        var e = Event.current;
        float atMouse = (e.mousePosition - middle).magnitude;
        if (e.type == EventType.MouseDown && area.Contains(e.mousePosition))
        {
            var near = Enumerable.Range(0, Rings.Length).Where(i => Mathf.Abs(Rings[i].get(file) * scale - atMouse) <= GrabRingPx)
                                 .OrderBy(i => Mathf.Abs(Rings[i].get(file) * scale - atMouse)).ToList();
            if (near.Count > 0) { ringChosen = near[0]; GUIUtility.hotControl = control; e.Use(); }
        }
        else if (e.type == EventType.MouseDrag && GUIUtility.hotControl == control)
        {
            Undo.RecordObject(file, "Change range");
            Rings[ringChosen].set(file, Mathf.Round(atMouse / scale / RangeSnapUnits) * RangeSnapUnits);
            EditorUtility.SetDirty(file);
            e.Use();
        }
        else if (e.type == EventType.MouseUp && GUIUtility.hotControl == control)
        {
            GUIUtility.hotControl = 0;
            SaveHull(file);
            e.Use();
        }
        if (e.type == EventType.Repaint) PaintRings(file, area, scale);

        EditorGUILayout.Space(6);
        foreach (var ring in Rings)
        {
            EditorGUI.BeginChangeCheck();
            var was = GUI.contentColor;
            GUI.contentColor = ring.ink;
            float units = EditorGUILayout.Slider(ring.name, ring.get(file), 0f, MostRangeUnits);
            GUI.contentColor = was;
            if (EditorGUI.EndChangeCheck()) { Undo.RecordObject(file, "Change range"); ring.set(file, Mathf.Round(units / RangeSnapUnits) * RangeSnapUnits); SaveHull(file); }
        }
        EditorGUI.BeginChangeCheck();
        float margin = EditorGUILayout.Slider(new GUIContent("Hit area, % bigger than the box", "A shot from this ship counts if it touches a ship's box made this much bigger. Too small to draw."), file.weapons.hitMarginPercent, 0f, 100f);
        if (EditorGUI.EndChangeCheck()) { Undo.RecordObject(file, "Change range"); file.weapons.hitMarginPercent = margin; SaveHull(file); }
    }

    void PaintRings(ShipStats file, Rect area, float scale)
    {
        EditorGUI.DrawRect(area, new Color(0.1f, 0.11f, 0.12f));
        var clip = area; // the picture's edge cuts the rings
        GUI.BeginClip(clip);
        Vector2 middle = new Vector2(clip.width, clip.height) / 2f;
        var plot = PlotOnBoard();
        if (plot)
        {
            Vector2 size = plot.rectTransform.rect.size * (1000f / plot.PxPer1000Units) / rangesZoom * scale;
            Handles.DrawSolidRectangleWithOutline(new Rect(middle - size / 2f, size), new Color(0.3f, 0.8f, 0.4f, 0.08f), new Color(0.3f, 0.8f, 0.4f));
        }
        float half = rangesArena / 2f * scale;
        Handles.color = HullEdge;
        Vector2 a = middle + new Vector2(-half, -half), b = middle + new Vector2(half, -half), c = middle + new Vector2(half, half), d = middle + new Vector2(-half, half);
        Dots(a, b); Dots(b, c); Dots(c, d); Dots(d, a);
        for (int i = 0; i < Rings.Length; i++)
        {
            float units = Rings[i].get(file), r = units * scale;
            Handles.color = Rings[i].ink;
            var circle = new Vector3[RingPieces + 1];
            for (int k = 0; k <= RingPieces; k++)
            {
                float t = k * 2f * Mathf.PI / RingPieces;
                circle[k] = middle + new Vector2(Mathf.Cos(t), Mathf.Sin(t)) * r;
            }
            Handles.DrawAAPolyLine(i == ringChosen ? 3f : 1.5f, circle);
            // each label at its own place round the ring: two rings can have the same reach
            float deg = (20f + i * 26f) * Mathf.Deg2Rad;
            Vector2 at = middle + new Vector2(Mathf.Cos(deg), -Mathf.Sin(deg)) * r;
            var was = GUI.contentColor;
            GUI.contentColor = Rings[i].ink;
            GUI.Label(new Rect(at.x + 3f, at.y - 8f, 170f, 16f), Rings[i].name + "  " + units.ToString("N0"), EditorStyles.miniLabel);
            GUI.contentColor = was;
        }
        Handles.color = Color.white;
        Handles.DrawSolidDisc(middle, Vector3.forward, 2.5f);
        GUI.EndClip();
    }

    // The plot's size and scale are on the Plot prefab
    PlotGrid plotOnBoard;
    PlotGrid PlotOnBoard()
    {
        if (!plotOnBoard)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Board/Panels/Plot.prefab");
            if (prefab) plotOnBoard = prefab.GetComponentInChildren<PlotGrid>(true);
        }
        return plotOnBoard;
    }

    // The Board tab, or the Keys tab: the Keys section on its own
    void DrawBoard(bool keys)
    {
        if (playing)
        {
            EditorGUILayout.HelpBox("Play mode: changes here are lost when play stops, unless you tick the box below.", MessageType.Warning);
            keepPlayChanges = EditorGUILayout.ToggleLeft("Keep play-mode changes when play stops", keepPlayChanges);
        }
        else
        {
            bool unsaved = Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCount).Any(i => UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty);
            using (new EditorGUI.DisabledScope(!unsaved))
                if (GUILayout.Button(unsaved ? "Save" : "Saved")) { EditorSceneManager.SaveOpenScenes(); AssetDatabase.SaveAssets(); }
        }
        var shown = entries.Where(e => (e.section == "Keys") == keys).ToList();
        if (shown.Count == 0) { EditorGUILayout.HelpBox("Nothing tunable in the open scene. Open 02.1_Grid.", MessageType.Info); return; }
        foreach (var group in shown.GroupBy(e => e.section).OrderBy(g => g.Key))
        {
            bool open;
            if (!folds.TryGetValue(group.Key, out open)) open = true;
            open = EditorGUILayout.Foldout(open, group.Key, true, EditorStyles.foldoutHeader);
            folds[group.Key] = open;
            if (!open) continue;
            EditorGUI.indentLevel++;
            foreach (var e in group)
            {
                if (e.so.targetObject == null) { stale = true; Repaint(); continue; }
                e.so.Update();
                var p = e.so.FindProperty(e.path);
                if (p == null) continue;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(p, new GUIContent(e.label, p.tooltip + "\n(" + e.owner + ")"), true);
                if (EditorGUI.EndChangeCheck()) e.so.ApplyModifiedProperties();
            }
            EditorGUI.indentLevel--;
        }
    }

    static string Label(Entry e)
    {
        // "lamps.Array.data[2].glow" → "Glow — SEN" (the element's label, key or name)
        string name = ObjectNames.NicifyVariableName(e.path.Substring(e.path.LastIndexOf('.') + 1));
        int a = e.path.IndexOf(".Array.data[");
        if (a < 0) return name;
        var element = e.so.FindProperty(e.path.Substring(0, e.path.IndexOf(']', a) + 1));
        var label = element != null ? (element.FindPropertyRelative("label") ?? element.FindPropertyRelative("key") ?? element.FindPropertyRelative("name")) : null;
        string which = label != null && label.propertyType == SerializedPropertyType.String ? label.stringValue : e.path.Substring(a + 12, e.path.IndexOf(']', a) - a - 12);
        return name + " — " + which;
    }

    // Every [Tune] field on every component in the open scene, walking into serializable classes and arrays.
    static List<Entry> Collect()
    {
        var list = new List<Entry>();
        foreach (var mb in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (mb == null) continue;
            SerializedObject so = null;
            Walk(mb.GetType(), "", mb.name + " · " + mb.GetType().Name, mb, ref so, list);
        }
        foreach (var e in list) e.label = Label(e);
        // the same component on two objects: say which object
        foreach (var g in list.GroupBy(e => e.section + "|" + e.label).Where(g => g.Count() > 1))
            foreach (var e in g) e.label += " (" + e.so.targetObject.name + ")";
        return list;
    }

    static void Walk(Type type, string prefix, string owner, MonoBehaviour mb, ref SerializedObject so, List<Entry> list, int depth = 0)
    {
        if (type == null || depth > 4) return;
        for (var t = type; t != null && t != typeof(MonoBehaviour) && t != typeof(object); t = t.BaseType)
        {
            foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (f.IsNotSerialized || (!f.IsPublic && f.GetCustomAttribute<SerializeField>() == null)) continue;
                string path = prefix + f.Name;
                var tune = f.GetCustomAttribute<TuneAttribute>();
                if (tune != null)
                {
                    if (so == null) so = new SerializedObject(mb);
                    list.Add(new Entry { so = so, path = path, section = tune.Section, owner = owner });
                    continue;
                }
                var ft = f.FieldType;
                if (ft.IsArray && IsNested(ft.GetElementType()))
                {
                    if (so == null) so = new SerializedObject(mb);
                    var arr = so.FindProperty(path);
                    if (arr == null || !arr.isArray) continue;
                    for (int i = 0; i < arr.arraySize; i++) Walk(ft.GetElementType(), path + ".Array.data[" + i + "].", owner, mb, ref so, list, depth + 1);
                }
                else if (IsNested(ft)) Walk(ft, path + ".", owner, mb, ref so, list, depth + 1);
            }
        }
    }

    static bool IsNested(Type t) => t != null && t.IsClass && t != typeof(string) && !typeof(UnityEngine.Object).IsAssignableFrom(t) && t.IsSerializable; // [Serializable] is not a real attribute at runtime

    // ---- keep play-mode changes (Board tab) ------------------------------------------------------

    void OnPlayMode(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.ExitingPlayMode && keepPlayChanges)
        {
            kept.Clear();
            foreach (var e in entries ?? Collect())
            {
                if (e.so.targetObject == null) continue;
                e.so.Update();
                var p = e.so.FindProperty(e.path);
                if (p == null) continue;
                string id = GlobalObjectId.GetGlobalObjectIdSlow(e.so.targetObject).ToString();
                if (!kept.ContainsKey(id)) kept[id] = new Dictionary<string, string>();
                kept[id][e.path] = Write(p);
            }
        }
        else if (change == PlayModeStateChange.EnteredEditMode && kept.Count > 0)
        {
            foreach (var kv in kept)
            {
                GlobalObjectId gid;
                if (!GlobalObjectId.TryParse(kv.Key, out gid)) continue;
                var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(gid);
                if (obj == null) continue;
                var so = new SerializedObject(obj);
                foreach (var pv in kv.Value) { var p = so.FindProperty(pv.Key); if (p != null) Read(p, pv.Value); }
                so.ApplyModifiedProperties();
                EditorSceneManager.MarkSceneDirty(((Component)obj).gameObject.scene);
            }
            kept.Clear();
            keepPlayChanges = false;
            Debug.Log("[Tuning] play-mode changes kept. Save the scene to keep them for good.");
        }
        ships = null; // the ships of the run that just ended are gone
        stale = true;
        Repaint();
    }

    static string Write(SerializedProperty p)
    {
        switch (p.propertyType)
        {
            case SerializedPropertyType.Float: return p.doubleValue.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
            case SerializedPropertyType.Integer: return p.longValue.ToString();
            case SerializedPropertyType.Boolean: return p.boolValue ? "1" : "0";
            case SerializedPropertyType.String: return p.stringValue;
            case SerializedPropertyType.Enum: return p.enumValueIndex.ToString();
            case SerializedPropertyType.Vector2: return JsonUtility.ToJson(new V2 { v = p.vector2Value });
            case SerializedPropertyType.Color: return JsonUtility.ToJson(new C { c = p.colorValue });
            default: return null;
        }
    }

    static void Read(SerializedProperty p, string s)
    {
        if (s == null) return;
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        switch (p.propertyType)
        {
            case SerializedPropertyType.Float: p.doubleValue = double.Parse(s, ci); break;
            case SerializedPropertyType.Integer: p.longValue = long.Parse(s); break;
            case SerializedPropertyType.Boolean: p.boolValue = s == "1"; break;
            case SerializedPropertyType.String: p.stringValue = s; break;
            case SerializedPropertyType.Enum: p.enumValueIndex = int.Parse(s); break;
            case SerializedPropertyType.Vector2: p.vector2Value = JsonUtility.FromJson<V2>(s).v; break;
            case SerializedPropertyType.Color: p.colorValue = JsonUtility.FromJson<C>(s).c; break;
        }
    }

    [Serializable] struct V2 { public Vector2 v; }
    [Serializable] struct C { public Color c; }
}
