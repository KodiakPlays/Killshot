using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Step 77. The Debris Field's inspector: its settings, then Scatter (lay the pieces out again from the
/// same seed) and Reshuffle (a new seed). Both replace every piece under the field, hand-moved ones too,
/// in one undo.
/// </summary>
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
            go.transform.localScale = p.prefab.transform.localScale * (p.sizeKm / w);
        }
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(f.gameObject.scene);
    }

    // How wide a prefab is at its own scale, km, across x and y (the plane seen from above), from its meshes.
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
                        if ((a[i].at - a[j].at).magnitude < f.gapKm) return what + ": two pieces closer than the gap";
                }
            }
            return "ok";
        }
        finally { Object.DestroyImmediate(go); }
    }
}
