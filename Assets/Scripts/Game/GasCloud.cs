// =================================================================================================
// GasCloud.cs
//
// What it does:
// A gas cloud is a box of cold gas placed in a level. Ships fly through it and take no damage.
// Our ship's signal drops while it is inside. An enemy can only see our ship in the cloud from
// close by.
//
// How it works:
// - The cloud's width and height are set in the Inspector, in units.
// - Other scripts ask it two things: is a point inside the box, and how much of a ship's box is
//   inside it (0 to 1). For the second, it cuts the ship's box into 100 small squares and counts
//   the ones inside.
// - TacticalWorld finds the clouds at the start of a level. It decides if our ship is in a cloud.
//   It marks whether each cloud is seen on DRADIS or in visual range.
// - SensorSystem and EnemyBrain use that to lower our signal and the enemy's sight range.
// - The plot shows the cloud's picture in visual range. Past it, while DRADIS listens, it shows a
//   hollow circle out to DRADIS range.
// - The Level window uses it to draw, add and resize clouds.
// - In the Scene view it draws a blue box with the cloud's name.
//
// Technical:
// - Data: widthUnits and heightUnits, in units, centred on the object. The box never turns. It
//   lines up with north.
// - Inside(p) tests a world point. ShareInside(at, heading, half) gives the share of a ship's box
//   inside, 0 to 1.
// - TacticalWorld finds the clouds when a level starts. Each step it sets SensorSystem.inCloud and
//   each cloud's seen and inSight.
// - PlotPanel reads seen and inSight. It draws the picture (its cloudImage) or the hollow circle.
// - Numbers: cloudInsideShare, cloudSignalShare and cloudSeenUnits, in the Sensors part of the ship
//   stats file. The cloud's size is in the Inspector.
// - The cloud has no collider. Crashes and hits never see it.
// - Trap: only our ship is checked. An enemy ship in a cloud is not hidden.
// - Trap: for seen and inSight, TacticalWorld treats the cloud as a circle. Its radius is half the
//   longer side.
// - Other callers: LevelWindow and PlaytestRunner.
// =================================================================================================

using UnityEngine;

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
