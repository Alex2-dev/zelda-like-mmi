using System.Collections;
using UnityEngine;

/// <summary>
/// Gère la santé du joueur.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Santé")]
    public float m_maxHealth = 100f;

    [Header("Course")]
    [Tooltip("Multiplicateur de vitesse quand Shift est pressé")]
    public float m_runMultiplier = 1.8f;

    [Header("Debug (test uniquement)")]
    [Tooltip("Vie de départ — modifiable dans l'Inspector pour tester")]
    public float m_startHealth = -1f; // -1 = utilise m_maxHealth

    [SerializeField] private float m_currentHealth;
    private Coroutine m_healCoroutine;

    public float CurrentHealth => m_currentHealth;
    public float MaxHealth => m_maxHealth;

    void Start()
    {
        m_currentHealth = (m_startHealth > 0) ? m_startHealth : m_maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (m_currentHealth <= 0f) return;
        m_currentHealth = Mathf.Max(m_currentHealth - amount, 0f);
        if (m_currentHealth <= 0f)
            DeathScreenUI.Instance?.Show();
    }

    public void Heal(float amount)
    {
        m_currentHealth = Mathf.Min(m_currentHealth + amount, m_maxHealth);
    }

    public void StartHealOverTime(float perSecond, float duration)
    {
        if (m_healCoroutine != null)
            StopCoroutine(m_healCoroutine);
        m_healCoroutine = StartCoroutine(HealOverTimeCoroutine(perSecond, duration));
    }

    private IEnumerator HealOverTimeCoroutine(float perSecond, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
            Heal(perSecond);
        }
        m_healCoroutine = null;
    }

    public float GetHealthNormalized()
    {
        return m_currentHealth / m_maxHealth;
    }

    /// <summary>Restaure la santé depuis une sauvegarde.</summary>
    public void RestoreHealth(float current, float max)
    {
        if (max > 0f) m_maxHealth = max;
        m_currentHealth = Mathf.Clamp(current, 0f, m_maxHealth);
    }
}
