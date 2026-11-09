using UnityEngine;

/// <summary>
/// The campaign (owner 2026-11-10): its missions, in order, in one file, Assets/Campaign/Campaign.asset. The sector map
/// shows them as dots. Each dot appears once the mission before it is won. Drag a mission's level scene into its Level
/// slot; the slot also puts the scene in the build.
/// </summary>
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
    }

    [Tooltip("The missions, in order. Each opens when the one before it is won.")]
    public Mission[] missions;
}

/// <summary>On a text field: the inspector shows a slot to drag a scene into, and keeps the scene's name.</summary>
public class SceneAttribute : PropertyAttribute { }
