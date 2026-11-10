// =================================================================================================
// Polygon.cs
//
// What it does:
// Draws a filled shape with straight sides, such as a triangle, inside its box on the board. It is
// on the small arrows in the Helm panel prefab.
//
// How it works:
// - The corners are set in the Inspector. Each corner is a share of the box: x from the left and
//   y from the top, from 0 to 1.
// - It fills the shape by joining the first corner to each pair of corners after it. It needs at
//   least three corners. The shape must have no dents.
// - It uses the colour set on it in the Inspector.
// - Other scripts can give it new corners. No script does that now.
//
// Technical:
// - Data: points, a list of corners as shares of the box (x from the left, y from the top).
// - It is a UI graphic (MaskableGraphic). It builds its mesh in OnPopulateMesh.
// - SetPoints(list) swaps the corners and redraws. No script calls it.
// - Its default shape is a triangle pointing up.
// - Trap: a shape with a dent fills wrong. Fewer than three corners draws nothing.
// =================================================================================================

using UnityEngine;
using UnityEngine.UI;

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
