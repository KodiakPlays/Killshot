using UnityEngine;

/// <summary>
/// Step 34. A contact in the tactical world: where it is (this transform, in km) and which board
/// track it drives (HandoffBoardState.tracks, by key). TacticalWorld reads it once per board tick.
/// </summary>
public class WorldContact : MonoBehaviour
{
    [Tooltip("The board track this contact drives, e.g. t07 (TRACK 07).")]
    public string trackKey = "t07";
}
