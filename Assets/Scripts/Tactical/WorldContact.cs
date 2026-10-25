using UnityEngine;

/// <summary>
/// Step 34. A contact in the tactical world: where it is (this transform, in km) and which board
/// track it drives (HandoffBoardState.tracks, by key). TacticalWorld reads it once per board tick.
/// Its NavSystem, on the same object, moves it.
/// </summary>
[RequireComponent(typeof(NavSystem))]
public class WorldContact : MonoBehaviour
{
    [Tooltip("The board track this contact drives, e.g. t07 (GARUDA).")]
    public string trackKey = "t07";

    /// <summary>Step 49: this ship's own model — the same one ours runs on (the enemy is our ship
    /// with different stats, FTUE_PLAN §4). It takes our hits. Undamaged at load.</summary>
    public HandoffBoardState Ship
    {
        get
        {
            if (ship == null) { ship = new HandoffBoardState(); ship.UseNav(GetComponent<NavSystem>()); }
            return ship;
        }
    }
    [System.NonSerialized] private HandoffBoardState ship;
}
