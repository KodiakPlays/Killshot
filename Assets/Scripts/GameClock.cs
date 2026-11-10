// =================================================================================================
// GameClock.cs
//
// What it does:
// An older mission clock. It counts game time from a mission's start and stops when a time limit
// runs out. It is in the Prototype Dev scene, and no other script reads it. The battle board does
// not use this script.
//
// How it works:
// - Only one clock is kept. A second copy removes itself.
// - Every frame, unless paused, it adds to the time so far. It adds the frame's time, times a
//   speed setting. Then it tells anyone listening.
// - The time limit is 3,600 seconds (one hour) by default. When the time so far reaches it, the
//   clock stops and tells anyone listening. It writes "Mission time expired" to the Console.
//   A limit of 0 means no limit.
// - Pause stops the clock and freezes the whole game. Resume starts both again.
// - Other code can ask for the time so far and the time left. It can ask for the travel time
//   between two points at a given speed.
// - It can write a number of seconds as hours, minutes and seconds (00:00:00).
//
// Technical:
// - Data: elapsedGameTime in seconds. It grows by Time.deltaTime times the clock's timeScale field.
// - GameClock.Instance is set in Awake. No script reads it.
// - Events: OnTimeUpdated(seconds) every frame, OnMissionTimerExpired, OnPaused and OnResumed.
//   Nothing listens to them.
// - StrategicPause sets Unity's Time.timeScale to 0. Resume sets it back to 1. TogglePause
//   switches between them.
// - GetRemainingTime returns -1 with no limit. CalculateETA returns -1 for a speed of 0 or less.
// - Numbers: missionTimeLimitSeconds (3,600) and timeScale (1) are in the Inspector.
// - Trap: the clock's timeScale field and Unity's Time.timeScale are two different numbers.
// - Trap: Resume forces Time.timeScale to 1. Any other game speed is lost.
// - Trap: once the limit runs out, nothing starts the clock again.
// =================================================================================================

using UnityEngine;
using System;

public class GameClock : MonoBehaviour
{
    public static GameClock Instance { get; private set; }

    [Header("Clock Settings")]
    [SerializeField] private float missionTimeLimitSeconds = 3600f; // Total mission time (1 hour default)
    [SerializeField] private float timeScale = 1f; // Game time multiplier

    // Clock state
    private float elapsedGameTime = 0f;
    private bool isPaused = false;
    private bool isMissionActive = true;

    // Events
    public event Action<float> OnTimeUpdated; // Fires every frame with current elapsed time
    public event Action OnMissionTimerExpired; // Mission time ran out = lose condition
    public event Action OnPaused;
    public event Action OnResumed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!isMissionActive) return;

        // Clock does NOT advance when strategically paused per GDD
        if (!isPaused)
        {
            elapsedGameTime += Time.deltaTime * timeScale;
            OnTimeUpdated?.Invoke(elapsedGameTime);

            // Check mission timer
            if (missionTimeLimitSeconds > 0f && elapsedGameTime >= missionTimeLimitSeconds)
            {
                isMissionActive = false;
                OnMissionTimerExpired?.Invoke();
                Debug.Log("[GameClock] Mission time expired! Player loses.");
            }
        }
    }

    /// <summary>
    /// Strategic pause - clock stops. Per GDD: "The player may pause the game at anytime to make strategic decisions."
    /// </summary>
    public void StrategicPause()
    {
        if (!isPaused)
        {
            isPaused = true;
            Time.timeScale = 0f;
            OnPaused?.Invoke();
            Debug.Log("[GameClock] Strategic pause - clock stopped.");
        }
    }

    /// <summary>
    /// Resume from strategic pause.
    /// </summary>
    public void Resume()
    {
        if (isPaused)
        {
            isPaused = false;
            Time.timeScale = 1f;
            OnResumed?.Invoke();
            Debug.Log("[GameClock] Resumed - clock running.");
        }
    }

    /// <summary>
    /// Toggle pause state.
    /// </summary>
    public void TogglePause()
    {
        if (isPaused) Resume();
        else StrategicPause();
    }

    /// <summary>
    /// Get the current elapsed game time in seconds.
    /// </summary>
    public float GetElapsedTime() => elapsedGameTime;

    /// <summary>
    /// Get the remaining mission time in seconds. Returns -1 if no time limit.
    /// </summary>
    public float GetRemainingTime()
    {
        if (missionTimeLimitSeconds <= 0f) return -1f;
        return Mathf.Max(0f, missionTimeLimitSeconds - elapsedGameTime);
    }

    /// <summary>
    /// Format time as HH:MM:SS for display.
    /// </summary>
    public static string FormatTime(float seconds)
    {
        int hours = (int)(seconds / 3600f);
        int minutes = (int)((seconds % 3600f) / 60f);
        int secs = (int)(seconds % 60f);
        return $"{hours:D2}:{minutes:D2}:{secs:D2}";
    }

    /// <summary>
    /// Calculate ETA to a position given current speed.
    /// Used by comms/scanner systems per GDD.
    /// </summary>
    public float CalculateETA(Vector3 fromPos, Vector3 toPos, float speed)
    {
        if (speed <= 0f) return -1f;
        float distance = Vector3.Distance(fromPos, toPos);
        return distance / speed;
    }

    public bool IsPaused() => isPaused;
    public bool IsMissionActive() => isMissionActive;
    public float GetTimeScale() => timeScale;

    public void SetTimeScale(float scale)
    {
        timeScale = Mathf.Max(0f, scale);
    }
}
