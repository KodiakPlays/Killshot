using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    [Header("Mission Systems")]
    [SerializeField] private GameClock gameClock;
    [SerializeField] private QuestSystem questSystem;
    [SerializeField] private WorldBoundary worldBoundary;

    public void SpawnEnemyShip(Vector3 position, Quaternion rotation)
    {
        Instantiate(EnemyShipPrefab, position, rotation);
    }

    void Start()
    {
        // Initialize game state
        //Spawn enemy ships randomly at start
        if (EnemyShipPrefab != null)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 randomPos = new Vector3(Random.Range(-50, 50), Random.Range(-50, 50), 0);
                SpawnEnemyShip(randomPos, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] EnemyShipPrefab is not assigned! Cannot spawn enemies.");
        }

        // Initialize mission systems
        InitializeMissionSystems();
    }

    private void InitializeMissionSystems()
    {
        // GameClock
        if (gameClock == null)
        {
            gameClock = GetComponent<GameClock>();
            if (gameClock == null) gameClock = gameObject.AddComponent<GameClock>();
        }

        // QuestSystem
        if (questSystem == null)
        {
            questSystem = GetComponent<QuestSystem>();
            if (questSystem == null) questSystem = gameObject.AddComponent<QuestSystem>();
        }

        // WorldBoundary
        if (worldBoundary == null)
        {
            worldBoundary = GetComponent<WorldBoundary>();
            if (worldBoundary == null) worldBoundary = gameObject.AddComponent<WorldBoundary>();
        }

        // Subscribe to mission events
        questSystem.OnMissionFailed += (reason) => Debug.Log($"[GameManager] Mission Failed: {reason}");
        questSystem.OnMissionComplete += () => Debug.Log("[GameManager] Mission Complete!");
    }

    void Update()
    {
        // Strategic pause per GDD: player may pause at any time, clock stops during pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameClock != null) gameClock.TogglePause();
        }
    }
}
