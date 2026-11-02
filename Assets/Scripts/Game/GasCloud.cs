using UnityEngine;

/// <summary>
/// Gas Cloud (owner, 2026-11-01): a box of cold dust and vapour in a level. It has no collider: a ship flies in
/// and takes no damage. Our ship inside one cools down, and an enemy can only see it from close by. TacticalWorld
/// finds the clouds; the numbers are in each ship's stats file, under Sensors. Owner 2026-11-02: in visual range the
/// plot draws the cloud's picture (on the Plot panel's Art list); past it, while DRADIS listens, a hollow circle.
/// </summary>
public class GasCloud : MonoBehaviour
{
    [Tooltip("Width of the cloud, units (east to west).")]
    [Min(100f)] public float widthUnits = 2000f;
    [Tooltip("Height of the cloud, units (north to south).")]
    [Min(100f)] public float heightUnits = 1200f;

    /// <summary>Showing on the plot now: by the same rule as debris (TacticalWorld.SeeRuins).</summary>
    [System.NonSerialized] public bool seen;
    /// <summary>In visual range now (ours or a fighter's): the plot shows its picture, not the DRADIS circle.</summary>
    [System.NonSerialized] public bool inSight;

    public Vector2 At => transform.position;

    /// <summary>Is this world point in the cloud?</summary>
    public bool Inside(Vector2 p)
    {
        p -= At;
        return Mathf.Abs(p.x) <= widthUnits / 2 && Mathf.Abs(p.y) <= heightUnits / 2;
    }

    /// <summary>How much of a ship's box is in the cloud, 0 to 1: the box cut into 10 x 10 squares, the squares inside counted.</summary>
    public float ShareInside(Vector2 at, double heading, Vector2 half)
    {
        float rad = (float)heading * Mathf.Deg2Rad;
        Vector2 fore = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * half.y, right = new Vector2(Mathf.Cos(rad), -Mathf.Sin(rad)) * half.x;
        int inside = 0;
        for (int i = 0; i < 10; i++)
            for (int j = 0; j < 10; j++)
                if (Inside(at + fore * ((i + 0.5f) / 5f - 1f) + right * ((j + 0.5f) / 5f - 1f))) inside++;
        return inside / 100f;
    }

#if UNITY_EDITOR
    // the box, in the Level window map's blue; the faint fill makes it clickable in the Scene view
    private void OnDrawGizmos()
    {
        var size = new Vector3(widthUnits, heightUnits, 0f);
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = new Color(0.45f, 0.72f, 0.85f, 0.12f);
        Gizmos.DrawCube(transform.position, size);
        Gizmos.color = new Color(0.45f, 0.72f, 0.85f);
        Gizmos.DrawWireCube(transform.position, size);
        UnityEditor.Handles.Label(transform.position + new Vector3(-widthUnits, heightUnits, 0f) / 2, name);
    }
#endif
}
