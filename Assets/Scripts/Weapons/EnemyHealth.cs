using UnityEngine;

/// <summary>
/// Gère la santé d'un ennemi. Ajouter sur le GameObject ennemi.
/// SETUP :
/// - Ajouter ce script sur le GameObject ennemi
/// - Configurer m_maxHealth
/// - S'assurer que le tag "Enemy" est assigné au GameObject
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("Santé")]
    public float m_maxHealth = 100f;

    [SerializeField] private float m_currentHealth;

    [Header("Drops")]
    [Tooltip("Prefabs pouvant être droppés à la mort")]
    public GameObject[] m_dropPrefabs;

    [Tooltip("Probabilité de drop pour chaque prefab (0 à 1)")]
    [Range(0f, 1f)]
    public float[] m_dropChances;

    [Tooltip("Légère dispersion des drops (rayon en unités)")]
    public float m_dropScatter = 0.4f;

    void Start()
    {
        m_currentHealth = m_maxHealth;
    }

    /// Déclenché quand les HP tombent à 0 (avant destruction).
    public System.Action OnDeath;

    public void TakeDamage(float amount)
    {
        // Vérifie si un boss invincible
        BossBehavior boss = GetComponent<BossBehavior>();
        if (boss != null && boss.IsInvincible()) return;

        m_currentHealth = Mathf.Max(m_currentHealth - amount, 0f);
        if (m_currentHealth <= 0f)
        {
            OnDeath?.Invoke();
            // Si pas de boss (ou boss phase 2), détruit
            if (boss == null)
            {
                SpawnDrops();
                Destroy(gameObject);
            }
        }
    }

    public void SetHealth(float amount)
    {
        m_maxHealth     = amount;
        m_currentHealth = amount;
    }

    public float GetHealthNormalized() => m_currentHealth / m_maxHealth;

    private void SpawnDrops()
    {
        if (m_dropPrefabs == null) return;

        for (int i = 0; i < m_dropPrefabs.Length; i++)
        {
            if (m_dropPrefabs[i] == null) continue;

            float chance = (i < m_dropChances.Length) ? m_dropChances[i] : 1f;
            if (Random.value > chance) continue;

            Vector2 offset = Random.insideUnitCircle * m_dropScatter;
            Instantiate(m_dropPrefabs[i], (Vector2)transform.position + offset, Quaternion.identity);
        }
    }
}
