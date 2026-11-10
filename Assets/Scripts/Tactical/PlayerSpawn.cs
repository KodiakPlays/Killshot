// =================================================================================================
// PlayerSpawn.cs
//
// What it does:
// A Player Spawn marks where our ship starts in a level, and the way it faces. It is placed in a
// level scene, one per level. It is seen only in the Scene view and the Level window.
//
// How it works:
// - When play starts it finds TacticalWorld and asks it to put our ship here.
// - TacticalWorld places our ship at this spot, turned the way the spawn points. Then the plot
//   centres on our ship.
// - The Level window adds one with its Player spawn button, and only one per level.
// - In the Scene view it draws a white square with its name and a line for the bow. With no turn,
//   the bow points north.
//
// Technical:
// - It has no settings. It uses its transform: x and y in world units, and its Z angle.
// - Start() calls TacticalWorld.SpawnPlayer(position, Z angle). By then TacticalWorld has set up
//   its ships.
// - SpawnPlayer calls NavSystem.PlaceAt on board.State.nav. The heading is minus the Z angle,
//   rounded to a whole degree.
// - Then SpawnPlayer calls PlotPanel.CentreOnOurShip() and TacticalWorld.Sync().
// - Trap: with two Player Spawns, the last Start() wins. The Level window allows only one.
// - LevelWindow and PlaytestRunner also read it. The gizmo code is inside #if UNITY_EDITOR.
// =================================================================================================

using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    // Start, not Awake: TacticalWorld has set its ships up by now, whether this level was open with
    // the board or loaded on top of it
    private void Start()
    {
        var world = FindAnyObjectByType<TacticalWorld>();
        if (world) world.SpawnPlayer(transform.position, transform.eulerAngles.z);
    }

#if UNITY_EDITOR
    // Enemy Spawn's box, in the player's white (ink.data #E8EDF0). The line is the bow: north (+y) when not turned
    private void OnDrawGizmos()
    {
        float size = UnityEditor.HandleUtility.GetHandleSize(transform.position) * 0.4f;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.color = new Color(0.91f, 0.93f, 0.94f, 0.3f);
        Gizmos.DrawCube(Vector3.zero, Vector3.one * size);
        Gizmos.color = new Color(0.91f, 0.93f, 0.94f);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one * size);
        Gizmos.DrawLine(Vector3.zero, Vector3.up * size);
        UnityEditor.Handles.Label(transform.position + new Vector3(size * 0.7f, 0f, 0f), name);
    }
#endif
}
