using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RadarSystem : MonoBehaviour
{
    public Transform player; // Reference to the player object
    public GameObject radarPanel; // The radar UI panel
    public GameObject playerDotPrefab; // The player dot prefab
    public GameObject enemyDotPrefab; // The enemy dot prefab
    public float radarRange = 50f; // Range of the radar
    public float radarSize = 100f; // Size of the radar UI

    private List<GameObject> enemyDots = new List<GameObject>();

    void Update()
    {
        UpdatePlayerDot();
        UpdateEnemyDots();
    }

    private void UpdatePlayerDot()
    {
        // Place the player dot at the center of the radar
        Vector2 playerPositionOnRadar = Vector2.zero;
        GameObject playerDot = Instantiate(playerDotPrefab, radarPanel.transform);
        playerDot.GetComponent<RectTransform>().anchoredPosition = playerPositionOnRadar;
        playerDot.GetComponent<RectTransform>().localScale = new Vector2(0.5f, 0.5f);
    }

    private void UpdateEnemyDots()
    {
        // Clear previous enemy dots
        foreach (GameObject dot in enemyDots)
        {
            Destroy(dot);
        }
        enemyDots.Clear();
        radarRange = GetComponent<SpaceshipMovement>().detectionRadius;
        // Find all enemies within radar range
        Collider[] hits = Physics.OverlapSphere(player.position, radarRange);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector3 enemyPosition = hit.transform.position;
                Vector3 direction = enemyPosition - player.position;

                // Map world position to radar position
                Vector2 radarPosition = new Vector2(direction.x, direction.z) * (radarSize / radarRange);

                // Instantiate the enemy dot on the radar
                GameObject enemyDot = Instantiate(enemyDotPrefab, radarPanel.transform);
                enemyDot.GetComponent<RectTransform>().anchoredPosition = radarPosition;
                enemyDot.GetComponent<RectTransform>().localScale = new Vector2(0.5f,0.5f);
                enemyDots.Add(enemyDot);
            }
        }
    }
}
