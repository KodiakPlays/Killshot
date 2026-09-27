using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws every annotation as one UI mesh, rebuilt each frame from world
/// coordinates projected to screen.
///
/// One Graphic for the lot, so the whole plot's annotation layer costs a single
/// draw call however many marks are on it. Line thickness is in pixels, so
/// nothing thickens when you zoom in.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class PlotOverlay : MaskableGraphic
{
    [SerializeField] private PlotCamera plot;

    [Header("Style")]
    [SerializeField] private float lineWidth = 1.5f;
    [SerializeField] private float pointSize = 9f;
    [SerializeField] private float ringSegments = 72f;
    [SerializeField] private float dashLength = 9f;
    [SerializeField] private float dashGap = 6f;

    private readonly List<PlotAnnotation> marks = new List<PlotAnnotation>();

    /// <summary>The annotation being dragged right now, drawn but not yet committed.</summary>
    private PlotAnnotation preview;

    public PlotCamera Plot { get { return plot; } set { plot = value; } }
    public IList<PlotAnnotation> Marks { get { return marks; } }

    public void Add(PlotAnnotation m) { marks.Add(m); SetVerticesDirty(); }
    public void Clear() { marks.Clear(); SetVerticesDirty(); }

    public void SetPreview(PlotAnnotation m) { preview = m; SetVerticesDirty(); }
    public void CommitPreview()
    {
        if (preview != null) marks.Add(preview);
        preview = null;
        SetVerticesDirty();
    }

    /// <summary>Remove the most recent mark. Returns false if there was nothing to remove.</summary>
    public bool Undo()
    {
        if (marks.Count == 0) return false;
        marks.RemoveAt(marks.Count - 1);
        SetVerticesDirty();
        return true;
    }

    /// <summary>Remove the mark nearest this world point, within a pixel radius.</summary>
    public bool EraseNear(Vector2 world, float pixelRadius)
    {
        if (plot == null || marks.Count == 0) return false;
        float worldRadius = pixelRadius * plot.WorldPerPixel;
        int best = -1;
        float bestDist = float.MaxValue;
        for (int i = 0; i < marks.Count; i++)
        {
            float d = DistanceToMark(marks[i], world);
            if (d < bestDist) { bestDist = d; best = i; }
        }
        if (best < 0 || bestDist > worldRadius) return false;
        marks.RemoveAt(best);
        SetVerticesDirty();
        return true;
    }

    private static float DistanceToMark(PlotAnnotation m, Vector2 p)
    {
        if (m.kind == PlotAnnotation.Kind.Point) return Vector2.Distance(m.a, p);
        if (m.kind == PlotAnnotation.Kind.Ring)
            return Mathf.Abs(Vector2.Distance(m.a, p) - m.Range);

        // Distance to the segment a-b.
        Vector2 ab = m.b - m.a;
        float len2 = ab.sqrMagnitude;
        if (len2 < 1e-6f) return Vector2.Distance(m.a, p);
        float t = Mathf.Clamp01(Vector2.Dot(p - m.a, ab) / len2);
        return Vector2.Distance(p, m.a + ab * t);
    }

    private void Update()
    {
        // The projection changes whenever the camera moves or zooms, so the mesh
        // has to be rebuilt. Only when there is something to draw.
        if (marks.Count > 0 || preview != null) SetVerticesDirty();
    }

    // ── mesh building ────────────────────────────────────────────────────────

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (plot == null) return;

        for (int i = 0; i < marks.Count; i++) Draw(vh, marks[i]);
        if (preview != null) Draw(vh, preview);
    }

    /// <summary>World to this RectTransform's local space, via the plot camera.</summary>
    private Vector2 ToLocal(Vector2 world)
    {
        Vector2 screen = plot.PlotToScreen(world);
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, screen, canvas != null ? canvas.worldCamera : null, out local);
        return local;
    }

    private void Draw(VertexHelper vh, PlotAnnotation m)
    {
        Vector2 a = ToLocal(m.a);
        switch (m.kind)
        {
            case PlotAnnotation.Kind.Point:
                Cross(vh, a, pointSize, m.colour);
                break;

            case PlotAnnotation.Kind.Ruler:
                Dashed(vh, a, ToLocal(m.b), lineWidth, m.colour);
                Cross(vh, a, pointSize * 0.7f, m.colour);
                Cross(vh, ToLocal(m.b), pointSize * 0.7f, m.colour);
                break;

            case PlotAnnotation.Kind.Ring:
                Ring(vh, a, Vector2.Distance(a, ToLocal(m.b)), lineWidth, m.colour);
                break;
        }
    }

    private void Segment(VertexHelper vh, Vector2 p0, Vector2 p1, float w, Color c)
    {
        Vector2 d = p1 - p0;
        float len = d.magnitude;
        if (len < 0.01f) return;
        Vector2 n = new Vector2(-d.y, d.x) / len * (w * 0.5f);

        int i0 = vh.currentVertCount;
        vh.AddVert(p0 - n, c, Vector2.zero);
        vh.AddVert(p0 + n, c, Vector2.zero);
        vh.AddVert(p1 + n, c, Vector2.zero);
        vh.AddVert(p1 - n, c, Vector2.zero);
        vh.AddTriangle(i0, i0 + 1, i0 + 2);
        vh.AddTriangle(i0, i0 + 2, i0 + 3);
    }

    private void Dashed(VertexHelper vh, Vector2 p0, Vector2 p1, float w, Color c)
    {
        float len = Vector2.Distance(p0, p1);
        if (len < 0.01f) return;
        Vector2 dir = (p1 - p0) / len;
        float t = 0f;
        while (t < len)
        {
            float e = Mathf.Min(t + dashLength, len);
            Segment(vh, p0 + dir * t, p0 + dir * e, w, c);
            t = e + dashGap;
        }
    }

    private void Cross(VertexHelper vh, Vector2 p, float size, Color c)
    {
        float h = size * 0.5f;
        Segment(vh, new Vector2(p.x - h, p.y), new Vector2(p.x + h, p.y), lineWidth, c);
        Segment(vh, new Vector2(p.x, p.y - h), new Vector2(p.x, p.y + h), lineWidth, c);
    }

    private void Ring(VertexHelper vh, Vector2 centre, float radius, float w, Color c)
    {
        if (radius < 0.5f) return;
        int seg = Mathf.Clamp(Mathf.RoundToInt(ringSegments), 12, 180);
        Vector2 prev = centre + new Vector2(radius, 0f);
        for (int i = 1; i <= seg; i++)
        {
            float ang = (i / (float)seg) * Mathf.PI * 2f;
            Vector2 next = centre + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * radius;
            Segment(vh, prev, next, w, c);
            prev = next;
        }
    }
}
