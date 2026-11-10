// =================================================================================================
// DebrisFieldEditor.cs
//
// What it does:
// This is the Inspector for a Debris Field in a level. It shows the field's settings and adds two
// buttons, Scatter and Reshuffle. It is an editor tool. The game itself does not use it.
//
// How it works:
// - It draws the normal Debris Field settings, then the two buttons under them.
// - Scatter deletes every piece under the field, hand-moved ones too. It asks DebrisField for a
//   fresh layout from the same seed. Then it places a copy of a piece prefab at each spot.
// - Each copy is turned and sized to the layout. It measures how wide the prefab's model is
//   across. Then it scales the copy to the size the layout asks for.
// - Reshuffle picks a new random seed, then does a Scatter.
// - All of this is one undo step. It marks the scene as changed.
// - A note under the buttons shows how many pieces were placed, out of how many asked for. It
//   turns into a warning when the gap between pieces left no room for the rest.
// - The Level window calls Scatter when it adds a new debris field. It calls it again after you
//   resize one on its map.
// - It has a self-test. It tries every shape and spread on a hidden field. It checks that each
//   piece is inside the shape and no two are closer than the gap. It checks that the same seed
//   gives the same layout. No other script calls this test.
//
// Technical:
// - [CustomEditor(typeof(DebrisField))] makes Unity draw this Inspector for every DebrisField.
// - Scatter(DebrisField) is public and static. LevelWindow calls it on Add and after a resize drag.
// - Scatter reads DebrisField.Layout(). Each piece gives a prefab, a spot (at), turnDeg and
//   sizeUnits.
// - Copies stay linked to their prefab. They go under the field, at local x and y, with z at 0.
// - Scale is the prefab's scale times sizeUnits, divided by its width. Across() finds that width.
// - Across() measures the prefab's meshes seen from above, in units. No mesh counts as 1 unit wide.
// - Trap: Scatter deletes every child of the field. Anything else placed under it goes too.
// - Reshuffle sets DebrisField.seed to a random whole number of 1 or more.
// - Numbers: count, shape, spread, gap, sizes and seed are on the DebrisField, in the Inspector.
// - Check() returns "ok" or what failed. It uses a hidden object that is never saved.
// =================================================================================================

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(DebrisField))]
public class DebrisFieldEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var f = (DebrisField)target;
        EditorGUILayout.Space(10);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Scatter", GUILayout.Height(30))) Scatter(f);
            if (GUILayout.Button("Reshuffle", GUILayout.Height(30)))
            {
                Undo.RecordObject(f, "Reshuffle debris");
                f.seed = Random.Range(1, int.MaxValue);
                Scatter(f);
            }
        }
        int n = f.transform.childCount;
        EditorGUILayout.HelpBox(n + " of " + f.count + " pieces placed. If fewer than asked, the gap leaves no more room: make the shape bigger or the gap smaller.\n"
            + "Scatter and Reshuffle replace every piece under this field, hand-moved ones too.", n < f.count ? MessageType.Warning : MessageType.Info);
    }

    /// <summary>Lay the field's pieces out again from its seed, as real prefab copies under it (one undo).</summary>
    public static void Scatter(DebrisField f)
    {
        Undo.SetCurrentGroupName("Scatter debris");
        int group = Undo.GetCurrentGroup();
        for (int i = f.transform.childCount - 1; i >= 0; i--) Undo.DestroyObjectImmediate(f.transform.GetChild(i).gameObject);
        var across = new Dictionary<GameObject, float>();
        foreach (var p in f.Layout())
        {
            if (!p.prefab) continue;
            if (!across.TryGetValue(p.prefab, out float w)) across[p.prefab] = w = Across(p.prefab);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(p.prefab, f.gameObject.scene);
            Undo.RegisterCreatedObjectUndo(go, "Scatter debris");
            go.transform.SetParent(f.transform, false);
            go.transform.localPosition = new Vector3(p.at.x, p.at.y, 0f);
            go.transform.localRotation = Quaternion.Euler(0f, 0f, p.turnDeg) * p.prefab.transform.localRotation;
            go.transform.localScale = p.prefab.transform.localScale * (p.sizeUnits / w);
        }
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(f.gameObject.scene);
    }

    // How wide a prefab is at its own scale, units, across x and y (the plane seen from above), from its meshes.
    static float Across(GameObject prefab)
    {
        bool any = false;
        var b = new Bounds();
        foreach (var mf in prefab.GetComponentsInChildren<MeshFilter>())
        {
            if (!mf.sharedMesh) continue;
            var m = mf.transform.localToWorldMatrix;
            var mb = mf.sharedMesh.bounds;
            for (int i = 0; i < 8; i++)
            {
                var corner = m.MultiplyPoint3x4(mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) * 2 - 1, (i & 2) - 1, (i & 4) / 2 - 1)));
                if (any) b.Encapsulate(corner); else { b = new Bounds(corner, Vector3.zero); any = true; }
            }
        }
        return any ? Mathf.Max(b.size.x, b.size.y, 0.001f) : 1f;
    }

    /// <summary>The layout's own check, run through MCP (later the Test Runner): for every shape and
    /// spread, each piece is inside the shape, no two are closer than the gap, and the same seed gives
    /// the same layout. Returns "ok" or what failed.</summary>
    public static string Check()
    {
        var go = new GameObject("Debris check") { hideFlags = HideFlags.HideAndDontSave }; // never saved, never dirties a scene
        try
        {
            var f = go.AddComponent<DebrisField>();
            f.pieces = new[] { go };
            foreach (DebrisField.Shape shape in System.Enum.GetValues(typeof(DebrisField.Shape)))
            foreach (DebrisField.Spread spread in System.Enum.GetValues(typeof(DebrisField.Spread)))
            {
                f.shape = shape; f.spread = spread;
                var a = f.Layout();
                var b = f.Layout();
                string what = shape + "/" + spread;
                if (a.Count == 0) return what + ": nothing placed";
                for (int i = 0; i < a.Count; i++)
                {
                    if (!f.Inside(a[i].at)) return what + ": a piece outside the shape";
                    if (a[i].at != b[i].at) return what + ": the same seed gave another layout";
                    for (int j = 0; j < i; j++)
                        if ((a[i].at - a[j].at).magnitude < f.gapUnits) return what + ": two pieces closer than the gap";
                }
            }
            return "ok";
        }
        finally { Object.DestroyImmediate(go); }
    }
}
