using UnityEngine;

/// <summary>
/// Player Spawn: where the player's ship starts in a level (owner, 2026-10-26). A box seen only in the
/// Scene view, never on the board, like Enemy Spawn. At play it puts the player's ship here, facing the way the arrow points, and the plot
/// opens on it. One per level.
/// </summary>
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
