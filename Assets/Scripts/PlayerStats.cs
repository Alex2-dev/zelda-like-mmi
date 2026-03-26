using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gère la santé et l'intégrité du joueur.
///
/// SETUP :
/// - Attacher ce script au GameObject "Player"
/// - Le Player doit avoir un CircleCollider2D en mode "Is Trigger" = coché (c'est ton rayon de vue)
/// - Sur les ennemis voulus, ajouter le script "IntegrityTrigger" sur leur collider
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Santé")]
    public float m_maxHealth = 100f;

    [Header("Intégrité")]
    public float m_maxIntegrity = 100f;

    [Tooltip("Points d'intégrité gagnés par seconde quand un ennemi est dans le rayon")]
    public float m_integrityIncreaseRate = 15f;

    [Tooltip("Points d'intégrité perdus par seconde quand aucun ennemi n'est dans le rayon")]
    public float m_integrityDecreaseRate = 8f;

    [Tooltip("Secondes sans voir d'ennemi avant que l'intégrité commence à diminuer")]
    public float m_integrityDecreaseDelay = 3f;

    [Header("Course (disponible en intégrité maximale)")]
    [Tooltip("Multiplicateur de vitesse quand Shift est pressé en intégrité maximale")]
    public float m_runMultiplier = 1.8f;

    [Header("Debug (test uniquement)")]
    [Tooltip("Vie de départ — modifiable dans l'Inspector pour tester")]
    public float m_startHealth = -1f; // -1 = utilise m_maxHealth

    // Valeurs courantes (SerializeField = visibles ET modifiables dans l'Inspector pendant le jeu)
    [SerializeField] private float m_currentHealth;
    [SerializeField] private float m_currentIntegrity;
    private float m_timeSinceLastEnemySeen;
    private Coroutine m_healCoroutine;
    private bool m_enemyVisible;
    private bool m_permanentIntegrity;

    // Compte le nombre d'ennemis actuellement dans le rayon de vue
    private int m_enemiesInRange = 0;

    private AlterEgoManager m_alterEgoManager;

    // Propriétés publiques en lecture seule
    public float CurrentHealth => m_currentHealth;
    public float CurrentIntegrity => m_currentIntegrity;
    public float MaxHealth => m_maxHealth;
    public float MaxIntegrity => m_maxIntegrity;

    void Awake()
    {
        m_alterEgoManager = GetComponent<AlterEgoManager>();
    }

    void Start()
    {
        m_currentHealth = (m_startHealth > 0) ? m_startHealth : m_maxHealth;
        m_currentIntegrity = 0f;
        m_timeSinceLastEnemySeen = 0f;
        m_enemyVisible = false;
        m_enemiesInRange = 0;

        LevelSettings levelSettings = FindObjectOfType<LevelSettings>();
        if (levelSettings != null)
            m_permanentIntegrity = levelSettings.m_permanentIntegrity;

        if (m_permanentIntegrity)
            m_currentIntegrity = m_maxIntegrity;
    }

    void Update()
    {
        if (m_permanentIntegrity)
        {
            m_currentIntegrity = m_maxIntegrity;
            m_enemyVisible = true;
            return;
        }

        // L'ennemi est visible si au moins 1 IntegrityTrigger est dans le CircleCollider2D
        m_enemyVisible = m_enemiesInRange > 0;

        if (m_enemyVisible)
        {
            m_timeSinceLastEnemySeen = 0f;
            m_currentIntegrity = Mathf.Min(
                m_currentIntegrity + m_integrityIncreaseRate * Time.deltaTime,
                m_maxIntegrity
            );
        }
        else
        {
            m_timeSinceLastEnemySeen += Time.deltaTime;
            if (m_timeSinceLastEnemySeen >= m_integrityDecreaseDelay)
            {
                m_currentIntegrity = Mathf.Max(
                    m_currentIntegrity - m_integrityDecreaseRate * Time.deltaTime,
                    0f
                );
            }
        }
    }

    // Un IntegrityTrigger entre dans le CircleCollider2D du joueur
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<IntegrityTrigger>() != null)
            m_enemiesInRange++;
    }

    // Un IntegrityTrigger sort du CircleCollider2D du joueur
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<IntegrityTrigger>() != null)
            m_enemiesInRange = Mathf.Max(0, m_enemiesInRange - 1);
    }

    public void TakeDamage(float amount)
    {
        m_currentHealth = Mathf.Max(m_currentHealth - amount, 0f);
    }

    public void Heal(float amount)
    {
        m_currentHealth = Mathf.Min(m_currentHealth + amount, m_maxHealth);
    }

    public bool IsAtFullIntegrity()
    {
        return m_currentIntegrity >= m_maxIntegrity;
    }

    public bool IsEnemyVisible()
    {
        return m_enemyVisible;
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

    public float GetIntegrityNormalized()
    {
        return m_currentIntegrity / m_maxIntegrity;
    }

}
