using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(ShipStability))]
public class PlayerShip : MonoBehaviour, IDamageable
{
    [Header("Movement Settings")]
    [SerializeField] private float rateOfAcceleration = 50f; // Speed change per second
    [SerializeField] public float turnRate = 90f; // Degrees per second (1 decimal place precision)
    [SerializeField] private float dodgeDistance = 10f; // Distance to dodge left/right
    [SerializeField] private float dodgeDuration = 0.2f; // Time to complete dodge (quick but smooth)

    [Header("Engine Power Settings")]
    [SerializeField] private float enginePowerDrainRate = 0.2f; // Engine power bars drained per second at full speed (100 units/s)

    [Header("Hyperspeed Settings")]
    [SerializeField] private float hyperspeedEngineThreshold = 0.8f;  // Min engine efficiency (0-1) required to engage hyperspeed
    [SerializeField] private float hyperspeedSpeedMultiplier = 4f;    // Speed multiplier while in hyperspeed
    [SerializeField] private float hyperspeedDrainRate = 2f;          // Engine bars drained per second (exceeds reactor regen)

    [Header("Drift Settings")]
    [SerializeField] private float driftCompensationRate = 8f;      // Lateral correction strength (units/s), scaled by stability ratio
    [SerializeField] private float maxLateralDrift = 40f;            // Maximum lateral drift speed (units/s)
    [SerializeField] private float driftStabilityDrainRate = 0.4f;  // Stability drained per unit of lateral speed per second

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform; // Reference to the camera transform
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 10, -15); // Camera offset from ship

    // Systems
    private PowerManager powerManager;
    private ShipStability stability;
    private WeaponManager weaponManager;
    private HullSystem hullSystem;
    private InternalSubsystems internalSubsystems;
    
    // Movement state
    private float currentSpeed; // Current forward/backward speed
    public float targetSpeed; // Target speed based on input
    private float currentTurnSpeed; // Current turning speed
    private float targetTurnSpeed; // Target turning speed based on input
    
    // Camera state
    private Vector3 targetCameraPosition; // Target camera position
    
    // Engine drain accumulator
    private float engineDrainAccumulator = 0f;

    // Hyperspeed state
    private bool isInHyperspeed = false;
    private float hyperspeedDrainAccumulator = 0f;

    // Dodge state
    private bool isDodging; // Whether ship is currently dodging
    private float dodgeStartTime; // When dodge started
    private Vector3 dodgeStartPosition; // Position when dodge started
    private Vector3 dodgeTargetPosition; // Target position for dodge
    
    // Constants
    private const float SPEED_UNITS_PER_BAR = 20f; // Each bar represents 20 units of speed

	[Header("Testing Overrides")]
	[SerializeField] private bool testMode_IgnoreWeaponPower = false;

	private Rigidbody rb;

    // Controller d-pad edge detection
    private bool _dpadUpPrev    = false;
    private bool _dpadDownPrev  = false;
    private bool _dpadLeftPrev  = false;
    private bool _dpadRightPrev = false;

    // Railgun standby state
    public bool IsOnStandby { get; private set; }

    // Hyperspeed state
    public bool IsInHyperspeed => isInHyperspeed;

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

        hullSystem = GetComponent<HullSystem>();
        if (hullSystem == null)
        {
            hullSystem = gameObject.AddComponent<HullSystem>();
        }

        internalSubsystems = GetComponent<InternalSubsystems>();
        if (internalSubsystems == null)
        {
            internalSubsystems = gameObject.AddComponent<InternalSubsystems>();
        }

        // Subscribe to ship destruction
        hullSystem.OnShipDestroyed += HandleShipDestroyed;

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

    /// <summary>
    /// Take directional hull damage per GDD spec.
    /// Damage hits the quadrant determined by the hit direction.
    /// </summary>
    public void TakeDamage(float damage, Vector3 hitDirection)
    {
        if (hullSystem == null) return;
        HullSide side = hullSystem.DetermineHitSide(hitDirection);
        hullSystem.TakeDamage(side, damage);
        ControllerHaptics.TookDamage();
    }

    /// <summary>
    /// Legacy overload - damage hits Prow by default.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (hullSystem != null)
        {
            hullSystem.TakeDamage(HullSide.Prow, damage);
        }
    }

    // IDamageable implementation
    float IDamageable.TakeDamage(float amount)
    {
        TakeDamage(amount); // Delegates to the Prow-default overload
        return amount;
    }

    public float GetCurrentHealth()
    {
        return hullSystem != null ? hullSystem.GetTotalHealth() : 0f;
    }

    public float GetMaxHealth()
    {
        return hullSystem != null ? hullSystem.GetTotalMaxHealth() : 150f;
    }

    public bool CanBeDamaged()
    {
        return hullSystem != null;
    }

    private void HandleShipDestroyed()
    {
        Debug.Log("[PlayerShip] Ship destroyed!");
        Destroy(gameObject);
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
        if (IsOnStandby) return; // Block all input during railgun standby

        // Handle power input
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Engine power
        {
            powerManager.ToggleSystemState(powerManager.engines);
            ControllerHaptics.PowerToggled();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) // Arms power
        {
            powerManager.ToggleSystemState(powerManager.arms);
            ControllerHaptics.PowerToggled();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) // Sig power
        {
            powerManager.ToggleSystemState(powerManager.sig);
            ControllerHaptics.PowerToggled();
        }

        // Emergency Vent — V: vent all systems immediately
        if (Input.GetKeyDown(KeyCode.V))
        {
            powerManager.EmergencyVent();
            ControllerHaptics.Instance?.Pulse(0.60f, 0.30f, 0.25f);
        }

        // Black Alert — N: vent all, then auto-engage Engines + Arms
        if (Input.GetKeyDown(KeyCode.N))
        {
            powerManager.BlackAlert();
            ControllerHaptics.Instance?.Pulse(0.80f, 0.50f, 0.40f);
        }

        // Hyperspeed — Left Shift to engage / disengage
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            float engineEff = powerManager.GetSystemEfficiency("engines");
            if (!isInHyperspeed && engineEff > hyperspeedEngineThreshold)
            {
                isInHyperspeed = true;
                Debug.Log("[PlayerShip] Hyperspeed engaged.");
            }
            else if (isInHyperspeed)
            {
                isInHyperspeed = false;
                hyperspeedDrainAccumulator = 0f;
                Debug.Log("[PlayerShip] Hyperspeed disengaged.");
            }
            else
            {
                Debug.Log($"[PlayerShip] Cannot engage hyperspeed — engine power at {engineEff * 100f:F0}% (need > {hyperspeedEngineThreshold * 100f:F0}%).");
            }
        }

        // Auto-disengage when engine power is fully depleted
        if (isInHyperspeed && powerManager.GetSystemEfficiency("engines") <= 0f)
        {
            isInHyperspeed = false;
            hyperspeedDrainAccumulator = 0f;
            ControllerHaptics.HyperspeedLost();
            Debug.Log("[PlayerShip] Hyperspeed lost — engine power depleted.");
        }

        // Dodge and weapons are unavailable during hyperspeed
        if (!isInHyperspeed)
        {
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
        }

        // Testing controls
        if (Input.GetKeyDown(KeyCode.F2))
        {
            testMode_IgnoreWeaponPower = !testMode_IgnoreWeaponPower;
            Debug.Log($"Weapon Power Override: {(testMode_IgnoreWeaponPower ? "ENABLED" : "DISABLED")}");
        }

        // === Xbox Controller Input ===
        float rtAxis = Input.GetAxis("RightTrigger");
        float ltAxis = Input.GetAxis("LeftTrigger");
        bool rtPressed = rtAxis > 0.5f;
        bool ltPressed = ltAxis > 0.5f;

        // B button (joystick button 1): tap to toggle boost / hyperspeed
        if (Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            float engineEff = powerManager.GetSystemEfficiency("engines");
            if (!isInHyperspeed)
            {
                if (engineEff > hyperspeedEngineThreshold)
                {
                    isInHyperspeed = true;
                    ControllerHaptics.HyperspeedOn();
                    Debug.Log("[PlayerShip] Hyperspeed engaged (B).");
                }
                else
                {
                    Debug.Log($"[PlayerShip] Cannot engage hyperspeed — engine power at {engineEff * 100f:F0}%.");
                }
            }
            else
            {
                isInHyperspeed = false;
                hyperspeedDrainAccumulator = 0f;
                ControllerHaptics.HyperspeedOff();
                Debug.Log("[PlayerShip] Hyperspeed disengaged (B).");
            }
        }

        // Fire: RT (LT+RT is reserved for railgun charging in Railgun.cs)
        if (!isInHyperspeed && rtPressed && !ltPressed)
        {
            TryFireWeapons();
        }

        // LB (joystick button 4): dodge left
        if (!isInHyperspeed && Input.GetKeyDown(KeyCode.JoystickButton4))
        {
            TryDodge(-1f);
        }

        // RB (joystick button 5): dodge right
        if (!isInHyperspeed && Input.GetKeyDown(KeyCode.JoystickButton5))
        {
            TryDodge(1f);
        }

        // Y button (joystick button 3): cycle weapons
        if (Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            weaponManager?.SwitchToNextWeapon();
            ControllerHaptics.WeaponSwitched();
        }

        // D-Pad: direct power system toggles
        float dpadX = Input.GetAxis("DPadX");
        float dpadY = Input.GetAxis("DPadY");
        bool dpadUp    = dpadY >  0.5f;
        bool dpadDown  = dpadY < -0.5f;
        bool dpadLeft  = dpadX < -0.5f;
        bool dpadRight = dpadX >  0.5f;

        if (dpadUp    && !_dpadUpPrev)    { powerManager.ToggleSystemState(powerManager.engines); ControllerHaptics.PowerToggled(); Debug.Log("[PlayerShip] Toggled Engines"); }
        if (dpadDown  && !_dpadDownPrev)  { powerManager.ToggleSystemState(powerManager.arms);    ControllerHaptics.PowerToggled(); Debug.Log("[PlayerShip] Toggled Arms"); }
        if (dpadLeft  && !_dpadLeftPrev)  { powerManager.ToggleSystemState(powerManager.sig);     ControllerHaptics.PowerToggled(); Debug.Log("[PlayerShip] Toggled Sig"); }
        if (dpadRight && !_dpadRightPrev) { powerManager.ToggleSystemState(powerManager.bay);     ControllerHaptics.PowerToggled(); Debug.Log("[PlayerShip] Toggled Bay"); }

        _dpadUpPrev    = dpadUp;
        _dpadDownPrev  = dpadDown;
        _dpadLeftPrev  = dpadLeft;
        _dpadRightPrev = dpadRight;

        // A button (joystick button 0): toggle Support
        if (Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            powerManager.ToggleSystemState(powerManager.support);
            ControllerHaptics.PowerToggled();
            Debug.Log("[PlayerShip] Toggled Support");
        }

        // X button (joystick button 2): cycle radar zoom — handled in Radar.cs

        // Select button (joystick button 6): Emergency Vent
        if (Input.GetKeyDown(KeyCode.JoystickButton6))
        {
            powerManager.EmergencyVent();
            ControllerHaptics.Instance?.Pulse(0.60f, 0.30f, 0.25f);
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

            ControllerHaptics.DodgeExecuted();
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
                weaponManager.FireActiveWeapon(transform.position + transform.up * 1000f, 1.0f); // Use maximum power for testing
                return;
            }
            
            // Normal power check - can only fire if weapons have some power (minimum 20% for emergency firing)
            if (weaponPower > 0.2f)
            {
                weaponManager.FireActiveWeapon(transform.position + transform.up * 1000f, weaponPower);
            }
            else
            {
                Debug.Log("Insufficient weapon power to fire! (Use testing override if needed)");
            }
        }
    }

    /// <summary>Disables all ship input for <paramref name="duration"/> seconds, then reboots essential systems.</summary>
    public void EnterRailgunStandby(float duration)
    {
        if (!IsOnStandby)
            StartCoroutine(RailgunStandbyCoroutine(duration));
    }

    private System.Collections.IEnumerator RailgunStandbyCoroutine(float duration)
    {
        IsOnStandby = true;
        currentSpeed = 0f;
        targetSpeed = 0f;
        Debug.Log("[PlayerShip] Railgun fired — entering standby. All systems offline. Manual startup required.");

        yield return new WaitForSeconds(duration);

        // Bring reactor back online — all other systems must be started manually by the player.
        if (powerManager != null)
            powerManager.RebootReactor();

        IsOnStandby = false;
        Debug.Log("[PlayerShip] Reactor online — awaiting manual system startup.");
    }

    private void FixedUpdate()
    {
        if (IsOnStandby)
        {
            // Ship coasts on existing momentum during standby
            rb.linearVelocity = transform.up * currentSpeed;
            return;
        }

        float enginePower = powerManager.GetSystemEfficiency("engines");
        
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

        // Controller left stick Y for thrust (only if no keyboard input)
        if (thrustInput == 0f)
        {
            float stickThrust = Input.GetAxis("ControllerThrust");
            if (Mathf.Abs(stickThrust) > 0.1f)
                thrustInput = stickThrust;
        }
        
        if (enginePower <= 0f)
        {
            // No engine power - ship crawls at 5% max speed to hint the player should power engines
            targetSpeed = thrustInput < -0.01f ? 0f : 5f;
        }
        else if (Mathf.Abs(thrustInput) > 0.01f)
        {
            // Calculate target speed based on thrust direction
            // Apply subsystem debuffs per GDD: engine damage reduces max speed
            float subsystemSpeedMult = internalSubsystems != null ? internalSubsystems.GetSpeedMultiplier() : 1f;
            float hyperspeedMult = isInHyperspeed ? hyperspeedSpeedMultiplier : 1f;
            float maxThrust = 100f * enginePower * subsystemSpeedMult * hyperspeedMult; // Engine power + subsystem damage affects max thrust
            
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
        
        // --- Drift Physics ---
        // Decompose the existing Rigidbody velocity into ship-local components so lateral
        // momentum (drift) persists between frames rather than being force-aligned each tick.
        Vector3 existingVelocity = rb.linearVelocity;
        float forwardComponent = Vector3.Dot(existingVelocity, transform.up);
        float lateralComponent = Vector3.Dot(existingVelocity, transform.right);
        lateralComponent = Mathf.Clamp(lateralComponent, -maxLateralDrift, maxLateralDrift);

        // Apply thrust: push forward component toward targetSpeed
        float speedChangeRate = rateOfAcceleration * Time.fixedDeltaTime;
        forwardComponent = Mathf.MoveTowards(forwardComponent, targetSpeed, speedChangeRate);
        currentSpeed = forwardComponent;

        // Drift compensation: stability ratio (0–1) scales how aggressively lateral drift is
        // corrected. Always active — full stability = fast correction, zero stability = free drift.
        float stabilityRatio = stability.GetStabilityPercentage();
        float lateralCorrection = driftCompensationRate * stabilityRatio * Time.fixedDeltaTime;
        // Add a small velocity-proportional term so large drifts correct proportionally faster
        lateralCorrection += driftCompensationRate * stabilityRatio * Mathf.Abs(lateralComponent) * 0.05f * Time.fixedDeltaTime;
        lateralComponent = Mathf.MoveTowards(lateralComponent, 0f, lateralCorrection);

        // Passive stability drain from uncompensated lateral drift — keeps stability always relevant
        if (Mathf.Abs(lateralComponent) > 1f)
        {
            float driftDrain = driftStabilityDrainRate * Mathf.Abs(lateralComponent) * Time.fixedDeltaTime;
            stability.ApplyStabilityDrain(driftDrain);
        }

        // Reconstruct velocity from forward + lateral components
        rb.linearVelocity = transform.up * forwardComponent + transform.right * lateralComponent;

        // Handle dodge movement (smooth positional interpolation) — velocity is handled above
        if (isDodging)
        {
            float elapsedTime = Time.time - dodgeStartTime;
            float t = Mathf.Clamp01(elapsedTime / dodgeDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            Vector3 dodgePosition = Vector3.Lerp(dodgeStartPosition, dodgeTargetPosition, smoothT);
            rb.MovePosition(dodgePosition);

            if (t >= 1f)
                isDodging = false;
        }

        // Handle A/D input for turning
        float turnInput = Input.GetAxis("Horizontal");
        
        if (Mathf.Abs(turnInput) > 0.01f && !stability.IsStabilityDepleted())
        {
            // A turns left (negative), D turns right (positive)
            // Turn rate affected by engine power + subsystem debuffs per GDD (bridge/engine damage increases turn time)
            float subsystemTurnMult = internalSubsystems != null ? internalSubsystems.GetTurnRateMultiplier() : 1f;
            targetTurnSpeed = turnInput * turnRate * enginePower * subsystemTurnMult;
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
            
            // Spec: engine power reduces stability decay by 5% per PWR-1
            float stabilityDecayMult = powerManager != null ? powerManager.GetEngineStabilityDecayMultiplier() : 1f;
            stability.CalculateTurnStabilityDrain(Mathf.Abs(rotationThisFrame), Mathf.Abs(currentSpeed), stabilityDecayMult);
        }

        // Speed-based engine power drain — higher speed consumes engine bars faster
        if (Mathf.Abs(currentSpeed) > 1f && enginePower > 0f)
        {
            float speedRatio = Mathf.Abs(currentSpeed) / 100f;
            engineDrainAccumulator += speedRatio * enginePowerDrainRate * Time.fixedDeltaTime;
            while (engineDrainAccumulator >= 1f)
            {
                engineDrainAccumulator -= 1f;
                powerManager.RemoveEnginesPower();
            }
        }

        // Hyperspeed drain — outpaces reactor regeneration, forcing power depletion over time
        if (isInHyperspeed)
        {
            hyperspeedDrainAccumulator += hyperspeedDrainRate * Time.fixedDeltaTime;
            while (hyperspeedDrainAccumulator >= 1f)
            {
                hyperspeedDrainAccumulator -= 1f;
                powerManager.RemoveEnginesPower();
            }

            // Continuous low rumble while in hyperspeed
            ControllerHaptics.SetContinuous(0.20f, 0.10f);
        }
        else
        {
            ControllerHaptics.StopAll();
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

            if (UIController.Instance != null)
            {
                UIController.Instance.UpdateCompass(cameraEuler.z);
                UIController.Instance.WorldGridRotUpdate(cameraEuler.z);
                UIController.Instance.WorldGridLocUpdate(cameraTransform.position);
            }
        }

        // Spec: when stability is fully depleted, the ship incurs internal damage to hull and systems
        if (stability.IsStabilityDepleted())
            hullSystem.TakeDamage(hullSystem.DetermineHitSide(-transform.up), 5f * Time.fixedDeltaTime);

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

    private void OnDestroy()
    {
        // Unsubscribe from hull events
        if (hullSystem != null)
        {
            hullSystem.OnShipDestroyed -= HandleShipDestroyed;
        }
    }
}
