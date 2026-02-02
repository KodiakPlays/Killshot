using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject EnemyShipPrefab;

    public void SpawnEnemyShip(Vector3 position, Quaternion rotation)
    {
        Instantiate(EnemyShipPrefab, position, rotation);
    }

    void Start()
    {
        // Initialize game state
        //Spawn enemy ships randomly at start
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(-50, 50), Random.Range(-50, 50), 0);
            SpawnEnemyShip(randomPos, Quaternion.identity);
        }
    }
}
