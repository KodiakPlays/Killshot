using UnityEngine;

[RequireComponent(typeof(ShipStability))]
public class PlayerShip : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float accelerationRate = 20f; // Units per bar
    [SerializeField] private float turnRate = 90f; // Degrees per second
    [SerializeField] private float dodgeForce = 20f;
    [SerializeField] private float dodgeDistance = 5f;

    // Systems
    private PowerManager powerManager;
    private ShipStability stability;
    
    // Movement state
    private float currentSpeed;
    private float targetThrust;
    private Vector2 dodgeDirection;

	[Header("Health Settings")]
	[SerializeField] private float maxHealth = 100f;
	[SerializeField] private float currentHealth;

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

    private void FixedUpdate()
    {
        float enginePower = powerManager.GetSystemEfficiency("engines");
        
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

        // Handle A/D input for rotation
        float turnInput = Input.GetAxis("Horizontal");
        if (Mathf.Abs(turnInput) > 0.01f && !stability.IsStabilityDepleted())
        {
            float rotation = turnInput * turnRate * Time.fixedDeltaTime * enginePower;
            
            // Calculate stability drain based on turn severity and speed
            stability.CalculateTurnStabilityDrain(rotation, Mathf.Abs(currentSpeed));
            
            // Apply rotation
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, rotation, 0));
        }

        // Apply damage if stability is critical
        if (stability.IsStabilityCritical() && Time.frameCount % 30 == 0) // Check every 30 frames
        {
            TakeDamage(1);
        }
    }
}
