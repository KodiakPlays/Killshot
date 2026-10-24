using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Step 43. The radar sweep: a bright leading edge with a faint tail fading behind it, turning
/// round our ship while DRADIS is on (UIStateSpec §9.2 rd-sweep; colours sweepEdge and sweepTail
/// from board-logic.js renderVals). Drawn once with the edge pointing north and filling its rect;
/// HandoffPlotPanel sizes the rect to the sensor circle and turns it.
/// </summary>
public class HandoffSweepGraphic : MaskableGraphic
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
