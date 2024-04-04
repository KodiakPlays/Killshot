using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;
//UnityEngine.Gizmos.DrawWireSphere;
using UnityEngine.UIElements;

public class SpaceshipMovement : MonoBehaviour
{
    //Quaternion rotationAngle;
    //Vector3 upDownAngle;

    [SerializeField] float SpaceshipMoveSpeed = 0f;

    [SerializeField] TextMeshProUGUI turnAngle;
    [SerializeField] TextMeshProUGUI elevationAngle;

    [SerializeField] float detectionRadius;
    [SerializeField] LayerMask enemyLayer;

    [SerializeField] float speed;
    [SerializeField] TextMeshProUGUI speedText;

    [SerializeField] Power power;

    [SerializeField] float moveSpeed;
    [SerializeField] float targetMovement;
    [SerializeField] bool isMove;

    [SerializeField] float rotSpeed;
    [SerializeField] float targetRotAngle;
    [SerializeField] bool isRotate;

    Quaternion currentRotation;
    Vector3 currentPosition;

    // Update is called once per frame
    private void Start()
    {
        speed = SpaceshipMoveSpeed;
        speedText.text = speed.ToString() + " km/h";

    }
    void Update()
    {
        SpaceshipMove();
        EnemySpaseshipDetection();
    }
    private void FixedUpdate()
    {
        if (isRotate)
        {
            RotateSpaceship();
        }
        if (isMove)
        {
            RaiseSpaceship();
        }

    }
   
    void RotateSpaceship()
    {
        Quaternion targetRotation = Quaternion.Euler(0, targetRotAngle, 0);
        Quaternion newTargetRotation =  targetRotation * currentRotation;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, newTargetRotation, rotSpeed * Time.deltaTime);
        if (transform.rotation == newTargetRotation)
        {
            isRotate = false;
            targetRotAngle = 0;
        }


    }
    void RaiseSpaceship()
    {
        Vector3 targetPosition = new Vector3(0, targetMovement, 0);
        Vector3 newTargetPosition = currentPosition + targetPosition;
        transform.position = Vector3.MoveTowards(transform.position, newTargetPosition, moveSpeed * Time.deltaTime);

        if (transform.position == newTargetPosition)
        {
            isMove = false;
            targetMovement = 0;
        }

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
                targetMovement += 1f;
                elevationAngle.text = targetMovement.ToString();
                Debug.Log("targetMovement: " + targetMovement);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                //GOES DOWNSIDE

                targetMovement -= 1f;
                elevationAngle.text = targetMovement.ToString();
                Debug.Log("targetMovement: " + targetMovement);
            }

        }

        //BEARING MOVEMENT
        if (GameManager.Instance.bActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                //TURN LEFT
                targetRotAngle += -15f;
                Debug.Log("rotationAmount " + targetRotAngle);
                turnAngle.text = targetRotAngle.ToString();
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                //TURN RIGHT
                targetRotAngle += 15f;
                Debug.Log("rotationAmount " + targetRotAngle);
                turnAngle.text = targetRotAngle.ToString();
            }
        }

        //SMMOTH ROTATION AFTER ENTER
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            // Stores curent rotation and position
            currentRotation = transform.rotation;
            currentPosition = transform.position;
            if (power.enginePower >= 1)
            {
                power.enginePower--;

                //Vector3 currentPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);

                isRotate = true;
                isMove = true;

                if (power.enginePower <= 2 && targetRotAngle <= 30)
                {
                    Debug.Log("Safe turn");
                }
                if (power.enginePower <= 2 && targetRotAngle >= 60 && (targetRotAngle <= 105))
                {
                    Debug.Log("sharp turn need more power. chance of damage to stability");
                }
                if (power.enginePower <= 2 && targetRotAngle >= 105)
                {
                    Debug.Log("Aggressive turn, Need more power, higher chances of damage to stability");
                }
                //targetRotAngle = 0;
                turnAngle.text = "0";
                //------------------UP DOWN TURN--------------------
                // transform.position = currentPos + upDownAngle;

                //targetMovement = 0;
                elevationAngle.text = "0";

            }

            //ADD POWER TO THE SPEED
            SpaceshipMoveSpeed = speed;

            if (power.sensorPower >= 1)
            {
                detectionRadius += 10;
                power.sensorPower--;
                Debug.Log("Sensors dec by 1 and Detection redius inc by 10");
                Debug.Log("detectionRadius: " + detectionRadius.ToString());
                Debug.Log("sespo" + power.sensorPower);
            }
        }
    }
    void EnemySpaseshipDetection()
    {
        Collider[] hitCollider = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        foreach (Collider collider in hitCollider)
        {
            Debug.Log("Enemy Detected: " + collider.gameObject.name);
            //GameManager.Instance.isEnemyDetect = true;
            // collider.gameObject.GetComponent<Enemy>().enabled = true;

        }
    }


    public void SpeedInc()
    {
        if (GameManager.Instance.sActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (speed < 50)
            {
                speed += 5f;
            }
            else
            {
                speed = 50f;
            }
            speedText.text = speed.ToString() + " km/h";
        }



    }
    public void SpeedDec()
    {
        if (GameManager.Instance.sActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (speed > 0f)
            {
                speed -= 5f;
            }
            else
            {
                speed = 0f;
            }
            speedText.text = speed.ToString() + " km/h";
        }


    }
}
