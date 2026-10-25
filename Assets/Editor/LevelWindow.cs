using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Starwolf → Level (owner, 2026-10-25). Each level is a small scene in Assets/Scenes/Levels, loaded
/// on top of the board at play (TacticalWorld). Open one here to lay it out in the Scene view, seen
/// from above (x east, y north, 1 unit = 1 km), and add parts to it.
/// </summary>
public class LevelWindow : EditorWindow
{
    const string Board = "Assets/Scenes/Tactical/02.1_Grid.unity";
    const float WholeMapKm = 45000f; // the Scene view's size when a level opens: the whole battle space, top to bottom

    [MenuItem("Starwolf/Level")]
    static void Open() => GetWindow<LevelWindow>("Starwolf Level");

    void OnFocus() => Repaint();

    void OnGUI()
    {
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

        EditorGUILayout.Space(10);
        var level = OpenLevelScene();
        EditorGUILayout.LabelField(level.IsValid() ? "Add to " + level.name : "Open a level to add to it", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(!level.IsValid()))
            if (GUILayout.Button("Enemy spawn")) AddEnemySpawn(level);
    }

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
        Look(Vector3.zero, WholeMapKm);
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
        Look(Vector3.zero, WholeMapKm);
    }

    // where the Scene view is looking, so it lands on screen; drag it from there
    static void AddEnemySpawn(Scene level)
    {
        var view = SceneView.lastActiveSceneView;
        var at = view ? view.pivot : Vector3.zero;
        var go = new GameObject("Enemy Spawn", typeof(EnemySpawn));
        SceneManager.MoveGameObjectToScene(go, level);
        go.transform.position = new Vector3(at.x, at.y, 0f);
        Undo.RegisterCreatedObjectUndo(go, "Add enemy spawn");
        EditorSceneManager.MarkSceneDirty(level);
        Selection.activeGameObject = go;
    }

    // seen from above, north up
    static void Look(Vector3 at, float sizeKm)
    {
        var view = SceneView.lastActiveSceneView;
        if (!view) return;
        view.in2DMode = true;
        view.LookAt(at, Quaternion.identity, sizeKm);
    }
}
