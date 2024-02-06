using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevationMap : MonoBehaviour
{
    [SerializeField] Transform spaceship;
    // Update is called once per frame
    void Update()
    {
        Vector3 newPose = spaceship.position;
        newPose.z = transform.rotation.z;
        transform.position = newPose;
    }
}

