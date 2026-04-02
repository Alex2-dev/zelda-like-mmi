using UnityEngine;

/// <summary>
/// Définit une zone de spawn ou d'exclusion pour les ennemis.
/// Utilise un BoxCollider2D trigger pour détecter le joueur de façon fiable.
/// SETUP :
/// - Ajouter sur un GameObject enfant de l'EnemySpawner.
/// - Ajuster Zone Size et Density Percentage.
/// - Cocher Is Exclusion Zone pour bloquer le spawn et le mouvement dans cette zone.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class SpawnZone : MonoBehaviour
{
    public static readonly System.Collections.Generic.List<SpawnZone> ExclusionZones =
        new System.Collections.Generic.List<SpawnZone>();

    [Header("Configuration de la zone")]
    [SerializeField] private Vector2 m_zoneSize = Vector2.one * 5f;
    [SerializeField] private bool m_isExclusionZone = false;

    [Header("Densité (ignoré si zone d'exclusion)")]
    [Range(0f, 100f)]
    [SerializeField] private float m_densityPercentage = 50f;

    [Header("Visualisation")]
    [SerializeField] private bool m_showGizmo = true;

    // Indique si le joueur est actuellement dans cette zone (via trigger physique)
    public bool IsPlayerInside { get; private set; }

    private BoxCollider2D m_collider;

    void Awake()
    {
        m_collider          = GetComponent<BoxCollider2D>();
        m_collider.isTrigger = true;
        m_collider.size      = m_zoneSize;
    }

    void OnEnable()  { if (m_isExclusionZone) ExclusionZones.Add(this); }
    void OnDisable() { ExclusionZones.Remove(this); IsPlayerInside = false; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) IsPlayerInside = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) IsPlayerInside = false;
    }

    void OnValidate()
    {
        // Met à jour le collider en temps réel quand on change la taille dans l'Inspector
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null) col.size = m_zoneSize;
    }

    public System.Collections.Generic.List<Vector3> GetValidSpawnPositions(
        UnityEngine.Tilemaps.Tilemap groundMap,
        UnityEngine.Tilemaps.Tilemap obstacleMap)
    {
        var positions = new System.Collections.Generic.List<Vector3>();
        if (groundMap == null) return positions;

        foreach (Vector3Int cell in groundMap.cellBounds.allPositionsWithin)
        {
            if (!groundMap.HasTile(cell)) continue;
            if (obstacleMap != null && obstacleMap.HasTile(cell)) continue;

            Vector3 worldPos = groundMap.GetCellCenterWorld(cell);
            if (ContainsPosition(worldPos))
                positions.Add(worldPos);
        }
        return positions;
    }

    public bool ContainsPosition(Vector3 worldPos)
    {
        Vector3 local = worldPos - transform.position;
        return Mathf.Abs(local.x) <= m_zoneSize.x * 0.5f &&
               Mathf.Abs(local.y) <= m_zoneSize.y * 0.5f;
    }

    public float GetDensityPercentage() => m_densityPercentage;
    public bool  IsExclusionZone()      => m_isExclusionZone;
    public Vector2 GetZoneSize()        => m_zoneSize;

    void OnDrawGizmos()
    {
        if (!m_showGizmo) return;
        Vector3 size = new Vector3(m_zoneSize.x, m_zoneSize.y, 0.1f);
        Gizmos.color = m_isExclusionZone ? new Color(1, 0, 0, 0.3f) : new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(transform.position, size);
        Gizmos.color = m_isExclusionZone ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, size);
    }
}
