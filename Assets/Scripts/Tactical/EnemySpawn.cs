using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Where an enemy ship starts in a level (owner, 2026-10-25). A box seen only in the Scene view,
/// never on the board. At play it moves its contact here, facing the way the arrow points. Wolf Hunt
/// (owner 2026-10-27): with a size, it starts at a random point in the box, facing a random way, every game.
/// </summary>
public class EnemySpawn : MonoBehaviour
{
    [Tooltip("The board track whose ship starts here, e.g. t07 (GARUDA).")]
    public string trackKey = "t07";
    [Tooltip("The box, units (x across, y up). The ship starts at a random point inside it, facing a random way. 0 x 0: "
           + "always the centre, facing the arrow.")]
    [FormerlySerializedAs("sizeKm")] public Vector2 sizeUnits = new Vector2(40000f, 12000f);

    // Start, not Awake: TacticalWorld has set its ships up by now, whether this level was open with
    // the board or loaded on top of it
    private void Start()
    {
        var world = FindAnyObjectByType<TacticalWorld>();
        if (!world) return;
        if (sizeUnits.x <= 0f || sizeUnits.y <= 0f) { world.Spawn(trackKey, transform.position, transform.eulerAngles.z); return; }
        var at = transform.position + new Vector3(Random.Range(-0.5f, 0.5f) * sizeUnits.x, Random.Range(-0.5f, 0.5f) * sizeUnits.y, 0f);
        var space = FindAnyObjectByType<BattleSpace>(); // never outside the battle space
        if (space) { at.x = Mathf.Clamp(at.x, -space.widthUnits / 2f + 1000f, space.widthUnits / 2f - 1000f); at.y = Mathf.Clamp(at.y, -space.heightUnits / 2f + 1000f, space.heightUnits / 2f - 1000f); }
        world.Spawn(trackKey, at, Random.Range(0f, 360f));
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
        if (sizeUnits.x > 0f && sizeUnits.y > 0f) { Gizmos.matrix = Matrix4x4.Translate(transform.position); Gizmos.DrawWireCube(Vector3.zero, new Vector3(sizeUnits.x, sizeUnits.y, 0f)); Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one); }
        Gizmos.DrawLine(Vector3.zero, Vector3.up * size);
        UnityEditor.Handles.Label(transform.position + new Vector3(size * 0.7f, 0f, 0f), name);
    }
#endif
}
