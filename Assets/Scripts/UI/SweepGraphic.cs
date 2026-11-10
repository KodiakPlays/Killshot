// =================================================================================================
// SweepGraphic.cs
//
// What it does:
// Draws the DRADIS sweep on the plot: a bright line that turns round our ship, with a faint tail
// fading out behind it. It is on the Plot panel prefab. It shows while DRADIS is on.
//
// How it works:
// - It draws the sweep once, pointing north, filling its box. The line runs from a small circle
//   round our ship out to the rim.
// - The tail is a band behind the line. It fades from faint to nothing over the tail's angle. The
//   angle is in the Tuning window, Board tab, Sensors.
// - PlotPanel does the moving. It sizes the box to the sensor circle, puts it on our ship, and
//   turns it every frame.
// - PlotPanel sets the line and tail strength for the white or amber grid. It sets the small
//   circle's size. The sweep is redrawn only when one of these changes.
//
// Technical:
// - Data: tailDegrees, edgeAlpha and tailAlpha (0 to 1), lineWidth and innerPx (px).
// - PlotPanel calls SetInner and SetAlphas. Each one redraws only when its value changes.
// - PlotPanel sizes, places and turns the sweep's box. This script never turns anything.
// - The picture uses the graphic's colour, made fainter by edgeAlpha and tailAlpha.
// - The tail is cut into slices of about 2 degrees. Each slice fades a little more.
// - Numbers: tailDegrees is in the "Sensors" [Tune] section. lineWidth is in the Inspector.
// - Trap: the tail is drawn behind the line for a clockwise turn. A turn the other way would lead
//   with the tail.
// - Trap: PlaytestRunner reads innerPx by its name. Renaming the field breaks that check.
// =================================================================================================

using UnityEngine;
using UnityEngine.UI;

public class SweepGraphic : MaskableGraphic
{
    [Tooltip("How far the fading tail reaches behind the edge, degrees. The design gives no figure.")]
    [Tune("Sensors")] [SerializeField] private float tailDegrees = 60f;
    [Tooltip("sweepEdge / sweepTail alpha: white grid .7 / .09, amber grid .75 / .11 (the panel sets them).")]
    [SerializeField] private float edgeAlpha = 0.7f, tailAlpha = 0.09f;
    [SerializeField] private float lineWidth = 1.5f;
    [Tooltip("Owner 2026-10-24: the sweep starts at a small circle round our ship, not at the ship. Its radius, px (the panel sets it).")]
    [SerializeField] private float innerPx;

    public void SetInner(float px)
    {
        if (px == innerPx) return;
        innerPx = px;
        SetVerticesDirty();
    }

    public void SetAlphas(float edge, float tail)
    {
        if (edge == edgeAlpha && tail == tailAlpha) return;
        edgeAlpha = edge; tailAlpha = tail;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        var r = rectTransform.rect;
        Vector2 c = r.center;
        float rad = Mathf.Min(r.width, r.height) / 2f;
        if (rad <= 0f) return;
        float inner = Mathf.Clamp(innerPx, 0f, rad);

        // the tail: a band behind the edge (the sweep turns clockwise) from the inner circle to the rim, fading
        // from tailAlpha to 0. Each slice has its own vertices so the fade runs round the circle, not along the radius.
        int seg = Mathf.Clamp(Mathf.CeilToInt(tailDegrees / 2f), 4, 90);
        for (int k = 0; k < seg; k++)
        {
            float b0 = -tailDegrees * k / seg, b1 = -tailDegrees * (k + 1) / seg; // compass degrees behind the edge
            Color c0 = Ink(tailAlpha * (1f - (float)k / seg)), c1 = Ink(tailAlpha * (1f - (float)(k + 1) / seg));
            int i = vh.currentVertCount;
            vh.AddVert(c + Dir(b0) * inner, c0, Vector2.zero);
            vh.AddVert(c + Dir(b0) * rad, c0, Vector2.zero);
            vh.AddVert(c + Dir(b1) * rad, c1, Vector2.zero);
            vh.AddVert(c + Dir(b1) * inner, c1, Vector2.zero);
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i, i + 2, i + 3);
        }

        // the edge: a line from the inner circle out to the rim, pointing north
        Color e = Ink(edgeAlpha);
        float w = lineWidth / 2f;
        int j = vh.currentVertCount;
        vh.AddVert(c + new Vector2(-w, inner), e, Vector2.zero);
        vh.AddVert(c + new Vector2(-w, rad), e, Vector2.zero);
        vh.AddVert(c + new Vector2(w, rad), e, Vector2.zero);
        vh.AddVert(c + new Vector2(w, inner), e, Vector2.zero);
        vh.AddTriangle(j, j + 1, j + 2);
        vh.AddTriangle(j, j + 2, j + 3);
    }

    Color Ink(float a) { var k = color; k.a *= a; return k; }

    // compass bearing (0 = north, clockwise) to a unit vector in this rect's space (y up)
    static Vector2 Dir(float deg) { float b = deg * Mathf.Deg2Rad; return new Vector2(Mathf.Sin(b), Mathf.Cos(b)); }
}
