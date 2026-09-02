using UnityEditor;
using UnityEngine;

/// <summary>
/// Apply / Capture buttons for a TacticalTuning asset.
///
/// Apply writes the asset's numbers into the open scene as real serialized
/// values. Capture reads the scene's current numbers back into the asset.
/// Nothing reads the asset at runtime, so there is no hidden indirection —
/// what you see on a component is what the game uses.
/// </summary>
[CustomEditor(typeof(TacticalTuning))]
public class TacticalTuningEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space(10);

        var tuning = (TacticalTuning)target;

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Apply to Scene", GUILayout.Height(30)))
                TacticalTuningOps.Apply(tuning);
            if (GUILayout.Button("Capture from Scene", GUILayout.Height(30)))
                TacticalTuningOps.Capture(tuning);
        }

        EditorGUILayout.HelpBox(
            "Apply writes these numbers into the open Tactical scene.\n" +
            "Capture reads the scene's current numbers back into this asset.\n\n" +
            "Turn rate is multiplied by engine power, so with no power the ship cannot turn.",
            MessageType.Info);
    }
}

public static class TacticalTuningOps
{
    [MenuItem("Starwolf/Tuning/Apply to Scene")]
    private static void ApplyMenu()
    {
        var t = Find();
        if (t != null) Apply(t);
    }

    [MenuItem("Starwolf/Tuning/Capture from Scene")]
    private static void CaptureMenu()
    {
        var t = Find();
        if (t != null) Capture(t);
    }

    private static TacticalTuning Find()
    {
        var guids = AssetDatabase.FindAssets("t:TacticalTuning");
        if (guids.Length == 0)
        {
            Debug.LogError("[Tuning] No TacticalTuning asset found. " +
                           "Create one with Assets > Create > Starwolf > Tactical Tuning.");
            return null;
        }
        return AssetDatabase.LoadAssetAtPath<TacticalTuning>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    // Set one [SerializeField] private field by name.
    private static void Set(Object target, string field, float v)
    {
        if (target == null) return;
        var so = new SerializedObject(target);
        var p = so.FindProperty(field);
        if (p == null) { Debug.LogWarning("[Tuning] no field '" + field + "' on " + target.GetType().Name); return; }
        if (p.propertyType == SerializedPropertyType.Integer) p.intValue = Mathf.RoundToInt(v);
        else p.floatValue = v;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private static float Get(Object target, string field, float fallback)
    {
        if (target == null) return fallback;
        var p = new SerializedObject(target).FindProperty(field);
        if (p == null) return fallback;
        return p.propertyType == SerializedPropertyType.Integer ? p.intValue : p.floatValue;
    }

    public static void Apply(TacticalTuning t)
    {
        var ship = GameObject.Find("PlayerShip");
        var camGO = GameObject.Find("Main Camera");
        if (ship == null || camGO == null)
        {
            Debug.LogError("[Tuning] Open a Tactical scene first — no PlayerShip or Main Camera found.");
            return;
        }

        var playerShip = ship.GetComponent<PlayerShip>();
        Set(playerShip, "rateOfAcceleration",   t.accelerationRate);
        Set(playerShip, "turnRate",             t.turnRate);
        Set(playerShip, "dodgeDistance",        t.dodgeDistance);
        Set(playerShip, "dodgeDuration",        t.dodgeDuration);
        Set(playerShip, "enginePowerDrainRate", t.enginePowerDrainRate);

        var power = ship.GetComponent<PowerManager>();
        Set(power, "powerFillRate",   t.powerFillRate);
        Set(power, "ventRate",        t.ventRate);
        Set(power, "reactorMaxPower", t.reactorMaxPower);

        var stab = ship.GetComponent<ShipStability>();
        Set(stab, "maxStability",                  t.maxStability);
        Set(stab, "stabilityRecoveryRate",         t.stabilityRecoveryRate);
        Set(stab, "turnStabilityDrainMultiplier",  t.turnStabilityDrainMultiplier);
        Set(stab, "speedStabilityDrainMultiplier", t.speedStabilityDrainMultiplier);
        Set(stab, "dodgeStabilityCost",            t.dodgeStabilityCost);

        var plot = camGO.GetComponent<PlotCamera>();
        Set(plot, "minSize",         t.zoomMin);
        Set(plot, "maxSize",         t.zoomMax);
        Set(plot, "stepPerNotch",    t.zoomStepPerNotch);
        Set(plot, "smoothing",       t.zoomSmoothing);
        Set(plot, "followSmoothing", t.followSmoothing);

        var cam = camGO.GetComponent<Camera>();
        if (cam != null) { cam.orthographicSize = t.zoomDefault; EditorUtility.SetDirty(cam); }

        var visual = ship.transform.Find("3D");
        var glyph = visual != null ? visual.GetComponent<ScreenConstantScale>() : null;
        Set(glyph, "minPixels", t.shipMinPixels);
        Set(glyph, "maxPixels", t.shipMaxPixels);

        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/UI/Shader/TacticalGrid.mat");
        if (mat != null)
        {
            mat.SetFloat("_CellSize",        t.gridCellSize);
            mat.SetFloat("_FineDivisions",   t.gridFineDivisions);
            mat.SetFloat("_MajorEvery",      t.gridMajorEvery);
            mat.SetFloat("_LineWidth",       t.gridLineWidthPixels);
            mat.SetFloat("_MajorWidthScale", t.gridMajorWidthScale);
            mat.SetFloat("_FineWidthScale",  t.gridFineWidthScale);
            mat.SetFloat("_FadeMin",         t.gridFadeMinPixels);
            mat.SetFloat("_FadeMax",         t.gridFadeMaxPixels);
            EditorUtility.SetDirty(mat);
        }

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Tuning] Applied '" + t.name + "' to the open scene.");
    }

    public static void Capture(TacticalTuning t)
    {
        var ship = GameObject.Find("PlayerShip");
        var camGO = GameObject.Find("Main Camera");
        if (ship == null || camGO == null)
        {
            Debug.LogError("[Tuning] Open a Tactical scene first.");
            return;
        }

        Undo.RecordObject(t, "Capture Tuning");

        var playerShip = ship.GetComponent<PlayerShip>();
        t.accelerationRate     = Get(playerShip, "rateOfAcceleration",   t.accelerationRate);
        t.turnRate             = Get(playerShip, "turnRate",             t.turnRate);
        t.dodgeDistance        = Get(playerShip, "dodgeDistance",        t.dodgeDistance);
        t.dodgeDuration        = Get(playerShip, "dodgeDuration",        t.dodgeDuration);
        t.enginePowerDrainRate = Get(playerShip, "enginePowerDrainRate", t.enginePowerDrainRate);

        var power = ship.GetComponent<PowerManager>();
        t.powerFillRate   = Get(power, "powerFillRate",   t.powerFillRate);
        t.ventRate        = Get(power, "ventRate",        t.ventRate);
        t.reactorMaxPower = Mathf.RoundToInt(Get(power, "reactorMaxPower", t.reactorMaxPower));

        var stab = ship.GetComponent<ShipStability>();
        t.maxStability                  = Get(stab, "maxStability",                  t.maxStability);
        t.stabilityRecoveryRate         = Get(stab, "stabilityRecoveryRate",         t.stabilityRecoveryRate);
        t.turnStabilityDrainMultiplier  = Get(stab, "turnStabilityDrainMultiplier",  t.turnStabilityDrainMultiplier);
        t.speedStabilityDrainMultiplier = Get(stab, "speedStabilityDrainMultiplier", t.speedStabilityDrainMultiplier);
        t.dodgeStabilityCost            = Get(stab, "dodgeStabilityCost",            t.dodgeStabilityCost);

        var plot = camGO.GetComponent<PlotCamera>();
        t.zoomMin          = Get(plot, "minSize",         t.zoomMin);
        t.zoomMax          = Get(plot, "maxSize",         t.zoomMax);
        t.zoomStepPerNotch = Get(plot, "stepPerNotch",    t.zoomStepPerNotch);
        t.zoomSmoothing    = Get(plot, "smoothing",       t.zoomSmoothing);
        t.followSmoothing  = Get(plot, "followSmoothing", t.followSmoothing);

        var cam = camGO.GetComponent<Camera>();
        if (cam != null) t.zoomDefault = cam.orthographicSize;

        var visual = ship.transform.Find("3D");
        var glyph = visual != null ? visual.GetComponent<ScreenConstantScale>() : null;
        t.shipMinPixels = Get(glyph, "minPixels", t.shipMinPixels);
        t.shipMaxPixels = Get(glyph, "maxPixels", t.shipMaxPixels);

        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/UI/Shader/TacticalGrid.mat");
        if (mat != null)
        {
            t.gridCellSize         = mat.GetFloat("_CellSize");
            t.gridFineDivisions    = mat.GetFloat("_FineDivisions");
            t.gridMajorEvery       = mat.GetFloat("_MajorEvery");
            t.gridLineWidthPixels  = mat.GetFloat("_LineWidth");
            t.gridMajorWidthScale  = mat.GetFloat("_MajorWidthScale");
            t.gridFineWidthScale   = mat.GetFloat("_FineWidthScale");
            t.gridFadeMinPixels    = mat.GetFloat("_FadeMin");
            t.gridFadeMaxPixels    = mat.GetFloat("_FadeMax");
        }

        EditorUtility.SetDirty(t);
        AssetDatabase.SaveAssets();
        Debug.Log("[Tuning] Captured the open scene into '" + t.name + "'.");
    }
}
