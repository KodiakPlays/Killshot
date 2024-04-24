using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class LaserMove : MonoBehaviour
{
    [SerializeField] float rotateSpeed;
    float tolerance = 1f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {/*
        Quaternion rotation;
        
        if (Input.GetKey(KeyCode.A) && transform.rotation.eulerAngles.y >= 314f )
        {
            rotation = Quaternion.Euler(0, 0, rotateSpeed * Time.deltaTime);
            transform.rotation *= rotation;
            //transform.Rotate(0,0, rotateSpeed);
        }
        if(Input.GetKey(KeyCode.D))// && currentRotationY >= 45f)// && transform.rotation.z >= -45)
        {
            rotation = Quaternion.Euler(0, 0, -rotateSpeed * Time.deltaTime);
            transform.rotation *= rotation;
            //transform.Rotate(0,0,-rotateSpeed);
        }
        if(Input.GetKey(KeyCode.W))
        {
            transform.Rotate(-rotateSpeed * Time.deltaTime, 0, 0);
        }
        if(Input.GetKey(KeyCode.S))
        {
            transform.Rotate(rotateSpeed * Time.deltaTime, 0,0);
        }

        Debug.Log(transform.rotation.eulerAngles.x + " x");
        Debug.Log(transform.rotation.eulerAngles.y + " y");
        Debug.Log(transform.rotation.eulerAngles.z + " z");

        Quaternion rotation1 = transform.rotation;
        Vector3 eulerRotation = rotation1.eulerAngles;

        Debug.Log("Euler Angles: " + eulerRotation);
        */
        float currentYAngle = transform.rotation.eulerAngles.y;
        if (currentYAngle >= 314 || currentYAngle <= 45)
        {
            if (Input.GetKey(KeyCode.A))
            {
                transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.Rotate(0, 0, -rotateSpeed * Time.deltaTime);
            }
            
        }
        if (Mathf.Abs(currentYAngle - 45f) <= tolerance)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 44f, transform.rotation.eulerAngles.z);
        }

        // Check if angle is close to 314
        if (Mathf.Abs(currentYAngle - 314f) <= tolerance || Mathf.Abs(currentYAngle - 314f + 360f) <= tolerance)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 316f, transform.rotation.eulerAngles.z);
        }

        float currentXAngle = transform.rotation.eulerAngles.x;
        if(currentXAngle <= 105 || currentXAngle >= 75)
        {
            if(Input.GetKey(KeyCode.W))
            {
                transform.Rotate(-rotateSpeed * Time.deltaTime, 0, 0);
            }
            if(Input.GetKey(KeyCode.S))
            {
                transform.Rotate(rotateSpeed * Time.deltaTime, 0, 0);
            }
            //Debug.Log(transform.rotation.eulerAngles);
        }

       
        //if (Mathf.Abs(currentXAngle - 75) <= tolerance)
        //{
        //    transform.rotation = Quaternion.Euler(74, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        //}
        //if (Mathf.Abs(currentXAngle - 105) <= tolerance || Mathf.Abs(currentYAngle - 105f ) <= tolerance)
        //{
        //    transform.rotation = Quaternion.Euler(104, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        //}

    }

}
