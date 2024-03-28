using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothRotation : MonoBehaviour
{
    public Quaternion targetRotate;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation =  Quaternion.RotateTowards(transform.rotation, targetRotate, 0.1f);
    }
}
