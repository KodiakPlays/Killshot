using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] LayerMask rockLayer;
    [SerializeField] LayerMask cloudLayer;

    [SerializeField] float speed;
    [SerializeField] TextMeshProUGUI speedText;

    [SerializeField] Power power;

    [SerializeField] float moveSpeed;
    [SerializeField] float targetMovement;
    [SerializeField] bool isMove;

    [SerializeField] float rotSpeed;
    [SerializeField] float targetRotAngle;
    [SerializeField] bool isRotate;

    [SerializeField] bool isRotateArrow;
    [SerializeField] float arrowRotAngle;

    Quaternion currentRotation;
    Vector3 currentPosition;

    [SerializeField] UnityEngine.UI.Slider elevCounterSlider;

    [SerializeField] GameObject arrow;

    [SerializeField] Damageable damageable;
    private void Awake()
    {
        LineRendIn();
    }
    // Update is called once per frame
    public bool isEleCounterAdd;
    private void Start()
    {
        isEleCounterAdd = true;
        speed = SpaceshipMoveSpeed;
        speedText.text = speed.ToString() + " km/h";

    }
    
    void Update()
    {
        SpaceshipMove();
        SpeedController();
        DrawCicle();
        ElevationCounter();
        //Detection 
        EnemySpaseshipDetection();
        RockDetection();
        CloudDetection();

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
        if(isRotateArrow)
        {
            //RotateArrow();
        }

    }
    void RotateArrow()
    {
        Quaternion targetRotation = Quaternion.Euler(-90, 0, 0);

        arrow.transform.rotation = Quaternion.RotateTowards(arrow.transform.rotation, Quaternion.Euler(-90, 0, 0), rotSpeed * Time.deltaTime);
        if(arrow.transform.rotation == Quaternion.Euler(-90,0,0))
        {
            isRotateArrow = false;
        }
    }
    void RotateSpaceship()
    {
        Debug.Log("is rotate");
        Quaternion targetRotation = Quaternion.Euler(0, targetRotAngle, 0);
        
        Quaternion newTargetRotation =  targetRotation * currentRotation;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, newTargetRotation, rotSpeed * Time.deltaTime);
       // Quaternion newarrowRotation = Quaternion.Euler(-90, 0.000f, 0.00000f);
       // arrow.transform.rotation = Quaternion.RotateTowards(arrow.transform.rotation, newarrowRotation, rotSpeed * Time.deltaTime);
        GameManager.Instance.bActive = false;
        GameManager.Instance.bRedImage.SetActive(true);
        GameManager.Instance.bGreenImage.SetActive(false);
        if (transform.rotation == newTargetRotation)
        {
            isRotate = false;
            targetRotAngle = 0;
            transform.rotation = newTargetRotation;
        }
    }
    void RaiseSpaceship()
    {
        Vector3 targetPosition = new Vector3(0,  targetMovement, 0);
        Vector3 newTargetPosition = currentPosition + targetPosition;

        transform.position = Vector3.MoveTowards(transform.position, newTargetPosition, moveSpeed * Time.deltaTime);

        GameManager.Instance.eActive = false;
        GameManager.Instance.eRedImage.SetActive(true);
        GameManager.Instance.eGreenImage.SetActive(false);

        if (Mathf.Abs(transform.position.y - newTargetPosition.y) < 0.1f)
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
                turnAngle.text = targetRotAngle.ToString();

                //arrowRotAngle += -15f;
                 //arrow.transform.rotation = Quaternion.Euler(-90, arrowRotAngle, 0);
                if(power.enginePower >= 1 && power.enginePower <= 3)
                {
                    if(targetRotAngle <= -45)
                    {
                        Debug.Log("Your taking safe turn");
                    }
                    if(targetRotAngle > -45 &&  targetRotAngle <= -60)
                    {
                        Debug.Log("Your taking sharp turn you can decrease the turn angle or you can inc the power");
                    }
                    if (targetRotAngle > -60)
                    {
                        Debug.Log("Your taking Agressive turn you have to decrease the turn angle or you can inc the power if you not then ship will get damage");
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                //TURN RIGHT
                targetRotAngle += 15f;
                turnAngle.text = targetRotAngle.ToString();

               // arrowRotAngle += 15f;
               // arrow.transform.rotation = Quaternion.Euler(-90, arrowRotAngle, 0);

                if (power.enginePower >= 1 && power.enginePower < 3)
                {
                    if (targetRotAngle <= 45 || targetRotAngle <= -45)
                    {
                        Debug.Log("Your taking safe turn");
                    }
                    if (targetRotAngle > 45 && targetRotAngle <= 60)
                    {
                        Debug.Log("Your taking sharp turn need more power. chance of damage to stability");
                    }
                    if (targetRotAngle > 60)
                    {
                        Debug.Log("Your taking Agressive turn  Need more power, higher chances of damage to stability");
                    }
                }
            }
        }

        //SMMOTH ROTATION AFTER ENTER
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            #region DEACTIVATE ALL BUTTONS AND TURN ON THE RED IMAGE
            GameManager.Instance.eActive = false;
            GameManager.Instance.bActive = false;
            GameManager.Instance.fActive = false;
            GameManager.Instance.pActive = false;
            GameManager.Instance.lActive = false;

            GameManager.Instance.eRedImage.SetActive(true);
            GameManager.Instance.eGreenImage.SetActive(false);
            GameManager.Instance.bRedImage.SetActive(true);
            GameManager.Instance.bGreenImage.SetActive(false);
            GameManager.Instance.fRedImage.SetActive(true);
            GameManager.Instance.fGreenImage.SetActive(false);
            GameManager.Instance.pRedImage.SetActive(true);
            GameManager.Instance.pGreenImage.SetActive(false);
            GameManager.Instance.lRedImage.SetActive(true);
            GameManager.Instance.lGreenImage.SetActive(false);

            #endregion
            arrowRotAngle = 0;
            // Stores curent rotation and position
            currentRotation = transform.rotation;
            currentPosition = transform.position;
            if (power.enginePower >= 1)
            {

                isRotate = true;
                isMove = true;
                isRotateArrow = true;

                #region POWER AND DAMAGE FOR ROTATION
                if (power.enginePower >= 1 && power.enginePower < 3)
                {
                    if (targetRotAngle <= 45 || targetRotAngle <= -45)
                    {
                        Debug.Log("safe turn");
                    }
                    if ((targetRotAngle > 45 && targetRotAngle <= 60) || (targetRotAngle > -45 && targetRotAngle <= -60))
                    {
                        Debug.Log(" sharp turn chance of damage to stability");
                        damageable.ApplyDamage(5);
                    }
                    if (targetRotAngle > 60 || targetRotAngle > -60)
                    {
                        Debug.Log("Agressive turn higher chances of damage to stability");
                        damageable.ApplyDamage(10);
                    }
                }
                if (power.enginePower >= 3 && power.enginePower < 6)
                {
                    if (targetRotAngle <= 60 || targetRotAngle <= -60)
                    {
                        Debug.Log("safe turn");
                    }
                    if ((targetRotAngle > 60 && targetRotAngle <= 90) || (targetRotAngle > -60 && targetRotAngle <= -90))
                    {
                        Debug.Log(" sharp turn chance of damage to stability");
                        damageable.ApplyDamage(5);
                    }
                    if (targetRotAngle > 120 || targetRotAngle > -120)
                    {
                        Debug.Log("Agressive turn higher chances of damage to stability");
                        damageable.ApplyDamage(10);
                    }
                }
                if (power.enginePower >= 6 && power.enginePower < 9)
                {
                    if (targetRotAngle <= 90 || targetRotAngle <= -90)
                    {
                        Debug.Log("safe turn");
                    }
                    if ((targetRotAngle > 120 && targetRotAngle <= 130) || (targetRotAngle > -120 && targetRotAngle <= -130))
                    {
                        Debug.Log(" sharp turn chance of damage to stability");
                        damageable.ApplyDamage(5);
                    }
                    if (targetRotAngle > 160 || targetRotAngle > -160)
                    {
                        Debug.Log("Agressive turn higher chances of damage to stability");
                        damageable.ApplyDamage(10);
                    }
                }
                #endregion

                #region POWER AND DAMAGE FOR RAISE UP & DOWN
                if (power.enginePower >= 1 && power.enginePower < 3)
                {
                    if (targetMovement <= 2 || targetMovement <= -2)
                    {
                        Debug.Log("safe Raise");
                    }
                    if ((targetRotAngle > 2 && targetRotAngle <= 4) || (targetRotAngle > -2 && targetRotAngle <= -4))
                    {
                        Debug.Log(" sharp Raise chance of damage to stability");
                        damageable.ApplyDamage(5);
                    }
                    if (targetRotAngle > 5 || targetRotAngle > -5)
                    {
                        Debug.Log("Agressive Raise higher chances of damage to stability");
                        damageable.ApplyDamage(10);
                    }
                }
                if (power.enginePower >= 3 && power.enginePower < 6)
                {
                    if (targetMovement <= 3 || targetMovement <= -3)
                    {
                        Debug.Log("safe Raise");
                    }
                    if ((targetRotAngle > 3 && targetRotAngle <= 5) || (targetRotAngle > -3 && targetRotAngle <= -5))
                    {
                        Debug.Log(" sharp Raise chance of damage to stability");
                        damageable.ApplyDamage(5);
                    }
                    if (targetRotAngle > 6 || targetRotAngle > -6)
                    {
                        Debug.Log("Agressive Raise higher chances of damage to stability");
                        damageable.ApplyDamage(10);
                    }
                }

                #endregion


                turnAngle.text = "0";
                elevationAngle.text = "0";

            }

            //ADD POWER TO THE SPEED
            SpaceshipMoveSpeed = speed;

            #region SENSORS
            if (power.sensorPower >= 1)
            {
               // detectionRadius += 5;
               // power.sensorPower--;

            }
            if(power.sensorPower == 1)
            {
                detectionRadius = 20;
            }
            if(power.sensorPower == 2)
            {
                detectionRadius = 30;
            }
            if(power.sensorPower == 3)
            {
                detectionRadius = 40;
            }
            if((power.sensorPower == 4))
            {
                detectionRadius = 50;
            }
            if(power.sensorPower == 5)
            {
                detectionRadius = 60;
            }
            if((power.sensorPower == 6))
            {
                detectionRadius = 70;
            }
            #endregion

        }
    }
    void EnemySpaseshipDetection()
    {
        Collider[] hitCollider = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        foreach (Collider collider in hitCollider)
        {
            Debug.Log("Enemy Detected: " + collider.gameObject.name);
            //AudioManager.Instance.PlayEnemyAlert();
        }
    }

    void RockDetection()
    {
        Collider[] hitCollider = Physics.OverlapSphere(transform.position, detectionRadius, rockLayer);
        foreach (Collider collider in hitCollider)
        {
            Debug.Log("Rock Detected: " + collider.gameObject.name);
            
            //AudioManager.Instance.PlayEnemyAlert();
        }
    }
    void CloudDetection()
    {
        Collider[] hitCollider = Physics.OverlapSphere(transform.position, detectionRadius, cloudLayer);
        foreach (Collider collider in hitCollider)
        {
            Debug.Log("Cloud Detected: " + collider.gameObject.name);
            //AudioManager.Instance.PlayEnemyAlert();
        }
    }
    public int segments = 25; 
    public Material circleMat;

    public LineRenderer lineRenderer;
    void LineRendIn()
    {
        lineRenderer = GetComponent<LineRenderer>();
        // Set LineRenderer properties
        lineRenderer.positionCount = segments + 1;
        lineRenderer.startWidth = 1f;
        lineRenderer.endWidth = 1f;
        lineRenderer.useWorldSpace = true;
        //lineRenderer.startColor = circleColor;
        //lineRenderer.endColor = circleColor;
        lineRenderer.material = circleMat;
        DrawCicle();
    }
    void DrawCicle()
    {
        // Calculate segment size
        float angleStep = 360f / segments;

        // Update LineRenderer positions
        for (int i = 0; i <= segments; i++)
        {
            float angle = angleStep * i;
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * detectionRadius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * detectionRadius;

            lineRenderer.SetPosition(i, transform.position + new Vector3(x, 0, z));
        }
    }

    public void SpeedInc()
    {
        if (GameManager.Instance.bActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
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
        if (GameManager.Instance.bActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
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
    public void SpeedController()
    {
        if(!(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && GameManager.Instance.bActive)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                SpeedInc();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                SpeedDec();
            }
        }
        

    }
    public float minY = -10f; 
    public float maxY = 10f;
    void ElevationCounter()
    {
        elevCounterSlider.value = this.gameObject.transform.position.y;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision != null)
        {
            if (collision.gameObject.layer == 15 || collision.gameObject.layer == 3)
            {
                Destroy(gameObject, 1f);
                //damageable.ApplyDamage(100);
            }
        }
    }
}
