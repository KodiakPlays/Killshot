// =================================================================================================
// Eased.cs
//
// What it does:
// A small helper that moves one number on the battle board smoothly to a new value, over a set
// time. The board's knob pointers, bars and rings use it. Examples are the SCALE knob, the hangar
// bars, the sensor gain, the helm ring and the plot's sensor circle. It is not on any object by
// itself.
//
// How it works:
// - Each one has a time in seconds and an easing curve. They are set in the Inspector of the
//   script that uses it.
// - Given a new value, it starts from the value now on screen and eases there over its time.
// - With a time of 0, on its first value, or outside play mode, it jumps to the value at once.
// - Every frame BoardManager moves every running one on, by real time. Each passes its new value
//   back to the script that uses it.
// - When the board closes, BoardManager stops them all.
//
// Technical:
// - Data: seconds and ease (a CssEase), set in the Inspector of the script that has it. shown is
//   the value on screen now.
// - Set(target, apply) starts a move. apply gets each value shown. A target equal to the last one
//   does nothing.
// - Users: BoardManager's dials, HangarPanel, HelmPanel, PlotPanel and SensorsPanel.
// - TickAll moves every running one on, by Time.unscaledTime. BoardManager calls it every frame,
//   and ClearAll when it goes away.
// - Running ones are kept in one shared list. In a scene with no BoardManager they never move on.
// - Trap: the move goes straight from number to number. An angle does not take the short way round.
// =================================================================================================

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Eased
{
    [Tooltip("Transition duration; 0 = instant (no CSS transition on this property).")]
    public float seconds;
    public CssEase ease = CssEase.Ease;

    [System.NonSerialized] private float from, to, shown, t0;
    [System.NonSerialized] private bool has;
    [System.NonSerialized] private System.Action<float> apply;

    static readonly List<Eased> live = new List<Eased>();

    public Eased() { }
    public Eased(float seconds, CssEase ease) { this.seconds = seconds; this.ease = ease; }

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
