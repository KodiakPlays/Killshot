using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowMovement : MonoBehaviour
{
    [SerializeField] GameObject target;

    // Update is called once per frame
    void Update()
    {
        Vector3 relativePos = transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(relativePos);
    }
}
