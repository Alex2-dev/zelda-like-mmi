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

    void Start()
    {
        m_currentHealth = m_maxHealth;
    }

    public void TakeDamage(float amount)
    {
        m_currentHealth = Mathf.Max(m_currentHealth - amount, 0f);
        if (m_currentHealth <= 0f)
            Destroy(gameObject);
    }

    public float GetHealthNormalized() => m_currentHealth / m_maxHealth;
}
