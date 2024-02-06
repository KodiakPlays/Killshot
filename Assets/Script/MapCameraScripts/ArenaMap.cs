using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaMap : MonoBehaviour
{
    [SerializeField] Transform spaceship;
    private Camera cam;
    private void Start()
    {
        cam = GetComponent<Camera>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (cam.orthographicSize == 15f)
            {
                cam.orthographicSize = 30f;
            }
            else if (cam.orthographicSize == 30f)
            {
                cam.orthographicSize = 15f;
            }
        }
    }
   
    public void Zoom()
    {
        if (cam.orthographicSize == 15f)
        {
            cam.orthographicSize = 30f;
        }
        else if (cam.orthographicSize == 30f)
        {
            cam.orthographicSize = 15f;
        }
    }
    private void LateUpdate()
    {
        Vector3 newpos = spaceship.position;
        newpos.y = transform.position.y;
        transform.position = newpos;
    }
}
