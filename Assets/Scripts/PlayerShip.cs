using UnityEngine;

[RequireComponent(typeof(ShipStability))]
public class PlayerShip : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float accelerationRate = 20f; // Units per bar
    [SerializeField] private float turnRate = 90f; // Degrees per second
    [SerializeField] private float turnAcceleration = 180f; // Degrees per second squared
    [SerializeField] private float maxAngularVelocity = 45f; // Maximum turn speed in degrees per second
    [SerializeField] private float angularDamping = 2f; // How quickly ship stops turning when no input
    [SerializeField] private float dodgeForce = 20f;
    [SerializeField] private float dodgeDistance = 5f;

    // Systems
    private PowerManager powerManager;
    private ShipStability stability;
    private Weapons weapons;
    
    // Movement state
    private float currentSpeed;
    private float targetThrust;
    private float currentAngularVelocity; // Current turning speed
    private float targetAngularVelocity; // Target turning speed
    private Vector2 dodgeDirection;

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

        // Handle dodge input
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
            dodgeDirection = direction;
            Vector3 dodgeForceVector = new Vector3(direction.x, 0, direction.y) * dodgeForce;
            rb.AddForce(dodgeForceVector, ForceMode.VelocityChange);
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
        
        // Handle W/S input for thrust
        float thrustInput = Input.GetAxis("Vertical");
        if (Mathf.Abs(thrustInput) > 0.01f)
        {
            // Each bar represents 20 units of speed
            targetThrust = -thrustInput * accelerationRate * enginePower;
            
            // Gradually change current speed towards target
            float acceleration = Mathf.Sign(targetThrust - currentSpeed) * accelerationRate * Time.fixedDeltaTime;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetThrust, Mathf.Abs(acceleration));
        }
        else if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            // Gradual deceleration when no input
            float deceleration = Mathf.Sign(-currentSpeed) * accelerationRate * 0.5f * Time.fixedDeltaTime;
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, Mathf.Abs(deceleration));
        }

        // Apply movement
        rb.linearVelocity = transform.forward * currentSpeed;

        // Handle A/D input for rotation - smooth spaceship-like turning
        float turnInput = Input.GetAxis("Horizontal");
        if (Mathf.Abs(turnInput) > 0.01f && !stability.IsStabilityDepleted())
        {
            // Calculate target angular velocity based on input and engine power
            targetAngularVelocity = turnInput * maxAngularVelocity * enginePower;
            
            // Gradually accelerate towards target angular velocity
            float angularAcceleration = Mathf.Sign(targetAngularVelocity - currentAngularVelocity) * turnAcceleration * Time.fixedDeltaTime;
            currentAngularVelocity = Mathf.MoveTowards(currentAngularVelocity, targetAngularVelocity, Mathf.Abs(angularAcceleration));
            
            // Calculate stability drain based on turn severity and speed
            float actualRotation = currentAngularVelocity * Time.fixedDeltaTime;
            stability.CalculateTurnStabilityDrain(actualRotation, Mathf.Abs(currentSpeed));
        }
        else
        {
            // No input - gradually reduce angular velocity (angular damping)
            targetAngularVelocity = 0f;
            float dampingDeceleration = Mathf.Sign(-currentAngularVelocity) * angularDamping * maxAngularVelocity * Time.fixedDeltaTime;
            currentAngularVelocity = Mathf.MoveTowards(currentAngularVelocity, 0f, Mathf.Abs(dampingDeceleration));
        }
        
        // Apply the rotation using angular velocity
        if (Mathf.Abs(currentAngularVelocity) > 0.01f)
        {
            float rotationThisFrame = currentAngularVelocity * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, rotationThisFrame, 0));
        }

        // Apply damage if stability is critical
        if (stability.IsStabilityCritical() && Time.frameCount % 30 == 0) // Check every 30 frames
        {
            TakeDamage(1);
        }
        
        // Debug info (remove this later)
        if (Input.GetKey(KeyCode.F1))
        {
            Debug.Log($"Engine Power: {powerManager.GetSystemEfficiency("engines")}, Current Speed: {currentSpeed}, Target Thrust: {targetThrust}, Input: {thrustInput}");
            Debug.Log($"Angular Velocity: {currentAngularVelocity:F1}°/s, Target: {targetAngularVelocity:F1}°/s, Turn Input: {turnInput:F2}");
            Debug.Log($"Weapon Power: {powerManager.GetSystemEfficiency("weapons")}, Ammo: {(weapons != null ? weapons.GetCurrentAmmo() : 0)}, Power Override: {testMode_IgnoreWeaponPower}");
        }
    }
}
