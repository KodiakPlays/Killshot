using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The plot's two grid layers, drawn as one UI mesh. Colours and line weights come from the handoff's
/// textures/plot-grid.json. Tint toggles white and amber (owner, 2026-10-04 — the handoff's red is replaced by amber).
/// Owner 2026-11-01 (a designer's steps): the squares on screen change size as you zoom. Each size fades out as its
/// squares close up, and the next size takes over. The lines are counted from the world's centre, so a line that is in
/// both sizes stays solid. A big square is always 5 x 5 of the squares on screen, so the big lines move up with them.
/// Owner 2026-10-29: the lines carry on past the battle space, and its edge is a heavier line.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class PlotGrid : MaskableGraphic
{
    public enum Tint { White, Amber }

    [Header("Scale (owner 2026-11-01): the square on screen is 50 units at x1, where our ship (200 units) is four squares long")]
    [Tooltip("Board px per 1,000 units at x1: 50 units is 38 px, a quarter of our ship's 152 px picture.")]
    [SerializeField] private float pxPer1000Units = 760f;
    [SerializeField, Range(0.01f, 10f)] private float zoom = 1f;
    [Tooltip("The square sizes, units, smallest first. Each is a whole number of the smallest. The last never fades.")]
    [Tune("Plot & our ship")] [SerializeField] private float[] stepUnits = { 50f, 100f, 200f, 500f, 1000f, 2000f, 5000f };
    [Tooltip("A big square is this many squares each way (owner 2026-11-01: 5 x 5, not 10 x 10).")]
    [Tune("Plot & our ship")] [SerializeField] private int bigEvery = 5;
    [SerializeField] private float lineWidthPx = 1f;
    [Tooltip("The battle space's edge (owner 2026-10-29).")]
    [SerializeField] private float edgeWidthPx = 3f;

    [Header("Fade (owner 2026-11-01): each size of square fades out as it closes up, and the next takes over")]
    [Tooltip("A square at least this wide, board px, shows at full strength.")]
    [Tune("Plot & our ship")] [SerializeField] private float stepFullPx = 35f;
    [Tooltip("A square this wide or narrower, board px, is gone.")]
    [Tune("Plot & our ship")] [SerializeField] private float stepGonePx = 25f;

    [Header("Tint")]
    [SerializeField] private Tint tint = Tint.White;
    [SerializeField] private Color whiteMinor = new Color32(0xE8, 0xED, 0xF0, 20);  // 8%, owner 2026-11-01: a quieter grid
    [Tooltip("As bright as ink.data .26 drawn over a small line was, now the small line leaves a gap for it.")]
    [SerializeField] private Color whiteMajor = new Color32(0xE8, 0xED, 0xF0, 61);  // 24%
    [SerializeField] private Color amberMinor = new Color32(0xE0, 0xA0, 0x20, 54);  // 21%
    [Tooltip("As bright as accent.caution .62 drawn over a small line was, now the small line leaves a gap for it.")]
    [SerializeField] private Color amberMajor = new Color32(0xE0, 0xA0, 0x20, 126); // 49%
    [Tooltip("The edge on the white grid: amber (owner 2026-10-29).")]
    [SerializeField] private Color whiteEdge = new Color32(0xE0, 0xA0, 0x20, 230);  // accent.caution .9
    [Tooltip("The edge on the amber grid: red, as amber would not show (owner 2026-10-29).")]
    [SerializeField] private Color amberEdge = new Color32(0xE0, 0x4B, 0x3C, 230);  // accent.alarm .9
    [Tooltip("On for the handoff board, which blends in gamma space (GammaBoardPresenter): the colours " +
             "above are pre-encoded so the canvas's linear conversion lands on them exactly.")]
    [SerializeField] private bool gammaBoard = true;

    [Header("Forming (owner 2026-10-26): the lines glide in from off the plot, big squares first, then small")]
    [Tooltip("When the big-square lines have all arrived, as a share of the forming time.")]
    [SerializeField, Range(0f, 1f)] private float bigLinesEnd = 0.5f;
    [Tooltip("When the small lines start, as a share of the forming time. They run to the end.")]
    [SerializeField, Range(0f, 1f)] private float smallLinesStart = 0.4f;
    [Tooltip("The share of its group's time one line takes to glide across. The rest spreads the starts across the plot.")]
    [SerializeField, Range(0.05f, 1f)] private float lineGlide = 0.55f;

    /// <summary>The smallest square, units: every line is a whole number of these from the world's centre.</summary>
    public float SmallestUnits => stepUnits[0];
    /// <summary>Board px per unit at this zoom.</summary>
    public float PxPerUnit => pxPer1000Units * zoom / 1000f;
    /// <summary>Step 120: how far the grid has formed, 0 (no lines) to 1 (whole). The plot panel sets it as the board appears.</summary>
    public float Formed { get { return formed; } set { if (value != formed) { formed = value; SetVerticesDirty(); } } }
    private float formed = 1f;
    /// <summary>The plot's scale: board px per 1,000 units at x1 (the plot panel reads it too).</summary>
    public float PxPer1000Units => pxPer1000Units;
    public Tint GridTint { get { return tint; } set { tint = value; SetVerticesDirty(); } }
    public float Zoom { get { return zoom; } set { zoom = Mathf.Clamp(value, 0.01f, 10f); SetVerticesDirty(); } }
    /// <summary>Step 41: how far the lines are shifted, board px (x right, y down), so the grid
    /// stays fixed in the world when the plot view is panned.</summary>
    public Vector2 OffsetPx { get { return offsetPx; } set { if (value != offsetPx) { offsetPx = value; SetVerticesDirty(); } } }
    private Vector2 offsetPx;
    /// <summary>Half the battle space's width (x) and height (y), units (the plot panel sets it from ShipState).</summary>
    public Vector2 ArenaHalfUnits { get { return arenaHalfUnits; } set { if (value != arenaHalfUnits) { arenaHalfUnits = value; SetVerticesDirty(); } } }
    private Vector2 arenaHalfUnits = new Vector2(50000f, 50000f);

    /// <summary>The square on screen now in units, and its width in px.</summary>
    public float CellUnits(out float cellPx)
    {
        int i = 0;
        while (i < stepUnits.Length - 1 && Shows(i, stepFullPx, stepGonePx) < 0.5f) i++;
        cellPx = stepUnits[i] * PxPerUnit;
        return stepUnits[i];
    }

    // How strongly squares of this size show at this zoom, 0 to 1. The last size never fades.
    float Shows(int i, float fullPx, float gonePx) => i >= stepUnits.Length - 1 ? 1f : Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(gonePx, fullPx, stepUnits[i] * PxPerUnit));

    static bool OnStep(double units, float step) => System.Math.Abs(System.Math.IEEERemainder(units, step)) < 0.5;

    /// <summary>How strongly a line this many units from the centre shows, 0 to 1. A size takes over as the one below
    /// it fades, so a line in both stays solid. The edge numbers use this too, with their own widths.</summary>
    public float LineShare(double units, float fullPx, float gonePx) => Share(units, 1, fullPx, gonePx);

    float Share(double units, int every, float fullPx, float gonePx)
    {
        float share = 0f, finer = 0f;
        for (int i = 0; i < stepUnits.Length; i++)
        {
            float shows = Shows(i, fullPx, gonePx);
            if (OnStep(units, stepUnits[i] * every)) share += shows * (1f - finer);
            finer = shows;
        }
        return Mathf.Min(share, 1f);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect r = rectTransform.rect;
        // the world's centre on this rect (x right, y down from its top-left), and the battle space's edges
        float ox = r.width / 2f + offsetPx.x, oy = r.height / 2f + offsetPx.y;
        float hw = arenaHalfUnits.x * PxPerUnit, hh = arenaHalfUnits.y * PxPerUnit;
        float x0 = ox - hw, x1 = ox + hw, y0 = oy - hh, y1 = oy + hh;
        bool white = tint == Tint.White;
        Lines(vh, r, ox, oy, Enc(white ? whiteMinor : amberMinor), Enc(white ? whiteMajor : amberMajor));
        // the edge on top, fading in as the last big line arrives
        Color e = Enc(white ? whiteEdge : amberEdge);
        e.a *= Glide(1f, 0f, bigLinesEnd);
        float w = edgeWidthPx / 2f;
        Box(vh, r, x0 - w, y0 - w, x0 + w, y1 + w, e);
        Box(vh, r, x1 - w, y0 - w, x1 + w, y1 + w, e);
        Box(vh, r, x0 + w, y0 - w, x1 - w, y0 + w, e);
        Box(vh, r, x0 + w, y1 - w, x1 - w, y1 + w, e);
    }

    // A box from (x0, y0) to (x1, y1), rect px (x right, y down), cut to the rect.
    static void Box(VertexHelper vh, Rect r, float x0, float y0, float x1, float y1, Color c)
    {
        x0 = Mathf.Max(x0, 0f); y0 = Mathf.Max(y0, 0f);
        x1 = Mathf.Min(x1, r.width); y1 = Mathf.Min(y1, r.height);
        if (x0 < x1 && y0 < y1) Quad(vh, r.xMin + x0, r.yMax - y1, x1 - x0, y1 - y0, c);
    }

    Color Enc(Color c)
    {
        if (!gammaBoard) return c;
        var g = c.gamma;
        g.a = c.a;
        return g;
    }

    // How far a line has glided in, 0 to 1. Its group runs from..to of the forming time, and `place` is its place
    // across the plot, 0 to 1: the further along, the later it starts.
    float Glide(float place, float from, float to)
    {
        float t = (formed - from) / Mathf.Max(to - from, 0.001f);
        return CssEase.Ease.Evaluate((t - place * (1f - lineGlide)) / lineGlide);
    }

    // Every line that shows, counted from (ox, oy) across the whole plot. Vertical lines come in turn from the top and
    // bottom, horizontal lines from the left and right.
    void Lines(VertexHelper vh, Rect r, float ox, float oy, Color minor, Color big)
    {
        float unit = stepUnits[0], step = unit * PxPerUnit;
        int nth = 0;
        for (int k = Mathf.CeilToInt(-ox / step - 1e-4f); ox + k * step <= r.width + 0.01f; k++)
        {
            Color c; float from, to;
            if (!LineLook(System.Math.Abs(k) * (double)unit, minor, big, out c, out from, out to)) continue;
            float x = ox + k * step, h = r.height * Glide(x / r.width, from, to);
            Quad(vh, r.xMin + x, r.yMax - (nth++ % 2 == 0 ? h : r.height), lineWidthPx, h, c);
        }
        for (int k = Mathf.CeilToInt(-oy / step - 1e-4f); oy + k * step <= r.height + 0.01f; k++)
        {
            Color c; float from, to;
            if (!LineLook(System.Math.Abs(k) * (double)unit, minor, big, out c, out from, out to)) continue;
            float y = oy + k * step, w = r.width * Glide(y / r.height, from, to);
            Quad(vh, r.xMin + (nth++ % 2 == 0 ? 0f : r.width - w), r.yMax - y - lineWidthPx, w, lineWidthPx, c);
        }
    }

    // A line's colour and when it forms; false if it doesn't show at this zoom. Big lines form first, then the rest.
    bool LineLook(double units, Color minor, Color big, out Color c, out float from, out float to)
    {
        float share = LineShare(units, stepFullPx, stepGonePx), bigShare = Share(units, bigEvery, stepFullPx, stepGonePx);
        c = Color.Lerp(minor, big, bigShare);
        c.a *= share;
        from = bigShare > 0.5f ? 0f : smallLinesStart;
        to = bigShare > 0.5f ? bigLinesEnd : 1f;
        return share > 0f;
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
