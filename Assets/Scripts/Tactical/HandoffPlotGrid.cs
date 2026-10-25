using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The plot's two grid layers, drawn as one UI mesh. Colours and line weights come from the handoff's
/// textures/plot-grid.json. Tint toggles white and amber (owner, 2026-10-04 — the handoff's red is replaced by amber).
/// Step 65 (owner 2026-10-25): the grid is the battle space, drawn only inside it, and square. The minor cell is
/// still the step whose on-screen size is nearest the target, 5 x 5 to a big square, but the lines start from the
/// battle space's corner, not the plot centre, so its edge is always a line. The design drew cells 4.4 % shorter
/// than wide; that can't hold a square edge.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class HandoffPlotGrid : MaskableGraphic
{
    public enum Tint { White, Amber }

    [Header("Scale")]
    [SerializeField] private float pxPer1000km = 60.9f;
    [SerializeField, Range(0.1f, 1f)] private float zoom = 1f;
    [SerializeField] private float[] stepsKm = { 5000, 2000, 1000, 500, 200, 100, 50, 20 };
    [SerializeField] private float targetCellPx = 61f;
    [SerializeField] private int majorEvery = 5;
    [SerializeField] private float lineWidthPx = 1f;

    [Header("Tint")]
    [SerializeField] private Tint tint = Tint.White;
    [SerializeField] private Color whiteMinor = new Color32(0xE8, 0xED, 0xF0, 33);  // ink.data .13
    [SerializeField] private Color whiteMajor = new Color32(0xE8, 0xED, 0xF0, 66);  // ink.data .26
    [SerializeField] private Color amberMinor = new Color32(0xE0, 0xA0, 0x20, 87);  // accent.caution .34
    [SerializeField] private Color amberMajor = new Color32(0xE0, 0xA0, 0x20, 158); // accent.caution .62
    [Tooltip("On for the handoff board, which blends in gamma space (GammaBoardPresenter): the colours " +
             "above are pre-encoded so the canvas's linear conversion lands on them exactly.")]
    [SerializeField] private bool gammaBoard = true;

    public int MajorEvery => majorEvery;
    public Tint GridTint { get { return tint; } set { tint = value; SetVerticesDirty(); } }
    public float Zoom { get { return zoom; } set { zoom = Mathf.Clamp(value, 0.1f, 1f); SetVerticesDirty(); } }
    /// <summary>Step 41: how far the lines are shifted, board px (x right, y down), so the grid
    /// stays fixed in the world when the plot view is panned.</summary>
    public Vector2 OffsetPx { get { return offsetPx; } set { if (value != offsetPx) { offsetPx = value; SetVerticesDirty(); } } }
    private Vector2 offsetPx;
    /// <summary>Half the battle space's width (x) and height (y), km (the plot panel sets it from HandoffBoardState).</summary>
    public Vector2 ArenaHalfKm { get { return arenaHalfKm; } set { if (value != arenaHalfKm) { arenaHalfKm = value; SetVerticesDirty(); } } }
    private Vector2 arenaHalfKm = new Vector2(50000f, 50000f);

    /// <summary>The minor step in km, and its width on screen.</summary>
    public float CellKm(out float cellPx)
    {
        float best = stepsKm[0];
        float bestErr = float.MaxValue;
        foreach (float km in stepsKm)
        {
            float px = pxPer1000km * zoom / 1000f * km;
            float err = Mathf.Abs(px - targetCellPx);
            if (err < bestErr) { bestErr = err; best = km; }
        }
        cellPx = pxPer1000km * zoom / 1000f * best;
        return best;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        float cell;
        CellKm(out cell);
        if (cell < 2f) return;
        Rect r = rectTransform.rect;
        // the world origin on this rect (x right, y down from its top-left), and the battle space's edges clipped to it
        float ox = r.width / 2f + offsetPx.x, oy = r.height / 2f + offsetPx.y;
        float hw = arenaHalfKm.x * pxPer1000km * zoom / 1000f, hh = arenaHalfKm.y * pxPer1000km * zoom / 1000f;
        float left = Mathf.Max(0f, ox - hw), right = Mathf.Min(r.width, ox + hw);
        float top = Mathf.Max(0f, oy - hh), bottom = Mathf.Min(r.height, oy + hh);
        if (left >= right || top >= bottom) return;
        bool white = tint == Tint.White;
        // from the battle space's top-left corner, so its edges fall on lines
        Lines(vh, r, ox - hw, oy - hh, cell, left, right, top, bottom, Enc(white ? whiteMinor : amberMinor));
        Lines(vh, r, ox - hw, oy - hh, cell * majorEvery, left, right, top, bottom, Enc(white ? whiteMajor : amberMajor));
    }

    Color Enc(Color c)
    {
        if (!gammaBoard) return c;
        var g = c.gamma;
        g.a = c.a;
        return g;
    }

    // Lines every `step` px from (ox, oy), only between the battle space's edges (left..right, top..bottom).
    void Lines(VertexHelper vh, Rect r, float ox, float oy, float step, float left, float right, float top, float bottom, Color c)
    {
        for (float x = ox + Mathf.Ceil((left - ox) / step - 1e-4f) * step; x <= right + 0.01f; x += step)
            Quad(vh, r.xMin + x, r.yMax - bottom, lineWidthPx, bottom - top, c);
        for (float y = oy + Mathf.Ceil((top - oy) / step - 1e-4f) * step; y <= bottom + 0.01f; y += step)
            Quad(vh, r.xMin + left, r.yMax - y - lineWidthPx, right - left, lineWidthPx, c);
    }

    static void Quad(VertexHelper vh, float x, float y, float w, float h, Color c)
    {
        int i = vh.currentVertCount;
        vh.AddVert(new Vector3(x, y), c, Vector2.zero);
        vh.AddVert(new Vector3(x, y + h), c, Vector2.zero);
        vh.AddVert(new Vector3(x + w, y + h), c, Vector2.zero);
        vh.AddVert(new Vector3(x + w, y), c, Vector2.zero);
        vh.AddTriangle(i, i + 1, i + 2);
        vh.AddTriangle(i + 2, i + 3, i);
    }
}
