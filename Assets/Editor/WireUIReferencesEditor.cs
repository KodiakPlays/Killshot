using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Editor utility that copies serialized UI references from UIController (on the Canvas)
/// to Radar, PlayerShip, ShipStability, PowerManager, and GameManager.
/// Run via the menu: Starwolf > Wire UI References.
/// The scene must be open with the Canvas and PlayerShip prefab instances present.
/// After running, save the scene to persist the assignments.
/// </summary>
public static class WireUIReferencesEditor
{
    [MenuItem("Starwolf/Wire UI References")]
    public static void WireReferences()
    {
        // --- Find source ---
        UIController uiController = Object.FindFirstObjectByType<UIController>();
        if (uiController == null)
        {
            Debug.LogError("[WireUI] UIController not found in scene.");
            return;
        }

        SerializedObject uiSO = new SerializedObject(uiController);

        // --- Find PlayerShip ---
        PlayerShip playerShip = Object.FindFirstObjectByType<PlayerShip>();
        if (playerShip == null)
        {
            Debug.LogError("[WireUI] PlayerShip not found in scene.");
            return;
        }

        int wired = 0;

        // --- Radar ---
        Radar radar = playerShip.GetComponent<Radar>();
        if (radar != null)
        {
            SerializedObject so = new SerializedObject(radar);
            CopyFields(uiSO, so, new[] {
                "radarSha", "radarGO", "radarBogeyGO",
                "scannerSlider", "scanSpeed", "scannerSha", "scannerImg",
                "scanPips", "shipPartsString", "targetGO", "bogieMesh"
            });

            // Self-reference: Radar.playerTransform = PlayerShip's own transform
            SerializedProperty playerTransProp = so.FindProperty("playerTransform");
            if (playerTransProp != null)
            {
                playerTransProp.objectReferenceValue = playerShip.transform;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(radar);
            wired++;
            Debug.Log("[WireUI] Radar references assigned.");
        }
        else
        {
            Debug.LogWarning("[WireUI] Radar component not found on PlayerShip.");
        }

        // --- PlayerShip ---
        {
            SerializedObject so = new SerializedObject(playerShip);
            CopyFields(uiSO, so, new[] {
                "shipHit", "shipHitCurve", "sensorLoad", "sensorLoadInfo",
                "sensorIcon", "sensorLoadSprites", "compassRect",
                "velocityMeterSha", "velocityMeterImg", "velocityMeterTMP"
            });
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(playerShip);
            wired++;
            Debug.Log("[WireUI] PlayerShip references assigned.");
        }

        // --- ShipStability ---
        ShipStability stability = playerShip.GetComponent<ShipStability>();
        if (stability != null)
        {
            SerializedObject so = new SerializedObject(stability);
            CopyFields(uiSO, so, new[] {
                "stabilityShader", "stabilityImg"
            });
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(stability);
            wired++;
            Debug.Log("[WireUI] ShipStability references assigned.");
        }

        // --- PowerManager ---
        PowerManager power = playerShip.GetComponent<PowerManager>();
        if (power != null)
        {
            SerializedObject so = new SerializedObject(power);
            CopyFields(uiSO, so, new[] {
                "btnPowerBool", "imgPowerMet", "shaPowerMet", "shaReactorMet",
                "uiSprite", "btnPowerBoolImage", "powerNodeSha", "powerNodeImg", "powerState"
            });
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(power);
            wired++;
            Debug.Log("[WireUI] PowerManager references assigned.");
        }

        // --- GameManager ---
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            SerializedObject so = new SerializedObject(gm);
            CopyFields(uiSO, so, new[] {
                "weaponScreenSha", "weaponScreenImage", "screenWepGO", "shipAngleTarget",
                "screenEnemyWeapon", "gridSha", "gridImg", "sc", "viewTex", "screenWorld",
                "shipiconGO", "bogieMesh", "animationCurve"
            });

            // Wire shipPlayer to the PlayerShip's transform
            SerializedProperty shipPlayerProp = so.FindProperty("shipPlayer");
            if (shipPlayerProp != null)
            {
                shipPlayerProp.objectReferenceValue = playerShip.transform;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(gm);
            wired++;
            Debug.Log("[WireUI] GameManager references assigned.");
        }

        // --- WorldBoundary (on GameManager or standalone) ---
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

        // Mark scene dirty so the user is prompted to save
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"[WireUI] Done — wired {wired} components. Save the scene to persist.");
    }

    private static void CopyFields(SerializedObject source, SerializedObject target, string[] fieldNames)
    {
        foreach (string field in fieldNames)
        {
            SerializedProperty srcProp = source.FindProperty(field);
            if (srcProp == null)
            {
                Debug.LogWarning($"[WireUI] Source field '{field}' not found on {source.targetObject.GetType().Name}.");
                continue;
            }

            SerializedProperty dstProp = target.FindProperty(field);
            if (dstProp == null)
            {
                Debug.LogWarning($"[WireUI] Target field '{field}' not found on {target.targetObject.GetType().Name}.");
                continue;
            }

            target.CopyFromSerializedProperty(srcProp);
        }
    }

    // ================================================================
    // Button onClick Wiring
    // ================================================================

    [MenuItem("Starwolf/Wire Button OnClicks")]
    public static void WireButtonOnClicks()
    {
        // --- Find key components (NO UIController as target) ---
        UIController uiController = Object.FindFirstObjectByType<UIController>();
        if (uiController == null)
        {
            Debug.LogError("[WireButtons] UIController not found in scene (needed to find Canvas buttons).");
            return;
        }

        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        PlayerShip playerShip = Object.FindFirstObjectByType<PlayerShip>();
        PowerManager powerManager = playerShip != null ? playerShip.GetComponent<PowerManager>() : null;
        CommsManager commsManager = Object.FindFirstObjectByType<CommsManager>();

        if (gm == null) Debug.LogWarning("[WireButtons] GameManager not found. WorldGridZoom / ability buttons may not wire.");
        if (powerManager == null) Debug.LogWarning("[WireButtons] PowerManager not found. Power buttons may not wire.");
        if (commsManager == null) Debug.LogWarning("[WireButtons] CommsManager not found. FrequancyTune buttons may not wire.");

        int fixedTargets = 0;
        int addedListeners = 0;
        int skippedGeneric = 0;

        // --- Step 1: Fix null targets on ALL existing persistent onClick calls ---
        // Remap m_TargetAssemblyTypeName to the correct scene component:
        //   UIController.WorldGridZoom   → GameManager
        //   UIController.ChargeBtn       → PowerManager
        //   UIController.VentBtn         → PowerManager
        //   UIController.FrequancyTune   → CommsManager
        //   PowerManager.*               → PowerManager (unchanged)
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

                // Skip already-resolved targets
                if (targetProp.objectReferenceValue != null) continue;

                Object resolvedTarget = null;
                string newTypeName = null;

                if (typeName.Contains("UIController"))
                {
                    // Route UIController methods to the proper system
                    switch (methodName)
                    {
                        case "WorldGridZoom":
                            resolvedTarget = gm;
                            newTypeName = "GameManager, Assembly-CSharp";
                            break;
                        case "FrequancyTune":
                            resolvedTarget = commsManager;
                            newTypeName = "CommsManager, Assembly-CSharp";
                            break;
                        case "ChargeBtn":
                        case "ChargeOn":
                        case "ChargeOff":
                        case "VentBtn":
                            resolvedTarget = powerManager;
                            newTypeName = "PowerManager, Assembly-CSharp";
                            break;
                        case "BtnScannerCloke":
                        case "BtnRepair":
                        case "SignalGhost":
                            resolvedTarget = gm;
                            newTypeName = "GameManager, Assembly-CSharp";
                            break;
                        default:
                            Debug.LogWarning($"[WireButtons] Unknown UIController method '{methodName}' on button '{btn.gameObject.name}' — skipping.");
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

        // --- Step 2: Add onClick for named ability / power buttons that have empty calls ---
        foreach (Button btn in allButtons)
        {
            if (btn.onClick.GetPersistentEventCount() > 0) continue; // Already has handlers

            string name = btn.gameObject.name;
            switch (name)
            {
                case "btn_scannerCloke":
                    if (gm != null)
                    {
                        UnityEventTools.AddVoidPersistentListener(btn.onClick, new UnityAction(gm.BtnScannerCloke));
                        EditorUtility.SetDirty(btn);
                        addedListeners++;
                        Debug.Log($"[WireButtons] {name} → GameManager.BtnScannerCloke()");
                    }
                    break;

                case "btn_repair":
                    if (gm != null)
                    {
                        UnityEventTools.AddVoidPersistentListener(btn.onClick, new UnityAction(gm.BtnRepair));
                        EditorUtility.SetDirty(btn);
                        addedListeners++;
                        Debug.Log($"[WireButtons] {name} → GameManager.BtnRepair()");
                    }
                    break;

                case "btn_signalGhost":
                    if (gm != null)
                    {
                        UnityEventTools.AddVoidPersistentListener(btn.onClick, new UnityAction(gm.SignalGhost));
                        EditorUtility.SetDirty(btn);
                        addedListeners++;
                        Debug.Log($"[WireButtons] {name} → GameManager.SignalGhost()");
                    }
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

        // Mark scene dirty
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"[WireButtons] Done — Fixed {fixedTargets} null targets, added {addedListeners} new listeners, skipped {skippedGeneric} generic buttons. Save the scene to persist.");
    }
}
