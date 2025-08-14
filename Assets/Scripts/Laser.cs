using UnityEngine;

public class Laser : MonoBehaviour
{
    public float speed = 30f;
    public float lifeTime = 3f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    public void Fire(Vector3 direction)
    {
        rb.linearVelocity = direction * speed;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
