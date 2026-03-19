// ============================================================
//  StalkerBehavior.cs — IA de l'ennemi chasseur de bugs
// ============================================================
//  SETUP Unity :
//  1. Créer un prefab ennemi avec Rigidbody2D, Collider2D, EnemyHealth.
//  2. Attacher ce script sur ce prefab.
//  3. Assigner ce prefab dans EventManager.m_stalkerEnemyPrefab
//     ou dans RoomContext.stalkerEnemyPrefab.
//
//  Comportement :
//  - Poursuit le joueur à une vitesse constante.
//  - Se détruit automatiquement quand la salle est désactivée.
//  - Notifie le RoomContext quand il est détruit (libère le slot stalker).
// ============================================================
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class StalkerBehavior : MonoBehaviour
{
    [Header("Paramètres de poursuite")]
    [Tooltip("Vitesse de déplacement du stalker (unités/s).")]
    public float m_speed = 1.8f;

    [Tooltip("Distance à partir de laquelle le stalker détecte le joueur.")]
    public float m_detectionRange = 12f;

    [Header("Glitch visuel")]
    [Tooltip("SpriteRenderer du stalker — son alpha clignote périodiquement.")]
    public SpriteRenderer m_spriteRenderer;

    // ── Références runtime ────────────────────────────────────
    private Transform   m_playerTransform = null;
    private Rigidbody2D m_rb;
    private RoomContext m_ownerRoom = null;
    private float       m_glitchTimer = 0f;

    void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();

        // Recherche le joueur dans la scène
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) m_playerTransform = player.transform;

        // Couleur rouge-glitch pour le distinguer des ennemis normaux
        if (m_spriteRenderer != null)
            m_spriteRenderer.color = new Color(1f, 0.2f, 0.2f);
    }

    /// <summary>Associe le stalker à la salle qui l'a spawné.</summary>
    public void SetTargetRoom(RoomContext room)
    {
        m_ownerRoom = room;
    }

    void Update()
    {
        // Clignotement périodique pour l'effet glitch
        m_glitchTimer += Time.deltaTime;
        if (m_spriteRenderer != null)
        {
            float flicker = Mathf.PerlinNoise(m_glitchTimer * 8f, 0f);
            Color c       = m_spriteRenderer.color;
            c.a           = Mathf.Clamp(flicker + 0.4f, 0.4f, 1f);
            m_spriteRenderer.color = c;
        }
    }

    void FixedUpdate()
    {
        if (m_playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, m_playerTransform.position);
        if (dist > m_detectionRange) return; // hors de portée → stationnaire

        // Déplacement vers le joueur
        Vector2 direction = ((Vector2)m_playerTransform.position - m_rb.position).normalized;
        m_rb.MovePosition(m_rb.position + direction * m_speed * Time.fixedDeltaTime);
    }

    void OnDestroy()
    {
        // Libère le slot stalker dans la salle
        if (m_ownerRoom != null)
            m_ownerRoom.hasStalkerActive = false;
    }

    void OnDisable()
    {
        // Si la salle est désactivée (transition), le stalker se détruit proprement
        if (m_ownerRoom != null && !m_ownerRoom.gameObject.activeSelf)
            Destroy(gameObject);
    }
}
