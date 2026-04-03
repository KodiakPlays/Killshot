using UnityEngine;
using System;

/// <summary>
/// Quest system per GDD spec.
/// 
/// Quests are structured as simple objective chains:
///   1. Locate Target - Triggered when player enters correct region or scans a key signal
///   2. Eliminate Target - Activated once flagship/enemy of interest is confirmed on radar. Completed when destroyed.
///   3. Exit Region - Requires player to reach hyperspace beacon and jump out.
/// 
/// No UI popups or quest journals. Purely a logic layer for mission flow.
/// After killing the enemy ship, escape marker appears on world map edge for jump point.
/// 
/// Losing conditions:
///   - Die in battle
///   - Desert the battlespace (WorldBoundary handles this)
///   - Run out of time (GameClock handles this)
/// </summary>
public enum QuestStage
{
    NotStarted,
    LocateTarget,
    EliminateTarget,
    ExitRegion,
    MissionComplete,
    MissionFailed
}

public enum MissionFailReason
{
    None,
    Destroyed,
    Desertion,
    TimeExpired,
    LifeSupportFailed
}

public class QuestSystem : MonoBehaviour
{
    public static QuestSystem Instance { get; private set; }

    [Header("Quest Settings")]
    [SerializeField] private float locateRegionRadius = 5000f; // Distance to target to trigger "locate"
    [SerializeField] private Transform jumpPointMarker; // The escape jump point (spawned when mission stage changes)

    // State
    private QuestStage currentStage = QuestStage.NotStarted;
    private Transform targetShip; // The ship to destroy
    private Vector3 jumpPointPosition;
    private bool targetLocated = false;
    private bool targetDestroyed = false;

    // Events
    public event Action<QuestStage> OnStageChanged;
    public event Action<MissionFailReason> OnMissionFailed;
    public event Action OnMissionComplete;
    public event Action<Vector3> OnJumpPointRevealed;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Subscribe to loss conditions
        var boundary = FindFirstObjectByType<WorldBoundary>();
        if (boundary != null)
        {
            boundary.OnDesertion += () => FailMission(MissionFailReason.Desertion);
        }

        var clock = GameClock.Instance;
        if (clock != null)
        {
            clock.OnMissionTimerExpired += () => FailMission(MissionFailReason.TimeExpired);
        }

        var subsystems = FindFirstObjectByType<InternalSubsystems>();
        if (subsystems != null)
        {
            subsystems.OnLifeSupportFailed += () => FailMission(MissionFailReason.LifeSupportFailed);
        }
    }

    /// <summary>
    /// Initialize the quest with a target ship to destroy.
    /// </summary>
    public void StartMission(Transform target)
    {
        targetShip = target;
        targetLocated = false;
        targetDestroyed = false;
        SetStage(QuestStage.LocateTarget);
        Debug.Log("[QuestSystem] Mission started: Locate the target.");
    }

    private void Update()
    {
        if (currentStage == QuestStage.MissionComplete || currentStage == QuestStage.MissionFailed)
            return;

        switch (currentStage)
        {
            case QuestStage.LocateTarget:
                UpdateLocateStage();
                break;
            case QuestStage.EliminateTarget:
                UpdateEliminateStage();
                break;
            case QuestStage.ExitRegion:
                UpdateExitStage();
                break;
        }
    }

    private void UpdateLocateStage()
    {
        if (targetShip == null) return;

        // Check if player is close enough to the target or has scanned it
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float dist = Vector3.Distance(player.transform.position, targetShip.position);
        if (dist <= locateRegionRadius)
        {
            targetLocated = true;
            SetStage(QuestStage.EliminateTarget);
            Debug.Log("[QuestSystem] Target located! Eliminate the target.");
        }
    }

    /// <summary>
    /// Call this when a scanner confirms the target (alternative to proximity detection).
    /// </summary>
    public void ConfirmTargetOnRadar()
    {
        if (currentStage == QuestStage.LocateTarget)
        {
            targetLocated = true;
            SetStage(QuestStage.EliminateTarget);
            Debug.Log("[QuestSystem] Target confirmed on radar! Eliminate the target.");
        }
    }

    private void UpdateEliminateStage()
    {
        // Check if target ship has been destroyed
        if (targetShip == null)
        {
            targetDestroyed = true;
            RevealJumpPoint();
            SetStage(QuestStage.ExitRegion);
            Debug.Log("[QuestSystem] Target eliminated! Escape to the jump point.");
        }
    }

    private void UpdateExitStage()
    {
        // Check if player has reached the jump point
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            FailMission(MissionFailReason.Destroyed);
            return;
        }

        float distToJump = Vector3.Distance(player.transform.position, jumpPointPosition);
        if (distToJump <= 500f) // Close enough to jump
        {
            CompleteMission();
        }
    }

    private void RevealJumpPoint()
    {
        // Per GDD: "A marker will appear on some edge of the world map"
        // Place jump point at a random position on the boundary edge
        float boundaryRadius = 45000f; // Slightly inside the boundary
        float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        jumpPointPosition = new Vector3(
            Mathf.Cos(angle) * boundaryRadius,
            Mathf.Sin(angle) * boundaryRadius,
            0f
        );

        // Instantiate or move the jump point marker
        if (jumpPointMarker != null)
        {
            jumpPointMarker.position = jumpPointPosition;
            jumpPointMarker.gameObject.SetActive(true);
        }

        OnJumpPointRevealed?.Invoke(jumpPointPosition);
        Debug.Log($"[QuestSystem] Jump point revealed at {jumpPointPosition}");
    }

    private void CompleteMission()
    {
        SetStage(QuestStage.MissionComplete);
        OnMissionComplete?.Invoke();
        Debug.Log("[QuestSystem] MISSION COMPLETE! Successfully escaped.");
    }

    private void FailMission(MissionFailReason reason)
    {
        if (currentStage == QuestStage.MissionFailed) return;
        
        SetStage(QuestStage.MissionFailed);
        OnMissionFailed?.Invoke(reason);

        string reasonText = reason switch
        {
            MissionFailReason.Destroyed => "Ship destroyed",
            MissionFailReason.Desertion => "Charged with cowardice - deserted the battlespace",
            MissionFailReason.TimeExpired => "Ran out of time",
            MissionFailReason.LifeSupportFailed => "Life support failed",
            _ => "Unknown"
        };
        Debug.Log($"[QuestSystem] MISSION FAILED: {reasonText}");
    }

    private void SetStage(QuestStage stage)
    {
        currentStage = stage;
        OnStageChanged?.Invoke(stage);
    }

    // Public getters
    public QuestStage GetCurrentStage() => currentStage;
    public bool IsTargetLocated() => targetLocated;
    public bool IsTargetDestroyed() => targetDestroyed;
    public Vector3 GetJumpPointPosition() => jumpPointPosition;
}
