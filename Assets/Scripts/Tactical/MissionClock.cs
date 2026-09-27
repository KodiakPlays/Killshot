using UnityEngine;

/// <summary>
/// The mission clock. One place that answers "what time is it".
///
/// Right now it is just time since the scene loaded. It exists as its own thing
/// so that when there is a real clock — one that can be paused, which the design
/// calls for — only this changes. Timestamps taken through here will not drift
/// against the clock the player is looking at.
/// </summary>
public static class MissionClock
{
    /// <summary>Seconds since the mission began.</summary>
    public static float Now { get { return Time.timeSinceLevelLoad; } }

    /// <summary>Format as T+M:SS, which is how the plot stamps a mark.</summary>
    public static string Stamp(float seconds)
    {
        if (seconds < 0f) seconds = 0f;
        int total = Mathf.FloorToInt(seconds);
        return "T+" + (total / 60) + ":" + (total % 60).ToString("00");
    }
}
