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
    #region VARIABLES
    [SerializeField] float SpaceshipMoveSpeed = 0f;
    [SerializeField] bool isEnterPress;

    [SerializeField] TextMeshProUGUI turnAngle;
    [SerializeField] TextMeshProUGUI elevationAngle;

    [SerializeField] float detectionRadius;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] LayerMask rockLayer;
    [SerializeField] LayerMask cloudLayer;

    [SerializeField] float speed;
    [SerializeField] TextMeshProUGUI speedText;
    [SerializeField] float speedLimit;
    float powerTurnSpeed;
    float powerRaiseSpeed;

    [SerializeField] Power power;

    [SerializeField] float moveSpeed;
    [SerializeField] float targetMovement;
    [SerializeField] bool isMove;
    [SerializeField] TextMeshProUGUI currentHeightOfShip;

    [SerializeField] float rotSpeed;
    [SerializeField] float targetRotAngle;
    [SerializeField] bool isRotate;

    [SerializeField] bool isRotateArrow;
    [SerializeField] float arrowRotAngle;

    Quaternion currentRotation;
    Vector3 currentPosition;

    [SerializeField] UnityEngine.UI.Slider elevCounterSlider;

    [SerializeField] Damageable damageable;

    float timeInterval1, timeInterval2;
    float timeDiff1 = 0.05f, timeDiff2 = 0.2f;

    [SerializeField] TextMeshProUGUI alertText;
    #endregion

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
        power.enginePower = 3;
        power.weaponPower = 3;
        power.sensorPower = 3;
        speedLimit = 50f;
        powerRaiseSpeed = 1f;
        powerTurnSpeed = 10f;

        isEnterPress = false;
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

        currentHeightOfShip.text = gameObject.transform.position.y.ToString();

        
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
    
    
    void RotateSpaceship()
    {
        Debug.Log("is rotate");
        Quaternion targetRotation = Quaternion.Euler(0, targetRotAngle, 0);
        
        Quaternion newTargetRotation =  targetRotation * currentRotation;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, newTargetRotation, rotSpeed * Time.deltaTime);
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

        if (Mathf.Abs(transform.position.y - newTargetPosition.y) < 0.05f)
        {
            isMove = false;
            targetMovement = 0;
        }


    }
    
    void SpaceshipMove()
    {
        //CONTINUES FORWORD MOVEMENT 

     
        
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            //ADD POWER TO THE SPEED
            SpaceshipMoveSpeed = speed;
            isEnterPress = true;
            
        }
        transform.Translate(Vector3.forward * Time.deltaTime * SpaceshipMoveSpeed);
        if (isEnterPress )
        {
            if (power.enginePower == 2)
            {
                speedLimit = 35f;
                if (speed > speedLimit)
                {
                    SmoothSpeedDec(35);

                }
                
                transform.Translate(Vector3.forward * Time.deltaTime * SpaceshipMoveSpeed);
            }
            if (power.enginePower == 3)
            {
                speedLimit = 50f;
                if (speed > speedLimit)
                {
                    SmoothSpeedDec(50);
                }

                transform.Translate(Vector3.forward * Time.deltaTime * SpaceshipMoveSpeed);
            }
            if (power.enginePower == 7)
            {
                speedLimit = 150f;

                transform.Translate(Vector3.forward * Time.deltaTime * SpaceshipMoveSpeed);
            }
            if (speed == speedLimit)
            {
                isEnterPress = false;
            }
        }
        //SpaceshipMoveSpeed = 13;
        //transform.Translate(Vector3.forward * Time.deltaTime * SpaceshipMoveSpeed);

        //ELEVATION MOVEMENT
        if (GameManager.Instance.eActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                //GOES UPSIDE
                targetMovement += 1f;
                elevationAngle.text = targetMovement.ToString();
                Debug.Log("targetMovement: " + targetMovement);

                if (power.enginePower == 2)
                {
                    if (targetMovement <= 2)
                    {
                        alertText.text = "Safe Raise";
                        alertText.color = Color.white;
                    }
                    if ((targetRotAngle > 2 && targetRotAngle <= 4))
                    {
                        alertText.text = "Sharp Raise, chance of damage to stability on increase the power";
                        alertText.color = Color.yellow;
                    }
                    if (targetRotAngle > 5)
                    {
                        alertText.text = "Agressive Raise Need more power, higher chances of damage to stability";
                        alertText.color = Color.red;
                    }
                }
                if (power.enginePower == 3)
                {
                    if (targetMovement <= 3)
                    {
                        alertText.text = "Safe Raise";
                        alertText.color = Color.white;
                    }
                    if ((targetRotAngle > 3 && targetRotAngle <= 5))
                    {
                        alertText.text = "Sharp Raise, chance of damage to stability on increase the power";
                        alertText.color = Color.yellow;
                    }
                    if (targetRotAngle > 6)
                    {
                        alertText.text = "Agressive Raise Need more power, higher chances of damage to stability";
                        alertText.color = Color.red;
                    }
                }
                if (power.enginePower == 7)
                {
                    if (targetMovement <= 5 )
                    {
                        alertText.text = "Safe Raise";
                        alertText.color = Color.white;
                    }
                    if ((targetRotAngle > 5 && targetRotAngle <= 7))
                    {
                        alertText.text = "Sharp Raise, chance of damage to stability on increase the power";
                        alertText.color = Color.yellow;
                    }
                    if (targetRotAngle > 7)
                    {
                        alertText.text = "Agressive Raise Need more power, higher chances of damage to stability";
                        alertText.color = Color.red;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                //GOES DOWNSIDE

                targetMovement -= 1f;
                elevationAngle.text = targetMovement.ToString();
                Debug.Log("targetMovement: " + targetMovement);
                if (power.enginePower == 2)
                {
                    if ( targetMovement <= -2)
                    {
                        alertText.text = "Safe Raise";
                        alertText.color = Color.white;
                    }
                    if ( (targetRotAngle > -2 && targetRotAngle <= -4))
                    {
                        alertText.text = "Sharp Raise, chance of damage to stability on increase the power";
                        alertText.color = Color.yellow;
                    }
                    if ( targetRotAngle > -5)
                    {
                        alertText.text = "Agressive Raise Need more power, higher chances of damage to stability";
                        alertText.color = Color.red;
                    }
                }
                if (power.enginePower == 3)
                {
                    if (targetMovement <= -3)
                    {
                        alertText.text = "Safe Raise";
                        alertText.color = Color.white;
                    }
                    if ( (targetRotAngle > -3 && targetRotAngle <= -5))
                    {
                        alertText.text = "Sharp Raise, chance of damage to stability on increase the power";
                        alertText.color = Color.yellow;
                    }
                    if ( targetRotAngle > -6)
                    {
                        alertText.text = "Agressive Raise Need more power, higher chances of damage to stability";
                        alertText.color = Color.red;
                    }
                }
                if (power.enginePower == 7)
                {
                    if ( targetMovement <= -5)
                    {
                        alertText.text = "Safe Raise";
                        alertText.color = Color.white;
                    }
                    if ( (targetRotAngle > -3 && targetRotAngle <= -5))
                    {
                        alertText.text = "Sharp Raise, chance of damage to stability on increase the power";
                        alertText.color = Color.yellow;
                    }
                    if ( targetRotAngle > -7)
                    {
                        alertText.text = "Agressive Raise Need more power, higher chances of damage to stability";
                        alertText.color = Color.red;
                    }
                }
            }

        }

        //BEARING MOVEMENT
        if (GameManager.Instance.bActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (Input.GetKey(KeyCode.A))
            {
                //TURN LEFT

                timeInterval1 += Time.deltaTime;
                if(timeInterval1 >= timeDiff1)
                {
                    timeInterval1 = 0;
                    targetRotAngle = targetRotAngle - 1f ;
                    turnAngle.text = targetRotAngle.ToString();
                }


                if (power.enginePower == 2)
                {
                    if ((targetRotAngle >= -45 && targetRotAngle <= 0 )||(targetRotAngle >= 0 && targetRotAngle <= 45))
                    {
                        alertText.text = "Safe turn";
                        alertText.color = Color.white;
                    }
                    if ((targetRotAngle < -45 && targetRotAngle >= -60) || (targetRotAngle > 45 && targetRotAngle <= 60))
                    {
                        alertText.text = "Sharp turn, chance of damage to stability oncrease the power";
                        alertText.color = Color.yellow;
                    }
                    if (targetRotAngle < -60 || targetRotAngle > 60)
                    {
                        alertText.text = "Agressive turn Need more power, higher chances of damage to stability";
                        alertText.color = Color.red;
                    }
                }
                if (power.enginePower == 3)
                {
                    if ((targetRotAngle >= -60 && targetRotAngle <= 0) || (targetRotAngle >= 0 && targetRotAngle <= 60) )
                    {
                        alertText.text = "Safe turn";
                        alertText.color = Color.white;
                    }
                    if ((targetRotAngle < -60 && targetRotAngle >= -90) || (targetRotAngle > 60 && targetRotAngle <= 90))
                    {
                        alertText.text = "Sharp turn, chance of damage to stability oncrease the power";
                        alertText.color = Color.yellow;
                    }
                    if (targetRotAngle < -120 || targetRotAngle > 120)
                    {
                        alertText.text = "Agressive turn Need more power, higher chances of damage to stability";
                        alertText.color = Color.red;
                    }
                }
                if (power.enginePower == 7)
                {
                    if ((targetRotAngle <= -120 && targetRotAngle <= 0)|| (targetRotAngle >= 0 && targetRotAngle <= 120))
                    {
                        alertText.text = "Safe turn";
                        alertText.color = Color.white;
                    }
                    if ((targetRotAngle < -120 && targetRotAngle >= -180) || (targetRotAngle > 120 && targetRotAngle <= 180))
                    {
                        alertText.text = "Sharp turn, chance of damage to stability oncrease the power";
                        alertText.color = Color.yellow;
                    }
                    if ( targetRotAngle < -180 || targetRotAngle > 180)
                    {
                        alertText.text = "Agressive turn Need more power, higher chances of damage to stability";
                        alertText.color = Color.red;
                    }
                }

            }
            if (Input.GetKey(KeyCode.D))
            {
                //TURN RIGHT
                timeInterval2 += Time.deltaTime;
                if (timeInterval2 >= timeDiff1)
                {
                    timeInterval2 = 0;
                    targetRotAngle = targetRotAngle + 1f ;
                    turnAngle.text = targetRotAngle.ToString();
                }
                    

               // arrowRotAngle += 15f;
               // arrow.transform.rotation = Quaternion.Euler(-90, arrowRotAngle, 0);

                if (power.enginePower == 2)
                {
                    if ((targetRotAngle >= -45 && targetRotAngle <= 0) || (targetRotAngle >= 0 && targetRotAngle <= 45))
                    {
                        alertText.text = "Safe turn";
                        alertText.color = Color.white;
                    }
                    if ((targetRotAngle < -45 && targetRotAngle >= -60) || (targetRotAngle > 45 && targetRotAngle <= 60))
                    {
                        alertText.text = "Sharp turn, chance of damage to stability oncrease the power";
                        alertText.color = Color.yellow;
                    }
                    if (targetRotAngle < -60 && targetRotAngle >= -150  || targetRotAngle > 60 && targetRotAngle <= 150)
                    {
                        alertText.text = "Agressive turn Need more power, higher chances of damage to stability";
                        alertText.color = Color.red;
                    }
                }
                if (power.enginePower == 3)
                {
                    if ((targetRotAngle >= -60 && targetRotAngle <= 0) || (targetRotAngle >= 0 && targetRotAngle <= 60))
                    {
                        alertText.text = "Safe turn";
                        alertText.color = Color.white;
                    }
                    if ((targetRotAngle < -60 && targetRotAngle >= -90) || (targetRotAngle > 60 && targetRotAngle <= 90))
                    {
                        alertText.text = "Sharp turn, chance of damage to stability oncrease the power";
                        alertText.color = Color.yellow;
                    }
                    if (targetRotAngle < -120 && targetRotAngle >= -200 || targetRotAngle > 120 && targetRotAngle <= 200)
                    {
                        alertText.text = "Agressive turn Need more power, higher chances of damage to stability";
                        alertText.color = Color.red;
                    }
                }
                if (power.enginePower == 7)
                {
                    if ((targetRotAngle <= -120 && targetRotAngle <= 0) || (targetRotAngle >= 0 && targetRotAngle <= 120))
                    {
                        alertText.text = "Safe turn";
                        alertText.color = Color.white;
                    }
                    if ((targetRotAngle < -120 && targetRotAngle >= -180) || (targetRotAngle > 120 && targetRotAngle <= 180))
                    {
                        alertText.text = "Sharp turn, chance of damage to stability oncrease the power";
                        alertText.color = Color.yellow;
                    }
                    if (targetRotAngle < -180 && targetRotAngle >= -250 || targetRotAngle > 180 && targetRotAngle <= 250)
                    {
                        alertText.text = "Agressive turn Need more power, higher chances of damage to stability";
                        alertText.color = Color.red;
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

                #region POWER AND DAMAGE FOR ROTATION AND RAISE
                if (power.enginePower == 2)
                {
                    powerTurnSpeed = 5;
                    rotSpeed = 5;
                    //TURN RIGHT LEFT
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
                    //RAISE UP OR DOWN
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
                if (power.enginePower == 3)
                {
                    powerTurnSpeed = 10;
                    rotSpeed = 10;
                    //TURN RIGHT LEFT
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
                    //RAISE UP OR DOWN
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
                if (power.enginePower == 7)
                {
                    rotSpeed = 20;
                    powerTurnSpeed = 20;
                    //TURN RIGHT LEFT
                    if (targetRotAngle <= 120 || targetRotAngle <= -120)
                    {
                        Debug.Log("safe turn");
                    }
                    if ((targetRotAngle > 120 && targetRotAngle <= 180) || (targetRotAngle > -120 && targetRotAngle <= -180))
                    {
                        Debug.Log(" sharp turn chance of damage to stability");
                        damageable.ApplyDamage(5);
                    }
                    if (targetRotAngle > 180 || targetRotAngle > -180)
                    {
                        Debug.Log("Agressive turn higher chances of damage to stability");
                        damageable.ApplyDamage(10);
                    }
                    //RAISE UP OR DOWN
                    if (targetMovement <= 5 || targetMovement <= -5)
                    {
                        Debug.Log("safe Raise");
                    }
                    if ((targetRotAngle > 5 && targetRotAngle <= 7) || (targetRotAngle > -3 && targetRotAngle <= -5))
                    {
                        Debug.Log(" sharp Raise chance of damage to stability");
                        damageable.ApplyDamage(5);
                    }
                    if (targetRotAngle > 7 || targetRotAngle > -7)
                    {
                        Debug.Log("Agressive Raise higher chances of damage to stability");
                        damageable.ApplyDamage(10);
                    }
                }
                #endregion


                //turnAngle.text = "0";
                elevationAngle.text = "0";
                
                StartDecrementingAngle();
               // StartDecrementingRaise();
            }

            

            #region SENSORS
            
            if(power.sensorPower == 2)
            {
                detectionRadius = 20;
            }
            if(power.sensorPower == 3)
            {
                detectionRadius = 30;
            }
            if((power.sensorPower == 7))
            {
                detectionRadius = 70;
            }
            
            #endregion

        }
        DecrementAngleOverTime();
       // DecrementRaisOverTime();
    }
    float n, n1;
    void StartDecrementingAngle()
    {
        n = 0;  // Reset the timer
        n1 = targetRotAngle;  // Update n1 with the current targetRotAngle
    }

    void DecrementAngleOverTime()
    {
        if (n1 > 0)  // Continue decrementing until n1 reaches 0
        {
            n += Time.deltaTime;
            if (n >= timeDiff2)
            {
                n = 0;  // Reset the timer
                n1 -= 1;  // Decrease n1
                //targetRotAngle -= 1;  // Decrease targetRotAngle
                turnAngle.text = n1.ToString();  // Update the UI text
            }
        }
    }
    float m, m1;
    void StartDecrementingRaise()
    {
        m = 0;  // Reset the timer
        m1 = targetMovement;  // Update n1 with the current targetRotAngle
    }

    void DecrementRaisOverTime()
    {
        if (m1 > 0)  // Continue decrementing until n1 reaches 0
        {
            m += Time.deltaTime;
            if (m >= timeDiff2)
            {
                m = 0;  // Reset the timer
                m1 -= 1;  // Decrease n1
                //targetRotAngle -= 1;  // Decrease targetRotAngle
                elevationAngle.text = m1.ToString();  
                elevCounterSlider.value = m1;
            }
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
            if (speed < speedLimit)
            {
                speed += 1f;
            }
            else
            {
                speed = speedLimit;
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
                speed -= 1f;
            }
            else
            {
                speed = 0f;
            }
            speedText.text = speed.ToString() + " km/h";
        }


    }
    void SmoothSpeedInc()
    {
        n += Time.deltaTime;
        if (n >= timeDiff1)
        {
            n = 0;  // Reset the timer
            if (speed < speedLimit)
            {
                speed += 1f;  // Increase speed
            }
            else
            {
                speed = speedLimit;
            }
            speedText.text = speed.ToString() + " km/h";  // Update the UI text
        }
    }

    void SmoothSpeedDec(float endSpeed)
    {
        n += Time.deltaTime;
        if (n >= timeDiff1)
        {
            n = 0;  // Reset the timer
            if (speed > endSpeed)
            {
                SpaceshipMoveSpeed -= 1;
                speed -= 1f;  // Decrease speed
            }
            else if(speed == endSpeed)
            {
                speed = endSpeed;
                SpaceshipMoveSpeed = endSpeed;
                isEnterPress = false;
            }
            speedText.text = speed.ToString() + " km/h";  // Update the UI text
        }
    }
    public void SpeedController()
    {
        if(!(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && GameManager.Instance.bActive)
        {
            if (Input.GetKey(KeyCode.W))
            {
                SmoothSpeedInc();
            }
            if (Input.GetKey(KeyCode.S))
            {
                SmoothSpeedDec(0);
            }
        }
        

    }
    void ElevationCounter()
    {
        //elevCounterSlider.value = this.gameObject.transform.position.y;
        elevCounterSlider.value = targetMovement;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision != null)
        {
            if (collision.gameObject.layer == 15 || collision.gameObject.layer == 3)
            {
                Destroy(gameObject);
                GameManager.Instance.EndGame(); 
                //damageable.ApplyDamage(100);
            }
        }
    }
}
