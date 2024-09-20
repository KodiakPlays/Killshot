using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearingScript : MonoBehaviour
{
    [SerializeField] Transform spaceship;


    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = spaceship.position;
        newPos.y = transform.position.y;
        transform.position = newPos;
    }
}
