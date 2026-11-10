// =================================================================================================
// Asteroid.cs
//
// What it does:
// A rock that guns can shoot to pieces. It belongs to the older physics weapons, where shots are
// real moving objects. No scene or prefab has it. The battle board does not use this script.
//
// How it works:
// - It goes on a rock object with a collider.
// - At the start it fills its health to the maximum (50 by default). Then it tries to tag itself
//   "Asteroid".
// - Guns hurt it through IDamageable, the damage interface of the older weapons. Each hit takes
//   health away and writes a line to the Console.
// - At half health or less it swaps to a "damaged" material, if one is set in the Inspector.
// - At zero health it shows a blast effect. It throws out 2 to 5 debris pieces with a random
//   push. It removes itself. The debris is removed after 10 seconds.
// - The old Railgun's beam damages it like any other target and keeps going through it.
// - In the Scene view, when selected, it draws a red ring that shrinks as health drops.
//
// Technical:
// - Data: currentHealth and maxHealth, in health points.
// - TakeDamage(damage) takes health away and returns the amount taken. It never takes more than
//   is left.
// - Callers: Shell, PDCBullet, Missile, BoardingPod and Railgun, through IDamageable. No script
//   names Asteroid directly.
// - CanBeDamaged() is false once it is destroyed. Later hits do nothing.
// - Numbers: all in the Inspector (health, debris count and push, damage threshold 0.5). The
//   Tuning window does not show them.
// - Trap: the "Asteroid" tag is not in the project's tag list. Start gives an error at that line
//   until the tag is added.
// - Trap: ShowDamage swaps the renderer's material. The original material is saved but never put
//   back.
// - A debris piece gets its push only if its prefab has a Rigidbody.
// =================================================================================================

using UnityEngine;

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
