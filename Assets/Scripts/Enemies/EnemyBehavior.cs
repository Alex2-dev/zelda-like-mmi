using UnityEngine;

/// <summary>
/// IA d'ennemi basique : détecte le joueur, le pourchasse, lui inflige des dégâts au contact.
/// SETUP :
/// - Ajouter sur le prefab ennemi avec SpriteRenderer, Rigidbody2D, Collider2D, EnemyHealth.
/// - Cocher "Is Trigger" sur le Collider2D pour la détection de contact.
/// - Ajouter un second Collider2D (non-trigger) pour la physique.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyBehavior : MonoBehaviour
{
    [Header("Déplacement")]
    public float m_speed = 2f;
    public float m_separationRadius = 0.6f;
    public float m_separationForce  = 3f;

    [Header("Dégâts")]
    public float m_damage = 10f;
    public float m_damageCooldown = 1f;

    private Rigidbody2D m_rb;
    private Transform m_player;
    private float m_lastDamageTime = -999f;

    void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_rb.gravityScale = 0f;
        m_rb.freezeRotation = true;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            m_player = playerObj.transform;
    }

    void FixedUpdate()
    {
        if (m_player == null) return;

        Vector2 dir = ((Vector2)m_player.position - (Vector2)transform.position).normalized;
        m_rb.velocity = dir * m_speed;

        // Séparation : pousse directement la position si trop proche d'un autre ennemi
        EnemyBehavior[] all = FindObjectsOfType<EnemyBehavior>();
        foreach (EnemyBehavior other in all)
        {
            if (other == this) continue;
            Vector2 diff = (Vector2)transform.position - (Vector2)other.transform.position;
            float dist = diff.magnitude;
            if (dist < m_separationRadius && dist > 0.001f)
                transform.position += (Vector3)(diff.normalized * (m_separationRadius - dist) * 0.5f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void TryDamagePlayer(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - m_lastDamageTime < m_damageCooldown) return;

        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.TakeDamage(m_damage);
            m_lastDamageTime = Time.time;
        }
    }

}
