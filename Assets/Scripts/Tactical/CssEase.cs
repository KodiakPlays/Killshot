using UnityEngine;

/// <summary>
/// CSS timing functions, so the handoff board moves with the same curves as the browser board.
/// cubic-bezier(x1, y1, x2, y2) is solved for x by Newton steps with a bisection fallback.
/// Named curves: "ease" = cubic-bezier(.25, .1, .25, 1), "linear".
/// </summary>
[System.Serializable]
public struct CssEase
{
    public float x1, y1, x2, y2;

    public CssEase(float x1, float y1, float x2, float y2) { this.x1 = x1; this.y1 = y1; this.x2 = x2; this.y2 = y2; }

    public static readonly CssEase Ease = new CssEase(0.25f, 0.1f, 0.25f, 1f);
    public static readonly CssEase Linear = new CssEase(0f, 0f, 1f, 1f);

    /// <summary>Eased progress for linear time t in 0..1. Overshoots above 1 when y1 or y2 do.</summary>
    public float Evaluate(float t)
    {
        t = Mathf.Clamp01(t);
        if (t <= 0f) return 0f;
        if (t >= 1f) return 1f;
        float u = t;
        for (int i = 0; i < 8; i++)
        {
            float x = Bez(u, x1, x2) - t;
            if (Mathf.Abs(x) < 1e-5f) return Bez(u, y1, y2);
            float dx = Slope(u, x1, x2);
            if (Mathf.Abs(dx) < 1e-6f) break;
            u -= x / dx;
        }
        float lo = 0f, hi = 1f;
        u = t;
        for (int i = 0; i < 30; i++)
        {
            float x = Bez(u, x1, x2);
            if (Mathf.Abs(x - t) < 1e-5f) break;
            if (x < t) lo = u; else hi = u;
            u = (lo + hi) / 2f;
        }
        return Bez(u, y1, y2);
    }

    static float Bez(float u, float a, float b) { float v = 1f - u; return 3f * v * v * u * a + 3f * v * u * u * b + u * u * u; }
    static float Slope(float u, float a, float b) { float v = 1f - u; return 3f * v * v * a + 6f * v * u * (b - a) + 3f * u * u * (1f - b); }
}
