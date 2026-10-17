using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Step 33h-3. The periscope sight picture: board-logic.js drawScope, redrawn every frame in
/// accent.alarm on the periscope ground — a corridor of ribs drifting inward, rails to a far
/// aperture, and the horizon and elevation ladder. Canvas px, y down, as the board draws it.
/// Sits under a circle mask (the aperture is a 340 px circle).
/// </summary>
public class HandoffScopeSight : MaskableGraphic
{
    [Tooltip("scopeZoom, set by the periscope panel.")]
    public float zoom = 1.4f;
    [Tooltip("scopeT per second. The board adds 0.0009 per animation frame, so 0.054 at 60 fps.")]
    [SerializeField] private float drift = 0.054f;
    [SerializeField] private Color ground = new Color32(0x0B, 0x04, 0x05, 0xFF);
    [SerializeField] private Color ink = new Color32(0xE0, 0x4B, 0x3C, 0xFF);

    private float t;
    private VertexHelper vh;
    private Rect r;

    private void Update()
    {
        t += drift * Time.unscaledDeltaTime;
        SetVerticesDirty();
    }

    // gamma board: pre-encode the colour, keep the canvas alpha
    Color A(float a) { var c = ink.gamma; c.a = a; return c; }
    Vector2 P(float x, float y) => new Vector2(r.xMin + x, r.yMax - y);

    void Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color ca, Color cb)
    {
        int i = vh.currentVertCount;
        vh.AddVert(a, ca, Vector2.zero); vh.AddVert(b, ca, Vector2.zero);
        vh.AddVert(c, cb, Vector2.zero); vh.AddVert(d, cb, Vector2.zero);
        vh.AddTriangle(i, i + 1, i + 2); vh.AddTriangle(i, i + 2, i + 3);
    }

    void Line(float x0, float y0, float x1, float y1, float width, Color c)
    {
        Vector2 a = P(x0, y0), b = P(x1, y1);
        Vector2 n = (b - a).normalized;
        n = new Vector2(-n.y, n.x) * (width / 2f);
        Quad(a + n, b + n, b - n, a - n, c, c);
    }

    protected override void OnPopulateMesh(VertexHelper v)
    {
        v.Clear();
        vh = v;
        r = GetPixelAdjustedRect();
        float w = r.width, h = r.height;
        if (w <= 0 || h <= 0) return;

        var g = ground.gamma; g.a = 1f;
        Quad(P(0, 0), P(w, 0), P(w, h), P(0, h), g, g);

        float z = zoom > 0 ? zoom : 1.4f;
        float cy = h * 0.5f, vp = w / 2f;
        float HW = (w / 2f) * z, HH = (h * 0.27f) * z;
        float FW = Mathf.Max(10f, HW * 0.13f), FH = Mathf.Max(6f, HH * 0.13f);
        System.Func<float, float> env = dx => FH + (HH - FH) * Mathf.Clamp01((dx - FW) / (HW - FW));

        // a tight glow at the far aperture only: radial A(.16) → A(0) out to HW x 0.28
        float gr = HW * 0.28f;
        const int seg = 48;
        Color c0 = A(0.16f), c1 = A(0f);
        for (int i = 0; i < seg; i++)
        {
            float a0 = i * Mathf.PI * 2f / seg, a1 = (i + 1) * Mathf.PI * 2f / seg;
            int k = vh.currentVertCount;
            vh.AddVert(P(vp, cy), c0, Vector2.zero);
            vh.AddVert(P(vp + Mathf.Cos(a0) * gr, cy + Mathf.Sin(a0) * gr), c1, Vector2.zero);
            vh.AddVert(P(vp + Mathf.Cos(a1) * gr, cy + Mathf.Sin(a1) * gr), c1, Vector2.zero);
            vh.AddTriangle(k, k + 1, k + 2);
        }

        // vertical ribs shed from both edges, drifting slowly inward
        const int N = 9;
        for (int i = 0; i < N; i++)
        {
            float u = ((i / (float)N) + t) % 1f;
            float near = 1f - u;
            float hold = FW * 2.6f;
            float dx = hold + (HW - hold) * near;
            float e = env(dx);
            float lw = 0.7f + 1.1f * near;
            var c = A(0.14f + 0.58f * near);
            Line(vp - dx, cy - e, vp - dx, cy + e, lw, c);
            Line(vp + dx, cy - e, vp + dx, cy + e, lw, c);
        }

        // longitudinal rails, from the near edge to the far aperture
        float[] ks = { 1f, 0.58f, 0.24f };
        for (int idx = 0; idx < 3; idx++)
            for (int sy = -1; sy <= 1; sy += 2)
                for (int sx = -1; sx <= 1; sx += 2)
                    Line(vp + sx * HW, cy + sy * HH * ks[idx], vp + sx * FW, cy + sy * FH * ks[idx], idx == 0 ? 1.4f : 1f, A(idx == 0 ? 0.6f : 0.2f));

        // far aperture: a small bright frame
        // ctx.rect strokes with mitred corners: the top and bottom edges run half a line width past
        // each corner, the sides fit between them
        var fa = A(0.85f);
        const float fw = 1.6f, hw = fw / 2f;
        Line(vp - FW - hw, cy - FH, vp + FW + hw, cy - FH, fw, fa);
        Line(vp - FW - hw, cy + FH, vp + FW + hw, cy + FH, fw, fa);
        Line(vp + FW, cy - FH + hw, vp + FW, cy + FH - hw, fw, fa);
        Line(vp - FW, cy - FH + hw, vp - FW, cy + FH - hw, fw, fa);

        // horizon and elevation ladder
        Line(0, cy, 26, cy, 1.4f, A(0.5f));
        Line(w, cy, w - 26, cy, 1.4f, A(0.5f));
        for (int i = -3; i <= 3; i++)
        {
            if (i == 0) continue;
            float y = cy + i * (h * 0.11f);
            if (y < 4 || y > h - 4) continue;
            Line(w - 26, y, w - 26 - (i % 2 == 0 ? 12 : 6), y, 1.4f, A(0.28f)); // the board leaves lineWidth at 1.4
        }
    }
}
