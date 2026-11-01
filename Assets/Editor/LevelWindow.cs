using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Starwolf → Level (owner, 2026-10-25). Each level is a small scene in Assets/Scenes/Levels, loaded
/// on top of the board at play (TacticalWorld). Open one here and the window shows it from above (owner,
/// 2026-10-27): the battle space's edge, every piece of debris as a dot, the spawn boxes, the gas clouds, and a grid
/// with the plot's numbers. Click anything on the map, or in the list, and its settings show under the map. Drag it to
/// move it; drag a white square to resize it (owner, 2026-11-01). A click on a piece of debris picks its group;
/// Option-click picks that one piece. The map shows a picked debris field in amber, as Scatter would lay it out
/// with those settings. Add parts with the buttons at the bottom. Save is at the top.
/// World units: x east, y north.
/// </summary>
public class LevelWindow : EditorWindow
{
    const string Board = "Assets/Scenes/Tactical/02.1_Grid.unity";
    const float WholeMapUnits = 45000f; // the Scene view's size when a level opens: the whole battle space, top to bottom

    static readonly Color Ground = new Color(0.04f, 0.05f, 0.06f), Edge = new Color(0.55f, 0.6f, 0.65f),
        Debris = new Color(0.6f, 0.63f, 0.66f), Preview = new Color(0.91f, 0.64f, 0.24f),
        Enemy = new Color(1f, 0.35f, 0.25f), Player = new Color(0.91f, 0.93f, 0.94f), Cloud = new Color(0.45f, 0.72f, 0.85f),
        GridLine = new Color(1f, 1f, 1f, 0.07f);

    const float MarkerGrabPx = 10f, GripPx = 9f, GridGapPx = 60f;

    Vector2 scroll;
    Component picked; // a debris field, a gas cloud, a spawn, or a Transform: a group of debris or one piece
    Editor pickedEditor;
    Vector3 dragOffset; // from the mouse to the picked thing's centre, world units
    int dragGrip = -1; // the white square being dragged; -1: the whole thing
    bool resized; // the white square really moved: a click on one changes nothing
    bool savedLook; // what the Save button last showed
    string previewKey;
    readonly List<Vector3> preview = new List<Vector3>(); // the picked field's layout as Scatter would make it now, its own space

    [MenuItem("Starwolf/Level")]
    static void Open() => GetWindow<LevelWindow>("Starwolf Level");

    void OnFocus() => Repaint();
    void OnHierarchyChange() => Repaint();
    void OnSelectionChange()
    {
        var part = Selection.activeGameObject ? PartOf(Selection.activeGameObject) : null;
        if (part) picked = part;
        Repaint();
    }
    static Component PartOf(GameObject go)
    {
        foreach (var kind in new[] { typeof(DebrisField), typeof(GasCloud), typeof(EnemySpawn), typeof(PlayerSpawn) })
        {
            var part = go.GetComponentInParent(kind);
            if (part) return part;
        }
        return go.layer == LayerMask.NameToLayer("WorldTerrain") || go.GetComponent<SpriteRenderer>() ? go.transform : null; // a sprite: the jump point
    }
    // the Save button follows changes made in the Inspector or the Scene view too
    void OnInspectorUpdate() { if (OpenLevelScene().isDirty != savedLook) Repaint(); }

    void OnGUI()
    {
        // a level can't be opened or changed while the game runs
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorGUILayout.HelpBox("Stop the game to open or edit a level.", MessageType.Info);
            return;
        }
        var level = OpenLevelScene();
        savedLook = level.isDirty;
        using (new EditorGUI.DisabledScope(!level.isDirty))
            if (GUILayout.Button(!level.IsValid() ? "Save" : level.isDirty ? "Save " + level.name : level.name + " saved", GUILayout.Height(28)))
                EditorSceneManager.SaveScene(level);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.LabelField("Levels", EditorStyles.boldLabel);
        foreach (var path in Levels())
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                bool open = SceneManager.GetSceneByPath(path).isLoaded;
                EditorGUILayout.LabelField(Path.GetFileNameWithoutExtension(path) + (open ? "   (open)" : ""));
                if (GUILayout.Button("Open", GUILayout.Width(60))) OpenLevel(path);
            }
        }
        if (GUILayout.Button("New level")) NewLevel();

        if (level.IsValid())
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(level.name + " from above", EditorStyles.boldLabel);
            if (Event.current.type == EventType.Layout) KeepSpawnsInside(level);
            DrawMap(level);
            EditorGUILayout.LabelField("Click to pick, drag to move, drag a white square to resize. Option-click picks one piece of debris instead of its group. Grey: debris.  Amber: what you picked, and a debris field as Scatter would lay it.  Red: enemy spawn.  White: player spawn.  Blue box: gas cloud.  J: the jump point.  Red line: the enemy's route; click the enemy, then drag the route's white squares.  Line: the battle space's edge.  Numbers: units from the centre, as on the plot.", EditorStyles.wordWrappedMiniLabel);
            DrawBattleSpace(level);
            DrawParts(level);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField(level.IsValid() ? "Add to " + level.name : "Open a level to add to it", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(!level.IsValid()))
        {
            if (GUILayout.Button("Enemy spawn")) Add<EnemySpawn>(level, "Enemy Spawn");
            if (GUILayout.Button("Player spawn")) AddPlayerSpawn(level);
            if (GUILayout.Button("Battle space")) AddBattleSpace(level);
            if (GUILayout.Button("Debris field")) AddDebrisField(level);
            if (GUILayout.Button("Gas cloud")) Add<GasCloud>(level, "Gas Cloud");
            if (GUILayout.Button("Mission")) Add<Mission>(level, "Mission"); // its goal; without one the level plays Wolf Hunt
        }
        EditorGUILayout.EndScrollView();
    }

    // ---- the map ---------------------------------------------------------------------------

    void DrawMap(Scene level)
    {
        float w = EditorGUIUtility.currentViewWidth - 30, h = Mathf.Clamp(w * 0.8f, 200, 520);
        var r = GUILayoutUtility.GetRect(w, h, GUILayout.ExpandWidth(true));
        var half = ArenaHalfUnits(level);
        float unitsPerPx = Mathf.Max(half.x * 2 / r.width, half.y * 2 / r.height) * 1.04f; // the whole battle space, a little margin
        System.Func<Vector3, Vector2> px = p => new Vector2(r.width / 2 + p.x / unitsPerPx, r.height / 2 - p.y / unitsPerPx); // inside the map's clip
        MapMouse(level, r, unitsPerPx, px);
        if (Event.current.type != EventType.Repaint) return;

        GUI.BeginClip(r); // anything outside the battle space stays inside the map's frame
        EditorGUI.DrawRect(new Rect(0, 0, r.width, r.height), Ground);
        DrawGrid(r, unitsPerPx, px);
        Frame(px(new Vector3(-half.x, half.y)), px(new Vector3(half.x, -half.y)), Edge);
        foreach (var piece in Pieces(level))
        {
            var b = piece.bounds;
            Dot(px(b.center), Mathf.Max(2f, Mathf.Max(b.size.x, b.size.y) / unitsPerPx), Debris);
        }
        foreach (var c in Find<GasCloud>(level))
        {
            Vector2 a = px(c.At + new Vector2(-c.widthUnits, c.heightUnits) / 2), b = px(c.At + new Vector2(c.widthUnits, -c.heightUnits) / 2);
            EditorGUI.DrawRect(Rect.MinMaxRect(a.x, a.y, b.x, b.y), new Color(Cloud.r, Cloud.g, Cloud.b, 0.15f));
            Frame(a, b, Cloud);
        }
        foreach (var s in Find<EnemySpawn>(level))
        {
            for (int i = 1; i < s.route.Length; i++) { Handles.color = Enemy; Handles.DrawLine(px(s.route[i - 1]), px(s.route[i])); }
            foreach (var p in s.route) Dot(px(p), 5f, Enemy);
            Marker(px(s.transform.position), Enemy, "E");
        }
        foreach (var m in Find<Mission>(level)) if (m.jumpPoint) Marker(px(m.jumpPoint.position), Player, "J");
        foreach (var s in Find<PlayerSpawn>(level)) Marker(px(s.transform.position), Player, "P");
        if (picked && picked.gameObject.scene == level)
        {
            var field = picked as DebrisField;
            if (field) foreach (var p in LaidOut(field)) Dot(px(field.transform.TransformPoint(p)), 2.5f, Preview);
            if (field) Outline(field, px);
            else if (picked is GasCloud c) Frame(px(c.At + new Vector2(-c.widthUnits, c.heightUnits) / 2), px(c.At + new Vector2(c.widthUnits, -c.heightUnits) / 2), Preview);
            else if (picked is EnemySpawn s) Frame(px(s.transform.position + new Vector3(-s.sizeUnits.x, s.sizeUnits.y) / 2), px(s.transform.position + new Vector3(s.sizeUnits.x, -s.sizeUnits.y) / 2), Preview);
            else if (picked is Transform t)
            {
                var b = BoundsOf(t);
                Frame(px(b.center + new Vector3(-b.extents.x, b.extents.y)), px(b.center + new Vector3(b.extents.x, -b.extents.y)), Preview);
            }
            else Frame(px(picked.transform.position) - Vector2.one * 8, px(picked.transform.position) + Vector2.one * 8, Preview);
            foreach (var g in Grips(picked)) Dot(px(FromOwnSpace(picked, g)), GripPx, Color.white);
        }
        GUI.EndClip();
    }

    // Lines every 500 units or a doubling of that, at least GridGapPx apart, counted from the centre as the plot's are, with its numbers.
    static void DrawGrid(Rect r, float unitsPerPx, System.Func<Vector3, Vector2> px)
    {
        float step = 500f;
        while (step / unitsPerPx < GridGapPx) step *= 2f;
        int across = Mathf.CeilToInt(r.width / 2 * unitsPerPx / step), down = Mathf.CeilToInt(r.height / 2 * unitsPerPx / step);
        for (int i = -across; i <= across; i++)
        {
            float x = px(new Vector3(i * step, 0f)).x;
            EditorGUI.DrawRect(new Rect(x, 0, 1, r.height), GridLine);
            GUI.Label(new Rect(x + 2, 0, 60, 14), (i * step).ToString("N0"), EditorStyles.miniLabel);
        }
        for (int i = -down; i <= down; i++)
        {
            float y = px(new Vector3(0f, i * step)).y;
            EditorGUI.DrawRect(new Rect(0, y, r.width, 1), GridLine);
            GUI.Label(new Rect(2, y, 60, 14), (i * step).ToString("N0"), EditorStyles.miniLabel);
        }
    }

    // Owner 2026-11-01: click picks, drag moves, a white square resizes. A resized debris field scatters again on letting go.
    void MapMouse(Scene level, Rect r, float unitsPerPx, System.Func<Vector3, Vector2> px)
    {
        var e = Event.current;
        int id = GUIUtility.GetControlID(FocusType.Passive);
        var world = new Vector3((e.mousePosition.x - r.center.x) * unitsPerPx, (r.center.y - e.mousePosition.y) * unitsPerPx, 0f);
        switch (e.GetTypeForControl(id))
        {
            case EventType.MouseDown:
                if (e.button != 0 || !r.Contains(e.mousePosition)) return;
                dragGrip = GripAt(e.mousePosition - r.position, px);
                if (dragGrip < 0) Pick(PartAt(level, world, e.mousePosition - r.position, unitsPerPx, e.alt, px));
                if (picked) { dragOffset = CentreOf(picked) - world; GUIUtility.hotControl = id; }
                e.Use();
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl != id || !picked) return;
                if (dragGrip >= 0) { SetSize(picked, dragGrip, OwnSpace(picked, world)); resized = true; }
                else Move(picked, world + dragOffset);
                EditorSceneManager.MarkSceneDirty(level);
                e.Use();
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl != id) return;
                GUIUtility.hotControl = 0;
                if (resized && picked is DebrisField field) DebrisFieldEditor.Scatter(field);
                dragGrip = -1;
                resized = false;
                e.Use();
                break;
        }
    }

    void Pick(Component part)
    {
        picked = part;
        Selection.activeGameObject = part ? part.gameObject : null;
    }

    static void Move(Component c, Vector2 to)
    {
        Undo.RecordObject(c.transform, "Move " + c.name);
        var from = CentreOf(c);
        c.transform.position += new Vector3(to.x - from.x, to.y - from.y, 0f);
    }

    static IEnumerable<Renderer> Pieces(Scene level) => Find<Renderer>(level).Where(r => r.gameObject.layer == LayerMask.NameToLayer("WorldTerrain"));

    // A piece's group: its debris field, or the group it is under in the level (Terrain/Asteroid field 1).
    static Component GroupOf(Transform piece)
    {
        var field = piece.GetComponentInParent<DebrisField>();
        if (field) return field;
        while (piece.parent && piece.parent.parent) piece = piece.parent;
        return piece;
    }

    // What a group's or a piece's meshes cover, seen from above.
    static Bounds BoundsOf(Transform t)
    {
        var meshes = t.GetComponentsInChildren<Renderer>(true);
        if (meshes.Length == 0) return new Bounds(t.position, Vector3.zero);
        var b = meshes[0].bounds;
        foreach (var m in meshes) b.Encapsulate(m.bounds);
        return b;
    }

    static Vector3 CentreOf(Component c) => c is Transform t ? BoundsOf(t).center : c.transform.position;

    // What is under the mouse: a spawn's marker, a piece of debris, a gas cloud, then a debris field. `onePiece`: the piece, not its group.
    static Component PartAt(Scene level, Vector3 world, Vector2 mouse, float unitsPerPx, bool onePiece, System.Func<Vector3, Vector2> px)
    {
        foreach (var s in Find<EnemySpawn>(level)) if ((px(s.transform.position) - mouse).magnitude <= MarkerGrabPx) return s;
        foreach (var s in Find<PlayerSpawn>(level)) if ((px(s.transform.position) - mouse).magnitude <= MarkerGrabPx) return s;
        foreach (var m in Find<Mission>(level)) if (m.jumpPoint && (px(m.jumpPoint.position) - mouse).magnitude <= MarkerGrabPx) return m.jumpPoint;
        foreach (var piece in Pieces(level))
        {
            var b = piece.bounds;
            var off = px(b.center) - mouse;
            if (Mathf.Max(Mathf.Abs(off.x), Mathf.Abs(off.y)) > Mathf.Max(5f, Mathf.Max(b.size.x, b.size.y) / 2 / unitsPerPx)) continue;
            if (!onePiece) return GroupOf(piece.transform);
            var whole = PrefabUtility.GetOutermostPrefabInstanceRoot(piece.gameObject);
            return whole ? whole.transform : piece.transform;
        }
        foreach (var c in Find<GasCloud>(level)) if (c.Inside(world)) return c;
        foreach (var f in Find<DebrisField>(level)) if (f.Inside(OwnSpace(f, world))) return f;
        return null;
    }

    int GripAt(Vector2 mouse, System.Func<Vector3, Vector2> px)
    {
        if (!picked) return -1;
        var grips = Grips(picked);
        for (int i = 0; i < grips.Length; i++)
            if ((px(FromOwnSpace(picked, grips[i])) - mouse).magnitude <= GripPx) return i;
        return -1;
    }

    // Debris fields turn with their object; everything else lies square to the grid.
    static Vector2 OwnSpace(Component c, Vector3 world) => c is DebrisField ? c.transform.InverseTransformPoint(world) : world - CentreOf(c);
    static Vector3 FromOwnSpace(Component c, Vector2 p) => c is DebrisField ? c.transform.TransformPoint(p) : CentreOf(c) + (Vector3)p;

    static Vector2[] Corners(float w, float h) => new[] { new Vector2(w, h) / 2, new Vector2(-w, h) / 2, new Vector2(-w, -h) / 2, new Vector2(w, -h) / 2 };

    // the white squares for what is picked, in its own space: an enemy spawn's box, then its route's points
    static Vector2[] Grips(Component c)
    {
        if (c is Transform t)
        {
            if (t.GetComponent<SpriteRenderer>()) return new Vector2[0]; // the jump point: its size on the plot is QuestSystem's reach
            var b = BoundsOf(t);
            return Corners(b.size.x, b.size.y);
        }
        if (c is GasCloud cloud) return Corners(cloud.widthUnits, cloud.heightUnits);
        if (c is EnemySpawn spawn)
        {
            var box = spawn.sizeUnits.x > 0f && spawn.sizeUnits.y > 0f ? Corners(spawn.sizeUnits.x, spawn.sizeUnits.y) : new Vector2[0]; // 0 x 0: it always starts at the centre
            return box.Concat(spawn.route.Select(p => p - (Vector2)CentreOf(c))).ToArray();
        }
        if (!(c is DebrisField f)) return new Vector2[0];
        switch (f.shape)
        {
            case DebrisField.Shape.Circle: return new[] { new Vector2(f.radiusUnits, 0f) };
            case DebrisField.Shape.Ring: return new[] { new Vector2(f.radiusUnits, 0f), new Vector2(f.innerRadiusUnits, 0f) };
            case DebrisField.Shape.Band: return Corners(f.lengthUnits, f.widthUnits);
            default: return new[] { new Vector2(f.lengthUnits / 2, 0f), new Vector2(-f.lengthUnits / 2, 0f) };
        }
    }

    // A white square dragged to `at`, in its own space: the size comes from how far it is from the centre, and the centre stays put.
    static void SetSize(Component c, int grip, Vector2 at)
    {
        if (c is Transform t) { Stretch(t, at); return; }
        Undo.RecordObject(c, "Resize " + c.name);
        Vector2 size = new Vector2(Mathf.Abs(at.x), Mathf.Abs(at.y)) * 2;
        if (c is GasCloud cloud) { cloud.widthUnits = Mathf.Max(100f, size.x); cloud.heightUnits = Mathf.Max(100f, size.y); }
        else if (c is EnemySpawn spawn)
        {
            int route = grip - (Grips(c).Length - spawn.route.Length);
            if (route >= 0) spawn.route[route] = (Vector2)CentreOf(c) + at;
            else spawn.sizeUnits = size;
        }
        else if (c is DebrisField f)
        {
            if (f.shape == DebrisField.Shape.Band) { f.lengthUnits = Mathf.Max(1f, size.x); f.widthUnits = size.y; }
            else if (f.shape == DebrisField.Shape.Line) f.lengthUnits = Mathf.Max(1f, size.x);
            else if (grip == 1) f.innerRadiusUnits = Mathf.Min(at.magnitude, f.radiusUnits);
            else f.radiusUnits = Mathf.Max(1f, at.magnitude, f.shape == DebrisField.Shape.Ring ? f.innerRadiusUnits : 0f);
        }
    }

    // the picked field's pieces as Scatter would lay them now; worked out again only when a setting changes
    List<Vector3> LaidOut(DebrisField f)
    {
        string key = f.GetEntityId() + EditorJsonUtility.ToJson(f);
        if (key == previewKey) return preview;
        previewKey = key;
        preview.Clear();
        foreach (var p in f.Layout()) preview.Add(new Vector3(p.at.x, p.at.y, 0f));
        return preview;
    }

    // the picked field's shape, as a line
    static void Outline(DebrisField f, System.Func<Vector3, Vector2> px)
    {
        var pts = new List<Vector3>();
        switch (f.shape)
        {
            case DebrisField.Shape.Circle:
            case DebrisField.Shape.Ring:
                for (int i = 0; i <= 64; i++) { float a = i / 64f * Mathf.PI * 2; pts.Add(new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * f.radiusUnits); }
                break;
            case DebrisField.Shape.Band:
                float x = f.lengthUnits / 2, y = f.widthUnits / 2;
                pts.AddRange(new[] { new Vector3(-x, -y), new Vector3(x, -y), new Vector3(x, y), new Vector3(-x, y), new Vector3(-x, -y) });
                break;
            default:
                pts.Add(new Vector3(-f.lengthUnits / 2, 0)); pts.Add(new Vector3(f.lengthUnits / 2, 0));
                break;
        }
        Handles.color = Preview;
        Handles.DrawPolyLine(pts.Select(p => (Vector3)px(f.transform.TransformPoint(p))).ToArray());
        if (f.shape == DebrisField.Shape.Ring)
        {
            float inner = Mathf.Min(f.innerRadiusUnits, f.radiusUnits);
            Handles.DrawPolyLine(Enumerable.Range(0, 65).Select(i => { float a = i / 64f * Mathf.PI * 2; return (Vector3)px(f.transform.TransformPoint(new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * inner)); }).ToArray());
        }
    }

    static void Dot(Vector2 c, float size, Color col) => EditorGUI.DrawRect(new Rect(c.x - size / 2, c.y - size / 2, size, size), col);

    static void Frame(Vector2 a, Vector2 b, Color col)
    {
        EditorGUI.DrawRect(new Rect(a.x, a.y, b.x - a.x, 1), col);
        EditorGUI.DrawRect(new Rect(a.x, b.y, b.x - a.x, 1), col);
        EditorGUI.DrawRect(new Rect(a.x, a.y, 1, b.y - a.y), col);
        EditorGUI.DrawRect(new Rect(b.x, a.y, 1, b.y - a.y + 1), col);
    }

    static void Marker(Vector2 c, Color col, string letter)
    {
        Dot(c, 9, col);
        var style = new GUIStyle(EditorStyles.miniBoldLabel) { normal = { textColor = col } };
        GUI.Label(new Rect(c.x + 6, c.y - 8, 20, 16), letter, style);
    }

    // the battle space's half width and height, units: the level's Battle Space (decision 50), else the
    // board's (Step 65); 50,000 if no board is open
    static Vector2 ArenaHalfUnits(Scene level)
    {
        var space = Find<BattleSpace>(level).FirstOrDefault();
        if (space) return new Vector2(space.widthUnits / 2, space.heightUnits / 2);
        var board = FindAnyObjectByType<BoardManager>();
        if (!board) return new Vector2(50000, 50000);
        var so = new SerializedObject(board);
        var w = so.FindProperty("state.arenaWidthUnits");
        var h = so.FindProperty("state.arenaHeightUnits");
        return w != null && h != null ? new Vector2((float)w.doubleValue / 2, (float)h.doubleValue / 2) : new Vector2(50000, 50000);
    }

    // ---- the battle space ------------------------------------------------------------------

    // its size, typed in here; the map follows as you type
    static void DrawBattleSpace(Scene level)
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Battle space", EditorStyles.boldLabel);
        var space = Find<BattleSpace>(level).FirstOrDefault();
        if (!space)
        {
            var half = ArenaHalfUnits(level);
            EditorGUILayout.HelpBox("This level uses the board's battle space, " + (half.x * 2).ToString("N0") + " × " + (half.y * 2).ToString("N0") + " units. Press Battle space below to give it its own size.", MessageType.None);
            return;
        }
        var so = new SerializedObject(space);
        EditorGUILayout.PropertyField(so.FindProperty("widthUnits"), new GUIContent("Width, units"));
        EditorGUILayout.PropertyField(so.FindProperty("heightUnits"), new GUIContent("Height, units"));
        so.ApplyModifiedProperties();
    }

    // Spawns always start inside the battle space (owner, 2026-10-27): one outside, or in the turn-back
    // warning's band, moves to just inside the edge
    const float SpawnPastWarningUnits = 500f; // this far inside the board's warning band (arenaWarnUnits)
    static float SpawnInsetUnits()
    {
        var board = FindAnyObjectByType<BoardManager>();
        var warn = board ? new SerializedObject(board).FindProperty("state.arenaWarnUnits") : null;
        return (warn != null ? (float)warn.doubleValue : 1000f) + SpawnPastWarningUnits;
    }

    static void KeepSpawnsInside(Scene level)
    {
        var half = Vector2.Max(Vector2.zero, ArenaHalfUnits(level) - Vector2.one * SpawnInsetUnits());
        foreach (var t in Find<EnemySpawn>(level).Select(s => s.transform).Concat(Find<PlayerSpawn>(level).Select(s => s.transform)))
        {
            var p = t.position;
            var inside = new Vector3(Mathf.Clamp(p.x, -half.x, half.x), Mathf.Clamp(p.y, -half.y, half.y), p.z);
            if (inside == p) continue;
            Undo.RecordObject(t, "Keep a spawn inside the battle space");
            t.position = inside;
            EditorSceneManager.MarkSceneDirty(level);
        }
    }

    // one per level, so a second press finds the one it has
    static void AddBattleSpace(Scene level)
    {
        var space = Find<BattleSpace>(level).FirstOrDefault();
        if (space) { Selection.activeGameObject = space.gameObject; return; }
        Add<BattleSpace>(level, "Battle Space");
        Selection.activeGameObject.transform.position = Vector3.zero; // its edge is always round the grid's centre
    }

    // One piece grows or shrinks. A group's pieces spread out or draw in round its centre, each keeping its own size.
    static void Stretch(Transform t, Vector2 at)
    {
        var b = BoundsOf(t);
        float byX = Mathf.Max(0.05f, Mathf.Abs(at.x) / Mathf.Max(b.extents.x, 1f)), byY = Mathf.Max(0.05f, Mathf.Abs(at.y) / Mathf.Max(b.extents.y, 1f));
        if (t.GetComponent<Renderer>() || PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject))
        {
            Undo.RecordObject(t, "Resize " + t.name);
            t.localScale *= Mathf.Max(byX, byY);
            return;
        }
        foreach (Transform piece in t)
        {
            Undo.RecordObject(piece, "Resize " + t.name);
            var off = piece.position - b.center;
            piece.position = new Vector3(b.center.x + off.x * byX, b.center.y + off.y * byY, piece.position.z);
        }
    }

    // ---- what is in the level -----------------------------------------------------------------

    static IEnumerable<Component> Parts(Scene level) =>
        Find<PlayerSpawn>(level).Cast<Component>().Concat(Find<EnemySpawn>(level)).Concat(Find<Mission>(level).Where(m => m.jumpPoint).Select(m => m.jumpPoint)).Concat(Find<GasCloud>(level)).Concat(Find<DebrisField>(level))
            .Concat(Pieces(level).Select(p => GroupOf(p.transform))).Distinct();

    // Everything that can be picked, then the picked thing's position, its settings and Delete.
    void DrawParts(Scene level)
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("In this level", EditorStyles.boldLabel);
        foreach (var c in Parts(level).ToList())
        {
            bool on = c == picked;
            string label = c is DebrisField f ? f.name + "  (" + f.shape + ", " + f.transform.childCount + " pieces)"
                : c is Transform && c.transform.childCount > 0 ? c.name + "  (" + c.transform.childCount + " pieces)" : c.name;
            if (GUILayout.Toggle(on, label, EditorStyles.radioButton) && !on) Pick(c);
        }
        if (!picked || picked.gameObject.scene != level) return;
        EditorGUILayout.Space(4);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField(picked.name, EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            var at = EditorGUILayout.Vector2Field("Position, units (x east, y north)", CentreOf(picked));
            if (EditorGUI.EndChangeCheck())
            {
                Move(picked, at);
                EditorSceneManager.MarkSceneDirty(level);
            }
            if (!(picked is Transform))
            {
                Editor.CreateCachedEditor(picked, null, ref pickedEditor);
                pickedEditor.OnInspectorGUI();
            }
            if (GUILayout.Button("Delete " + picked.name))
            {
                Undo.DestroyObjectImmediate(picked.gameObject);
                picked = null;
                EditorSceneManager.MarkSceneDirty(level);
                GUIUtility.ExitGUI();
            }
        }
    }

    static IEnumerable<T> Find<T>(Scene level) where T : Component =>
        level.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<T>(true));

    // ---- levels ------------------------------------------------------------------------------

    static string[] Levels() => !AssetDatabase.IsValidFolder(TacticalWorld.LevelFolder) ? new string[0] :
        AssetDatabase.FindAssets("t:Scene", new[] { TacticalWorld.LevelFolder }).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToArray();

    static Scene OpenLevelScene()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.path.StartsWith(TacticalWorld.LevelFolder + "/")) return s;
        }
        return default;
    }

    // the board with the level on top, as at play. The board stays the active scene, so its lighting
    // and settings are the ones that play
    static void OpenLevel(string path)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(Board, OpenSceneMode.Single);
        EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        Look(Vector3.zero, WholeMapUnits);
    }

    static void NewLevel()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (!AssetDatabase.IsValidFolder(TacticalWorld.LevelFolder)) AssetDatabase.CreateFolder("Assets/Scenes", "Levels");
        int n = 1;
        string path;
        do path = $"{TacticalWorld.LevelFolder}/Level_{n++:00}.unity"; while (File.Exists(path));
        EditorSceneManager.OpenScene(Board, OpenSceneMode.Single);
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        EditorSceneManager.SaveScene(scene, path);
        // a level has to be in Build Settings to load at play
        EditorBuildSettings.scenes = EditorBuildSettings.scenes.Append(new EditorBuildSettingsScene(path, true)).ToArray();
        AssetDatabase.SaveAssets();
        Look(Vector3.zero, WholeMapUnits);
    }

    // where the Scene view is looking, so it lands on screen; drag it from there
    static void Add<T>(Scene level, string name) where T : Component
    {
        var view = SceneView.lastActiveSceneView;
        var at = view ? view.pivot : Vector3.zero;
        var go = new GameObject(name, typeof(T));
        SceneManager.MoveGameObjectToScene(go, level);
        go.transform.position = new Vector3(at.x, at.y, 0f);
        Undo.RegisterCreatedObjectUndo(go, "Add " + name);
        EditorSceneManager.MarkSceneDirty(level);
        Selection.activeGameObject = go;
    }

    // Step 73: one per level, so a second press finds the one it has
    static void AddPlayerSpawn(Scene level)
    {
        foreach (var root in level.GetRootGameObjects())
        {
            var start = root.GetComponentInChildren<PlayerSpawn>(true);
            if (!start) continue;
            Selection.activeGameObject = start.gameObject;
            if (SceneView.lastActiveSceneView) SceneView.lastActiveSceneView.LookAt(start.transform.position);
            return;
        }
        Add<PlayerSpawn>(level, "Player Spawn");
    }

    // Step 77: the kit's pieces and rock, scattered at once so the field shows
    static void AddDebrisField(Scene level)
    {
        Add<DebrisField>(level, "Debris Field");
        var f = Selection.activeGameObject.GetComponent<DebrisField>();
        f.pieces = new[] { "Pieces/Block", "Pieces/Drum", "Pieces/Girder", "Pieces/Plate", "Rock" }
            .Select(p => AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Terrain/" + p + ".prefab")).Where(p => p).ToArray();
        DebrisFieldEditor.Scatter(f);
    }

    // seen from above, north up
    static void Look(Vector3 at, float sizeUnits)
    {
        var view = SceneView.lastActiveSceneView;
        if (!view) return;
        view.in2DMode = true;
        view.LookAt(at, Quaternion.identity, sizeUnits);
    }
}
