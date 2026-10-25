using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Step 77 (owner, 2026-10-26). A debris field in a level: a shape filled with wreckage, rock or station
/// pieces. Its pieces are real copies saved in the level under this object, so the field is the same every
/// play and any piece can be moved by hand. Scatter and Reshuffle in its inspector lay them out again
/// (Editor/DebrisFieldEditor). Nothing here runs at play. Units are km (1 unit = 1 km, x east, y north);
/// the shape turns with this object.
/// </summary>
public class DebrisField : MonoBehaviour
{
    public enum Shape { Circle, Ring, Band, Line }
    public enum Spread { Even, ThickerInMiddle, Clumps }

    [Header("Shape, km")]
    public Shape shape = Shape.Circle;
    [Tooltip("Circle and ring: the outer radius.")]
    [Min(1)] public float radiusKm = 8000f;
    [Tooltip("Ring: the inner radius.")]
    [Min(0)] public float innerRadiusKm = 5000f;
    [Tooltip("Band and line: the length, along this object's x (east when not turned).")]
    [Min(1)] public float lengthKm = 20000f;
    [Tooltip("Band: the width.")]
    [Min(0)] public float widthKm = 4000f;

    [Header("Pieces")]
    [Tooltip("Prefabs to scatter, picked at random. The kit is in Assets/Prefabs/Terrain.")]
    public GameObject[] pieces = new GameObject[0];
    [Min(0)] public int count = 120;
    [Tooltip("Each piece's size across, km, picked between these two.")]
    public Vector2 sizeKm = new Vector2(150f, 450f);
    [Tooltip("The smallest gap between two pieces' middles, km. A few hundred km lets a ship fly through and hide behind (Step 73).")]
    [Min(0)] public float gapKm = 400f;

    [Header("Spread")]
    public Spread spread = Spread.Even;
    [Tooltip("Clumps: how many.")]
    [Min(1)] public int clumps = 5;
    [Tooltip("The same number gives the same layout. Reshuffle picks a new one.")]
    public int seed = 1;

    public struct Piece { public GameObject prefab; public Vector2 at; public float turnDeg, sizeKm; }

    /// <summary>Where each piece goes, in this object's space, km. The same seed gives the same layout.
    /// Fewer than count come back when the gap leaves no more room.</summary>
    public List<Piece> Layout()
    {
        var placed = new List<Piece>();
        if (pieces == null || pieces.Length == 0) return placed;
        var rng = new System.Random(seed);
        var centres = new Vector2[clumps];
        for (int i = 0; i < clumps; i++) centres[i] = Point(rng, Spread.Even);
        // ponytail: each new piece is checked against every placed one; fine for a few thousand pieces
        for (int tries = 0; placed.Count < count && tries < count * 50; tries++)
        {
            var at = spread == Spread.Clumps ? NearClump(rng, centres) : Point(rng, spread);
            if (!Inside(at) || TooClose(at, placed)) continue;
            placed.Add(new Piece
            {
                prefab = pieces[rng.Next(pieces.Length)],
                at = at,
                turnDeg = Next(rng) * 360f,
                sizeKm = Mathf.Lerp(sizeKm.x, sizeKm.y, Next(rng)),
            });
        }
        return placed;
    }

    /// <summary>Is this point (this object's space, km) inside the shape?</summary>
    public bool Inside(Vector2 p)
    {
        switch (shape)
        {
            case Shape.Circle: return p.magnitude <= radiusKm;
            case Shape.Ring: { float r = p.magnitude; return r <= radiusKm && r >= Mathf.Min(innerRadiusKm, radiusKm); }
            case Shape.Band: return Mathf.Abs(p.x) <= lengthKm / 2 && Mathf.Abs(p.y) <= widthKm / 2;
            default: return Mathf.Abs(p.x) <= lengthKm / 2 && Mathf.Abs(p.y) <= sizeKm.y / 2; // a line, a piece wide
        }
    }

    // A point in the shape. ThickerInMiddle crowds it toward the middle: the centre, the ring's middle
    // line, the band's or line's centre line.
    Vector2 Point(System.Random rng, Spread s)
    {
        bool mid = s == Spread.ThickerInMiddle;
        float u = Next(rng), a = Next(rng) * 2 * Mathf.PI;
        float across = mid ? Next(rng) - Next(rng) : Next(rng) * 2 - 1; // -1..1; the difference of two crowds 0
        switch (shape)
        {
            case Shape.Circle:
                return Dir(a) * radiusKm * (mid ? u : Mathf.Sqrt(u)); // the square root spreads it evenly over the area
            case Shape.Ring:
            {
                float lo = Mathf.Min(innerRadiusKm, radiusKm), hi = radiusKm;
                return Dir(a) * (mid ? (lo + hi) / 2 + across * (hi - lo) / 2 : Mathf.Sqrt(Mathf.Lerp(lo * lo, hi * hi, u)));
            }
            case Shape.Band:
                return new Vector2((u * 2 - 1) * lengthKm / 2, across * widthKm / 2);
            default:
                return new Vector2((u * 2 - 1) * lengthKm / 2, across * sizeKm.y / 2);
        }
    }

    // A point near one of the clump centres: within a sixth of the field's size.
    Vector2 NearClump(System.Random rng, Vector2[] centres)
    {
        var c = centres[rng.Next(centres.Length)];
        float reach = (shape == Shape.Circle || shape == Shape.Ring ? radiusKm : lengthKm / 2) / 6f;
        return c + Dir(Next(rng) * 2 * Mathf.PI) * reach * Mathf.Sqrt(Next(rng));
    }

    bool TooClose(Vector2 at, List<Piece> placed)
    {
        float g2 = gapKm * gapKm;
        foreach (var p in placed) if ((p.at - at).sqrMagnitude < g2) return true;
        return false;
    }

    static float Next(System.Random rng) => (float)rng.NextDouble();
    static Vector2 Dir(float a) => new Vector2(Mathf.Cos(a), Mathf.Sin(a));

#if UNITY_EDITOR
    // the shape's outline and the field's name, in the debris grey; the pieces show as themselves
    private void OnDrawGizmos()
    {
        using (new UnityEditor.Handles.DrawingScope(new Color(0.62f, 0.66f, 0.7f, 0.8f), Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one)))
        {
            float top;
            switch (shape)
            {
                case Shape.Circle:
                    UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, radiusKm);
                    top = radiusKm;
                    break;
                case Shape.Ring:
                    UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, radiusKm);
                    UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, Mathf.Min(innerRadiusKm, radiusKm));
                    top = radiusKm;
                    break;
                case Shape.Band:
                {
                    float x = lengthKm / 2, y = widthKm / 2;
                    UnityEditor.Handles.DrawPolyLine(new Vector3(-x, -y), new Vector3(x, -y), new Vector3(x, y), new Vector3(-x, y), new Vector3(-x, -y));
                    top = y;
                    break;
                }
                default:
                    UnityEditor.Handles.DrawLine(new Vector3(-lengthKm / 2, 0), new Vector3(lengthKm / 2, 0));
                    top = 0;
                    break;
            }
            UnityEditor.Handles.Label(new Vector3(0, top, 0), name);
        }
    }
#endif
}
