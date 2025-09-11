using UnityEngine;

public class Shell : MonoBehaviour
{
    private Vector3 velocity;
    private float damage;
    private float maxLifetime = 15f; // Maximum time the shell can exist
    private float creationTime;

    public void Initialize(Vector3 initialVelocity, float damageAmount)
    {
        velocity = initialVelocity;
        damage = damageAmount;
        creationTime = Time.time;
    }

    private void Update()
    {
        // Move the shell
        transform.position += velocity * Time.deltaTime;

        // Check if shell has exceeded its lifetime
        if (Time.time - creationTime > maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if we hit something damageable
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        // Create impact effect here if desired

        // Destroy the shell
        Destroy(gameObject);
    }
}
