// =================================================================================================
// ReadOnlyDrawer.cs
//
// What it does:
// This file changes how two kinds of marked settings look in the Inspector. A setting marked
// [ReadOnly] shows greyed out and cannot be edited. A text setting marked [Scene] shows as a slot
// you can drop a scene into. Both work in the editor only.
//
// How it works:
// - ReadOnlyDrawer turns editing off, draws the setting as normal, then turns editing back on.
//   GameManager uses [ReadOnly] for its enemy count, game over and player alive values.
// - SceneDrawer reads the scene name typed in the setting. It looks for a scene with that name.
//   If none is found, the label adds "(no scene called ...)".
// - When you drop a scene into the slot, it saves the scene's name in the setting.
// - It makes sure that scene is in Build Settings and switched on. If it is missing, it adds
//   it.
// - The Campaign file uses [Scene] for the level slot of each mission.
//
// Technical:
// - Two drawers in one file: ReadOnlyDrawer for ReadOnlyAttribute, SceneDrawer for SceneAttribute.
// - ReadOnlyAttribute is declared at the end of GameManager.cs. SceneAttribute is declared in
//   Campaign.cs.
// - ReadOnlyDrawer sets GUI.enabled to false around a normal PropertyField. Lists and nested
//   values keep their full height.
// - Users: GameManager's three [ReadOnly] values, and Campaign.Mission.level for [Scene].
// - SceneDrawer keeps only the scene's file name in the string. It finds the scene with
//   AssetDatabase.FindAssets and an exact name match.
// - Trap: two scenes with the same file name in different folders. It takes the first one found.
// - A dropped scene goes into EditorBuildSettings.scenes, switched on. Clearing the slot leaves
//   the scene in Build Settings.
// =================================================================================================

using UnityEditor;
using UnityEngine;

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
