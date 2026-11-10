// =================================================================================================
// CssEase.cs
//
// What it does:
// This is a speed curve for moving things on the board, such as sliding drawers and flipping
// rockers. It uses the same curves as a web page, written as four numbers. It does not run by
// itself. Other board scripts use it.
//
// How it works:
// - A curve is four numbers. They can be set in the Inspector wherever a script shows one.
// - Two curves come ready made: Ease, the web's standard ease, and Linear, an even speed.
// - Evaluate takes how far through the move it is, from 0 to 1. It gives back how far the thing
//   should have moved. With some numbers this goes past 1 for a moment, and the thing overshoots.
// - It works out the answer with a quick repeated guess. If that fails, it halves the range again
//   and again until it is close enough.
// - BoardMotion, Rockers, PlotGrid and Eased use it. Eased is used by the Sensors, Plot, Hangar
//   and Helm panels.
//
// Technical:
// - Data: x1, y1, x2 and y2, the two middle points of a CSS cubic-bezier curve. Time and progress
//   run from 0 to 1.
// - Evaluate(t) clamps t to 0 to 1. It tries 8 Newton steps, then up to 30 halving steps.
// - CssEase.Ease is (0.25, 0.1, 0.25, 1). CssEase.Linear is (0, 0, 1, 1).
// - BoardMotion and Rockers keep their curves as Inspector fields. Each Eased keeps one curve.
//   PlotGrid calls CssEase.Ease.Evaluate.
// - Numbers: set in the Inspector. An Eased marked [Tune] also shows its curve on the Tuning
//   window's Board tab.
// - Trap: y1 or y2 above 1 gives a result above 1. Code using it must allow the overshoot.
// =================================================================================================

using UnityEngine;

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
