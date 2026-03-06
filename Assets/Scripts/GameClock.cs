using UnityEngine;
using System;

/// <summary>
/// Universal game clock per GDD spec.
/// 
/// Governs enemy ship movement, ETA calculations, scanner sweeps, and mission timing.
/// All intercepted messages reference the game clock to report arrival times, movement speeds, and rendezvous points.
/// The clock does NOT pause when systems are offline or in menus.
/// The player may pause the game at any time for strategic decisions - the clock STOPS during this pause.
/// </summary>
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
