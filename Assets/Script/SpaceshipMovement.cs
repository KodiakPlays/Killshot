using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SpaceshipMovement : MonoBehaviour
{
    Vector3 rotationAngle;
    Vector3 upDownAngle;

    [SerializeField] float SpaceshipMoveSpeed = 50f;

    [SerializeField] TextMeshProUGUI turnAngle;
    [SerializeField] TextMeshProUGUI elevationAngle;

    [SerializeField] float detectionRadius;
    [SerializeField] LayerMask enemyLayer;

    [SerializeField] float power;
    [SerializeField] TextMeshProUGUI powerText;

    //public Enemy[] enemy;
    // Update is called once per frame
    private void Start()
    {
        power = SpaceshipMoveSpeed;

        //enemy = GetComponent<Enemy>();
    }
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
            if(transform.position.y <= 50 && transform.position.y >= -50)
            {
                if (Input.GetKeyDown(KeyCode.W))
                {
                    //GOES UPSIDE
                    upDownAngle += new Vector3(0, 10f, 0);
                    Debug.Log("RaiseAmount " + upDownAngle);
                    elevationAngle.text = upDownAngle.y.ToString();

                }
                if (Input.GetKeyDown(KeyCode.S))
                {
                    //GOES DOWNSIDE
                    upDownAngle += new Vector3(0, -10f, 0);
                    Debug.Log("RaiseAmount " + upDownAngle);
                    elevationAngle.text = upDownAngle.y.ToString();
                }
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
        //Quaternion Currentpos;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Vector3 currentRot = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z);
            Vector3 currentPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            Debug.Log("Enter key");
            //------------------RIGHT LEFT TURN--------------------
            transform.Rotate(currentRot + rotationAngle);
            /*
            Quaternion targetRotationLR = Quaternion.Euler(transform.rotation.eulerAngles + rotationAngle);
            // transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotationLR, 1000 * Time.deltaTime);

            float timer = 0f;
            float alpha = 0f;
            while (alpha <= 1)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotationLR, alpha);
                alpha = timer * 0.01f;
                timer += Time.deltaTime;
            }
            */
            rotationAngle = new Vector3(0, 0, 0);
            turnAngle.text = "0";

            //------------------UP DOWN TURN--------------------
            // transform.Rotate(currentpos + upDownAngle);
            if (transform.position.y <= 50 && transform.position.y >= -50)
            {
                transform.position = currentPos + upDownAngle;
                //transform.position = Vector3.MoveTowards(currentPos, currentPos + upDownAngle, 25 * Time.deltaTime);
            }
            else if(transform.position.y > 50)
            {
                transform.position = new Vector3(transform.position.x, 50, transform.position.z);
            }
            else if(transform.position.y < -50)
            {
                transform.position = new Vector3(transform.position.x, -50, transform.position.z);
            }
            
            //transform.position = currentPos + upDownAngle;
            /*
            //Quaternion targetRotationUD = Quaternion.LookRotation(transform.rotation.eulerAngles + upDownAngle);
            //transform.rotation = Quaternion.Lerp(Currentpos, targetRotationUD, 2 * Time.deltaTime);
            */
            upDownAngle = new Vector3(0, 0, 0);
            elevationAngle.text = "0";

            //ADD POWER TO THE SPEED
            SpaceshipMoveSpeed = power;
        }
    }
    void EnemySpaseshipDetection()
    {
        Collider[] hitCollider = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        foreach (Collider collider in hitCollider)
        {
            Debug.Log("Enemy Detected: " + collider.gameObject.name);
            //GameManager.Instance.isEnemyDetect = true;
            collider.gameObject.GetComponent<Enemy>().enabled = true;
        }
    }

    public void PowerInc()
    {
        if (GameManager.Instance.pActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (power <= 5)
            {
                power += 0.2f;
            }
            else
            {
                power = 5f;
            }
            powerText.text = power.ToString();
        }

        

    }
    public void PowerDec()
    {
        if (GameManager.Instance.pActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (power >= 0.5f)
            {
                power -= 0.2f;
            }
            else
            {
                power = 0.5f;
            }
            powerText.text = power.ToString();
        }
       

    }
}
