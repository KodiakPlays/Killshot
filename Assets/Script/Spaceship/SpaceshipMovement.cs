
using TMPro;
using UnityEngine;

public class SpaceshipMovement : MonoBehaviour
{
    //Quaternion rotationAngle;
    //Vector3 upDownAngle;
    #region VARIABLES
    [SerializeField] float SpaceshipMoveSpeed = 0f;
    [SerializeField] float SpaceshipMinSpeed ;
    public int speedLimitMin, speedLimitMax, speedLimitAvg;
    public bool isEnterPress;

    [SerializeField] TextMeshProUGUI turnAngle;
    [SerializeField] TextMeshProUGUI elevationAngle;

    public float detectionRadius;
    [SerializeField] float minDetectionRadius, maxDetectionRadius, avgDetectionRadius;
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
    //[SerializeField] TextMeshProUGUI currentHeightOfShip;

    [SerializeField] float rotSpeed;
    [SerializeField] float targetRotAngle;
    public float RotAngle;
    [SerializeField] bool isRotate;

    Quaternion currentRotation;
    Vector3 currentPosition;

    [SerializeField] UnityEngine.UI.Slider elevCounterSlider;

    [SerializeField] Damageable damageable;

    float timeInterval1, timeInterval2;
    float timeDiff1 = 0.05f, timeDiff2 = 1f;

    public TextMeshProUGUI alertText;

   
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
        power.enginePower = GameManager.Instance.avgPower;
        power.weaponPower = GameManager.Instance.avgPower;
        power.sensorPower = GameManager.Instance.avgPower;
        detectionRadius = avgDetectionRadius;
        speedLimit = speedLimitAvg;//50f;
        powerRaiseSpeed = 1f;
        powerTurnSpeed = 10f;

        isEnterPress = false;
    }
    
    void Update()
    {
        SpaceshipMove();
        SpeedController();
        DrawCircle();
        //ElevationCounter();
        //Detection 
        EnemySpaceshipDetection();
        RockDetection();
        CloudDetection();

        elevCounterSlider.value = gameObject.transform.position.y;
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
    ///<summary>
    ///this is the main fuction of the script which controlls the ship movement
    ///</summary>
    void SpaceshipMove()
    {
        //CONTINUES FORWORD MOVEMENT 

        transform.Translate(Vector3.forward * Time.deltaTime * SpaceshipMoveSpeed);


        //SpaceshipMoveSpeed = 13;
        //transform.Translate(Vector3.forward * Time.deltaTime * SpaceshipMoveSpeed);

        #region WITHOUT ACTIVE/INACTIVE CONDITION
        // Elevation
        Elevation();

        // Turn
        Turn();

        #endregion

        #region OLD CODE WITH ACTIVE CONDITION

        /*
        //ELEVATION MOVEMENT

        if (GameManager.Instance.eActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            Elevation();
        }

        //BEARING MOVEMENT
        if (GameManager.Instance.bActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            Turn();
        }
        */
        #endregion

        //SMMOTH ROTATION AFTER ENTER
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            alertText.text = "";
            #region DEACTIVATE ALL BUTTONS AND TURN ON THE RED IMAGE
            GameManager.Instance.eActive = true;
            GameManager.Instance.bActive = true;
            GameManager.Instance.fActive = false;
            GameManager.Instance.pActive = false;
            //GameManager.Instance.lActive = false;

            GameManager.Instance.eRedImage.SetActive(false);
            GameManager.Instance.eGreenImage.SetActive(true);
            GameManager.Instance.bRedImage.SetActive(false);
            GameManager.Instance.bGreenImage.SetActive(true);
            GameManager.Instance.fRedImage.SetActive(true);
            GameManager.Instance.fGreenImage.SetActive(false);
            GameManager.Instance.pRedImage.SetActive(true);
            GameManager.Instance.pGreenImage.SetActive(false);
            GameManager.Instance.lRedImage.SetActive(true);
            GameManager.Instance.lGreenImage.SetActive(false);

            #endregion
            // Stores curent rotation and position
            currentRotation = transform.rotation;
            currentPosition = transform.position;
            if (power.enginePower >= 1)
            {
                isRotate = true;
                isMove = true;

                #region POWER AND DAMAGE FOR ROTATION AND RAISE
                if (power.enginePower == 2)
                {
                    alertText.text = "";
                    powerTurnSpeed = 5;
                    rotSpeed = 5;
                    //TURN RIGHT LEFT
                    if ((targetRotAngle >= -45 && targetRotAngle <= 0) || (targetRotAngle >= 0 && targetRotAngle <= 45))
                    {
                        Debug.Log("safe turn");
                    }
                    if ((targetRotAngle < -45 && targetRotAngle >= -60) || (targetRotAngle > 45 && targetRotAngle <= 60))
                    {
                        Debug.Log(" sharp turn chance of damage to stability");
                        damageable.ApplyDamage(5);
                    }
                    if ((targetRotAngle < -60 && targetRotAngle >= -150) || (targetRotAngle > 60 && targetRotAngle <= 150))
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
                        //Debug.Log("safe turn");
                    }
                    if ((targetRotAngle > 60 && targetRotAngle <= 90) || (targetRotAngle > -60 && targetRotAngle <= -90))
                    {
                       // Debug.Log(" sharp turn chance of damage to stability");
                        damageable.ApplyDamage(5);
                    }
                    if (targetRotAngle > 120 || targetRotAngle > -120)
                    {
                        //Debug.Log("Agressive turn higher chances of damage to stability");
                        damageable.ApplyDamage(10);
                    }
                    //RAISE UP OR DOWN
                    if (targetMovement <= 3 || targetMovement <= -3)
                    {
                        //Debug.Log("safe Raise");
                    }
                    if ((targetRotAngle > 3 && targetRotAngle <= 5) || (targetRotAngle > -3 && targetRotAngle <= -5))
                    {
                        ////Debug.Log(" sharp Raise chance of damage to stability");
                        damageable.ApplyDamage(5);
                    }
                    if (targetRotAngle > 6 || targetRotAngle > -6)
                    {
                       // Debug.Log("Agressive Raise higher chances of damage to stability");
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
                       // Debug.Log("safe turn");
                    }
                    if ((targetRotAngle > 120 && targetRotAngle <= 180) || (targetRotAngle > -120 && targetRotAngle <= -180))
                    {
                        //Debug.Log(" sharp turn chance of damage to stability");
                        damageable.ApplyDamage(5);
                    }
                    if (targetRotAngle > 180 || targetRotAngle > -180)
                    {
                       // Debug.Log("Agressive turn higher chances of damage to stability");
                        damageable.ApplyDamage(10);
                    }
                    //RAISE UP OR DOWN
                    if (targetMovement <= 5 || targetMovement <= -5)
                    {
                       // Debug.Log("safe Raise");
                    }
                    if ((targetRotAngle > 5 && targetRotAngle <= 7) || (targetRotAngle > -3 && targetRotAngle <= -5))
                    {
                        //Debug.Log(" sharp Raise chance of damage to stability");
                        damageable.ApplyDamage(5);
                    }
                    if (targetRotAngle > 7 || targetRotAngle > -7)
                    {
                       // Debug.Log("Agressive Raise higher chances of damage to stability");
                        damageable.ApplyDamage(10);
                    }
                }
                #endregion


                //turnAngle.text = "0";
                elevationAngle.text = "0";

                StartDecrementingAngle();
                //  StartDecrementingRaise();
            }



            #region SENSORS

            if (power.sensorPower == GameManager.Instance.minPower)
            {
                detectionRadius = minDetectionRadius;
            }
            if (power.sensorPower == GameManager.Instance.avgPower)
            {
                detectionRadius = avgDetectionRadius;
            }
            if ((power.sensorPower == GameManager.Instance.maxPower))
            {
                detectionRadius = maxDetectionRadius;
            }

            #endregion

        }
        DecrementAngleOverTime();
        // DecrementRaisOverTime();
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            //ADD POWER TO THE SPEED
            SpaceshipMoveSpeed = speed;
            isEnterPress = true;

        }

        if (isEnterPress)
        {
            if (power.enginePower == GameManager.Instance.minPower)
            {
                speedLimit = speedLimitMin;
                if (speed > speedLimit)
                {
                    SmoothSpeedDec(speedLimitMin);

                }

                transform.Translate(Vector3.forward * Time.deltaTime * SpaceshipMoveSpeed);
            }
            if (power.enginePower == GameManager.Instance.avgPower)
            {
                speedLimit = speedLimitAvg;
                if (speed > speedLimit)
                {
                    SmoothSpeedDec(speedLimitAvg);
                }

                transform.Translate(Vector3.forward * Time.deltaTime * SpaceshipMoveSpeed);
            }
            if (power.enginePower == GameManager.Instance.maxPower)
            {
                speedLimit = speedLimitMax;

                transform.Translate(Vector3.forward * Time.deltaTime * SpaceshipMoveSpeed);
            }
            if (speed == speedLimit)
            {
                isEnterPress = false;
            }
        }
    }
    ///<summary>
    ///Rotate the ship using RotateTowards function 
    ///and set the targetRotAngle variable 0 after it turn at set rotation
    ///</summary>
    void RotateSpaceship()
    {
        Quaternion targetRotation = Quaternion.Euler(0, targetRotAngle, 0);
        
        Quaternion newTargetRotation =  targetRotation * currentRotation;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, newTargetRotation, rotSpeed * Time.deltaTime);
        //GameManager.Instance.bActive = false;
       // GameManager.Instance.bRedImage.SetActive(true);
        GameManager.Instance.bGreenImage.SetActive(true);
        
        if (transform.rotation == newTargetRotation)
        {
            isRotate = false;
            
            targetRotAngle = 0;
            transform.rotation = newTargetRotation;
        }
    }
    ///<summary>
    ///Raise the ship and set targetMovement variable 0 
    ///</summary>
    void RaiseSpaceship()
    {
        //Debug.Log("Move true");
        Vector3 targetPosition = new Vector3(0,  targetMovement, 0);
        Vector3 newTargetPosition = currentPosition + targetPosition;

        //transform.position = Vector3.MoveTowards(transform.position, newTargetPosition, moveSpeed * Time.deltaTime );
        transform.position = Vector3.Lerp(transform.position, newTargetPosition, moveSpeed * Time.deltaTime);
       // GameManager.Instance.eActive = false;
        //GameManager.Instance.eRedImage.SetActive(true);
        GameManager.Instance.eGreenImage.SetActive(true);

        if (Mathf.Abs(transform.position.y - newTargetPosition.y) < 0.05f)
        {
            isMove = false;
            targetMovement = 0;
            transform.position = newTargetPosition;
        }


    }
    ///<summary>
    ///It sets the number to raise ship upside
    ///</summary> 
    public void XButton()
    {
        //GOES DOWNSIDE

        targetMovement -= 1f;
        elevationAngle.text = targetMovement.ToString();
        Debug.Log("targetMovement: " + targetMovement);
        if (power.enginePower == 2)
        {
            if ((targetMovement >= -2 && targetMovement <= 0) || (targetMovement <= 2 && targetMovement >= 0))
            {
                alertText.text = "Safe Raise";
                alertText.color = Color.white;
            }
            if ((targetMovement < -2 && targetMovement >= -4) || (targetMovement > 2 && targetMovement <= 4))
            {
                alertText.text = "Sharp Raise, chance of damage to stability on increase the power";
                alertText.color = Color.yellow;
            }
            if ((targetMovement < -5 && targetMovement > -8) || (targetMovement > 5 && targetMovement < 8))
            {
                alertText.text = "Agressive Raise Need more power, higher chances of damage to stability";
                alertText.color = Color.red;
            }
        }
        if (power.enginePower == 3)
        {
            if ((targetMovement >= -3 && targetMovement <= 0) || (targetMovement <= 3 && targetMovement >= 0))
            {
                alertText.text = "Safe Raise";
                alertText.color = Color.white;
            }
            if ((targetMovement < -3 && targetMovement >= -5) || (targetMovement > 3 && targetMovement <= 5))
            {
                alertText.text = "Sharp Raise, chance of damage to stability on increase the power";
                alertText.color = Color.yellow;
            }
            if ((targetMovement < -6 && targetMovement > -8) || (targetMovement > 6 && targetMovement < 8))
            {
                alertText.text = "Agressive Raise Need more power, higher chances of damage to stability";
                alertText.color = Color.red;
            }
        }
        if (power.enginePower == 7)
        {
            if ((targetMovement >= -5 && targetMovement <= 0) || (targetMovement <= 5 && targetMovement >= 0))
            {
                alertText.text = "Safe Raise";
                alertText.color = Color.white;
            }
            if ((targetMovement < -3 && targetMovement >= -5) || (targetMovement > 5 && targetMovement <= 7))
            {
                alertText.text = "Sharp Raise, chance of damage to stability on increase the power";
                alertText.color = Color.yellow;
            }
            if ((targetMovement < -7 && targetMovement > -9) || (targetMovement > 7 && targetMovement < 9))
            {
                alertText.text = "Agressive Raise Need more power, higher chances of damage to stability";
                alertText.color = Color.red;
            }
        }

        
    }
    ///<summary>
    ///It sets the number to raise ship downside
    ///</summary> 
    public void SpaceButton()
    {
        //GOES UPSIDE

        targetMovement += 1f;
        elevationAngle.text = targetMovement.ToString();
        Debug.Log("targetMovement: " + targetMovement);

        if (power.enginePower == 2)
        {
            if (targetMovement <= 2 && targetMovement >= 0 || targetMovement >= -2 && targetMovement <= 0)
            {
                alertText.text = "Safe Raise";
                alertText.color = Color.white;
            }
            if ((targetMovement > 2 && targetMovement <= 4) || (targetMovement < -2 && targetMovement >= -4))
            {
                alertText.text = "Sharp Raise, chance of damage to stability on increase the power";
                alertText.color = Color.yellow;
            }
            if ((targetMovement > 5 && targetMovement < 8) || (targetMovement < -5 && targetMovement > -8))
            {
                alertText.text = "Agressive Raise Need more power, higher chances of damage to stability";
                alertText.color = Color.red;
            }
        }
        if (power.enginePower == 3)
        {
            if ((targetMovement <= 3 && targetMovement >= 0) || (targetMovement >= -3 && targetMovement <= 0))
            {
                alertText.text = "Safe Raise";
                alertText.color = Color.white;
            }
            if ((targetMovement > 3 && targetMovement <= 5) || (targetMovement < -3 && targetMovement >= -5))
            {
                alertText.text = "Sharp Raise, chance of damage to stability on increase the power";
                alertText.color = Color.yellow;
            }
            if ((targetMovement > 6 && targetMovement < 8) || (targetRotAngle < -6 && targetMovement < -8))
            {
                alertText.text = "Agressive Raise Need more power, higher chances of damage to stability";
                alertText.color = Color.red;
            }
        }
        if (power.enginePower == 7)
        {
            if ((targetMovement <= 5 && targetMovement >= 0) || (targetMovement >= -5 && targetMovement <= 0))
            {
                alertText.text = "Safe Raise";
                alertText.color = Color.white;
            }
            if ((targetMovement > 5 && targetMovement <= 7) || (targetMovement < -3 && targetMovement >= -5))
            {
                alertText.text = "Sharp Raise, chance of damage to stability on increase the power";
                alertText.color = Color.yellow;
            }
            if ((targetMovement > 7 && targetMovement < 9) || (targetMovement < -7 && targetMovement > -9))
            {
                alertText.text = "Agressive Raise Need more power, higher chances of damage to stability";
                alertText.color = Color.red;
            }
        }
    }
    ///<summary>
    ///It takes the Inputs X/Space to set Elevation of spaceship
    ///</summary>
    void Elevation() {
        if (Input.GetKeyDown(KeyCode.X))
        {
            XButton();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpaceButton();
        }
    }
    ///<summary>
    ///It sets the number to turn ship Left
    ///</summary> 
    public void TurnLeft()
    {
        //TURN LEFT

        timeInterval1 += Time.deltaTime;
        if (timeInterval1 >= timeDiff1)
        {
            timeInterval1 = 0;
            targetRotAngle = targetRotAngle - 1f;
            turnAngle.text = targetRotAngle.ToString();

            RotAngle = targetRotAngle;
        }

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
            if (targetRotAngle < -60 || targetRotAngle > 60)
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
            if (targetRotAngle < -120 || targetRotAngle > 120)
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
            if (targetRotAngle < -180 || targetRotAngle > 180)
            {
                alertText.text = "Agressive turn Need more power, higher chances of damage to stability";
                alertText.color = Color.red;
            }
        }
    }
    ///<summary>
    ///It sets the number to turn ship right
    ///</summary> 
    public void TurnRight()
    {
        //TURN RIGHT
        timeInterval2 += Time.deltaTime;
        if (timeInterval2 >= timeDiff1)
        {
            timeInterval2 = 0;
            targetRotAngle = targetRotAngle + 1f;
            turnAngle.text = targetRotAngle.ToString();

            RotAngle = targetRotAngle;
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
            if ((targetRotAngle < -60 && targetRotAngle >= -150) || (targetRotAngle > 60 && targetRotAngle <= 150))
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
    ///<summary>
    ///It takes the Inputs A/D to set Bearing or turn angle of spaceship    
    ///</summary>
    void Turn()
    {
        if (Input.GetKey(KeyCode.A))
        {
            TurnLeft();
        }
        if (Input.GetKey(KeyCode.D))
        {
            TurnRight();
        }
    }
    
    float n, n1;
   
    void StartDecrementingAngle()
    {
        n = 0;  // Reset the timer
        n1 = targetRotAngle;  // Update n1 with the current targetRotAngle
    }
    ///<summary>
    ///it decrease the Turn text to 0 slowly
    ///</summary>
    void DecrementAngleOverTime()
    {
        //timeDiff2 = rotSpeed;
        if (n1 > 0)  // Continue decrementing until n1 reaches 0
        {
            n += rotSpeed * Time.deltaTime;
            if (n >= timeDiff2)
            { 
                n = 0;  // Reset the timer
                n1 -= 1;  // Decrease n1
                //targetRotAngle -= 1;  // Decrease targetRotAngle
                turnAngle.text = n1.ToString();  // Update the UI text
                RotAngle = n1;
            }
        }
        if(n1 < 0)
        {
            n += rotSpeed * Time.deltaTime;
            if(n >= timeDiff2)
            {
                n = 0;
                n1 += 1;
                turnAngle.text = n1.ToString();
                RotAngle = n1;
            }
        }
    }


    ///<summary>
    ///Detect the enemy ship by taking transform of enemy if enemy enter some specific radius of Spaceship
    ///</summary>
    void EnemySpaceshipDetection()
    {
        GameManager.Instance.isEnemyDetect = false;
        Collider[] hitCollider = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        foreach (Collider collider in hitCollider)
        {
           // Debug.Log("Enemy Detected: " + collider.gameObject.name);
           // Debug.Log("isEnemyDetect " + GameManager.Instance.isEnemyDetect);
            GameManager.Instance.isEnemyDetect = true;
            GameManager.Instance.detectedEnemy = collider.gameObject;
            //AudioManager.Instance.PlayEnemyAlert();
            
        }

       // GameManager.Instance.isEnemyDetect = false;
    }
    ///<summary>
    ///Detect the Rock ship by taking transform of rock is rock enter some specific radius of Spaceship    ///</summary>
    void RockDetection()
    {
        Collider[] hitCollider = Physics.OverlapSphere(transform.position, detectionRadius, rockLayer);
        foreach (Collider collider in hitCollider)
        {
            Debug.Log("Rock Detected: " + collider.gameObject.name);
            
            //AudioManager.Instance.PlayEnemyAlert();
        }
    }
    ///<summary>
    ///detect the Cloud ship by taking transform of Cloud if Cloud enter some specific  radius of Spaceship
    ///for now it only debug the cloud detection
    ///</summary>

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
    ///<summary>
    ///Sets the line render variables and call draw circle function    ///</summary>
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
        DrawCircle();
    }
    ///<summary>
    ///Draw circle using Line render around the ship    ///</summary>
    
    void DrawCircle()
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
    
    ///<summary>
    ///Set the speed text smoothly increase    ///</summary>
    
    public void SmoothSpeedInc()
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
    ///<summary> Set the speed text smoothly decrease  ///</summary>
    public void SmoothSpeedDec(float endSpeed)
    {
        n += Time.deltaTime;
        if (n >= timeDiff1)
        {
            n = 0;  // Reset the timer
            if (speed > endSpeed)
            {
                //SpaceshipMoveSpeed -= 1;
                speed -= 1f;  // Decrease speed
            }
            else if(speed == endSpeed)
            {
                speed = endSpeed;
               // SpaceshipMoveSpeed = endSpeed;
                isEnterPress = false;
            }
            speedText.text = speed.ToString() + " km/h";  // Update the UI text
        }
    }
    ///<summary>
    ///controlling the speed by taking input from user using W/S keyword///</summary>
    public void SpeedController()
    {
        if (Input.GetKey(KeyCode.W))
        {
            SmoothSpeedInc();
        }
        if (Input.GetKey(KeyCode.S))
        {
            SmoothSpeedDec(SpaceshipMinSpeed);
        }
        #region OLD CODE WITH BEARING ACTIVATION 
        /*
        if (!(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && GameManager.Instance.bActive)
        {
            if (Input.GetKey(KeyCode.W) && !isEnterPress)
            {
                SmoothSpeedInc();
            }
            if (Input.GetKey(KeyCode.S) && !isEnterPress)
            {
                SmoothSpeedDec(5);
            }
        }*/
        #endregion


    }
   
    
}
