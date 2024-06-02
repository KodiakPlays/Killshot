using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevationMap : MonoBehaviour
{
    [SerializeField] Transform spaceship;
    // Update is called once per frame
    void Update()
    {
        if (spaceship != null)
        {
            Vector3 newPos = transform.position;
            newPos.z = spaceship.position.z + 3;
            transform.position = newPos;

            transform.rotation = spaceship.rotation;

        }
    }
}

