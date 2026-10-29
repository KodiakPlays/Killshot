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
/// 2026-10-27): the battle space's edge, every piece of debris as a dot, the spawn boxes. Pick a debris field
/// (click it on the map, or in the list) and its settings show under the map; the map shows, in amber, how
/// Scatter would lay it out with those settings, before you press it. Add parts with the buttons at the bottom.
/// World units: x east, y north.
/// </summary>
public class LevelWindow : EditorWindow
{
    const string Board = "Assets/Scenes/Tactical/02.1_Grid.unity";
    const float WholeMapUnits = 45000f; // the Scene view's size when a level opens: the whole battle space, top to bottom

    static readonly Color Ground = new Color(0.04f, 0.05f, 0.06f), Edge = new Color(0.55f, 0.6f, 0.65f),
        Debris = new Color(0.6f, 0.63f, 0.66f), Preview = new Color(0.91f, 0.64f, 0.24f),
        Enemy = new Color(1f, 0.35f, 0.25f), Player = new Color(0.91f, 0.93f, 0.94f);

    Vector2 scroll;
    DebrisField picked;
    Editor pickedEditor;
    string previewKey;
    readonly List<Vector3> preview = new List<Vector3>(); // the picked field's layout as Scatter would make it now, its own space

    [MenuItem("Starwolf/Level")]
    static void Open() => GetWindow<LevelWindow>("Starwolf Level");

    void OnFocus() => Repaint();
    void OnHierarchyChange() => Repaint();
    void OnSelectionChange()
    {
        var f = Selection.activeGameObject ? Selection.activeGameObject.GetComponentInParent<DebrisField>() : null;
        if (f) picked = f;
        Repaint();
    }

    void OnGUI()
    {
        // a level can't be opened or changed while the game runs
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorGUILayout.HelpBox("Stop the game to open or edit a level.", MessageType.Info);
            return;
        }
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

        var level = OpenLevelScene();
        if (level.IsValid())
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(level.name + " from above", EditorStyles.boldLabel);
            if (Event.current.type == EventType.Layout) KeepSpawnsInside(level);
            DrawMap(level);
            EditorGUILayout.LabelField("Grey: debris.  Amber: the picked field, as Scatter would lay it.  Red: enemy spawn.  White: player spawn.  Line: the battle space's edge.", EditorStyles.wordWrappedMiniLabel);
            DrawBattleSpace(level);
            DrawFields(level);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField(level.IsValid() ? "Add to " + level.name : "Open a level to add to it", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(!level.IsValid()))
        {
            if (GUILayout.Button("Enemy spawn")) Add<EnemySpawn>(level, "Enemy Spawn");
            if (GUILayout.Button("Player spawn")) AddPlayerSpawn(level);
            if (GUILayout.Button("Battle space")) AddBattleSpace(level);
            if (GUILayout.Button("Debris field")) { AddDebrisField(level); picked = Selection.activeGameObject.GetComponent<DebrisField>(); }
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
        float kmPerPx = Mathf.Max(half.x * 2 / r.width, half.y * 2 / r.height) * 1.04f; // the whole battle space, a little margin
        System.Func<Vector3, Vector2> px = p => new Vector2(r.width / 2 + p.x / kmPerPx, r.height / 2 - p.y / kmPerPx); // inside the map's clip

        // a click on a debris field picks it
        var e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0 && r.Contains(e.mousePosition))
        {
            var world = new Vector3((e.mousePosition.x - r.center.x) * kmPerPx, (r.center.y - e.mousePosition.y) * kmPerPx, 0f);
            foreach (var f in Find<DebrisField>(level))
            {
                var local = f.transform.InverseTransformPoint(world);
                if (!f.Inside(new Vector2(local.x, local.y))) continue;
                picked = f;
                Selection.activeGameObject = f.gameObject;
                e.Use();
                break;
            }
        }
        if (e.type != EventType.Repaint) return;

        GUI.BeginClip(r); // anything outside the battle space stays inside the map's frame
        EditorGUI.DrawRect(new Rect(0, 0, r.width, r.height), Ground);
        Frame(px(new Vector3(-half.x, half.y)), px(new Vector3(half.x, -half.y)), Edge);
        int terrain = LayerMask.NameToLayer("WorldTerrain");
        foreach (var root in level.GetRootGameObjects())
            foreach (var rend in root.GetComponentsInChildren<Renderer>(true))
            {
                if (rend.gameObject.layer != terrain) continue;
                var b = rend.bounds;
                Dot(px(b.center), Mathf.Max(2f, Mathf.Max(b.size.x, b.size.y) / kmPerPx), Debris);
            }
        if (picked && picked.gameObject.scene == level)
        {
            Outline(picked, px);
            foreach (var p in LaidOut(picked)) Dot(px(picked.transform.TransformPoint(p)), 2.5f, Preview);
        }
        foreach (var s in Find<EnemySpawn>(level)) Marker(px(s.transform.position), Enemy, "E");
        foreach (var s in Find<PlayerSpawn>(level)) Marker(px(s.transform.position), Player, "P");
        GUI.EndClip();
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
    const float SpawnInsetUnits = 3000f; // past the board's warning band (arenaWarnUnits, 2,500)
    static void KeepSpawnsInside(Scene level)
    {
        var half = Vector2.Max(Vector2.zero, ArenaHalfUnits(level) - Vector2.one * SpawnInsetUnits);
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

    // ---- the debris fields -----------------------------------------------------------------

    void DrawFields(Scene level)
    {
        var fields = Find<DebrisField>(level).ToList();
        if (fields.Count == 0) { EditorGUILayout.HelpBox("No debris fields in this level yet. Add one with the Debris field button below.", MessageType.None); return; }
        EditorGUILayout.LabelField("Debris fields", EditorStyles.boldLabel);
        foreach (var f in fields)
        {
            bool on = f == picked;
            if (GUILayout.Toggle(on, f.name + "  (" + f.shape + ", " + f.transform.childCount + " pieces)", EditorStyles.radioButton) && !on)
            {
                picked = f;
                Selection.activeGameObject = f.gameObject;
            }
        }
        if (!picked || picked.gameObject.scene != level) return;
        EditorGUILayout.Space(4);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            Editor.CreateCachedEditor(picked, typeof(DebrisFieldEditor), ref pickedEditor);
            pickedEditor.OnInspectorGUI();
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
