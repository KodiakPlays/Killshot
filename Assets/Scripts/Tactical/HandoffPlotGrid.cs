using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The plot's two grid layers, drawn as one UI mesh. Every default comes from the handoff's
/// textures/plot-grid.json. The minor cell is the step whose on-screen size is nearest the target;
/// both layers are centred on the plot, as the board's CSS background-position 50% 50% does.
/// Tint toggles white and amber (owner, 2026-10-04 — the handoff's red is replaced by amber).
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
    [Tooltip("Cell height as a fraction of cell width (plot-grid.json, layers).")]
    [SerializeField] private float cellHeightRatio = 0.9557f;
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

    public Tint GridTint { get { return tint; } set { tint = value; SetVerticesDirty(); } }
    public float Zoom { get { return zoom; } set { zoom = Mathf.Clamp(value, 0.1f, 1f); SetVerticesDirty(); } }

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
        bool white = tint == Tint.White;
        Lines(vh, r, cell, cell * cellHeightRatio, Enc(white ? whiteMinor : amberMinor));
        Lines(vh, r, cell * majorEvery, cell * majorEvery * cellHeightRatio, Enc(white ? whiteMajor : amberMajor));
    }

    Color Enc(Color c)
    {
        if (!gammaBoard) return c;
        var g = c.gamma;
        g.a = c.a;
        return g;
    }

    // Lines at (size - cell) / 2 + n * cell from the top-left, both axes.
    void Lines(VertexHelper vh, Rect r, float cw, float ch, Color c)
    {
        float x0 = (r.width - cw) / 2f;
        x0 -= Mathf.Ceil(x0 / cw) * cw;
        for (float x = x0; x <= r.width; x += cw)
            if (x >= 0) Quad(vh, r.xMin + x, r.yMin, lineWidthPx, r.height, c);

        float y0 = (r.height - ch) / 2f;
        y0 -= Mathf.Ceil(y0 / ch) * ch;
        for (float y = y0; y <= r.height; y += ch)
            if (y >= 0) Quad(vh, r.xMin, r.yMax - y - lineWidthPx, r.width, lineWidthPx, c);
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
