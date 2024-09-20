using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevationMap : MonoBehaviour
{
    Camera camera;
    public float zoomValue;
    [SerializeField] Transform spaceship;
    private void Start()
    {
        camera = GetComponent<Camera>();
    }
    // Update is called once per frame
    void Update()
    {
        if (spaceship != null)
        {
            camera.orthographicSize = zoomValue -20;

            Vector3 newPos = transform.position;
            newPos.z = spaceship.position.z + 3;
            transform.position = newPos;

            transform.rotation = spaceship.rotation;

        }
    }
    public void ZoomInOut(float zoom)
    {
        zoomValue = zoom;
    }
}

