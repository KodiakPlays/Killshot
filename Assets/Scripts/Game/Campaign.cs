// =================================================================================================
// Campaign.cs
//
// What it does:
// The list of campaign missions, in order. The list is one file in the project:
// Assets/Campaign/Campaign.asset. The sector map reads it and shows one dot per mission.
//
// How it works:
// - Each mission has a title, a level scene, a briefing text and a spot on the sector map.
// - Each mission also has an off switch. A mission switched off is skipped by the game, as if it
//   weren't in the list. Switch a test mission off for a player build.
// - The Starwolf > Campaign window edits this file: the order, the switches and the closing line.
// - The spot is in px from the middle of a 1920 x 1080 screen.
// - The level is a slot in the Inspector. You drag the level's scene into it, and it keeps the
//   scene's name. The scene slot is drawn by SceneDrawer, in Assets/Editor/ReadOnlyDrawer.cs.
//   That drawer adds the scene to the build's scene list.
// - This file makes the [Scene] mark. It turns a text field into that scene slot.
// - SectorMap reads the list. A mission opens once the one before it is won.
// - The file also has the story's closing line. The end card shows it after the last mission.
// - Won counts how many of its missions a save has won. The save slots show that count.
//
// Technical:
// - Data: missions is a list of Campaign.Mission: title, level, brief, at and off. level is the
//   scene name as text.
// - On lists the missions switched on, in order. SectorMap, SaveSlots and SaveGame read On, never
//   missions. Won counts only those.
// - The window is CampaignWindow.cs, in the Editor folder.
// - closingLine is read by EndCard. Empty, the end card goes straight on to the title.
// - Won(done) counts the missions whose level is in a save's list of won levels. SaveSlots and
//   SaveGame.FinishedWith call it.
// - SectorMap, SaveSlots and EndCard each get the file in the Inspector. SectorMap passes it on to
//   SaveGame.campaign.
// - SceneAttribute is the [Scene] mark. Its drawer is SceneDrawer, in the Editor folder.
// - Trap: saves keep won levels by scene name. A renamed level scene loses its wins in old saves.
// =================================================================================================

using UnityEngine;

public class Campaign : ScriptableObject
{
    [System.Serializable]
    public class Mission
    {
        [Tooltip("The name on the map and in the box.")]
        public string title;
        [Tooltip("The level it plays. Drag its scene here from Assets/Scenes/Levels.")]
        [Scene] public string level;
        [Tooltip("The briefing on the map's left screen.")]
        [TextArea(3, 12)] public string brief;
        [Tooltip("Where its dot is on the map, px from the middle of the 1920 x 1080 screen.")]
        public Vector2 at;
        [Tooltip("Switched off: the game skips it, as if it weren't in the list. For a test mission, kept out of a player build (owner 2026-11-11).")]
        public bool off;
    }

    [Tooltip("The missions, in order. Each opens when the one before it is won.")]
    public Mission[] missions;

    [Tooltip("The story's closing line. After the last mission is won, the end card shows it, then the STARWOLF title.")]
    [TextArea(2, 6)] public string closingLine;

    /// <summary>The missions switched on, in order: the ones the game plays.</summary>
    public Mission[] On => System.Array.FindAll(missions, m => !m.off);

    /// <summary>How many of the missions switched on are in this list of levels won.</summary>
    public int Won(System.Collections.Generic.List<string> done)
    {
        int n = 0;
        foreach (var m in On) if (done.Contains(m.level)) n++;
        return n;
    }
}

/// <summary>On a text field: the inspector shows a slot to drag a scene into, and keeps the scene's name.</summary>
public class SceneAttribute : PropertyAttribute { }
