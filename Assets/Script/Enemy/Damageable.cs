using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField] float initialHealth;
    [SerializeField] float currentHealth;
    [SerializeField] UnityEngine.UI.Slider healthbar;

    void Start()
    {
        currentHealth = initialHealth;
        healthbar.minValue = 0;
        healthbar.maxValue = initialHealth;
    }

    private void Update()
    {
        healthbar.value = currentHealth;
    }
    public void ApplyDamage(float damage)
    {
        Debug.Log("Apply Damage");
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
        AudioManager.Instance.PlayEnemyExplosion();
    }
}
