using System.Collections.Generic;
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
    public enum Kind { Point, Line, Ruler, Ring }

    public Kind kind;
    public Vector2 a;          // world
    public Vector2 b;          // world — unused for Point
    public Color colour = Color.white;

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

    public static PlotAnnotation Point(Vector2 world, Color c)
    {
        return new PlotAnnotation { kind = Kind.Point, a = world, b = world, colour = c };
    }

    public static PlotAnnotation Between(Kind k, Vector2 from, Vector2 to, Color c)
    {
        return new PlotAnnotation { kind = k, a = from, b = to, colour = c };
    }
}
