using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Editor utility that wires non-UI references (e.g. playerTransform) and button onClicks.
/// All UI serialized fields now live on UIController only.
/// Run via the menu: Starwolf > Wire UI References.
/// </summary>
public static class WireUIReferencesEditor
{
    [MenuItem("Starwolf/Wire UI References")]
    public static void WireReferences()
    {
        UIController uiController = Object.FindFirstObjectByType<UIController>();
        if (uiController == null)
        {
            Debug.LogError("[WireUI] UIController not found in scene.");
            return;
        }

        PlayerShip playerShip = Object.FindFirstObjectByType<PlayerShip>();
        if (playerShip == null)
        {
            Debug.LogError("[WireUI] PlayerShip not found in scene.");
            return;
        }

        int wired = 0;

        // --- Radar (only playerTransform) ---
        Radar radar = playerShip.GetComponent<Radar>();
        if (radar != null)
        {
            SerializedObject so = new SerializedObject(radar);
            SerializedProperty playerTransProp = so.FindProperty("playerTransform");
            if (playerTransProp != null)
            {
                playerTransProp.objectReferenceValue = playerShip.transform;
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(radar);
            wired++;
            Debug.Log("[WireUI] Radar.playerTransform assigned.");
        }

        // --- WorldBoundary ---
        WorldBoundary wb = Object.FindFirstObjectByType<WorldBoundary>();
        if (wb != null)
        {
            SerializedObject so = new SerializedObject(wb);
            SerializedProperty ptProp = so.FindProperty("playerTransform");
            if (ptProp != null)
            {
                ptProp.objectReferenceValue = playerShip.transform;
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(wb);
            wired++;
            Debug.Log("[WireUI] WorldBoundary.playerTransform assigned.");
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"[WireUI] Done — wired {wired} components. Save the scene to persist.");
    }

    // ================================================================
    // Button onClick Wiring
    // ================================================================

    [MenuItem("Starwolf/Wire Button OnClicks")]
    public static void WireButtonOnClicks()
    {
        UIController uiController = Object.FindFirstObjectByType<UIController>();
        if (uiController == null)
        {
            Debug.LogError("[WireButtons] UIController not found in scene.");
            return;
        }

        PlayerShip playerShip = Object.FindFirstObjectByType<PlayerShip>();
        PowerManager powerManager = playerShip != null ? playerShip.GetComponent<PowerManager>() : null;

        if (powerManager == null) Debug.LogWarning("[WireButtons] PowerManager not found. Power buttons may not wire.");

        int fixedTargets = 0;
        int addedListeners = 0;
        int skippedGeneric = 0;

        Button[] allButtons = uiController.GetComponentsInChildren<Button>(true);
        Debug.Log($"[WireButtons] Found {allButtons.Length} buttons on Canvas.");

        foreach (Button btn in allButtons)
        {
            SerializedObject so = new SerializedObject(btn);
            SerializedProperty callsProp = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");

            bool modified = false;
            for (int i = 0; i < callsProp.arraySize; i++)
            {
                SerializedProperty callProp = callsProp.GetArrayElementAtIndex(i);
                SerializedProperty targetProp = callProp.FindPropertyRelative("m_Target");
                SerializedProperty typeProp = callProp.FindPropertyRelative("m_TargetAssemblyTypeName");
                string methodName = callProp.FindPropertyRelative("m_MethodName").stringValue;
                string typeName = typeProp.stringValue;

                if (targetProp.objectReferenceValue != null) continue;

                Object resolvedTarget = null;
                string newTypeName = null;

                // UI methods now all live on UIController
                if (typeName.Contains("UIController") || typeName.Contains("GameManager") || typeName.Contains("CommsManager"))
                {
                    switch (methodName)
                    {
                        case "WorldGridZoom":
                        case "ChargeBtn":
                        case "ChargeOn":
                        case "ChargeOff":
                        case "VentBtn":
                        case "BtnScannerCloke":
                        case "BtnRepair":
                        case "SignalGhost":
                        case "FrequancyTune":
                            resolvedTarget = uiController;
                            newTypeName = "UIController, Assembly-CSharp";
                            break;
                        default:
                            Debug.LogWarning($"[WireButtons] Unknown method '{methodName}' on button '{btn.gameObject.name}' — skipping.");
                            break;
                    }
                }
                else if (typeName.Contains("PowerManager"))
                {
                    resolvedTarget = powerManager;
                }

                if (resolvedTarget != null)
                {
                    targetProp.objectReferenceValue = resolvedTarget;
                    if (newTypeName != null)
                    {
                        typeProp.stringValue = newTypeName;
                    }
                    modified = true;
                    fixedTargets++;
                    Debug.Log($"[WireButtons] Fixed target: {btn.gameObject.name}.{methodName} → {resolvedTarget.GetType().Name}");
                }
            }

            if (modified)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(btn);
            }
        }

        // --- Step 2: Add onClick for named ability buttons with empty calls ---
        foreach (Button btn in allButtons)
        {
            if (btn.onClick.GetPersistentEventCount() > 0) continue;

            string name = btn.gameObject.name;
            switch (name)
            {
                case "btn_scannerCloke":
                    UnityEventTools.AddVoidPersistentListener(btn.onClick, new UnityAction(uiController.BtnScannerCloke));
                    EditorUtility.SetDirty(btn);
                    addedListeners++;
                    Debug.Log($"[WireButtons] {name} → UIController.BtnScannerCloke()");
                    break;

                case "btn_repair":
                    UnityEventTools.AddVoidPersistentListener(btn.onClick, new UnityAction(uiController.BtnRepair));
                    EditorUtility.SetDirty(btn);
                    addedListeners++;
                    Debug.Log($"[WireButtons] {name} → UIController.BtnRepair()");
                    break;

                case "btn_signalGhost":
                    UnityEventTools.AddVoidPersistentListener(btn.onClick, new UnityAction(uiController.SignalGhost));
                    EditorUtility.SetDirty(btn);
                    addedListeners++;
                    Debug.Log($"[WireButtons] {name} → UIController.SignalGhost()");
                    break;

                case "btn_emergencyVent":
                    if (powerManager != null)
                    {
                        UnityEventTools.AddVoidPersistentListener(btn.onClick, new UnityAction(powerManager.EmergencyVent));
                        EditorUtility.SetDirty(btn);
                        addedListeners++;
                        Debug.Log($"[WireButtons] {name} → PowerManager.EmergencyVent()");
                    }
                    break;

                case "btn_blackAlert":
                    if (powerManager != null)
                    {
                        UnityEventTools.AddVoidPersistentListener(btn.onClick, new UnityAction(powerManager.BlackAlert));
                        EditorUtility.SetDirty(btn);
                        addedListeners++;
                        Debug.Log($"[WireButtons] {name} → PowerManager.BlackAlert()");
                    }
                    break;

                default:
                    skippedGeneric++;
                    break;
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"[WireButtons] Done — Fixed {fixedTargets} null targets, added {addedListeners} new listeners, skipped {skippedGeneric} generic buttons. Save the scene to persist.");
    }
}
