using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class SpaceshipMovement : MonoBehaviour
{
    Vector3 rotationAngle;
    Vector3 upDownAngle;

    [SerializeField] float SpaceshipMoveSpeed = 0.5f;

    [SerializeField] TextMeshProUGUI turnAngle;
    [SerializeField] TextMeshProUGUI elevationAngle;

    [SerializeField] float detectionRadius;
    [SerializeField] LayerMask enemyLayer;
    // Update is called once per frame
    void Update()
    {
        SpaceshipMove();
        EnemySpaseshipDetection();
    }
    void SpaceshipMove()
    {
        //CONTINUES FORWORD MOVEMENT 
        transform.Translate(Vector3.forward * Time.deltaTime * SpaceshipMoveSpeed);

        //ELEVATION MOVEMENT
        if (GameManager.Instance.eActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                //GOES UPSIDE
                upDownAngle += new Vector3(-10, 0, 0);
                Debug.Log("rotationAmount " + upDownAngle);
                elevationAngle.text = upDownAngle.x.ToString();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                //GOES DOWNSIDE
                upDownAngle += new Vector3(10, 0, 0);
                Debug.Log("rotationAmount " + upDownAngle);
                elevationAngle.text = upDownAngle.x.ToString();
            }
        }

        //BEARING MOVEMENT
        if (GameManager.Instance.bActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                //TURN LEFT
                rotationAngle += new Vector3(0, -10, 0);
                Debug.Log("rotationAmount " + rotationAngle);
                turnAngle.text = rotationAngle.y.ToString();
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                //TURN RIGHT
                rotationAngle += new Vector3(0, 10, 0);
                Debug.Log("rotationAmount " + rotationAngle);
                turnAngle.text = rotationAngle.y.ToString();
            }
        }

        //SMMOTH ROTATION AFTER ENTER
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
    void EnemySpaseshipDetection()
    {
        Collider[] hitCollider = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        foreach (Collider collider in hitCollider)
        {
            Debug.Log("Enemy Detected: " + collider.gameObject.name);
        }
    }

    public void PowerInc()
    {
        if (GameManager.Instance.pActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if(SpaceshipMoveSpeed <= 5)
            {
                SpaceshipMoveSpeed += 0.2f;
            }
            else
            {
                SpaceshipMoveSpeed = 5f;
            }
        }
        
    }
    public void PowerDec()
    {
        if (GameManager.Instance.pActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if(SpaceshipMoveSpeed >= 0.5f)
            {
                SpaceshipMoveSpeed -= 0.2f;
            }
            else
            {
                SpaceshipMoveSpeed = 0.5f;
            }
            
        }
            
    }
}
