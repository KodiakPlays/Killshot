using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Step 33j. One CSS transition on one number: a new target starts from the value on screen and
/// eases there over <see cref="seconds"/> (UIStateSpec §9.1). Zero seconds means no transition —
/// the design leaves that property instant. HandoffBoardControls ticks every live one each frame.
/// </summary>
[System.Serializable]
public class HandoffEased
{
    [Tooltip("Transition duration; 0 = instant (no CSS transition on this property).")]
    public float seconds;
    public CssEase ease = CssEase.Ease;

    [System.NonSerialized] private float from, to, shown, t0;
    [System.NonSerialized] private bool has;
    [System.NonSerialized] private System.Action<float> apply;

    static readonly List<HandoffEased> live = new List<HandoffEased>();

    public HandoffEased() { }
    public HandoffEased(float seconds, CssEase ease) { this.seconds = seconds; this.ease = ease; }

    /// <summary>Move toward target; apply receives each value shown.</summary>
    public void Set(float target, System.Action<float> apply)
    {
        this.apply = apply;
        if (!has || seconds <= 0f || !Application.isPlaying)
        {
            has = true; shown = to = target; live.Remove(this);
            apply(target);
            return;
        }
        if (target == to) return;
        from = shown; to = target; t0 = Time.unscaledTime;
        if (!live.Contains(this)) live.Add(this);
    }

    /// <summary>Drop every running transition (the board is going away).</summary>
    public static void ClearAll() => live.Clear();

    public static void TickAll()
    {
        float now = Time.unscaledTime;
        for (int i = live.Count - 1; i >= 0; i--)
        {
            var e = live[i];
            float k = (now - e.t0) / e.seconds;
            if (k >= 1f) { e.shown = e.to; live.RemoveAt(i); }
            else e.shown = Mathf.LerpUnclamped(e.from, e.to, e.ease.Evaluate(k));
            e.apply(e.shown);
        }
    }
}
