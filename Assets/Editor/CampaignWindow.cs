// =================================================================================================
// CampaignWindow.cs
//
// What it does:
// The Starwolf > Campaign window. It sets up the campaign: which missions it has, in what order,
// and the end card's closing line. It edits the Campaign file, Assets/Campaign/Campaign.asset.
//
// How it works:
// - The top list is the missions in play order. Drag a row to move it. + adds a mission and -
//   removes the one picked.
// - Each row has an On switch, its number, its name and its level. A mission switched off is
//   skipped by the game. Switch a test mission off for a player build.
// - Pick a row to see all of that mission below: name, level, briefing and its dot on the map.
// - Drag a level's scene into a Level slot. The slot also adds the scene to the build.
// - Below that is the closing line. Empty, the last mission goes straight to the title.
// - The travel bar's numbers are not here. They are in Starwolf > Tuning, with the Ship scene open.
// - Every change saves the Campaign file at once. Ctrl+Z undoes it.
// - What happens inside a mission (its checkpoints and their words) is set in the level, not here.
// - At the bottom, Test: play from here (owner 2026-11-11). Each button starts play mode at one
//   point: the title, the save slots, the end card, or, for the picked mission, the sector map
//   before it, the mission itself, or the ship's rooms after it.
// - The missions before the picked one count as won. The rooms after it come with some damage and
//   supplies used, and a fresh travel bar.
// - A test uses a hidden save, slot 9, made fresh each time and deleted when play stops. The
//   title and save slots buttons use the real slots, as the game does.
//
// Technical:
// - It finds the first Campaign file in the project, through AssetDatabase.FindAssets.
// - It edits through a SerializedObject. The list is a ReorderableList over Campaign.missions.
// - The Level slot is SceneDrawer, in ReadOnlyDrawer.cs, from the [Scene] mark on Mission.level.
// - Campaign.On lists the missions switched on. SectorMap, SaveSlots and SaveGame read it.
// - Test sets EditorSceneManager.playModeStartScene and three SessionState keys (SaveGame.TestKey
//   and the two after it), then enters play mode. SaveGame.TestStart reads the keys before the
//   first scene loads. WatchPlay clears all of it when play stops.
// - Hurt fills the rooms test from Assets/Ships/Asura.asset: the prow at half armour, the reactor
//   at 2, the engine room at 3, every gun and launcher and the marines at half. Test values only.
// - Trap: saves keep won missions by their level's scene name. Changing a mission's level loses
//   its wins in old saves.
// =================================================================================================

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

public class CampaignWindow : EditorWindow
{
    Campaign campaign;
    SerializedObject so;
    ReorderableList list;
    Vector2 scroll;

    [MenuItem("Starwolf/Campaign")]
    static void Open() => GetWindow<CampaignWindow>("Campaign");

    void OnEnable() => Load();
    void OnProjectChange() { if (!campaign) Load(); }

    void Load()
    {
        var guids = AssetDatabase.FindAssets("t:Campaign");
        campaign = guids.Length > 0 ? AssetDatabase.LoadAssetAtPath<Campaign>(AssetDatabase.GUIDToAssetPath(guids[0])) : null;
        if (!campaign) { so = null; list = null; return; }
        so = new SerializedObject(campaign);
        var missions = so.FindProperty("missions");
        list = new ReorderableList(so, missions, true, true, true, true);
        list.drawHeaderCallback = r => EditorGUI.LabelField(r, "MISSIONS, in play order. Drag to move.");
        list.drawElementCallback = (r, i, active, focused) =>
        {
            var m = missions.GetArrayElementAtIndex(i);
            var off = m.FindPropertyRelative("off");
            r.y += 2f;
            r.height = EditorGUIUtility.singleLineHeight;
            float w = r.width - 84f;
            off.boolValue = !EditorGUI.ToggleLeft(new Rect(r.x, r.y, 46f, r.height), "On", !off.boolValue);
            EditorGUI.LabelField(new Rect(r.x + 50f, r.y, 30f, r.height), (i + 1).ToString());
            EditorGUI.PropertyField(new Rect(r.x + 84f, r.y, w * 0.45f, r.height), m.FindPropertyRelative("title"), GUIContent.none);
            EditorGUI.PropertyField(new Rect(r.x + 88f + w * 0.45f, r.y, w * 0.55f - 4f, r.height), m.FindPropertyRelative("level"), GUIContent.none);
        };
        list.onAddCallback = l =>
        {
            int i = missions.arraySize;
            missions.arraySize++;
            var m = missions.GetArrayElementAtIndex(i);
            m.FindPropertyRelative("title").stringValue = "NEW MISSION";
            m.FindPropertyRelative("level").stringValue = "";
            m.FindPropertyRelative("brief").stringValue = "";
            m.FindPropertyRelative("at").vector2Value = Vector2.zero;
            m.FindPropertyRelative("off").boolValue = false;
            l.index = i;
        };
    }

    void OnGUI()
    {
        if (!campaign)
        {
            EditorGUILayout.HelpBox("No Campaign file found. It should be at Assets/Campaign/Campaign.asset.", MessageType.Warning);
            if (GUILayout.Button("Look again")) Load();
            return;
        }
        so.Update();
        scroll = EditorGUILayout.BeginScrollView(scroll);
        list.DoLayoutList();

        var missions = so.FindProperty("missions");
        if (list.index >= 0 && list.index < missions.arraySize)
        {
            var m = missions.GetArrayElementAtIndex(list.index);
            EditorGUILayout.LabelField("Mission " + (list.index + 1), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m.FindPropertyRelative("off"), new GUIContent("Switched off", "The game skips it. For a test mission, kept out of a player build."));
            EditorGUILayout.PropertyField(m.FindPropertyRelative("title"), new GUIContent("Name", "The name on the sector map."));
            EditorGUILayout.PropertyField(m.FindPropertyRelative("level"), new GUIContent("Level", "The level it plays. Drag its scene here."));
            EditorGUILayout.PropertyField(m.FindPropertyRelative("brief"), new GUIContent("Briefing", "The text on the sector map's left screen."));
            EditorGUILayout.PropertyField(m.FindPropertyRelative("at"), new GUIContent("Map dot", "Where its dot is on the sector map, px from the middle of the 1920 x 1080 screen."));
        }
        else EditorGUILayout.HelpBox("Pick a mission in the list to see all of it.", MessageType.None);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("End card", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(so.FindProperty("closingLine"), new GUIContent("Closing line"));
        EditorGUILayout.HelpBox("Empty: after the last mission the game goes straight to the title, where STARWOLF flickers in.", MessageType.None);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Travel bar", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Its costs and what each action gives are in Starwolf > Tuning > Travel bar, with the Ship scene open.", MessageType.None);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Test: play from here", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Starts play mode at that point. Tests use a hidden save (slot 9), made fresh each time and deleted when play stops. Your saves are never touched.", MessageType.None);
        using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
        {
            var on = campaign.On;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("TITLE")) Test("00_Start", -1, false, "");
            if (GUILayout.Button("SAVE SLOTS")) Test("SaveSlots", -1, false, "");
            if (GUILayout.Button("END CARD")) Test("EndCard", on.Length, false, "");
            EditorGUILayout.EndHorizontal();
            int at = list.index >= 0 && list.index < campaign.missions.Length ? System.Array.IndexOf(on, campaign.missions[list.index]) : -1;
            if (at >= 0)
            {
                EditorGUILayout.LabelField((at + 1) + "  " + on[at].title + ": the missions before it count as won.");
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("SECTOR MAP")) Test("SectorMap", at, false, "");
                if (GUILayout.Button("PLAY IT")) Test("02.1_Grid", at, false, on[at].level);
                if (GUILayout.Button("SHIP ROOMS AFTER IT")) Test("Ship", at + 1, true, "");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox("SHIP ROOMS AFTER IT: as after its win, with some damage, rounds, torpedoes and marines used, and a fresh travel bar.", MessageType.None);
            }
            else EditorGUILayout.HelpBox("Pick a mission that is switched on to test it.", MessageType.None);
        }
        EditorGUILayout.EndScrollView();

        if (so.ApplyModifiedProperties()) AssetDatabase.SaveAssetIfDirty(campaign);
    }

    // Play mode from one scene. won: missions counted as won on the test save, or -1 for no test save
    // (the title and save slots use the real slots). rooms: the ship as after a win, with a fresh travel bar.
    void Test(string scene, int won, bool rooms, string level)
    {
        var start = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Tactical/" + scene + ".unity");
        if (!start) { Debug.LogWarning("[Campaign] No scene " + scene + " in Assets/Scenes/Tactical."); return; }
        SaveGame.Delete(SaveGame.TestSlot);
        if (won >= 0)
        {
            var on = campaign.On;
            SaveGame.Play(SaveGame.TestSlot);
            var save = SaveGame.Current;
            for (int i = 0; i < won && i < on.Length; i++) save.done.Add(on[i].level);
            save.dotsShown = Mathf.Min(won, on.Length);
            if (rooms) Hurt(save);
            SaveGame.Save();
            SaveGame.Leave();
        }
        SessionState.SetBool(SaveGame.TestKey, true);
        SessionState.SetString(SaveGame.TestCampaignKey, won >= 0 ? AssetDatabase.GetAssetPath(campaign) : "");
        SessionState.SetString(SaveGame.TestLevelKey, level);
        EditorSceneManager.playModeStartScene = start;
        EditorApplication.EnterPlaymode();
    }

    // Some damage and supplies used, read from our ship's stats file, so the rooms have something to do. Test values only.
    static void Hurt(SaveGame.Data save)
    {
        var ship = AssetDatabase.LoadAssetAtPath<ShipStats>("Assets/Ships/Asura.asset");
        if (!ship) return;
        for (int i = 0; i < DamageSystem.Sides.Length; i++)
            save.damage.Add(new SaveGame.Part { name = DamageSystem.Sides[i] + " ARMOUR", now = ship.damage.sideArmourMax[i] * (i == 0 ? 0.5f : 1f), most = ship.damage.sideArmourMax[i] });
        save.damage.Add(new SaveGame.Part { name = "REACTOR", now = 2, most = 5 });
        save.damage.Add(new SaveGame.Part { name = "ENGINE ROOM", now = 3, most = 5 });
        foreach (var m in ship.weapons.mounts) save.stores.Add(new SaveGame.Part { name = m.label, now = Mathf.Floor(m.ammo * 0.5f), most = m.ammo });
        save.stores.Add(new SaveGame.Part { name = SaveGame.Marines, now = Mathf.Floor(ship.hangar.marines * 0.5f), most = ship.hangar.marines });
        save.travelOpen = true;
        save.travelUsed = 0;
    }

    // Play stops: the test is over. Its save goes, and the Play button starts from the open scenes again.
    // Done while play is stopping, not after: a test button pressed straight after can't be undone by it.
    [InitializeOnLoadMethod]
    static void WatchPlay() => EditorApplication.playModeStateChanged += s =>
    {
        if (s != PlayModeStateChange.ExitingPlayMode || !SessionState.GetBool(SaveGame.TestKey, false)) return;
        SessionState.EraseBool(SaveGame.TestKey);
        SessionState.EraseString(SaveGame.TestCampaignKey);
        SessionState.EraseString(SaveGame.TestLevelKey);
        EditorSceneManager.playModeStartScene = null;
        SaveGame.Delete(SaveGame.TestSlot);
    };
}
