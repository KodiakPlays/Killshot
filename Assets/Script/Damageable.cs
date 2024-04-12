using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField] float initialHealth;
    [SerializeField] float currentHealth;

    void Start()
    {
        currentHealth = initialHealth;
    }

    public void ApplyDamage(float damage)
    {
        if (currentHealth <= 0)
        {
            return;
        }
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            DestroyEnemyShip();
        }
    }
    void DestroyEnemyShip()
    {
        Destroy(gameObject);
    }
}
