using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaMap : MonoBehaviour
{
    [SerializeField] Transform spaceship;
    private Camera cam;
    bool isZoom;
    private void Start()
    {
        isZoom = true;
        cam = GetComponent<Camera>();
    }
    private void Update()
    {
        /* OLD CODE FOR ZOOM CAMERA AFTER PRESS F KEY
        if (Input.GetKeyDown(KeyCode.F) && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (cam.orthographicSize <= 500f)
            {
                cam.orthographicSize = 500f;
                isZoom = false;
            }
            else if (cam.orthographicSize == 500f)
            {
                isZoom = true;
                cam.orthographicSize = 100f;
            }
        }*/
    }
   
    public void ZoomOut()
    {
        if (isZoom)
        {
            Vector3 newpos = spaceship.position;
            newpos.y = transform.position.y;
            transform.position = newpos;
        }
        else
        {
            transform.position = new Vector3(0, 100, 0);
        }
    }
    private void LateUpdate()
    {
        ZoomOut();
    }
}
