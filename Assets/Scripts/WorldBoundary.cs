// =================================================================================================
// WorldBoundary.cs
//
// What it does:
// The edge of the play area for the older physics player ship. A ship that stays outside too
// long is charged with cowardice and destroyed. It is on an object in the Prototype Dev scene.
// The battle board does not use this script. The board's edge rule is in ShipState.
//
// How it works:
// - At the start it finds the object tagged "Player", if no ship is set in the Inspector.
// - The play area is a circle around the centre of the world. By default the edge is 50,000 units
//   from the centre.
// - Every frame it checks the ship's distance from the centre.
// - Within 5,000 units of the edge it gives a warning once and writes it to the Console.
// - Past the edge a 30-second countdown starts. It tells anyone listening the time left each
//   frame.
// - Coming back inside the edge stops and resets the countdown. It tells listeners the ship is
//   back.
// - When the countdown ends, it removes the player's ship and writes COWARDICE to the Console.
//   After that it stops checking.
// - No script listens to its messages.
//
// Technical:
// - Data: playableAreaRadius and warningDistance in units, desertionTimer in seconds.
// - The centre is the world origin (Vector3.zero). The distance is a straight 3D distance.
// - Events: OnBoundaryWarning, OnOutOfBounds, OnDesertionTimerTick(seconds left), OnDesertion and
//   OnReturnedToBounds.
// - Getters: IsOutOfBounds, IsWarning, GetDesertionTimeRemaining, GetPlayableAreaRadius and
//   HasDeserted. No script calls them.
// - Numbers: radius (50,000), warning distance (5,000) and countdown (30 s) are in the Inspector.
// - Trap: it removes the ship object outright. HullSystem and GameManager never hear of it, and
//   no defeat is written.
// - The countdown uses scaled Time.deltaTime.
// =================================================================================================

using UnityEngine;
using System;

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
