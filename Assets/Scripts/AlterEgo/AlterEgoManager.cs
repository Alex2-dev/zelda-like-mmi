using System.Collections;
using UnityEngine;

/// <summary>
/// Gère la transformation alter ego du joueur.
///
/// SETUP :
/// - Attacher ce script au GameObject "Player"
/// - Assigner m_alterEgoSprite dans l'Inspector
/// - Glisser ce composant dans le champ m_alterEgoManager de PlayerBehavior
/// </summary>
public class AlterEgoManager : MonoBehaviour
{
    [Header("Alter Ego")]
    [Tooltip("Sprite affiché en forme alter ego")]
    public Sprite m_alterEgoSprite;

    [Header("Animation")]
    [Tooltip("Controller normal du joueur (Main_Character.controller)")]
    public RuntimeAnimatorController m_normalController;

    [Tooltip("Override controller alter ego (AlterEgo.overrideController)")]
    public RuntimeAnimatorController m_alterEgoController;

    [Tooltip("Durée maximale en alter ego (secondes)")]
    public float m_maxAlterEgoTime = 15f;

    [Tooltip("Dégâts infligés au retour forcé (timer écoulé)")]
    public float m_returnDamage = 10f;

    [Tooltip("Délai avant de pouvoir repasser en alter ego après retour (secondes)")]
    public float m_cooldown = 5f;

    [Header("Téléportation")]
    [Tooltip("Transform vide placé devant le joueur (enfant du Player, suit la direction)")]
    public Transform m_teleportTarget;

    [Tooltip("Distance de téléportation (unités Unity)")]
    public float m_teleportDistance = 3f;

    [Tooltip("Délai entre deux téléportations (secondes)")]
    public float m_dashCooldown = 1f;

    // État
    public bool IsAlterEgo { get; private set; }
    public bool IsDashing  { get; private set; }

    // Timers internes
    private float m_remainingAlterEgoTime;
    private float m_currentCooldown;
    private float m_currentDashCooldown;

    // Références
    private SpriteRenderer m_spriteRenderer;
    private Animator       m_animator;
    private PlayerStats    m_playerStats;
    private PlayerBehavior m_playerBehavior;
    private Rigidbody2D    m_rb2D;
    private Collider2D[]   m_colliders;

    // Sprite original (utilisé uniquement si pas d'Animator)
    private Sprite m_normalSprite;

    void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_animator       = GetComponent<Animator>();
        m_playerStats    = GetComponent<PlayerStats>();
        m_playerBehavior = GetComponent<PlayerBehavior>();
        m_rb2D           = GetComponent<Rigidbody2D>();
        m_colliders      = GetComponentsInChildren<Collider2D>();
        m_normalSprite   = m_spriteRenderer.sprite;
    }

    void Update()
    {
        // Décompte cooldown de transformation
        if (!IsAlterEgo && m_currentCooldown > 0f)
            m_currentCooldown -= Time.deltaTime;

        // Aucun input si une UI bloquante est ouverte
        if (MessageUI.IsOpen || InventoryUI.IsOpen) return;

        // Décompte cooldown de dash
        if (m_currentDashCooldown > 0f)
            m_currentDashCooldown -= Time.deltaTime;

        // Bascule alter ego
        bool alterEgoPressed = InputManager.Instance != null
            ? InputManager.Instance.AlterEgoPressed
            : Input.GetKeyDown(KeyCode.Tab);
        if (alterEgoPressed)
        {
            if (IsAlterEgo)
                ExitAlterEgo(forced: false);
            else if (m_currentCooldown <= 0f)
                EnterAlterEgo();
        }

        // Timer alter ego
        if (IsAlterEgo)
        {
            m_remainingAlterEgoTime -= Time.deltaTime;
            if (m_remainingAlterEgoTime <= 0f)
                ExitAlterEgo(forced: true);
        }

        // Dash / téléportation (alter ego uniquement)
        bool dashPressed = InputManager.Instance != null
            ? InputManager.Instance.DashPressed
            : Input.GetKeyDown(KeyCode.F);
        if (IsAlterEgo && dashPressed && m_currentDashCooldown <= 0f && !IsDashing)
            Teleport();
    }

    private void EnterAlterEgo()
    {
        IsAlterEgo = true;
        m_remainingAlterEgoTime = m_maxAlterEgoTime;
        if (m_animator != null)
            m_animator.runtimeAnimatorController = m_alterEgoController;
        else
            m_spriteRenderer.sprite = m_alterEgoSprite;
        HiddenEnemyMarker.SetAlterEgoMode(true);
    }

    private void ExitAlterEgo(bool forced)
    {
        IsAlterEgo = false;
        if (m_animator != null)
            m_animator.runtimeAnimatorController = m_normalController;
        else
            m_spriteRenderer.sprite = m_normalSprite;
        m_currentCooldown = m_cooldown;
        HiddenEnemyMarker.SetAlterEgoMode(false);

        if (forced && m_playerStats != null)
            m_playerStats.TakeDamage(m_returnDamage);
    }

    private void Teleport()
    {
        if (m_teleportTarget == null) return;

        Vector2 dir = m_playerBehavior.GetShootDirectionVector();
        Vector2 destination = (Vector2)transform.position + dir * m_teleportDistance;

        // Désactive la physique le temps du TP pour que MovePosition ne l'écrase pas
        m_rb2D.isKinematic = true;
        transform.position = destination;
        m_rb2D.position    = destination;
        m_rb2D.velocity    = Vector2.zero;
        m_rb2D.isKinematic = false;

        m_currentDashCooldown = m_dashCooldown;
    }
}
