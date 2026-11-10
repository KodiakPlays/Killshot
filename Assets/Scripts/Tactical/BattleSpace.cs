// =================================================================================================
// BattleSpace.cs
//
// What it does:
// A Battle Space sets how big a level's battle space is, in units. It is placed in a level scene,
// one per level. A level without one keeps the size set on the board's BoardManager.
//
// How it works:
// - Its width and height are set in the Inspector, or in the Level window. Each is at least 1,000
//   units.
// - When play starts it finds the board's BoardManager. It sets the board's battle space to this
//   width and height. Then it redraws the plot at the same zoom.
// - The battle space is always centred on the grid's centre, wherever this object is placed.
// - EnemySpawn reads it to keep the enemy's start inside the battle space.
// - In the Scene view it draws the edge as a grey box. Its size is written at the top left.
//
// Technical:
// - Data: widthUnits and heightUnits, units, at least 1,000 each. The default is 100,000 each.
// - Start() writes them to arenaWidthUnits and arenaHeightUnits in BoardManager.State. Then
//   BoardManager.SetZoom(ZoomExact) redraws the plot.
// - Without a Battle Space, the size is those two values, [Tune("Arena")] in ShipState.
// - Start() runs after the board is set up, whether the level opened with it or loaded on top.
// - It uses FindAnyObjectByType. With no BoardManager it does nothing.
// - EnemySpawn and LevelWindow read it. LevelWindow adds it, at the grid's centre.
// - Trap: QuestSystem can grow the battle space later in a drill, over the same two values.
// - The gizmo is editor-only code, inside #if UNITY_EDITOR.
// =================================================================================================

using UnityEngine;
using UnityEngine.Serialization;

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
