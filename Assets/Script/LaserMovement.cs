using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaserMovement : MonoBehaviour
{
    [SerializeField] float rotateSpeed;
    float tolerance = 1f;

    void Update()
    {
        if(GameManager.Instance.lActive && !GameManager.Instance.bActive && !GameManager.Instance.eActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            float currentYAngle = transform.rotation.eulerAngles.y;
            Debug.Log(currentYAngle);
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
            if (Mathf.Abs(currentYAngle - 315f) <= tolerance)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 316f, transform.rotation.eulerAngles.z);
            }
        }
        float currentXAngle = transform.rotation.eulerAngles.x;
        if (currentXAngle <= 105 || currentXAngle >= 75)
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.Rotate(-rotateSpeed * Time.deltaTime, 0, 0);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.Rotate(rotateSpeed * Time.deltaTime, 0, 0);
            }
            //Debug.Log(transform.rotation.eulerAngles);
        }

    }
    
}
