
using UnityEngine;
using System.Collections;
using TMPro;
public class OnCollisionDestroyed : MonoBehaviour
{
    [SerializeField] Damageable damageable;
    public int damage = 10;

    [SerializeField] TMP_Text timerText; // Assign in Inspector
    private bool isCountingDown = false;
    [SerializeField] int winTimmer = 5;
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
        if(other.gameObject.tag == "EscapePoint" && !isCountingDown)
        {
            Debug.Log("win win!!");
            //Time.timeScale = 0; // to pause the game
            // Invoke("ActiveWinPanel", 0.5f);

            StartCoroutine(StartCountdown());
        }
    }

    IEnumerator StartCountdown()
    {
        isCountingDown = true;
        Time.timeScale = 0; // Pause the game
        int timeLeft = winTimmer;

        while (timeLeft >= 0)
        {
            if (timerText != null)
            {
                timerText.text = "Escape in: " + timeLeft.ToString();
            }
            yield return new WaitForSecondsRealtime(1f); // Use WaitForSecondsRealtime to work when Time.timeScale = 0
            timeLeft--;
        }

        // Call ActiveWinPanel after countdown ends
        ActiveWinPanel();
    }
    void ActiveWinPanel()
    {
        GameManager.Instance.GameWinPanale.SetActive(true);
        Time.timeScale = 0;
    }
}
