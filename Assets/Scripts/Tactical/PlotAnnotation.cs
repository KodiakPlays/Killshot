using UnityEngine;

/// <summary>
/// One mark a tool has left on the plot.
///
/// Positions are stored in WORLD coordinates and drawn in SCREEN space. That is
/// the whole trick: the mark stays pinned to the world as you pan and zoom, but
/// its lines stay one pixel wide instead of ballooning.
/// </summary>
[System.Serializable]
public class PlotAnnotation
{
    public enum Kind { Point, Ruler, Ring }

    public Kind kind;
    public Vector2 a;              // world
    public Vector2 b;              // world — same as a for a Point
    public Color colour = Color.white;

    /// <summary>Short name shown on the plot, e.g. P3.</summary>
    public string designator = "";

    /// <summary>Mission time this was dropped, in seconds. Points only.</summary>
    public float stampedAt = -1f;

    /// <summary>Range from a to b, in world units.</summary>
    public float Range { get { return Vector2.Distance(a, b); } }

    /// <summary>
    /// Bearing from a to b, in compass degrees: 0 is +Y (north), 90 is +X (east).
    /// This is the convention the whole game uses — not atan2's maths convention.
    /// </summary>
    public float Bearing
    {
        get
        {
            Vector2 d = b - a;
            float deg = Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg;
            return deg < 0f ? deg + 360f : deg;
        }
    }

    /// <summary>What this mark says on the plot. Empty means draw no label.</summary>
    public string Label
    {
        get
        {
            switch (kind)
            {
                case Kind.Point:
                    return stampedAt >= 0f
                        ? designator + "  " + MissionClock.Stamp(stampedAt)
                        : designator;

                case Kind.Ruler:
                    return "RNG " + Mathf.RoundToInt(Range).ToString("N0") + " u\n" +
                           "BRG " + Mathf.RoundToInt(Bearing).ToString("000") + "°";

                case Kind.Ring:
                    return "R " + Mathf.RoundToInt(Range).ToString("N0") + " u";
            }
            return "";
        }
    }

    /// <summary>Where the label sits, in world coordinates.</summary>
    public Vector2 LabelAnchor
    {
        get { return kind == Kind.Point ? a : b; }
    }

    public static PlotAnnotation Point(Vector2 world, Color c, string designator, float missionTime)
    {
        return new PlotAnnotation {
            kind = Kind.Point, a = world, b = world, colour = c,
            designator = designator, stampedAt = missionTime
        };
    }

    public static PlotAnnotation Between(Kind k, Vector2 from, Vector2 to, Color c)
    {
        return new PlotAnnotation { kind = k, a = from, b = to, colour = c };
    }
}
