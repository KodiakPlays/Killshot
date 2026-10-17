using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Starwolf.Intro.Editor
{
    public static class StarwolfIntroTools
    {
        const string Root = "Assets/StarwolfIntro/";
        [MenuItem("Tools/Starwolf/Create Intro Scene")]
        public static void CreateScene()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string path = Root + "Scenes/StarwolfIntro.unity";
            if (File.Exists(path))
            {
                EditorSceneManager.OpenScene(path);
                return;
            }
            Directory.CreateDirectory(Root + "Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Display Camera", typeof(Camera), typeof(AudioListener));
            var cam = camera.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
            cam.cullingMask = 0; cam.orthographic = true;
            camera.tag = "MainCamera";
            var controller = new GameObject("STARWOLF Intro", typeof(StarwolfIntroController)).GetComponent<StarwolfIntroController>();
            controller.glyphData = AssetDatabase.LoadAssetAtPath<TextAsset>(Root + "Resources/StarwolfIntro/glyphs.json");
            controller.displayShader = AssetDatabase.LoadAssetAtPath<Shader>(Root + "Shaders/PhosphorDisplay.shader");
            controller.hardwareHum = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "Resources/StarwolfIntro/hardware-hum.wav");
            if (!controller.glyphData || !controller.displayShader || !controller.hardwareHum) throw new Exception("Missing intro assets.");
            if (!EditorSceneManager.SaveScene(scene, path)) throw new Exception("Scene save failed.");
            Selection.activeGameObject = controller.gameObject;
        }
        [MenuItem("Tools/Starwolf/Validate Shared Data")]
        public static void Validate()
        {
            var data = PhosphorData.Load(AssetDatabase.LoadAssetAtPath<TextAsset>(Root + "Resources/StarwolfIntro/glyphs.json"));
            foreach (var word in new[] { "SECTION 36", "INITIALISING...", "STARWOLF", "NEW GAME", "LOAD GAME", "SETTINGS" })
            {
                var cells = data.Cells(word, word == "SECTION 36" || word == "STARWOLF", out var size);
                if (cells.Count != cells.Distinct().Count()) throw new Exception("Duplicate cells: " + word);
                if (cells.Any(p => p.x < 0 || p.y < 0 || p.x >= size.x || p.y >= size.y)) throw new Exception("Out of bounds: " + word);
            }
            foreach (char c in "SCO36")
            {
                var glyph = data.Get(c);
                if (glyph.rows[0].Any(x => x != '1') || glyph.rows[9].Any(x => x != '1'))
                    throw new Exception("Corner or terminal cut detected: " + c);
            }
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(Root + "Shaders/PhosphorDisplay.shader");
            if (ShaderUtil.ShaderHasError(shader)) throw new Exception("Display shader has errors.");
            Debug.Log("STARWOLF: shared matrices, square corners, cell bounds and shader validation passed.");
        }
        // Safe for an isolated validation project. Does not alter the user's Build Settings.
        public static void BuildHandoff()
        {
            Validate(); CreateScene();
            AssetDatabase.ExportPackage(Root.TrimEnd('/'), "Starwolf-Intro.unitypackage", ExportPackageOptions.Recurse);
            Debug.Log("STARWOLF_HANDOFF_VALIDATED");
        }
    }
}
