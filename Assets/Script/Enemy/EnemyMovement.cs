using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{

    public Vector3 movementAreaCenter; // Center of the area within which the enemy can move
    public Vector3 movementAreaSize; // Size of the area (width, height, depth)
    public float movementSpeed = 5f; // Speed of the enemy's movement
    public float pauseTime = 2f; // Time to pause before moving to the next position

    private Vector3 targetPosition; // Current target position
    private bool isMoving = true; // Whether the enemy is currently moving
    private float pauseTimer = 0f; // Timer for pausing between movements
    //[SerializeField] SpaceshipMovement spaceshipMove;

    void Start()
    {
        SetRandomTargetPosition();
    }

    void Update()
    {
        if (isMoving && !GameManager.Instance.isEnemyDetect)
        {
            MoveToTarget();
        }
        else
        {
            PauseBeforeNextMove();
        }
    }

    void SetRandomTargetPosition()
    {
        // Choose a random position within the defined area
        float x = Random.Range(movementAreaCenter.x - movementAreaSize.x / 2, movementAreaCenter.x + movementAreaSize.x / 2);
        float y = Random.Range(movementAreaCenter.y - movementAreaSize.y / 2, movementAreaCenter.y + movementAreaSize.y / 2);
        float z = Random.Range(movementAreaCenter.z - movementAreaSize.z / 2, movementAreaCenter.z + movementAreaSize.z / 2);

        targetPosition = new Vector3(x, y, z);
        isMoving = true; // Enable movement
    }

    void MoveToTarget()
    {
        // Move the enemy toward the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

        // Check if the enemy has reached the target position
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false; // Stop moving
            pauseTimer = pauseTime; // Start the pause timer
        }
    }

    void PauseBeforeNextMove()
    {
        // Count down the pause timer
        pauseTimer -= Time.deltaTime;
        if (pauseTimer <= 0f)
        {
            SetRandomTargetPosition(); // Choose a new target position
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw the movement area in the Scene view for visualization
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(movementAreaCenter, movementAreaSize);
    }
}
