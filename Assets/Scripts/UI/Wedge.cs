// =================================================================================================
// Wedge.cs
//
// What it does:
// Draws a filled pie slice inside its box on the battle board. The Sensors panel uses one for the
// ACTIVE SCAN cone on its dial. The Engineering panel uses them for the ring pieces that show each
// room's strength.
//
// How it works:
// - The slice has a start angle and a width in degrees. 0 is straight up, and angles go clockwise.
// - The panel that uses it sets the angles. The Sensors panel sets them from the cone's bearing
//   and width. The Engineering panel sets a fixed ring of pieces for each room. It lights one
//   piece per point of room strength.
// - It fans out from the middle of its box to the rim. Wider slices use more pieces, up to the
//   number set in the Inspector.
// - It uses the colour set on it. The panel that uses it sets that colour.
// - It converts the colour for the board's way of mixing colours. A switch in the Inspector turns
//   this off.
//
// Technical:
// - Data: fromDeg and sweepDeg (degrees, 0 up, clockwise), segments (the most pieces for a full
//   circle) and gammaBoard.
// - Set(from, sweep) changes the slice and redraws. SensorsPanel calls it for the scan cone.
//   EngineeringPanel calls it for each piece of a room's ring.
// - SensorsPanel passes the cone's bearing minus its half width, and twice the half width.
// - It builds one UI mesh: a fan of triangles from the box's middle.
// - The radius is half the box's shorter side.
// - A slice of 0 degrees or less draws nothing.
// =================================================================================================

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class Wedge : MaskableGraphic
{
    [SerializeField] private float fromDeg;
    [SerializeField] private float sweepDeg = 80f;
    [SerializeField, Range(8, 180)] private int segments = 72;
    [Tooltip("On for the handoff board (gamma-space blend): the colour is pre-encoded.")]
    [SerializeField] private bool gammaBoard = true;

    public void Set(float from, float sweep)
    {
        fromDeg = from;
        sweepDeg = sweep;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (sweepDeg <= 0f) return;
        Rect r = rectTransform.rect;
        Vector2 c = r.center;
        float rad = Mathf.Min(r.width, r.height) / 2f;
        Color col = color;
        if (gammaBoard) { var g = col.gamma; g.a = col.a; col = g; }

        vh.AddVert(c, col, Vector2.zero);
        int n = Mathf.Max(2, Mathf.CeilToInt(segments * sweepDeg / 360f));
        for (int i = 0; i <= n; i++)
        {
            float a = (fromDeg + sweepDeg * i / n) * Mathf.Deg2Rad; // CSS: 0 up, clockwise
            vh.AddVert(c + new Vector2(Mathf.Sin(a), Mathf.Cos(a)) * rad, col, Vector2.zero);
            if (i > 0) vh.AddTriangle(0, i, i + 1);
        }
    }
}
