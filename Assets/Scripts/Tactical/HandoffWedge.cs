using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A filled pie slice in its rect: the active scan cone on the sensors dial. The board draws it as
/// a CSS conic gradient whose stops the package does not give; the cone's angles and colour come
/// from board-logic.js (coneKnobFrom, coneArc, coneInk). Angles are CSS: 0 up, clockwise.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class HandoffWedge : MaskableGraphic
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
