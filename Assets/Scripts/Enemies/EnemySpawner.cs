using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Spawn automatique d'ennemis normaux dans des zones définies.
/// SETUP :
/// - Ajouter sur le GameObject parent de la salle.
/// - Glisser les prefabs ennemis dans m_enemyPrefabs.
/// - Créer des enfants avec le script SpawnZone pour définir les zones.
/// - Régler m_enemyCount selon la difficulté de la salle.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs ennemis")]
    public GameObject[] m_enemyPrefabs;

    [Header("Nombre d'ennemis à spawner")]
    public int m_enemyCount = 3;

    [Header("Minimum de distance entre ennemis au spawn")]
    public float m_minSpawnDistance = 1f;

    private List<GameObject> m_spawnedEnemies = new List<GameObject>();
    private SpawnZone[] m_spawnZones;

    void OnEnable()
    {
        m_spawnZones = GetComponentsInChildren<SpawnZone>();
        if (m_spawnZones.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] Aucune SpawnZone trouvée!");
            return;
        }
        SpawnEnemies();
    }

    void OnDisable()
    {
        ClearEnemies();
    }

    private void SpawnEnemies()
    {
        if (m_enemyPrefabs == null || m_enemyPrefabs.Length == 0) return;

        Tilemap groundMap   = null;
        Tilemap obstacleMap = null;

        foreach (Tilemap tm in GetComponentsInChildren<Tilemap>())
        {
            if (tm.gameObject.name == "Ground")    groundMap   = tm;
            if (tm.gameObject.name == "Obstacles") obstacleMap = tm;
        }

        if (groundMap == null)
        {
            Debug.LogWarning("[EnemySpawner] Tilemap 'Ground' non trouvée.");
            return;
        }

        // Séparer zones de spawn et zones d'exclusion
        List<SpawnZone> spawnZones     = new List<SpawnZone>();
        List<SpawnZone> exclusionZones = new List<SpawnZone>();
        foreach (SpawnZone zone in m_spawnZones)
        {
            if (zone.IsExclusionZone()) exclusionZones.Add(zone);
            else                        spawnZones.Add(zone);
        }

        // Construire le pool de positions valides
        List<Vector3> allSpawnPoints = new List<Vector3>();
        foreach (SpawnZone zone in spawnZones)
        {
            List<Vector3> positions = zone.GetValidSpawnPositions(groundMap, obstacleMap);
            float density = zone.GetDensityPercentage() / 100f;
            int enemiesInZone = Mathf.RoundToInt(m_enemyCount * density);

            for (int i = 0; i < enemiesInZone && positions.Count > 0; i++)
            {
                Vector3 pos = positions[Random.Range(0, positions.Count)];

                bool excluded = false;
                foreach (SpawnZone ex in exclusionZones)
                    if (ex.ContainsPosition(pos)) { excluded = true; break; }

                if (!excluded) allSpawnPoints.Add(pos);
            }
        }

        // Spawner les ennemis
        int spawnedCount = 0;
        foreach (Vector3 spawnPos in allSpawnPoints)
        {
            if (spawnedCount >= m_enemyCount) break;

            bool tooClose = false;
            foreach (GameObject spawnedEnemy in m_spawnedEnemies)
            {
                if (Vector3.Distance(spawnPos, spawnedEnemy.transform.position) < m_minSpawnDistance)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            GameObject prefab = m_enemyPrefabs[Random.Range(0, m_enemyPrefabs.Length)];
            GameObject enemy  = Instantiate(prefab, spawnPos, Quaternion.identity);

            EnemyBehavior behavior = enemy.GetComponent<EnemyBehavior>();
            if (behavior != null)
                behavior.m_homeZone = GetZoneContaining(spawnPos, spawnZones);

            m_spawnedEnemies.Add(enemy);
            spawnedCount++;
        }
    }

    private SpawnZone GetZoneContaining(Vector3 pos, List<SpawnZone> zones)
    {
        foreach (SpawnZone zone in zones)
            if (zone.ContainsPosition(pos)) return zone;
        return null;
    }

    private void ClearEnemies()
    {
        foreach (GameObject enemy in m_spawnedEnemies)
            if (enemy != null) Destroy(enemy);
        m_spawnedEnemies.Clear();
    }
}
