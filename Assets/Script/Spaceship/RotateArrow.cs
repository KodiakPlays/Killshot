using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateArrow : MonoBehaviour
{
    public RectTransform uiImage; // The UI Image to rotate
    public RectTransform arenaCenter; // The center of the circle (like the arena or screen center)
    public float radius = 100f; // Radius of the circle
    public SpaceshipMovement spaceshipMove;
    private float angle = 0f; // Initial angle in degrees

    void Update()
    {
        // Set the angle directly from spaceshipMove.RotAngle (no accumulation)
        float angle = spaceshipMove.RotAngle;

        // Convert the angle to radians for trigonometry
        float angleInRadians = angle * Mathf.Deg2Rad;

        // Calculate the new position based on the angle
        float x = Mathf.Sin(angleInRadians) * radius;
        float y = Mathf.Cos(angleInRadians) * radius;

        // Update the UI Image position relative to the arena center
        uiImage.anchoredPosition = new Vector2(x, y) + arenaCenter.anchoredPosition;

        // Now, rotate the UI image to face the direction it is moving
        float uiRotationAngle = angle; // Since angle is already in degrees, we can use it directly.

        // Apply the rotation to the UI image
        uiImage.rotation = Quaternion.Euler(0, 0, -uiRotationAngle); // Rotate to face the direction of movement


        /*  // Set the angle directly from spaceshipMove.targetRotAngle (no accumulation)
          float angle = spaceshipMove.RotAngle;

          // Convert the angle to radians for trigonometry
          float angleInRadians = angle * Mathf.Deg2Rad;

          // Calculate the new position based on the angle
          float x = Mathf.Sin(angleInRadians) * radius;
          float y = Mathf.Cos(angleInRadians) * radius;

          // Update the UI Image position relative to the arena center
          uiImage.anchoredPosition = new Vector2(x, y) + arenaCenter.anchoredPosition;*/
    }


}
