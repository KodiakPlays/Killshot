
using UnityEngine;

public class LaserMovement : MonoBehaviour
{
    [SerializeField] float rotateSpeed;// Speed at which the laser rotates
    //[SerializeField] Transform target;
    //float tolerance = 1f;
    //[SerializeField] float enemyDamage;
    [SerializeField] Laser laser;
    [SerializeField] ArcRenderer arcRenderer;
    [SerializeField] ChargeLaser chargeLaserScript;

    float currentYAngle;// Current rotation angle on the Y-axis
    float currentXAngle;// Current rotation angle on the X-axis
   
    private void Awake()
    {

        arcRenderer = gameObject.GetComponent<ArcRenderer>();
        arcRenderer.enabled = false;
        arcRenderer.lineRenderer.enabled = false;

        currentYAngle = this.transform.rotation.eulerAngles.y;
        currentXAngle = this.transform.rotation.eulerAngles.x;
        rotateSpeed = 100f; // Adjust rotateSpeed as needed
    }
    void Update()
    {
        if (GameManager.Instance.lActive && !GameManager.Instance.bActive && !GameManager.Instance.eActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            LaserMove();// Call the method to move the laser
        }
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && GameManager.Instance.lActive)
        {
            //Debug.Log("Press shoot button");

            if(chargeLaserScript != null)
            {

                if (chargeLaserScript.isCharged)
                {
                    //Debug.Log("fire");
                    chargeLaserScript.isCharged = false;
                    Invoke("FireLaser", 0.5f);
                }
            }
            
            
        }
        if (GameManager.Instance.lActive)
        {
            arcRenderer.enabled = true;
            arcRenderer.lineRenderer.enabled = true;
        }
        else
        {
            arcRenderer.enabled = false;
            arcRenderer.lineRenderer.enabled = false;
        }
    }

    /* OLD LASER MOVEMENT CODE
    void LaserMovment()
    {
        currentYAngle = this.transform.rotation.eulerAngles.y;
        currentXAngle = this.transform.rotation.eulerAngles.x;
        rotateSpeed = 100f; // Adjust rotateSpeed as needed

        // Ensure currentYAngle is within -180 to 180 range
        if (currentYAngle > 180)
        {
            currentYAngle -= 360;
        }
        if (currentXAngle > 180)
        {
            currentXAngle -= 360;
        }

        // Rotate the object within the clamped range
        if (Input.GetKey(KeyCode.A) && currentYAngle > -45)
        {
            transform.Rotate(0, -rotateSpeed * Time.deltaTime, 0);
        }
        if (Input.GetKey(KeyCode.D) && currentYAngle < 45)
        {
            transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
        }

        // Rotate the object within the clamped range for X axis
        if (Input.GetKey(KeyCode.W) && currentXAngle > -15)
        {
            transform.Rotate(-rotateSpeed * Time.deltaTime, 0, 0);
        }
        if (Input.GetKey(KeyCode.S) && currentXAngle < 15)
        {
            transform.Rotate(rotateSpeed * Time.deltaTime, 0, 0);
        }

        // Ensure the angles remain clamped
        currentYAngle = transform.rotation.eulerAngles.y;
        currentXAngle = transform.rotation.eulerAngles.x;

        if (currentYAngle > 180)
        {
            currentYAngle -= 360;
        }

        if (currentXAngle > 180)
        {
            currentXAngle -= 360;
        }

        if (currentYAngle < -45)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, -45, transform.rotation.eulerAngles.z);
        }
        else if (currentYAngle > 45)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 45, transform.rotation.eulerAngles.z);
        }

        if (currentXAngle < -15)
        {
            transform.rotation = Quaternion.Euler(-15, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
        else if (currentXAngle > 15)
        {
            transform.rotation = Quaternion.Euler(15, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
    }
    */
    void LaserMove()
    {
        /// Update angles based on input
        if (Input.GetKey(KeyCode.F))
        {
            currentYAngle -= rotateSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.H))
        {
            currentYAngle += rotateSpeed * Time.deltaTime;
        }
        

        // Clamp the angles
        currentYAngle = Mathf.Clamp(currentYAngle, -45f, 45f);

        // Apply the clamped rotation directly
        transform.localRotation = Quaternion.Euler(currentXAngle, currentYAngle, 0f);
    }
    void FireLaser()
    {
        Debug.Log("Fire from laser movement");
        laser.FireLaser();
        GameManager.Instance.lActive = false;
        GameManager.Instance.lGreenImage.SetActive(false);
        GameManager.Instance.lRedImage.SetActive(true);

    }
    
}
