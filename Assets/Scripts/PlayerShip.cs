using UnityEngine;

public class PlayerShip : MonoBehaviour
{
	[Header("Movement Settings")]
	[SerializeField] private float thrustForce = 10f;
	[SerializeField] private float rotationSpeed = 100f;
	[SerializeField] private float maxSpeed = 20f;

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
		}
		currentHealth = maxHealth;
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

	private void FixedUpdate()
	{
		// Get input from keyboard or controller
		float moveInput = Input.GetAxis("Vertical"); // W/S, Up/Down, Controller stick Y
		float turnInput = Input.GetAxis("Horizontal"); // A/D, Left/Right, Controller stick X

		// Calculate horizontal velocity (XZ plane)
		Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

		// Thrust forward/backward (in XZ plane)
		if (Mathf.Abs(moveInput) > 0.01f)
		{
			Vector3 thrust = transform.forward * -moveInput * thrustForce;
			rb.AddForce(thrust, ForceMode.Acceleration);
		}
		else
		{
			// Apply drag to slow down when no input
			Vector3 drag = -horizontalVelocity * 0.1f; // Adjust drag factor as needed
			rb.AddForce(drag, ForceMode.Acceleration);
			// Optionally, stop completely if very slow
			if (horizontalVelocity.magnitude < 0.1f)
			{
				rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
			}
		}

		// Clamp max speed (XZ plane only)
		if (horizontalVelocity.magnitude > maxSpeed)
		{
			horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
			rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
		}

		// Rotate left/right (Y axis)
		float rotation = turnInput * rotationSpeed * Time.fixedDeltaTime;
		rb.MoveRotation(rb.rotation * Quaternion.Euler(0, rotation, 0));
	}
}
