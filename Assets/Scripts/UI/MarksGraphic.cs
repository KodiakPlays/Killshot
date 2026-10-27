using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Step 33i. The plot marks, drawn: a dot (POINT), a line with end dots (RULER), and a dashed ring
/// with its bearing line (COMPASS), in own-force blue (UIStateSpec §8.4). Positions are given in
/// board px; sizes are screen px, so marks keep their size across zoom (--lbl = 1 / zoom).
/// </summary>
public class MarksGraphic : MaskableGraphic
{
    public struct Shape
    {
        public string kind;
        public Vector2 a, b;   // board px, y down
        public float alpha;
        public float inner, half; // "sector" only: inner radius px, half-width degrees
        public Color ink;         // a colour of its own, pre-encoded; left clear, it takes the graphic's colour
    }

    [Tooltip("Point dot, ruler end dots (px).")]
    [Tune("Plot tools")]
    [SerializeField] private float pointDot = 7f, endDot = 6f;
    [Tune("Plot tools")]
    [SerializeField] private float lineWidth = 1.5f;
    [Tooltip("Dashed ring: dash and gap along the ring (px), from asset-map.csv mark_dash (3 px dash, 2 px gap).")]
    [Tune("Plot tools")]
    [SerializeField] private float dash = 3f, dashGap = 2f;
    [Tooltip("Board px of this rect's top-left (the plot sits at 104,100).")]
    public Vector2 origin = new Vector2(104, 100);

    private readonly List<Shape> shapes = new List<Shape>();
    private VertexHelper vh;

    public void Set(List<Shape> s) { shapes.Clear(); shapes.AddRange(s); SetVerticesDirty(); }

    Vector2 L(Vector2 board) { var r = rectTransform.rect; return new Vector2(r.xMin + board.x - origin.x, r.yMax - (board.y - origin.y)); }

    protected override void OnPopulateMesh(VertexHelper v)
    {
        v.Clear();
        vh = v;
        foreach (var s in shapes)
        {
            var c = s.ink.a > 0f ? s.ink : color; c.a *= s.alpha;
            if (s.kind == "point") Dot(s.a, pointDot / 2f, c);
            else if (s.kind == "ruler") { Line(s.a, s.b, c); Dot(s.a, endDot / 2f, c); Dot(s.b, endDot / 2f, c); }
            // Step 42: the sensor circle — a solid ring, and the faint fill inside it
            else if (s.kind == "circle") Ring(s.a, (s.b - s.a).magnitude, c, false);
            else if (s.kind == "disc") Dot(s.a, (s.b - s.a).magnitude, c);
            // Step 44: a passive bearing, fading out from our ship
            else if (s.kind == "fade") { var end = c; end.a = 0f; Line(s.a, s.b, c, end); }
            // Step 46: a track's projected course (dashed line) and a weapon's reach (dashed ring, no radius)
            else if (s.kind == "dash") DashedLine(s.a, s.b, c);
            else if (s.kind == "dashring") Ring(s.a, (s.b - s.a).magnitude, c);
            // owner 2026-10-26: the active scan cone — a faint slice of ring round a, and its two edge lines
            else if (s.kind == "sector") Sector(s.a, s.b, s.inner, s.half, c);
            else if (s.kind == "line") Line(s.a, s.b, c);
            else { Line(s.a, s.b, c); Ring(s.a, (s.b - s.a).magnitude, c); }
        }
    }

    void Line(Vector2 a, Vector2 b, Color c) => Line(a, b, c, c);

    void Line(Vector2 a, Vector2 b, Color c, Color cb)
    {
        Vector2 p = L(a), q = L(b);
        if ((q - p).sqrMagnitude < 1e-6f) return;
        Vector2 n = (q - p).normalized;
        n = new Vector2(-n.y, n.x) * (lineWidth / 2f);
        int i = vh.currentVertCount;
        vh.AddVert(p + n, c, Vector2.zero); vh.AddVert(q + n, cb, Vector2.zero);
        vh.AddVert(q - n, cb, Vector2.zero); vh.AddVert(p - n, c, Vector2.zero);
        vh.AddTriangle(i, i + 1, i + 2); vh.AddTriangle(i, i + 2, i + 3);
    }

    void DashedLine(Vector2 a, Vector2 b, Color c)
    {
        float len = (b - a).magnitude, period = dash + dashGap;
        if (len < 1e-3f || period <= 0f) return;
        Vector2 d = (b - a) / len;
        for (float s = 0f; s < len; s += period) Line(a + d * s, a + d * Mathf.Min(len, s + dash), c);
    }

    void Dot(Vector2 at, float r, Color c)
    {
        Vector2 p = L(at);
        int seg = Mathf.Clamp(Mathf.CeilToInt(r / 3f), 16, 720); // smooth at any size
        int i0 = vh.currentVertCount;
        vh.AddVert(p, c, Vector2.zero);
        for (int k = 0; k <= seg; k++)
        {
            float a = k * Mathf.PI * 2f / seg;
            vh.AddVert(p + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r, c, Vector2.zero);
        }
        for (int k = 1; k <= seg; k++) vh.AddTriangle(i0, i0 + k, i0 + k + 1);
    }

    // filled from `inner` out to the rim point's distance, `half` degrees either side of centre → rim
    void Sector(Vector2 centre, Vector2 rim, float inner, float half, Color c)
    {
        Vector2 p = L(centre), q = L(rim) - p;
        float r = q.magnitude;
        if (r <= inner || half <= 0f) return;
        float mid = Mathf.Atan2(q.y, q.x), w = half * Mathf.Deg2Rad;
        int seg = Mathf.Clamp(Mathf.CeilToInt(2f * w * r / 3f), 2, 720); // smooth at any size
        for (int k = 0; k < seg; k++)
        {
            float t0 = mid - w + 2f * w * k / seg, t1 = mid - w + 2f * w * (k + 1) / seg;
            Vector2 d0 = new Vector2(Mathf.Cos(t0), Mathf.Sin(t0)), d1 = new Vector2(Mathf.Cos(t1), Mathf.Sin(t1));
            int i = vh.currentVertCount;
            vh.AddVert(p + d0 * r, c, Vector2.zero); vh.AddVert(p + d1 * r, c, Vector2.zero);
            vh.AddVert(p + d1 * inner, c, Vector2.zero); vh.AddVert(p + d0 * inner, c, Vector2.zero);
            vh.AddTriangle(i, i + 1, i + 2); vh.AddTriangle(i, i + 2, i + 3);
        }
    }

    void Ring(Vector2 centre, float r, Color c, bool dashed = true)
    {
        if (r <= 0f) return;
        Vector2 p = L(centre);
        float rin = r - lineWidth / 2f, rout = r + lineWidth / 2f;
        float circ = 2f * Mathf.PI * r;
        int n = dashed ? Mathf.Max(1, Mathf.FloorToInt(circ / (dash + dashGap))) : 1;
        float step = Mathf.PI * 2f / n, on = dashed ? step * dash / (dash + dashGap) : step;
        for (int k = 0; k < n; k++)
        {
            float a0 = k * step;
            int parts = Mathf.Max(1, Mathf.CeilToInt(on * r / 3f));
            for (int j = 0; j < parts; j++)
            {
                float t0 = a0 + on * j / parts, t1 = a0 + on * (j + 1) / parts;
                Vector2 d0 = new Vector2(Mathf.Cos(t0), Mathf.Sin(t0)), d1 = new Vector2(Mathf.Cos(t1), Mathf.Sin(t1));
                int i = vh.currentVertCount;
                vh.AddVert(p + d0 * rout, c, Vector2.zero); vh.AddVert(p + d1 * rout, c, Vector2.zero);
                vh.AddVert(p + d1 * rin, c, Vector2.zero); vh.AddVert(p + d0 * rin, c, Vector2.zero);
                vh.AddTriangle(i, i + 1, i + 2); vh.AddTriangle(i, i + 2, i + 3);
            }
        }
    }
}
