using UnityEngine;

public class MapIconScaler : MonoBehaviour
{
    public Camera mapCamera; // Reference to the Map Camera
    public Transform[] iconTransform; // Reference to the icon (player or enemy)

    void Update()
    {
        for (int i = 0; i < iconTransform.Length; i++)
        {
            // Adjust icon scale based on the camera's orthographic size
            float scaleFactor = mapCamera.orthographicSize / 10f; // Adjust 10f to control scaling behavior
            iconTransform[i].localScale = new Vector3(1 / scaleFactor, 1 / scaleFactor, 1);
        }
        
    }
}
