using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Spawn automatique d'ennemis normaux dans une salle.
/// SETUP :
/// - Ajouter sur le GameObject parent de la salle (avec RoomContext).
/// - Glisser les prefabs ennemis dans m_enemyPrefabs.
/// - Créer des GameObjects vides comme points de spawn → les glisser dans m_spawnPoints.
/// - Régler m_enemyCount selon la difficulté de la salle.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs ennemis")]
    public GameObject[] m_enemyPrefabs;

    [Header("Points de spawn")]
    public Transform[] m_spawnPoints;

    [Header("Nombre d'ennemis à spawner")]
    public int m_enemyCount = 3;

    private List<GameObject> m_spawnedEnemies = new List<GameObject>();

    void OnEnable()
    {
        SpawnEnemies();
    }

    void OnDisable()
    {
        ClearEnemies();
    }

    private void SpawnEnemies()
    {
        if (m_enemyPrefabs == null || m_enemyPrefabs.Length == 0) return;

        List<Vector3> validPositions = GetGroundPositions();
        if (validPositions.Count == 0)
        {
            Debug.LogWarning("[EnemySpawner] Aucune position 'Ground' trouvée.");
            return;
        }

        for (int i = 0; i < m_enemyCount; i++)
        {
            GameObject prefab = m_enemyPrefabs[Random.Range(0, m_enemyPrefabs.Length)];
            Vector3 spawnPos  = validPositions[Random.Range(0, validPositions.Count)];

            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            m_spawnedEnemies.Add(enemy);
        }
    }

    private List<Vector3> GetGroundPositions()
    {
        List<Vector3> positions = new List<Vector3>();

        Tilemap groundMap    = null;
        Tilemap obstacleMap  = null;

        foreach (Tilemap tm in GetComponentsInChildren<Tilemap>())
        {
            if (tm.gameObject.name == "Ground")   groundMap   = tm;
            if (tm.gameObject.name == "Obstacles") obstacleMap = tm;
        }

        if (groundMap == null) return positions;

        BoundsInt bounds = groundMap.cellBounds;
        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            if (!groundMap.HasTile(cell)) continue;

            // Exclut la cellule si un obstacle est présent au même endroit
            if (obstacleMap != null && obstacleMap.HasTile(cell)) continue;

            positions.Add(groundMap.GetCellCenterWorld(cell));
        }

        return positions;
    }

    private void ClearEnemies()
    {
        foreach (GameObject enemy in m_spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        m_spawnedEnemies.Clear();
    }
}
