using UnityEngine;

/// <summary>
/// Where an enemy ship starts in a level (owner, 2026-10-25). A box seen only in the Scene view,
/// never on the board. At play it moves its contact here, facing the way the arrow points.
/// </summary>
public class EnemySpawn : MonoBehaviour
{
    [Tooltip("The board track whose ship starts here, e.g. t07 (GARUDA).")]
    public string trackKey = "t07";

    // Start, not Awake: TacticalWorld has set its ships up by now, whether this level was open with
    // the board or loaded on top of it
    private void Start()
    {
        var world = FindAnyObjectByType<TacticalWorld>();
        if (world) world.Spawn(trackKey, transform.position, transform.eulerAngles.z);
    }

#if UNITY_EDITOR
    // the same size on screen at any zoom; filled so a click selects it. The line is the bow: north
    // (+y) when not turned
    private void OnDrawGizmos()
    {
        float size = UnityEditor.HandleUtility.GetHandleSize(transform.position) * 0.4f;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.color = new Color(1f, 0.35f, 0.25f, 0.3f);
        Gizmos.DrawCube(Vector3.zero, Vector3.one * size);
        Gizmos.color = new Color(1f, 0.35f, 0.25f);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one * size);
        Gizmos.DrawLine(Vector3.zero, Vector3.up * size);
        UnityEditor.Handles.Label(transform.position + new Vector3(size * 0.7f, 0f, 0f), name);
    }
#endif
}
