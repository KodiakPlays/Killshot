using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A filled convex polygon over the rect: a CSS clip-path polygon(). Points are fractions of the
/// box, x from the left and y from the top, as CSS writes them.
/// </summary>
public class Polygon : MaskableGraphic
{
    [SerializeField] private Vector2[] points = { new Vector2(0.5f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f) };

    public void SetPoints(Vector2[] p) { points = p; SetVerticesDirty(); }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (points == null || points.Length < 3) return;
        var r = GetPixelAdjustedRect();
        foreach (var p in points) vh.AddVert(new Vector3(r.xMin + p.x * r.width, r.yMax - p.y * r.height), color, Vector2.zero);
        for (int i = 1; i < points.Length - 1; i++) vh.AddTriangle(0, i, i + 1);
    }
}
