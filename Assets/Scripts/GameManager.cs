using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ─────────────────────────────────────────────────────────────────────────────
// EnemyGroup — one row in the inspector for a single enemy prefab type
// ─────────────────────────────────────────────────────────────────────────────
[System.Serializable]
public class EnemyGroup
{
    [Tooltip("The enemy ship prefab to spawn.")]
    public GameObject prefab;

    [Tooltip("How many of this enemy type to spawn at start.")]
    [Min(0)] public int count = 3;

    [Tooltip("If true, the enemy wanders via its patrol behaviour. Disable to keep the enemy stationary until the player is detected.")]
    public bool patrol = true;

    [Tooltip("If true, a new enemy of this type respawns when one is destroyed.")]
    public bool respawnOnDeath = false;

    [Tooltip("Seconds before a respawn occurs (only used when respawnOnDeath = true).")]
    [Min(0f)] public float respawnDelay = 10f;

    [HideInInspector] public int activeCount = 0;
}

// ─────────────────────────────────────────────────────────────────────────────
// GameManager
// ─────────────────────────────────────────────────────────────────────────────
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ── Spawn Configuration ──────────────────────────────────────────────────

    [Header("Enemy Groups")]
    [Tooltip("Add one entry per enemy type. Each entry controls prefab, count and respawn behaviour.")]
    public List<EnemyGroup> enemyGroups = new List<EnemyGroup>();

    [Header("Spawn Area")]
    [Tooltip("Centre point for enemy spawning. Leave unassigned to use the world origin.")]
    public Transform spawnCentre;

    [Tooltip("Enemies spawn at a random distance between these two values from the spawn centre.")]
    public float minScatterDistance = 30f;
    public float maxScatterDistance = 80f;

    [Tooltip("Minimum distance between any two spawned enemies.")]
    [Min(0f)] public float minSeparation = 5f;

    [Tooltip("Maximum attempts to find a valid spawn position before giving up.")]
    [Min(1)] public int maxSpawnAttempts = 20;

    // ── Player ───────────────────────────────────────────────────────────────

    [Header("Player")]
    [Tooltip("Where the player ship spawns. Leave unassigned to use the scene's existing player.")]
    public Transform playerSpawnPoint;
    public GameObject playerShipPrefab;

    // ── Win / Lose ────────────────────────────────────────────────────────────

    [Header("Win / Lose Conditions")]
    [Tooltip("Destroy all enemies to win. Disable to ignore enemy count for win state.")]
    public bool winOnAllEnemiesDestroyed = true;

    [Tooltip("Seconds to wait after win/lose before triggering the callback.")]
    [Min(0f)] public float endSequenceDelay = 3f;

    // ── Runtime State (read-only in inspector) ────────────────────────────────

    [Header("Runtime Info (read-only)")]
    [SerializeField, ReadOnly] private int totalEnemiesAlive = 0;
    [SerializeField, ReadOnly] private bool gameOver = false;
    [SerializeField, ReadOnly] private bool playerAlive = true;

    // internal tracking
    private List<GameObject> activeEnemies = new List<GameObject>();
    private GameObject playerInstance;

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        SpawnPlayer();
        SpawnAllEnemyGroups();
    }

    // ── Player Spawn ──────────────────────────────────────────────────────────

    private void SpawnPlayer()
    {
        if (playerShipPrefab == null) return;

        Vector3 pos    = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
        Quaternion rot = playerSpawnPoint != null ? playerSpawnPoint.rotation : Quaternion.identity;
        playerInstance = Instantiate(playerShipPrefab, pos, rot);

        // Subscribe to hull destroyed event if PlayerShip is present
        PlayerShip ps = playerInstance.GetComponent<PlayerShip>();
        if (ps != null)
        {
            HullSystem hull = playerInstance.GetComponent<HullSystem>();
            if (hull != null)
                hull.OnShipDestroyed += OnPlayerDestroyed;
        }
    }

    // ── Enemy Spawning ────────────────────────────────────────────────────────

    private void SpawnAllEnemyGroups()
    {
        foreach (EnemyGroup group in enemyGroups)
        {
            group.activeCount = 0;
            for (int i = 0; i < group.count; i++)
                SpawnEnemy(group);
        }
    }

    private void SpawnEnemy(EnemyGroup group)
    {
        if (group.prefab == null)
        {
            Debug.LogWarning($"[GameManager] EnemyGroup has no prefab assigned — skipping.");
            return;
        }

        Vector3 spawnPos = FindSpawnPosition();
        GameObject enemy = Instantiate(group.prefab, spawnPos, Quaternion.identity);
        activeEnemies.Add(enemy);
        group.activeCount++;
        totalEnemiesAlive++;

        // Apply group settings to the spawned enemy
        EnemyShip es = enemy.GetComponent<EnemyShip>();
        if (es != null)
            es.patrolEnabled = group.patrol;

        // Subscribe to death so we can track it
        if (es != null)
        {
            HullSystem hull = enemy.GetComponent<HullSystem>();
            if (hull != null)
                hull.OnShipDestroyed += () => OnEnemyDestroyed(enemy, group);
            else
                // Fallback: poll via a small proxy (EnemyShip doesn't use HullSystem)
                StartCoroutine(WatchEnemyDestroy(enemy, group));
        }
        else
        {
            StartCoroutine(WatchEnemyDestroy(enemy, group));
        }
    }

    private IEnumerator WatchEnemyDestroy(GameObject enemy, EnemyGroup group)
    {
        while (enemy != null)
            yield return null;
        OnEnemyDestroyed(enemy, group);
    }

    private void OnEnemyDestroyed(GameObject enemy, EnemyGroup group)
    {
        if (gameOver) return;

        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);

        group.activeCount = Mathf.Max(0, group.activeCount - 1);
        totalEnemiesAlive = Mathf.Max(0, totalEnemiesAlive - 1);

        if (group.respawnOnDeath)
            StartCoroutine(RespawnAfterDelay(group, group.respawnDelay));

        CheckWinCondition();
    }

    private IEnumerator RespawnAfterDelay(EnemyGroup group, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!gameOver)
            SpawnEnemy(group);
    }

    // ── Position Finding ──────────────────────────────────────────────────────

    private Vector3 FindSpawnPosition()
    {
        Vector3 centre = spawnCentre != null ? spawnCentre.position : Vector3.zero;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            float angle    = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minScatterDistance, maxScatterDistance);
            Vector3 candidate = centre + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * distance;

            if (IsClearPosition(candidate))
                return candidate;
        }

        // Fallback: just pick a random ring position without separation check
        float fallbackAngle    = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float fallbackDistance = Random.Range(minScatterDistance, maxScatterDistance);
        return centre + new Vector3(Mathf.Cos(fallbackAngle), Mathf.Sin(fallbackAngle), 0f) * fallbackDistance;
    }

    private bool IsClearPosition(Vector3 position)
    {
        foreach (GameObject existing in activeEnemies)
        {
            if (existing == null) continue;
            if (Vector3.Distance(position, existing.transform.position) < minSeparation)
                return false;
        }
        return true;
    }

    // ── Win / Lose ────────────────────────────────────────────────────────────

    private void CheckWinCondition()
    {
        if (!winOnAllEnemiesDestroyed || gameOver) return;
        if (totalEnemiesAlive <= 0)
            StartCoroutine(EndSequence(won: true));
    }

    private void OnPlayerDestroyed()
    {
        if (gameOver) return;
        playerAlive = false;
        StartCoroutine(EndSequence(won: false));
    }

    private IEnumerator EndSequence(bool won)
    {
        gameOver = true;
        Debug.Log($"[GameManager] {(won ? "VICTORY" : "DEFEAT")}");
        yield return new WaitForSeconds(endSequenceDelay);
        // Hook additional logic here (load scene, show UI, etc.)
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Manually spawn an enemy from the given group index.</summary>
    public void SpawnEnemyFromGroup(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= enemyGroups.Count) return;
        SpawnEnemy(enemyGroups[groupIndex]);
    }

    /// <summary>Directly spawn an enemy prefab at a world position.</summary>
    public void SpawnEnemyShip(Vector3 position, Quaternion rotation)
    {
        if (enemyGroups.Count == 0 || enemyGroups[0].prefab == null)
        {
            Debug.LogWarning("[GameManager] No enemy groups configured.");
            return;
        }
        GameObject enemy = Instantiate(enemyGroups[0].prefab, position, rotation);
        activeEnemies.Add(enemy);
        totalEnemiesAlive++;
        StartCoroutine(WatchEnemyDestroy(enemy, enemyGroups[0]));
    }

    public int  GetTotalEnemiesAlive()  => totalEnemiesAlive;
    public bool IsGameOver()            => gameOver;
    public bool IsPlayerAlive()         => playerAlive;
}

// ── ReadOnly attribute (inspector display helper) ────────────────────────────
public class ReadOnlyAttribute : PropertyAttribute { }
