
using UnityEngine;

public class OnCollisionDestroyed : MonoBehaviour
{
    [SerializeField] Damageable damageable;
    public int damage = 10;
    private void Start()
    {
        damageable = GetComponent<Damageable>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
        {
            //spaceship will destroy if it collide with enemy ship and gameover panle will show
            if (collision.gameObject.layer == 3)
            {
                Destroy(gameObject, 0.5f);
                GameManager.Instance.EndGame();
            }
            //Apply damage if spaceship will collide with rock
            if (collision.gameObject.layer == 15)
            {
                damageable.ApplyDamage(damage);
                Destroy(collision.gameObject, 1);
            }
            
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        //after triggering this game object gamepanle will be active
        if(other.gameObject.tag == "EscapePoint")
        {
            Debug.Log("win win!!");
            Invoke("ActiveWinPanel", 0.5f);
        }
    }
    void ActiveWinPanel()
    {
        GameManager.Instance.GameWinPanale.SetActive(true);
        Time.timeScale = 0;
    }
}
