using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Starwolf → Tuning: every value marked [Tune("Section")] on the open scene's components, in one
/// window, grouped by section (owner, 2026-10-17). Edits go straight to the real values. Changes
/// made in play mode are normally lost when play stops; tick "Keep play-mode changes" to copy them
/// back afterwards.
/// </summary>
public class StarwolfTuningWindow : EditorWindow
{
    [MenuItem("Starwolf/Tuning")]
    static void Open() => GetWindow<StarwolfTuningWindow>("Starwolf Tuning");

    class Entry { public SerializedObject so; public string path; public string section; public string owner; public string label; }

    Vector2 scroll;
    readonly Dictionary<string, bool> folds = new Dictionary<string, bool>();
    List<Entry> entries;
    // IMGUI draws each frame twice (Layout, then Repaint) and both passes must draw the same controls,
    // so what to draw is decided on Layout only: the list is rebuilt there, and play mode is read there
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
            if (stale || entries == null) { entries = Collect(); stale = false; }
        }
        if (entries == null) return;
        if (playing)
        {
            EditorGUILayout.HelpBox("Play mode: changes here are lost when play stops, unless you tick the box below.", MessageType.Warning);
            keepPlayChanges = EditorGUILayout.ToggleLeft("Keep play-mode changes when play stops", keepPlayChanges);
        }
        if (entries.Count == 0) { EditorGUILayout.HelpBox("Nothing tunable in the open scene. Open 02.1_Grid.", MessageType.Info); return; }

        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var group in entries.GroupBy(e => e.section).OrderBy(g => g.Key))
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
        EditorGUILayout.EndScrollView();
    }

    static string Label(Entry e)
    {
        // "buses.Array.data[2].signalPerUnit" (PowerSystem) → "Signal Per Unit — SEN"
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

    // ---- keep play-mode changes ---------------------------------------------------------------

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
