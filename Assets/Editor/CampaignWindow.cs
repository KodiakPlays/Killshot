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
//
// Technical:
// - It finds the first Campaign file in the project, through AssetDatabase.FindAssets.
// - It edits through a SerializedObject. The list is a ReorderableList over Campaign.missions.
// - The Level slot is SceneDrawer, in ReadOnlyDrawer.cs, from the [Scene] mark on Mission.level.
// - Campaign.On lists the missions switched on. SectorMap, SaveSlots and SaveGame read it.
// - Trap: saves keep won missions by their level's scene name. Changing a mission's level loses
//   its wins in old saves.
// =================================================================================================

using UnityEditor;
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
        EditorGUILayout.EndScrollView();

        if (so.ApplyModifiedProperties()) AssetDatabase.SaveAssetIfDirty(campaign);
    }
}
