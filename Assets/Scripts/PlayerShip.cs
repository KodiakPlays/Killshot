using UnityEngine;

[RequireComponent(typeof(ShipStability))]
public class PlayerShip : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float rateOfAcceleration = 50f; // Speed change per second
    [SerializeField] private float turnRate = 90f; // Degrees per second (1 decimal place precision)
    [SerializeField] private float emergencyDodgeForce = 500f; // Force applied for emergency dodge

    [Header("Camera Settings")]
    [SerializeField] private float cameraRotationSpeed = 45f; // How fast camera follows ship rotation
    [SerializeField] private float cameraRotationDelay = 2f; // Delay before camera starts rotating after ship stops turning
    [SerializeField] private Transform cameraTransform; // Reference to the camera transform
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 10, -15); // Camera offset from ship

    // Systems
    private PowerManager powerManager;
    private ShipStability stability;
    private Weapons weapons;
    
    // Movement state
    private float currentSpeed; // Current forward/backward speed
    private float targetSpeed; // Target speed based on input
    private float currentTurnSpeed; // Current turning speed
    private float targetTurnSpeed; // Target turning speed based on input
    
    // Camera rotation state
    private float currentCameraRotation; // Current camera Y rotation
    private float targetCameraRotation; // Target camera Y rotation (follows ship)
    private Vector3 targetCameraPosition; // Target camera position
    private float lastTurnTime; // Time when ship last turned
    private bool isShipTurning; // Whether ship is currently turning
    
    // Constants
    private const float SPEED_UNITS_PER_BAR = 20f; // Each bar represents 20 units of speed

	[Header("Health Settings")]
	[SerializeField] private float maxHealth = 100f;
	[SerializeField] private float currentHealth;

	[Header("Testing Overrides")]
	[SerializeField] private bool testMode_IgnoreWeaponPower = false;

	private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.linearDamping = 0;
            rb.angularDamping = 0;
        }
        
        currentHealth = maxHealth;
        
        // Get system references
        powerManager = GetComponent<PowerManager>();
        if (powerManager == null)
        {
            powerManager = gameObject.AddComponent<PowerManager>();
        }

        stability = GetComponent<ShipStability>();
        if (stability == null)
        {
            stability = gameObject.AddComponent<ShipStability>();
        }

        weapons = GetComponent<Weapons>();
        if (weapons == null)
        {
            weapons = gameObject.AddComponent<Weapons>();
        }

        // Initialize camera reference if not set
        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
                Debug.Log("PlayerShip: Automatically found and assigned Main Camera for ship following");
            }
            else
            {
                Debug.LogWarning("PlayerShip: No Main Camera found. Please assign Camera Transform manually in inspector for camera following to work.");
            }
        }

        // Initialize camera rotation to match ship (only if camera is separate)
        if (cameraTransform != null)
        {
            // Ensure camera is not a child of this ship
            if (cameraTransform.IsChildOf(transform))
            {
                Debug.LogError("PlayerShip: Camera should NOT be a child of the ship for proper camera following. Please make camera a separate GameObject.");
            }
            
            currentCameraRotation = cameraTransform.eulerAngles.y;
            targetCameraRotation = transform.eulerAngles.y;
            
            // Initialize camera position
            targetCameraPosition = transform.position + cameraOffset;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        if (currentHealth <= 0)
        {
            // Handle player death
            Destroy(gameObject);
        }
    }

    // Public methods for UI/external systems
    public float GetCurrentSpeedInBars()
    {
        return currentSpeed / SPEED_UNITS_PER_BAR;
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public float GetTurnRate()
    {
        return turnRate;
    }

    public void SetCameraTransform(Transform camera)
    {
        cameraTransform = camera;
        if (cameraTransform != null)
        {
            currentCameraRotation = cameraTransform.eulerAngles.y;
            targetCameraRotation = transform.eulerAngles.y;
            targetCameraPosition = transform.position + cameraOffset;
        }
    }

    private void Update()
    {
        // Handle power input
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Engine power
        {
            powerManager.ToggleSystemState(powerManager.engines);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) // Weapon power
        {
            powerManager.ToggleSystemState(powerManager.weapons);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) // Sensor power
        {
            powerManager.ToggleSystemState(powerManager.sensors);
        }

        // Handle dodge input - Q goes left, E goes right
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryDodge(Vector2.left);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            TryDodge(Vector2.right);
        }

        // Handle weapon firing
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftControl))
        {
            TryFireWeapons();
        }

        // Testing controls
        if (Input.GetKeyDown(KeyCode.F2))
        {
            testMode_IgnoreWeaponPower = !testMode_IgnoreWeaponPower;
            Debug.Log($"Weapon Power Override: {(testMode_IgnoreWeaponPower ? "ENABLED" : "DISABLED")}");
        }
    }

    private void TryDodge(Vector2 direction)
    {
        if (stability.CanPerformDodge())
        {
            stability.ApplyDodge();
            
            // Dodge is a lateral shift without changing facing direction
            // Convert 2D direction to 3D world space relative to ship's orientation
            Vector3 worldDirection = transform.right * direction.x + transform.forward * direction.y;
            Vector3 dodgeForceVector = worldDirection * emergencyDodgeForce;
            
            // Apply as an impulse force for immediate lateral movement
            rb.AddForce(dodgeForceVector, ForceMode.VelocityChange);
            
            Debug.Log($"Emergency dodge {(direction.x < 0 ? "LEFT" : "RIGHT")} - Stability: {stability.GetCurrentStability():F1}%");
        }
        else
        {
            Debug.Log("Cannot dodge: " + (stability.CanDodgeAgain() ? "Insufficient stability" : "Dodge on cooldown"));
        }
    }

    private void TryFireWeapons()
    {
        if (weapons != null)
        {
            float weaponPower = powerManager.GetSystemEfficiency("weapons");
            
            // Testing override - allow firing without power
            if (testMode_IgnoreWeaponPower)
            {
                weapons.TryFire(1.0f); // Use maximum power for testing
                return;
            }
            
            // Normal power check - can only fire if weapons have some power (minimum 20% for emergency firing)
            if (weaponPower > 0.2f)
            {
                weapons.TryFire(weaponPower);
            }
            else
            {
                Debug.Log("Insufficient weapon power to fire! (Use testing override if needed)");
            }
        }
    }

    private void FixedUpdate()
    {
        float enginePower = powerManager.GetSystemEfficiency("engines");
        
        // Ensure minimum engine power for basic movement (emergency power)
        enginePower = Mathf.Max(0.2f, enginePower);
        
        // Handle W/S input for thrust - W accelerates, S decelerates/reverses
        float thrustInput = 0f;
        
        if (Input.GetKey(KeyCode.W))
        {
            // W accelerates forward
            thrustInput = 1f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            // S decelerates or accelerates backward
            thrustInput = -1f;
        }
        // No input = maintain current speed (no deceleration)
        
        if (Mathf.Abs(thrustInput) > 0.01f)
        {
            // Calculate target speed based on thrust direction
            float maxThrust = 100f * enginePower; // Engine power affects max thrust
            
            if (thrustInput > 0)
            {
                // W pressed - accelerate forward
                targetSpeed = maxThrust;
            }
            else
            {
                // S pressed - decelerate or go reverse
                if (currentSpeed > 0)
                {
                    // Currently moving forward, decelerate toward 0 then continue to reverse
                    targetSpeed = -maxThrust;
                }
                else
                {
                    // Already at 0 or moving backward, accelerate backward
                    targetSpeed = -maxThrust;
                }
            }
        }
        else
        {
            // No thrust input - maintain current speed (don't change targetSpeed)
            targetSpeed = currentSpeed;
        }
        
        // Gradually change current speed towards target speed
        float speedChangeRate = rateOfAcceleration * Time.fixedDeltaTime;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChangeRate);
        
        // Apply forward/backward movement
        rb.linearVelocity = transform.forward * currentSpeed;

        // Handle A/D input for turning - ship turns immediately, camera follows gradually
        float turnInput = Input.GetAxis("Horizontal");
        bool wasTurning = isShipTurning;
        isShipTurning = Mathf.Abs(turnInput) > 0.01f && !stability.IsStabilityDepleted();
        
        if (isShipTurning)
        {
            // A turns left (negative), D turns right (positive)
            // Turn rate is affected by engine power
            targetTurnSpeed = turnInput * turnRate * enginePower;
            lastTurnTime = Time.time; // Update last turn time while turning
        }
        else
        {
            // No input - gradually stop turning
            targetTurnSpeed = 0f;
            
            // If we just stopped turning, record the time
            if (wasTurning && !isShipTurning)
            {
                lastTurnTime = Time.time;
            }
        }
        
        // Gradually change current turn speed towards target - this creates momentum
        float turnAcceleration = turnRate * 2f * Time.fixedDeltaTime; // How quickly we reach target turn speed
        currentTurnSpeed = Mathf.MoveTowards(currentTurnSpeed, targetTurnSpeed, turnAcceleration);
        
        // Apply ship rotation immediately and calculate stability drain
        if (Mathf.Abs(currentTurnSpeed) > 0.01f)
        {
            float rotationThisFrame = currentTurnSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, rotationThisFrame, 0));
            
            // Update target camera rotation to follow ship
            targetCameraRotation = transform.eulerAngles.y;
            
            // Calculate stability drain based on turn severity and current speed
            stability.CalculateTurnStabilityDrain(Mathf.Abs(rotationThisFrame), Mathf.Abs(currentSpeed));
        }

        // Camera follows ship continuously without damping
        if (cameraTransform != null)
        {
            // Update target camera position to follow ship
            targetCameraPosition = transform.position + cameraOffset;
            
            // Follow position immediately without damping
            cameraTransform.position = targetCameraPosition;
            
            // Check if enough time has passed since ship stopped turning
            bool shouldRotateCamera = !isShipTurning && (Time.time - lastTurnTime) >= cameraRotationDelay;
            
            if (shouldRotateCamera)
            {
                // Calculate the shortest angular distance for rotation
                float angleDifference = Mathf.DeltaAngle(currentCameraRotation, targetCameraRotation);
                
                // Gradually rotate camera towards target
                float cameraRotationStep = cameraRotationSpeed * Time.fixedDeltaTime;
                if (Mathf.Abs(angleDifference) > cameraRotationStep)
                {
                    currentCameraRotation += Mathf.Sign(angleDifference) * cameraRotationStep;
                }
                else
                {
                    currentCameraRotation = targetCameraRotation;
                }
                
                // Apply camera rotation
                Vector3 cameraEuler = cameraTransform.eulerAngles;
                cameraEuler.y = currentCameraRotation;
                cameraTransform.eulerAngles = cameraEuler;
            }
        }

        // Damage system disabled for now
        // Apply damage if stability is critical
        // if (stability.IsStabilityCritical() && Time.frameCount % 30 == 0) // Check every 30 frames
        // {
        //     TakeDamage(1);
        // }
        
        // Debug info (remove this later)
        if (Input.GetKey(KeyCode.F1))
        {
            float speedBars = currentSpeed / SPEED_UNITS_PER_BAR;
            float stabilityPercent = stability.GetStabilityPercentage() * 100f;
            float timeSinceLastTurn = Time.time - lastTurnTime;
            bool shouldRotateCamera = !isShipTurning && timeSinceLastTurn >= cameraRotationDelay;
            
            Debug.Log($"=== MOVEMENT DEBUG ===");
            Debug.Log($"Engine Power: {enginePower:F2} | Speed: {currentSpeed:F1} ({speedBars:F1} bars) | Target: {targetSpeed:F1}");
            Debug.Log($"Turn Speed: {currentTurnSpeed:F1}°/s | Target: {targetTurnSpeed:F1}°/s | Max Turn Rate: {turnRate:F1}°/s");
            Debug.Log($"Ship Rotation: {transform.eulerAngles.y:F1}° | Camera: {currentCameraRotation:F1}° | Target: {targetCameraRotation:F1}°");
            Debug.Log($"Ship Turning: {isShipTurning} | Time Since Turn: {timeSinceLastTurn:F1}s | Should Rotate Camera: {shouldRotateCamera}");
            Debug.Log($"Ship Position: {transform.position} | Camera Position: {(cameraTransform != null ? cameraTransform.position.ToString() : "No Camera")}");
            Debug.Log($"Stability: {stabilityPercent:F1}% | Can Dodge: {stability.CanDodgeAgain()} | Critical: {stability.IsStabilityCritical()}");
            Debug.Log($"Weapon Power: {powerManager.GetSystemEfficiency("weapons"):F2} | Ammo: {(weapons != null ? weapons.GetCurrentAmmo() : 0)} | Override: {testMode_IgnoreWeaponPower}");
        }
    }
}
