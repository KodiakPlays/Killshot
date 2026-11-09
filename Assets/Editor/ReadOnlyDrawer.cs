using UnityEditor;
using UnityEngine;

/// <summary>
/// Renders fields marked [ReadOnly] as greyed-out and non-editable in the inspector.
/// </summary>
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}

/// <summary>
/// A text field marked [Scene] shows as a scene slot. Dropping a scene in keeps its name and adds it to the build's
/// scene list if it isn't there, so the game can load it.
/// </summary>
[CustomPropertyDrawer(typeof(SceneAttribute))]
public class SceneDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string name = property.stringValue;
        SceneAsset now = null;
        if (name != "")
            foreach (var guid in AssetDatabase.FindAssets(name + " t:Scene"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == name) { now = AssetDatabase.LoadAssetAtPath<SceneAsset>(path); break; }
            }
        if (name != "" && now == null) label = new GUIContent(label.text + " (no scene called " + name + ")", label.tooltip);

        EditorGUI.BeginChangeCheck();
        var picked = (SceneAsset)EditorGUI.ObjectField(position, label, now, typeof(SceneAsset), false);
        if (!EditorGUI.EndChangeCheck()) return;
        property.stringValue = picked ? picked.name : "";
        if (!picked) return;

        string pickedPath = AssetDatabase.GetAssetPath(picked);
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes) if (s.path == pickedPath) { s.enabled = true; EditorBuildSettings.scenes = scenes; return; }
        ArrayUtility.Add(ref scenes, new EditorBuildSettingsScene(pickedPath, true));
        EditorBuildSettings.scenes = scenes;
    }
}
