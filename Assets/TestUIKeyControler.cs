using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class TestUIKeyControler : MonoBehaviour
{
    public UIController uiContoller;
    public Image gridBK;
    public Material gridMat;
    [SerializeField] private Shader gridShad;
    [SerializeField] private Transform cameraTransform; // Reference to camera for compass rotation
    float rotation = 0;
    float movmentSpeed = 100;
    private float lastCameraRotation = 0f; // Track last camera rotation to detect changes

    void Start()
    {
        gridMat = new Material(gridShad);
        gridBK.GetComponent<Image>().material = gridMat;

        gridMat.SetFloat("_SpeedMovement", rotation);
        gridMat.SetFloat("_SpeedRotation", movmentSpeed);
        
        // Auto-assign camera if not set
        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
        }
        
        // Initialize last camera rotation
        if (cameraTransform != null)
        {
            lastCameraRotation = cameraTransform.eulerAngles.y;
        }
    }

    void Update()
    {
        // Get PlayerShip reference for actual values
        PlayerShip playerShip = FindFirstObjectByType<PlayerShip>();
        
        //increase speed
        if (Input.GetKey(KeyCode.W))
        {
            gridMat.SetFloat("_SpeedMovement", (++movmentSpeed / 100));

            // Get actual speed from PlayerShip's Rigidbody if available
            float actualSpeed = 0f;
            if (playerShip != null)
            {
                Rigidbody rb = playerShip.GetComponent<Rigidbody>();
                actualSpeed = rb != null ? rb.linearVelocity.magnitude : 0f;
            }
            uiContoller.updateSpeedometer(actualSpeed);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            gridMat.SetFloat("_SpeedMovement", (--movmentSpeed / 100));

            // Get actual speed from PlayerShip's Rigidbody if available (negative for reverse)
            float actualSpeed = 0f;
            if (playerShip != null)
            {
                Rigidbody rb = playerShip.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Check if moving backward by comparing velocity direction with forward direction
                    float forwardDot = Vector3.Dot(rb.linearVelocity.normalized, playerShip.transform.forward);
                    actualSpeed = rb.linearVelocity.magnitude * (forwardDot < 0 ? -1 : 1);
                }
            }
            uiContoller.updateSpeedometer(actualSpeed);
        }

        //bring speedometer back to 0 or show current speed
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))
        {
            // Show current actual speed instead of just 0
            float currentSpeed = 0f;
            if (playerShip != null)
            {
                Rigidbody rb = playerShip.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    float forwardDot = Vector3.Dot(rb.linearVelocity.normalized, playerShip.transform.forward);
                    currentSpeed = rb.linearVelocity.magnitude * (forwardDot < 0 ? -1 : 1);
                }
            }
            uiContoller.updateSpeedometer(currentSpeed);
        }

        //ship rotates - but compass only updates when camera rotates
        if (Input.GetKey(KeyCode.A))
        {
            gridMat.SetFloat("_SpeedRotation", (++rotation / 100));
            // Compass update removed - now handled by camera rotation tracking
        }
        if (Input.GetKey(KeyCode.D))
        {
            gridMat.SetFloat("_SpeedRotation", (--rotation / 100));
            // Compass update removed - now handled by camera rotation tracking
        }

        // Update compass based on camera rotation changes
        if (cameraTransform != null)
        {
            float currentCameraRotation = cameraTransform.eulerAngles.y;
            
            // Check if camera rotation has changed
            if (Mathf.Abs(Mathf.DeltaAngle(lastCameraRotation, currentCameraRotation)) > 0.1f)
            {
                // Camera has rotated, update compass
                uiContoller.updateCompass(currentCameraRotation);
                lastCameraRotation = currentCameraRotation;
            }
        }

        //ship gets hit/takes dmg effect
        if (Input.GetKeyDown(KeyCode.B))
        {
            uiContoller.updateShipHit(1f);
            
            // Apply damage to PlayerShip if available
            if (playerShip != null)
            {
                playerShip.TakeDamage(10f); // Test damage amount
            }
        }

        // Test dodge mechanics
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Left dodge - PlayerShip handles this input internally
            // UI could show dodge indicator here
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            // Right dodge - PlayerShip handles this input internally
            // UI could show dodge indicator here
        }

        // Test power management systems (PlayerShip handles these inputs internally)
        // Alpha1, Alpha2, Alpha3 are used by PlayerShip for power management
        // But we can still update UI to show power states
        if (Input.GetKeyDown(KeyCode.Alpha1) && playerShip != null)
        {
            // Engine power toggle - could update UI to show engine status
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && playerShip != null)
        {
            // Weapon power toggle - could update UI to show weapon status
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && playerShip != null)
        {
            // Sensor power toggle - could update UI to show sensor status
        }

        //load in sensor info (using different keys to avoid conflict with power management)
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            uiContoller.updateCompass(false, 3, 0, 0, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))//asteroid
        {
            uiContoller.updateCompass(true, 0, 25, 25, 25, 25);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))//orbiter
        {
            uiContoller.updateCompass(true, 1, 50, 50, 50, 50);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))//enemy ship
        {
            uiContoller.updateCompass(true, 2, 100, 100, 100, 100);
        }
    }
}
