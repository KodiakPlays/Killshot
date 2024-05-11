using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowMovement : MonoBehaviour
{
    [SerializeField] GameObject arrow;
    float targetRotAngle;
    bool isRotate;

    // Update is called once per frame
    void Update()
    {
        //BEARING MOVEMENT
        if (GameManager.Instance.bActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                //TURN LEFT
                targetRotAngle += -15f;
                arrow.transform.rotation = Quaternion.Euler(-90, targetRotAngle, 0);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                //TURN RIGHT
                targetRotAngle += 15f;
                arrow.transform.rotation = Quaternion.Euler(-90, targetRotAngle, 0);
            }
        }
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            arrow.transform.rotation = Quaternion.Euler(-90, 0, 0);
            //isRotate = true;
            //Quaternion targetRotation = Quaternion.Euler(-90, 0, 0);
            //arrow.transform.rotation = Quaternion.RotateTowards(arrow.transform.rotation, targetRotation, 5 * Time.deltaTime);
            //if (arrow.transform.rotation == targetRotation)
            //{
            //    isRotate = false;
            //    targetRotAngle = 0;
            //}
        }
    }
}
