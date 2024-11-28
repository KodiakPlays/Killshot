using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    Camera camera;
    public Transform playerIcon;
    public Transform laserIcon;
   // public float playerIconScale = 1f;
    public float zoomInValue;
    [SerializeField] Transform spaceship;
    //private Vector3 originalPlayerIconScale;
    //private Vector3 originalLaserIconScale;
    //private Vector3 originalLaserIconPos;

    private void Start()
    {
        camera = GetComponent<Camera>();

        //originalPlayerIconScale = playerIcon.localScale;
        //originalLaserIconScale = laserIcon.localScale;
        //originalLaserIconPos = laserIcon.localPosition ;
    }
    void Update()
    {
        camera.orthographicSize = zoomInValue ;
        Vector3 newpos = spaceship.position;
        newpos.y = transform.position.y;

        transform.position = newpos;
        transform.rotation = Quaternion.Euler(90, spaceship.rotation.eulerAngles.y, 0);
        /* OldCode
        camera.orthographicSize = zoomInValue - 20;
        Vector3 newpos = spaceship.position;
        newpos.y = transform.position.y;
        
        transform.position = newpos;
        transform.rotation = Quaternion.Euler(90, spaceship.rotation.eulerAngles.y,0);

        // Adjust the scale of the player icon to keep its size constant on the minimap
        float playerScaleFactor = camera.orthographicSize /20f;
        playerIcon.localScale = new Vector3(
            originalPlayerIconScale.x * playerScaleFactor,
            originalPlayerIconScale.y * playerScaleFactor,
            originalPlayerIconScale.z * playerScaleFactor
        );

        // Adjust the scale of the laser icon to keep its size constant on the minimap
        float laserScaleFactor = camera.orthographicSize / 20f;
        laserIcon.localScale = new Vector3(
            originalLaserIconScale.x * laserScaleFactor,
            originalLaserIconScale.y * laserScaleFactor,
            originalLaserIconScale.z * laserScaleFactor
        );
        laserIcon.localPosition = originalLaserIconPos;

        */


    }
    public void ZoomInOut(float zoom)
    {
        zoomInValue = zoom;
    }
}
