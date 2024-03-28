using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
//UnityEngine.Gizmos.DrawWireSphere;
using UnityEngine.UIElements;

public class SpaceshipMovement : MonoBehaviour
{
    Quaternion rotationAngle;
    Vector3 upDownAngle;

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

    //public Enemy[] enemy;
    // Update is called once per frame
    private void Start()
    {
        speed = SpaceshipMoveSpeed;
        speedText.text = speed.ToString() + " km/h";

        //enemy = GetComponent<Enemy>();
    }
    void Update()
    {
        SpaceshipMove();
        EnemySpaseshipDetection();
    }
    private void FixedUpdate()
    {
        if(isRotate)
        {
            RotateSpaceship();
        }
        if(isMove)
        {
            RaiseSpaceship();
        }

    }
    void RotateSpaceship()
    {
        Quaternion targetRotation = Quaternion.Euler(0, targetRotAngle, 0);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);
        if(transform.rotation == targetRotation)
        {
            isRotate = false;
        }
    }
    void RaiseSpaceship()
    {
        Vector3 targetPosition = new Vector3(transform.position.x, targetMovement, transform.position.z);
        transform.position =  Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if(transform.position == targetPosition)
        {
            isMove = false;
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
                //upDownAngle += new Vector3(0, 1f, 0);
                //Debug.Log("RaiseAmount " + upDownAngle);
                //elevationAngle.text = upDownAngle.y.ToString();

                targetMovement += 1f;
                elevationAngle.text = targetMovement.ToString();
                Debug.Log("targetMovement: " + targetMovement);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                //GOES DOWNSIDE
                //upDownAngle += new Vector3(0, -1f, 0);
                //Debug.Log("RaiseAmount " + upDownAngle);
                //elevationAngle.text = upDownAngle.y.ToString();

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
                //rotationAngle.y += -15;// new Vector3(0, -15, 0);
                //Debug.Log("rotationAmount " + rotationAngle);
                //turnAngle.text = rotationAngle.y.ToString();

                targetRotAngle += -15f;
                Debug.Log("rotationAmount " + targetRotAngle);
                turnAngle.text =targetRotAngle.ToString();
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                //TURN RIGHT
                //rotationAngle.y += 15;// new Vector3(0, 15, 0);
                //Debug.Log("rotationAmount " + rotationAngle);
                //turnAngle.text = rotationAngle.y.ToString();

                targetRotAngle += 15f;
                Debug.Log("rotationAmount " + targetRotAngle);
                turnAngle.text = targetRotAngle.ToString();
            }
        }
       
        //SMMOTH ROTATION AFTER ENTER
        if ((Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)))
        {
            if(power.enginePower >= 1)
            {
                power.enginePower--;

                Vector3 currentPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);

                isRotate = true;
                isMove = true;
              
                if (power.enginePower <= 2 && rotationAngle.y <= 30)
                {
                    Debug.Log("Safe turn");
                }
                if (power.enginePower <= 2 && rotationAngle.y >= 60 && (rotationAngle.y <= 105))
                {
                    Debug.Log("sharp turn need more power. chance of damage to stability");
                }
                if (power.enginePower <= 2 && rotationAngle.y >= 105)
                {
                    Debug.Log("Aggressive turn, Need more power, higher chances of damage to stability");
                }
                rotationAngle = new Quaternion(0, 0, 0, 0);
                turnAngle.text = "0";
                //------------------UP DOWN TURN--------------------
               // transform.position = currentPos + upDownAngle;
                
                upDownAngle = new Vector3(0, 0, 0);
                elevationAngle.text = "0";

            }

            //ADD POWER TO THE SPEED
            SpaceshipMoveSpeed = speed;

            if(power.sensorPower >= 1)
            {
                detectionRadius += 10;
                power.sensorPower--;
                Debug.Log("Sensors dec by 1 and Detection redius inc by 10");
                Debug.Log("detectionRadius: "+detectionRadius.ToString());
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
