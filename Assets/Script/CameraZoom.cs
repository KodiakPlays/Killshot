using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    Camera camera;
    public float zoomValue;
    [SerializeField] Transform spaceship;

    private void Start()
    {
        //camera.orthographicSize = zoomValue;
        camera = GetComponent<Camera>();
    }
    void Update()
    {
        camera.orthographicSize = zoomValue;
        Vector3 newpos = spaceship.position;
        newpos.y = transform.position.y;
        transform.position = newpos;
    }
    public void ZoomInOut(float zoom)
    {
        zoomValue = zoom;
    }
}
