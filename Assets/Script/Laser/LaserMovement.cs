
using UnityEngine;

public class LaserMovement : MonoBehaviour
{
    [SerializeField] float rotateSpeed;
    [SerializeField] Transform target;
    float tolerance = 1f;
    [SerializeField] Laser laser;
    [SerializeField] float enemyDamage;
    [SerializeField] ArcRenderer arcRenderer;
    [SerializeField] ChargeLaser chargeLaserScript;
    private void Awake()
    {
        arcRenderer = gameObject.GetComponent<ArcRenderer>();
        arcRenderer.enabled = false;
        arcRenderer.lineRenderer.enabled = false;
    }
    void Update()
    {
        if (GameManager.Instance.lActive && !GameManager.Instance.bActive && !GameManager.Instance.eActive && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            LaserMovment();
        }
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && GameManager.Instance.lActive)//
        {
            if (chargeLaserScript.isCharged)
            {
                chargeLaserScript.isCharged = false;
                Invoke("FireLaser", 0.5f);
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
    void LaserMovment()
    {
        float currentYAngle = transform.rotation.eulerAngles.y;
        float currentXAngle = transform.rotation.eulerAngles.x;
        float rotateSpeed = 100f; // Adjust rotateSpeed as needed

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
    void FireLaser()
    {
        Debug.Log("Fire from laser movement");
        laser.FireLaser();
        GameManager.Instance.lActive = false;
        GameManager.Instance.lGreenImage.SetActive(false);
        GameManager.Instance.lRedImage.SetActive(true);

        /*
        laser.FireLaser(transform.forward * laser.Distance);
        RaycastHit hit;
        Vector3 laserOrigin = laser.transform.position;
        Vector3 laserDirection = transform.forward;
        float laserDistance = laser.Distance;
        int layerMask = 3;

        if (Physics.Raycast(laserOrigin, laserDirection, out hit, laserDistance, layerMask))
        {
            Debug.Log($"Raycast hit: {hit.transform.name} at {hit.point}");

            if (hit.transform.CompareTag("Enemy"))
            {
                Debug.Log("hit enetity " + hit.transform.name);
                Destroy(hit.transform.gameObject);
            }
        }
        */
    }
    
    //void AddDamage()
    //{
    //    RaycastHit hit;
    //    if (Physics.Raycast(laser.transform.position, transform.forward, out hit, laser.Distance))
    //    {
    //        if (hit.transform.CompareTag("Enemy"))
    //        {
    //            hitPosition = hit.transform.position;
    //            damageable = hit.transform.gameObject.GetComponent<Damageable>();
    //            Debug.Log("hit enetity " + hit.transform.name);
    //            Destroy(hit.transform.gameObject);
    //            // enemyObj = hit.transform.gameObject;
    //        }
    //    }


    //}
    //void CanDamage()
    //{
    //    isDamage = false;
    //}
}
