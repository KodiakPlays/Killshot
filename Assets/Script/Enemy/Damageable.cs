using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField] float initialHealth;
    public float currentHealth;
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
    ///<summary>
    ///It use to apply the damage to ship
    ///</summary>
    public void ApplyDamage(float damage)
    {
        if(!(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            Debug.Log("Apply Damage");
            if (currentHealth <= 0)
            {
                return;
            }
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                //DestroyEnemyShip();
            }
        }
    }
    void DestroyEnemyShip()
    {
        Debug.Log("Destroy ship by Damage");
        Destroy(gameObject);
        //AudioManager.Instance.PlayEnemyExplosion();
    }
}
