using UnityEngine;
using System;

/// <summary>
/// World boundary system per GDD spec.
/// 
/// If the player leaves the playable area, a warning and timer appear.
/// If the player does not turn back in time, they are charged with cowardice
/// and the ship self-destructs. The game ends and the player must restart the mission.
/// </summary>
public class WorldBoundary : MonoBehaviour
{
    [Header("Boundary Settings")]
    [SerializeField] private float playableAreaRadius = 50000f; // Size of the playable area
    [SerializeField] private float warningDistance = 5000f; // Distance before boundary to start warning
    [SerializeField] private float desertionTimerDuration = 30f; // Seconds before self-destruct

    [Header("References")]
    [SerializeField] private Transform playerTransform;

    // State
    private bool isOutOfBounds = false;
    private bool isWarning = false;
    private float desertionTimer;
    private bool hasDeserted = false;

    // Events
    public event Action OnBoundaryWarning;        // Player approaching boundary
    public event Action OnOutOfBounds;             // Player crossed boundary, timer started
    public event Action<float> OnDesertionTimerTick; // Timer counting down (remaining seconds)
    public event Action OnDesertion;               // Player charged with cowardice, ship self-destructs
    public event Action OnReturnedToBounds;        // Player returned in time

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        desertionTimer = desertionTimerDuration;
    }

    private void Update()
    {
        if (playerTransform == null || hasDeserted) return;

        float distFromCenter = Vector3.Distance(playerTransform.position, Vector3.zero);
        float distFromBoundary = playableAreaRadius - distFromCenter;

        // Check warning zone
        if (distFromBoundary <= warningDistance && distFromBoundary > 0f)
        {
            if (!isWarning)
            {
                isWarning = true;
                OnBoundaryWarning?.Invoke();
                Debug.Log("[WorldBoundary] WARNING: Approaching boundary!");
            }
        }
        else if (distFromBoundary > warningDistance)
        {
            if (isWarning || isOutOfBounds)
            {
                ReturnToBounds();
            }
        }

        // Check out of bounds
        if (distFromBoundary <= 0f)
        {
            if (!isOutOfBounds)
            {
                isOutOfBounds = true;
                desertionTimer = desertionTimerDuration;
                OnOutOfBounds?.Invoke();
                Debug.Log("[WorldBoundary] OUT OF BOUNDS! Return immediately or be charged with cowardice!");
            }

            // Count down desertion timer
            desertionTimer -= Time.deltaTime;
            OnDesertionTimerTick?.Invoke(desertionTimer);

            if (desertionTimer <= 0f)
            {
                TriggerDesertion();
            }
        }
        else if (isOutOfBounds && distFromBoundary > 0f)
        {
            ReturnToBounds();
        }
    }

    private void ReturnToBounds()
    {
        isOutOfBounds = false;
        isWarning = false;
        desertionTimer = desertionTimerDuration;
        OnReturnedToBounds?.Invoke();
        Debug.Log("[WorldBoundary] Returned to playable area.");
    }

    private void TriggerDesertion()
    {
        hasDeserted = true;
        OnDesertion?.Invoke();
        Debug.Log("[WorldBoundary] COWARDICE! Ship self-destructing. Mission failed.");

        // Self-destruct the player ship
        if (playerTransform != null)
        {
            Destroy(playerTransform.gameObject);
        }
    }

    // Public getters
    public bool IsOutOfBounds() => isOutOfBounds;
    public bool IsWarning() => isWarning;
    public float GetDesertionTimeRemaining() => desertionTimer;
    public float GetPlayableAreaRadius() => playableAreaRadius;
    public bool HasDeserted() => hasDeserted;
}
