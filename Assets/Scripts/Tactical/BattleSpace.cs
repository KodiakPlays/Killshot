using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Battle Space: how big a level's battle space is, units (owner, 2026-10-27, decision 50). Seen only in
/// the Scene view, like the spawn boxes, as the battle space's edge. At play it sets the board's
/// battle space to this size, centred on the grid's centre. One per level; a level without one keeps
/// the board's own size (100,000 x 100,000 units).
/// </summary>
public class BattleSpace : MonoBehaviour
{
    [Tooltip("Width of the battle space, units (east to west).")]
    [FormerlySerializedAs("widthKm")] [Min(1000f)] public float widthUnits = 100000f;
    [Tooltip("Height of the battle space, units (north to south).")]
    [FormerlySerializedAs("heightKm")] [Min(1000f)] public float heightUnits = 100000f;

    // Start, like the spawn boxes: the board is set up by now, whether this level was open with it or
    // loaded on top of it
    private void Start()
    {
        var board = FindAnyObjectByType<BoardManager>();
        if (!board) return;
        board.State.arenaWidthUnits = widthUnits;
        board.State.arenaHeightUnits = heightUnits;
        board.SetZoom(board.ZoomExact); // redraws the plot at the new size
    }

#if UNITY_EDITOR
    // the edge, in the Level window map's grey; always at the grid's centre, wherever this object is
    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = new Color(0.55f, 0.6f, 0.65f);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(widthUnits, heightUnits, 0f));
        UnityEditor.Handles.Label(new Vector3(-widthUnits / 2, heightUnits / 2, 0f), "Battle space " + widthUnits.ToString("N0") + " × " + heightUnits.ToString("N0") + " units");
    }
#endif
}
