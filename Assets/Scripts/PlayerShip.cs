using UnityEngine;

[RequireComponent(typeof(ShipStability))]
public class PlayerShip : MonoBehaviour
{
    [Header("Script Refrence")]
    public UIController uiControler;

    [Header("Movement Settings")]
    [SerializeField] private float rateOfAcceleration = 50f; // Speed change per second
    [SerializeField] public float turnRate = 90f; // Degrees per second (1 decimal place precision)
    [SerializeField] private float dodgeDistance = 10f; // Distance to dodge left/right
    [SerializeField] private float dodgeDuration = 0.2f; // Time to complete dodge (quick but smooth)

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform; // Reference to the camera transform
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 10, -15); // Camera offset from ship

    // Systems
    private PowerManager powerManager;
    private ShipStability stability;
    private WeaponManager weaponManager;
    
    // Movement state
    private float currentSpeed; // Current forward/backward speed
    public float targetSpeed; // Target speed based on input
    private float currentTurnSpeed; // Current turning speed
    private float targetTurnSpeed; // Target turning speed based on input
    
    // Camera state
    private Vector3 targetCameraPosition; // Target camera position
    
    // Dodge state
    private bool isDodging; // Whether ship is currently dodging
    private float dodgeStartTime; // When dodge started
    private Vector3 dodgeStartPosition; // Position when dodge started
    private Vector3 dodgeTargetPosition; // Target position for dodge
    
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
            rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
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

        weaponManager = GetComponent<WeaponManager>();
        if (weaponManager == null)
        {
            weaponManager = gameObject.AddComponent<WeaponManager>();
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
            TryDodge(-1f); // Left dodge
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            TryDodge(1f); // Right dodge
        }

        // Handle weapon firing
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftControl))
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

    private void TryDodge(float direction)
    {
        if (stability.CanPerformDodge() && !isDodging)
        {
            stability.ApplyDodge();
            
            // Dodge is a pure lateral shift (left/right) without changing facing direction
            // direction: -1 for left, +1 for right
            isDodging = true;
            dodgeStartTime = Time.time;
            dodgeStartPosition = transform.position;
            
            // Calculate target position based on ship's right vector
            Vector3 dodgeOffset = transform.right * direction * dodgeDistance;
            dodgeTargetPosition = dodgeStartPosition + dodgeOffset;
            
            Debug.Log($"Emergency dodge {(direction < 0 ? "LEFT" : "RIGHT")} - Stability: {stability.GetCurrentStability():F1}%");
        }
        else if (isDodging)
        {
            Debug.Log("Cannot dodge: Already dodging");
        }
        else
        {
            Debug.Log("Cannot dodge: " + (stability.CanDodgeAgain() ? "Insufficient stability" : "Dodge on cooldown"));
        }
    }

    private void TryFireWeapons()
    {
        if (weaponManager != null)
        {
            float weaponPower = powerManager.GetSystemEfficiency("weapons");
            
            // Testing override - allow firing without power
            if (testMode_IgnoreWeaponPower)
            {
                weaponManager.FireActiveWeapon(transform.position + transform.forward * 1000f, 1.0f); // Use maximum power for testing
                return;
            }
            
            // Normal power check - can only fire if weapons have some power (minimum 20% for emergency firing)
            if (weaponPower > 0.2f)
            {
                weaponManager.FireActiveWeapon(transform.position + transform.forward * 1000f, weaponPower);
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
        
        // Handle dodge movement (smooth interpolation)
        if (isDodging)
        {
            float elapsedTime = Time.time - dodgeStartTime;
            float t = Mathf.Clamp01(elapsedTime / dodgeDuration);
            
            // Use smooth interpolation for natural movement
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            Vector3 dodgePosition = Vector3.Lerp(dodgeStartPosition, dodgeTargetPosition, smoothT);
            
            // Apply dodge position while maintaining forward velocity
            rb.MovePosition(dodgePosition);
            
            // Check if dodge is complete
            if (t >= 1f)
            {
                isDodging = false;              // Apply forward movement velocity after dodge completes
                rb.linearVelocity = transform.up * currentSpeed;
            }
        }
        else
        {
            // Apply normal forward/backward movement
            rb.linearVelocity = transform.up * currentSpeed;
        }

        // Handle A/D input for turning
        float turnInput = Input.GetAxis("Horizontal");
        
        if (Mathf.Abs(turnInput) > 0.01f && !stability.IsStabilityDepleted())
        {
            // A turns left (negative), D turns right (positive)
            // Turn rate is affected by engine power
            targetTurnSpeed = turnInput * turnRate * enginePower;
        }
        else
        {
            // No input - gradually stop turning
            targetTurnSpeed = 0f;
        }
        
        // Gradually change current turn speed towards target - this creates momentum
        float turnAcceleration = turnRate * 2f * Time.fixedDeltaTime; // How quickly we reach target turn speed
        currentTurnSpeed = Mathf.MoveTowards(currentTurnSpeed, targetTurnSpeed, turnAcceleration);
        
        // Apply ship rotation and calculate stability drain
        if (Mathf.Abs(currentTurnSpeed) > 0.01f)
        {
            float rotationThisFrame = currentTurnSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, 0, -rotationThisFrame));
            
            // Calculate stability drain based on turn severity and current speed
            stability.CalculateTurnStabilityDrain(Mathf.Abs(rotationThisFrame), Mathf.Abs(currentSpeed));
        }

        // Camera locked to ship - follows position and rotation immediately
        if (cameraTransform != null)
        {
            // Update target camera position to follow ship
            targetCameraPosition = transform.position + cameraOffset;
            
            // Follow position immediately
            cameraTransform.position = targetCameraPosition;
            
            // Lock camera rotation to ship rotation immediately
            Vector3 cameraEuler = cameraTransform.eulerAngles;
            cameraEuler.z = transform.eulerAngles.z;
            cameraTransform.eulerAngles = cameraEuler;

            uiControler.UpdateCompass(cameraEuler.z);
            uiControler.WorldGridRotUpdate(cameraEuler.z);
            uiControler.WorldGridLocUpdate(cameraTransform.position);
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
            
            Debug.Log($"=== MOVEMENT DEBUG ===");
            Debug.Log($"Engine Power: {enginePower:F2} | Speed: {currentSpeed:F1} ({speedBars:F1} bars) | Target: {targetSpeed:F1}");
            Debug.Log($"Turn Speed: {currentTurnSpeed:F1}°/s | Target: {targetTurnSpeed:F1}°/s | Max Turn Rate: {turnRate:F1}°/s");
            Debug.Log($"Ship Rotation: {transform.eulerAngles.z:F1}° | Camera Rotation: {(cameraTransform != null ? cameraTransform.eulerAngles.z.ToString("F1") : "No Camera")}°");
            Debug.Log($"Ship Position: {transform.position} | Camera Position: {(cameraTransform != null ? cameraTransform.position.ToString() : "No Camera")}");
            Debug.Log($"Stability: {stabilityPercent:F1}% | Can Dodge: {stability.CanDodgeAgain()} | Critical: {stability.IsStabilityCritical()}");
            Debug.Log($"Weapon Power: {powerManager.GetSystemEfficiency("weapons"):F2} | Active Weapon: {(weaponManager != null ? weaponManager.GetActiveWeaponType().ToString() : "None")} | Override: {testMode_IgnoreWeaponPower}");
        }
    }
}
