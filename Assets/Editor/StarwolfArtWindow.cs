// =================================================================================================
// StarwolfArtWindow.cs
//
// What it does:
// This is the Art window (Starwolf > Art). It lists every picture in one place: first the prefabs,
// then the open scenes. You swap a picture by dropping a new one in its box.
//
// How it works:
// - It reads every prefab in Assets/UI/Board/Panels and Assets/UI/Menus. The menus include the
//   Escape menu. For each of our scripts on them, it lists every picture slot: the Art lists, and
//   the pictures the code swaps in, such as rocker states and keys.
// - It reads each open scene. It lists picture slots on our scripts. It lists images whose
//   picture is not on an Art list. Anything in a scene that is a copy of a prefab is left out.
// - Each group is a fold-out with a count. Each row shows the part's name and the file name. It
//   shows how many places use that file, and a box with the picture.
// - A Find box filters by panel, part or file name. "Waiting for art" shows only empty boxes and
//   stand-ins.
// - Buttons open each closed scene from Assets/Scenes/Tactical in Build Settings. The pictures in
//   a scene show only while it is open.
// - You drop in a new picture. If the old one is used in other places, it asks: only here,
//   everywhere, or cancel.
// - Then it saves the prefab, or marks the scene as changed. Nothing is moved.
// - The list is rebuilt when a scene opens or the project or scene changes. Pictures cannot be
//   changed in play mode.
//
// Technical:
// - Opened from the menu, Starwolf > Art. No other script calls it.
// - Folders are constants at the top: PrefabFolders and SceneFolder (Assets/Scenes/Tactical/).
// - A Row is one picture slot: a SerializedObject and a property path. Its type is Sprite,
//   Texture or Texture2D.
// - "Our scripts" means scripts whose file is under Assets/Scripts/. RawImages with a fixed picture
//   are listed too. Render textures are skipped.
// - The Art list is the "art" field (Panel.ArtSlot[]) on Panel and ArtList.
// - Set() writes the new picture. That runs the script's OnValidate, and Panel.ApplyArt updates
//   its images.
// - A prefab row saves the prefab at once. A scene row only marks the scene changed. Save it by
//   hand.
// - "Everywhere" changes every row of the same type that shows the same old picture.
// - The change and its question box run on EditorApplication.delayCall, after the window draws.
// - Trap: "Waiting for art" also shows any row whose name has "stand-in" in it.
// =================================================================================================

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class StarwolfArtWindow : EditorWindow
{
    [MenuItem("Starwolf/Art")]
    static void Open() => GetWindow<StarwolfArtWindow>("Starwolf Art");

    // the board's panels, and the menus shown on several screens (the Escape menu)
    static readonly string[] PrefabFolders = { "Assets/UI/Board/Panels", "Assets/UI/Menus" };
    const string SceneFolder = "Assets/Scenes/Tactical/";

    class Row
    {
        public string group, label, path;
        public SerializedObject so;
        public System.Type type;
        public GameObject prefabRoot; // null for a picture in a scene
        public Object Value => so.FindProperty(path)?.objectReferenceValue;
    }

    List<Row> rows;
    bool stale = true, waitingOnly;
    string find = "";
    Vector2 scroll;
    readonly Dictionary<string, bool> folds = new Dictionary<string, bool>();

    void OnEnable() => EditorSceneManager.sceneOpened += SceneOpened;
    void OnDisable() => EditorSceneManager.sceneOpened -= SceneOpened;
    void SceneOpened(UnityEngine.SceneManagement.Scene s, OpenSceneMode m) { stale = true; Repaint(); }
    void OnHierarchyChange() { stale = true; Repaint(); }
    void OnProjectChange() { stale = true; Repaint(); }

    void OnGUI()
    {
        // IMGUI draws twice (Layout, then Repaint); both must draw the same rows, so rebuild on Layout only
        if (Event.current.type == EventType.Layout && (stale || rows == null)) { rows = Collect(); stale = false; }
        if (rows == null) return;
        if (rows.Any(r => !r.so.targetObject)) { stale = true; Repaint(); return; }

        bool playing = EditorApplication.isPlaying;
        if (playing) EditorGUILayout.HelpBox("Stop play mode to change pictures.", MessageType.Info);
        find = EditorGUILayout.TextField(new GUIContent("Find", "A panel, a part or a file name."), find);
        waitingOnly = EditorGUILayout.Toggle(new GUIContent("Waiting for art", "Only empty boxes and stand-ins."), waitingOnly);
        OpenSceneButtons();
        EditorGUILayout.Space(4);

        var uses = new Dictionary<Object, int>();
        foreach (var r in rows) { r.so.Update(); var v = r.Value; if (v) uses[v] = uses.TryGetValue(v, out int n) ? n + 1 : 1; }

        scroll = EditorGUILayout.BeginScrollView(scroll);
        using (new EditorGUI.DisabledScope(playing))
            foreach (var g in rows.GroupBy(r => r.group))
            {
                var shown = g.Where(Matches).ToList();
                if (shown.Count == 0) continue;
                bool forced = find != "" || waitingOnly;
                folds.TryGetValue(g.Key, out bool open);
                open = EditorGUILayout.Foldout(forced || open, $"{g.Key}   ({shown.Count})", true, EditorStyles.foldoutHeader);
                if (!forced) folds[g.Key] = open;
                if (!open) continue;
                foreach (var r in shown) DrawRow(r, uses);
                EditorGUILayout.Space(6);
            }
        EditorGUILayout.EndScrollView();
    }

    void DrawRow(Row r, Dictionary<Object, int> uses)
    {
        var v = r.Value;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(r.label, EditorStyles.boldLabel);
        string file = !v ? "empty" : uses[v] > 1 ? $"{v.name}   ·   used in {uses[v]} places" : v.name;
        EditorGUILayout.LabelField(file, EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
        var pick = EditorGUILayout.ObjectField(v, r.type, false, GUILayout.Width(52), GUILayout.Height(52));
        EditorGUILayout.EndHorizontal();
        // the dialog and the save run after this draw, not inside it
        if (pick != v) EditorApplication.delayCall += () => Change(r, v, pick);
    }

    bool Matches(Row r)
    {
        var v = r.Value;
        if (waitingOnly && v && r.label.IndexOf("stand-in", System.StringComparison.OrdinalIgnoreCase) < 0) return false;
        if (find == "") return true;
        bool Has(string s) => s.IndexOf(find, System.StringComparison.OrdinalIgnoreCase) >= 0;
        return Has(r.group) || Has(r.label) || (v && Has(v.name));
    }

    // A scene's own pictures show only while it is open: one button per closed scene in the build list.
    void OpenSceneButtons()
    {
        var closed = EditorBuildSettings.scenes.Select(s => s.path)
            .Where(p => p.StartsWith(SceneFolder) && !EditorSceneManager.GetSceneByPath(p).isLoaded).ToList();
        if (closed.Count == 0) return;
        EditorGUILayout.LabelField("A scene's own pictures show when it is open:", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();
        foreach (var p in closed)
            if (GUILayout.Button(System.IO.Path.GetFileNameWithoutExtension(p), EditorStyles.miniButton) &&
                !EditorApplication.isPlaying && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(p);
                GUIUtility.ExitGUI();
            }
        EditorGUILayout.EndHorizontal();
    }

    void Change(Row row, Object old, Object pick)
    {
        var targets = new List<Row> { row };
        if (old)
        {
            var others = rows.Where(r => r != row && r.type == row.type && r.Value == old).ToList();
            if (others.Count > 0)
            {
                int c = EditorUtility.DisplayDialogComplex("Change the picture",
                    $"{old.name} is also used in {others.Count} other {(others.Count == 1 ? "place" : "places")}.",
                    "Only here", "Cancel", $"Everywhere ({others.Count + 1})");
                if (c == 1) return;
                if (c == 2) targets.AddRange(others);
            }
        }
        foreach (var r in targets) Set(r, pick);
        Repaint();
    }

    // Applying the change runs the script's OnValidate: an Art list puts the picture on its images.
    static void Set(Row r, Object pick)
    {
        r.so.Update();
        r.so.FindProperty(r.path).objectReferenceValue = pick;
        r.so.ApplyModifiedProperties();
        if (r.prefabRoot) PrefabUtility.SavePrefabAsset(r.prefabRoot);
        else EditorSceneManager.MarkSceneDirty(((Component)r.so.targetObject).gameObject.scene);
    }

    static List<Row> Collect()
    {
        var list = new List<Row>();
        foreach (var path in AssetDatabase.FindAssets("t:Prefab", PrefabFolders).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p))
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (root) Add(list, root.name, root.GetComponentsInChildren<Component>(true), root);
        }
        // a scene's board panels are copies of the prefabs above: leave them out
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var scene = EditorSceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;
            var parts = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Component>(true))
                .Where(c => c && !PrefabUtility.IsPartOfPrefabInstance(c)).ToArray();
            Add(list, "Scene · " + scene.name, parts, null);
        }
        return list;
    }

    static void Add(List<Row> list, string group, Component[] parts, GameObject prefabRoot)
    {
        var onArtList = new HashSet<Object>();
        foreach (var c in parts)
        {
            bool ours = c is MonoBehaviour mb &&
                        (AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(mb)) ?? "").StartsWith("Assets/Scripts/");
            // a raw image with no picture, or a live one (a render texture), is filled by its code:
            // the ships drawn from their models, the board's own screen
            if (!ours && !(c is RawImage ri && ri.texture && !(ri.texture is RenderTexture))) continue;
            var so = new SerializedObject(c);
            var p = so.GetIterator();
            while (p.Next(true))
            {
                if (p.propertyType != SerializedPropertyType.ObjectReference) continue;
                if (p.propertyPath.StartsWith("art.Array") && p.objectReferenceValue is Image) onArtList.Add(p.objectReferenceValue);
                var type = PictureType(p.type);
                if (type == null) continue;
                list.Add(new Row { group = group, label = Label(c, p, prefabRoot), path = p.propertyPath, so = so, type = type, prefabRoot = prefabRoot });
            }
        }
        // in a scene, an image with a picture and no Art list (prefab images like that are set by their code)
        if (prefabRoot) return;
        foreach (var img in parts.OfType<Image>())
            if (img.sprite && !onArtList.Contains(img))
                list.Add(new Row { group = group, label = img.name, path = "m_Sprite", so = new SerializedObject(img), type = typeof(Sprite) });
    }

    // "PPtr<Sprite>", "PPtr<$Texture2D>" …
    static System.Type PictureType(string t)
    {
        t = t.Replace("PPtr<", "").Replace(">", "").Replace("$", "");
        return t == "Sprite" ? typeof(Sprite) : t == "Texture" ? typeof(Texture) : t == "Texture2D" ? typeof(Texture2D) : null;
    }

    // An Art list entry shows its own name; a state shows its entry and box ("REBOOT · Thrown").
    static string Label(Component c, SerializedProperty p, GameObject root)
    {
        if (c is RawImage) return c.name;
        string label = p.displayName;
        int cut = p.propertyPath.LastIndexOf(".Array.data[");
        if (cut >= 0)
        {
            string entryPath = p.propertyPath.Substring(0, p.propertyPath.IndexOf(']', cut) + 1);
            var entry = p.serializedObject.FindProperty(entryPath);
            var n = entry.FindPropertyRelative("name") ?? entry.FindPropertyRelative("id");
            string entryName = n != null && n.propertyType == SerializedPropertyType.String && n.stringValue != "" ? n.stringValue : entry.displayName;
            label = p.propertyPath.StartsWith("art.Array") || entryPath == p.propertyPath ? entryName : entryName + " · " + label;
        }
        return c.gameObject == root ? label : c.name + " · " + label;
    }
}
