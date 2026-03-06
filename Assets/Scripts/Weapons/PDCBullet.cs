using UnityEngine;

public class PDCBullet : MonoBehaviour
{
    private Vector3 velocity;
    private float damage;
    private float maxLifetime = 3f;
    private float creationTime;

    public void Initialize(Vector3 initialVelocity, float damageAmount)
    {
        velocity = initialVelocity;
        damage = damageAmount;
        creationTime = Time.time;
    }

    private void Update()
    {
        transform.position += velocity * Time.deltaTime;

        if (Time.time - creationTime > maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
