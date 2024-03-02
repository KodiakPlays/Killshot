using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class SpaceshipMovement : MonoBehaviour
{
    Vector3 rotationAngle;
    Vector3 upDownAngle;

    [SerializeField] TextMeshProUGUI turnAngle;
    [SerializeField] TextMeshProUGUI elevationAngle;
    // Update is called once per frame
    void Update()
    {
        //CONTINUES FORWORD MOVEMENT 
        transform.Translate(Vector3.forward * Time.deltaTime * 0.5f);
        if (GameManager.Instance.eActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if(Input.GetKeyDown(KeyCode.W))
            {
                //GOES UPSIDE
                upDownAngle += new Vector3(-10, 0, 0);
                Debug.Log("rotationAmount " + upDownAngle);
                elevationAngle.text = upDownAngle.x.ToString();
            } 
            if(Input.GetKeyDown(KeyCode.S))
            {
                //GOES DOWNSIDE
                upDownAngle += new Vector3(10, 0, 0);
                Debug.Log("rotationAmount " + upDownAngle);
                elevationAngle.text = upDownAngle.x.ToString();
            } 
        }
        if (GameManager.Instance.bActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                //TURN LEFT
                rotationAngle += new Vector3(0, -10, 0);
                Debug.Log("rotationAmount " + rotationAngle);
                turnAngle.text = rotationAngle.y.ToString();
            } 
            if(Input.GetKeyDown(KeyCode.D))
            {
                //TURN RIGHT
                rotationAngle += new Vector3(0, 10, 0);
                Debug.Log("rotationAmount " + rotationAngle);
                turnAngle.text = rotationAngle.y.ToString();
            }
        }
        /*
        //if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        //{
        //    Debug.Log("Enter key");
        //    //------------------RIGHT LEFT TURN--------------------
        //    //transform.Rotate(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z) + rotationAngle);
        //    Quaternion targetRotationLR = Quaternion.LookRotation(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z) + rotationAngle);
        //    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotationLR, 10 * Time.deltaTime);
        //    rotationAngle = new Vector3(0, 0, 0);
        //    turnAngle.text = "0";

        //    //------------------UP DOWN TURN--------------------
        //    //transform.Rotate(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z) + upDownAngle);
        //    Quaternion targetRotationUD = Quaternion.LookRotation(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z) + upDownAngle);
        //    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotationUD, 10* Time.deltaTime);
        //    upDownAngle = new Vector3(0, 0, 0);
        //    elevationAngle.text = "0";

        //    // Invoke("MakeItCenter", 5f);

        //    GameManager.Instance.enterKeyActivate = false;
        //}*/
        Quaternion Currentpos;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Debug.Log("Enter key");
            //------------------RIGHT LEFT TURN--------------------
            transform.Rotate(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z) + rotationAngle);
           Currentpos = transform.rotation;
            Quaternion targetRotationLR = Quaternion.Euler(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z) + rotationAngle);
            /*float alpha = 0;
           while (alpha <= 1)
           {
               transform.rotation = Quaternion.Lerp(Currentpos, targetRotationLR, alpha);
               alpha += Time.deltaTime;
           }
               //transform.rotation = Quaternion.Lerp(Currentpos, targetRotationLR, 2 * Time.deltaTime);
           */
            rotationAngle = new Vector3(0, 0, 0);
            turnAngle.text = "0";

            //------------------UP DOWN TURN--------------------
            transform.Rotate(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z) + upDownAngle);
            //Quaternion targetRotationUD = Quaternion.LookRotation(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z) + upDownAngle);
            //transform.rotation = Quaternion.Lerp(Currentpos, targetRotationUD, 2 * Time.deltaTime);
            upDownAngle = new Vector3(0, 0, 0);
            elevationAngle.text = "0";
            //
        }

    }
    void turn()
    {
        if (GameManager.Instance.enterKeyActivate)
        {
            Debug.Log("Enter key");
            //------------------RIGHT LEFT TURN--------------------
            //transform.Rotate(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z) + rotationAngle);
            Quaternion targetRotationLR = Quaternion.LookRotation(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z) + rotationAngle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationLR, 10 * Time.deltaTime);
            rotationAngle = new Vector3(0, 0, 0);
            turnAngle.text = "0";

            //------------------UP DOWN TURN--------------------
            //transform.Rotate(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z) + upDownAngle);
            Quaternion targetRotationUD = Quaternion.LookRotation(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z) + upDownAngle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationUD, 10 * Time.deltaTime);
            upDownAngle = new Vector3(0, 0, 0);
            elevationAngle.text = "0";

            // Invoke("MakeItCenter", 5f);

            GameManager.Instance.enterKeyActivate = false;
        }
    }
    void MakeItCenter()
    {
        Debug.Log("MakeItCenter");
        transform.rotation = Quaternion.Euler(0, transform.rotation.y, transform.rotation.z);
    }
}
