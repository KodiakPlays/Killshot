using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WholeMapCameraZoom : MonoBehaviour
{
   // public GameObject radarPanel; 
    public GameObject playerIcon;
    //public GameObject enemyIcon;
   // public GameObject Player;
   // public GameObject Enemy;
    Camera camera;
    public float zoomInValue;
    [SerializeField] Transform spaceship;
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
        camera.orthographicSize = 2500;
    }

    // Update is called once per frame
    void Update()
    {
        camera.orthographicSize =2500;
        playerIcon.transform.localScale = new Vector3(100,100,100);
        //Vector3 newpos = spaceship.position;
        //newpos.y = transform.position.y;

        //transform.position = newpos;
        //transform.rotation = Quaternion.Euler(90, spaceship.rotation.eulerAngles.y, 0);
    }
}
