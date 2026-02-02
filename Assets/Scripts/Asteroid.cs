using UnityEngine;

/// <summary>
/// Example destructible asteroid that can be damaged and destroyed by weapons
/// Railgun can penetrate through it, other weapons destroy on impact
/// </summary>
[RequireComponent(typeof(Collider))]
public class Asteroid : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float currentHealth;
    
    [Header("Destruction")]
    [SerializeField] private GameObject destructionEffect;
    [SerializeField] private GameObject[] debrisPieces; // Optional: spawn debris on destruction
    [SerializeField] private int minDebris = 2;
    [SerializeField] private int maxDebris = 5;
    [SerializeField] private float debrisForce = 5f;
    
    [Header("Visual Damage")]
    [SerializeField] private Material damagedMaterial; // Optional: change material when damaged
    [SerializeField] private float damageThreshold = 0.5f; // When to show damage (50% health)

    private Renderer asteroidRenderer;
    private Material originalMaterial;
    private bool isDestroyed = false;

    private void Start()
    {
        currentHealth = maxHealth;
        asteroidRenderer = GetComponent<Renderer>();
        
        if (asteroidRenderer != null)
        {
            originalMaterial = asteroidRenderer.material;
        }
        
        // Make sure it has the right tag for railgun penetration
        if (!CompareTag("Asteroid"))
        {
            gameObject.tag = "Asteroid";
        }
    }

    public float TakeDamage(float damage)
    {
        if (isDestroyed) return 0f;
        
        float actualDamage = Mathf.Min(damage, currentHealth);
        currentHealth -= actualDamage;
        
        Debug.Log($"Asteroid took {actualDamage} damage. Health: {currentHealth}/{maxHealth}");
        
        // Show visual damage if below threshold
        if (currentHealth <= maxHealth * damageThreshold && currentHealth > 0)
        {
            ShowDamage();
        }
        
        // Check for destruction
        if (currentHealth <= 0)
        {
            DestroyAsteroid();
        }
        
        return actualDamage;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public bool CanBeDamaged()
    {
        return !isDestroyed;
    }

    private void ShowDamage()
    {
        if (asteroidRenderer != null && damagedMaterial != null)
        {
            asteroidRenderer.material = damagedMaterial;
        }
    }

    private void DestroyAsteroid()
    {
        isDestroyed = true;
        
        // Spawn destruction effect
        if (destructionEffect != null)
        {
            Instantiate(destructionEffect, transform.position, Quaternion.identity);
        }
        
        // Spawn debris pieces
        if (debrisPieces != null && debrisPieces.Length > 0)
        {
            int debrisCount = Random.Range(minDebris, maxDebris + 1);
            
            for (int i = 0; i < debrisCount; i++)
            {
                GameObject debrisPrefab = debrisPieces[Random.Range(0, debrisPieces.Length)];
                if (debrisPrefab != null)
                {
                    Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
                    GameObject debris = Instantiate(debrisPrefab, transform.position + randomOffset, Random.rotation);
                    
                    // Add force to debris
                    Rigidbody rb = debris.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddForce(Random.insideUnitSphere * debrisForce, ForceMode.Impulse);
                        rb.AddTorque(Random.insideUnitSphere * debrisForce, ForceMode.Impulse);
                    }
                    
                    // Auto-destroy debris after some time
                    Destroy(debris, 10f);
                }
            }
        }
        
        // Destroy the asteroid
        Destroy(gameObject);
    }

    // Optional: Show health bar or debug info
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float healthPercent = currentHealth / maxHealth;
        Gizmos.DrawWireSphere(transform.position, 1f * healthPercent);
    }
}
