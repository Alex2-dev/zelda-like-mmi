using UnityEngine;

/// <summary>
/// Boss à deux phases :
/// - Phase 1 : fonce sur le joueur, attaque au corps à corps.
/// - Phase 2 (déclenché quand HP tombe à 0) : regagne de la vie, s'éloigne, tire des projectiles.
///
/// SETUP :
/// - Ajouter sur le prefab boss avec Rigidbody2D (Dynamic), Collider2D, EnemyHealth, Animator (optionnel).
/// - Assigner m_projectilePrefab (prefab avec BossProjectile.cs).
/// - Régler les stats dans l'Inspector.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class BossBehavior : MonoBehaviour
{
    // ── Phase 1 ──────────────────────────────────────────────────────────────
    [Header("Phase 1 — Corps à corps")]
    public float m_phase1Speed       = 3f;
    public float m_meleeDamage       = 20f;
    public float m_meleeCooldown     = 1f;
    public float m_meleeRange        = 1.2f;

    // ── Phase 2 ──────────────────────────────────────────────────────────────
    [Header("Phase 2 — Distance")]
    public float m_phase2Speed       = 2f;
    public float m_phase2MaxHealth   = 150f;   // HP remplis au début de la phase 2
    public float m_keepDistance      = 30f;    // distance min avec le joueur (fuit en dessous)
    public float m_projectileDamage  = 15f;
    public float m_projectileSpeed   = 8f;
    public float m_projectileRange   = 350f;  // couvre toute la salle (diagonale ~568 unités)
    public float m_shootCooldown     = 1.5f;
    public int   m_projectilesPerBurst = 3;    // nombre de projectiles par salve
    public float m_burstAngle        = 30f;    // angle total de dispersion de la salve
    public GameObject m_projectilePrefab;

    // ── Transition ────────────────────────────────────────────────────────────
    [Header("Transition de phase")]
    public float m_invincibilityDuration = 2f; // temps invincible pendant la transition
    public Color m_phase2Color = new Color(0.6f, 0f, 1f); // couleur violette en phase 2

    [Header("Zone du boss")]
    [Tooltip("SpawnZone qui délimite la zone du boss — il ne sortira pas de cette zone")]
    public SpawnZone m_bossZone;

    [Header("SFX")]
    [SerializeField] AudioClip m_sfxMeleeAttack;
    [SerializeField] AudioClip m_sfxShoot;
    [SerializeField] AudioClip m_sfxPhaseTransition;
    [SerializeField] AudioClip m_sfxDeath;

    // ── Références ────────────────────────────────────────────────────────────
    private enum Phase { Phase1, Transitioning, Phase2 }
    private Phase        m_phase = Phase.Phase1;
    private Rigidbody2D  m_rb;
    private EnemyHealth  m_health;
    private Transform    m_player;
    private Animator     m_animator;
    private SpriteRenderer m_renderer;

    private float m_lastMeleeTime   = -999f;
    private float m_lastShootTime   = -999f;
    private float m_transitionTimer = 0f;
    private bool  m_isInvincible    = false;

    void Awake()
    {
        m_rb       = GetComponent<Rigidbody2D>();
        m_health   = GetComponent<EnemyHealth>();
        m_animator = GetComponent<Animator>();
        m_renderer = GetComponent<SpriteRenderer>();

        m_rb.gravityScale  = 0f;
        m_rb.freezeRotation = true;

        // Désactiver EnemyBehavior pour éviter le conflit de vélocité
        EnemyBehavior eb = GetComponent<EnemyBehavior>();
        if (eb != null) eb.enabled = false;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) m_player = playerObj.transform;

        // Intercept les dégâts
        m_health.OnDeath += OnBossDeath;
    }

    void OnDestroy()
    {
        if (m_health != null) m_health.OnDeath -= OnBossDeath;
    }

    void FixedUpdate()
    {
        if (m_player == null) return;

        switch (m_phase)
        {
            case Phase.Phase1:      UpdatePhase1(); break;
            case Phase.Transitioning: UpdateTransition(); break;
            case Phase.Phase2:      UpdatePhase2(); break;
        }
    }

    // ── Phase 1 : fonce sur le joueur ────────────────────────────────────────

    private void UpdatePhase1()
    {
        // N'attaque que si le joueur est dans la zone
        if (m_bossZone != null && !m_bossZone.IsPlayerInside)
        {
            m_rb.velocity = Vector2.zero;
            return;
        }

        Vector2 dir = ((Vector2)m_player.position - (Vector2)transform.position).normalized;

        // Vérifie si avancer vers le joueur nous ferait sortir de la zone
        Vector2 nextPos = (Vector2)transform.position + dir * m_phase1Speed * Time.fixedDeltaTime;
        if (m_bossZone != null && !m_bossZone.ContainsPosition(nextPos))
        {
            // Reste dans la zone : s'arrête ou longe le bord
            Vector2 slideX = new Vector2(dir.x, 0f);
            Vector2 slideY = new Vector2(0f, dir.y);
            if (m_bossZone.ContainsPosition((Vector2)transform.position + slideX * m_phase1Speed * Time.fixedDeltaTime))
                dir = slideX.normalized;
            else if (m_bossZone.ContainsPosition((Vector2)transform.position + slideY * m_phase1Speed * Time.fixedDeltaTime))
                dir = slideY.normalized;
            else
                dir = Vector2.zero;
        }

        m_rb.velocity = dir * m_phase1Speed;
        UpdateAnimator(dir);

        float dist = Vector2.Distance(transform.position, m_player.position);
        if (dist <= m_meleeRange && Time.time - m_lastMeleeTime >= m_meleeCooldown)
            MeleeAttack();
    }

    private void MeleeAttack()
    {
        m_lastMeleeTime = Time.time;
        AudioManager.instance?.PlaySound(m_sfxMeleeAttack);
        PlayerStats stats = m_player.GetComponent<PlayerStats>();
        if (stats != null) stats.TakeDamage(m_meleeDamage);
    }

    // ── Transition ────────────────────────────────────────────────────────────

    private void StartTransition()
    {
        Debug.Log("[BossBehavior] Transition vers Phase 2 démarrée !");
        AudioManager.instance?.PlaySound(m_sfxPhaseTransition);
        m_phase          = Phase.Transitioning;
        m_isInvincible   = true;
        m_transitionTimer = 0f;
        m_rb.velocity    = Vector2.zero;

        // Flash visuel
        if (m_renderer != null) m_renderer.color = Color.white;
    }

    private void UpdateTransition()
    {
        m_transitionTimer += Time.fixedDeltaTime;

        // Flash clignotant
        if (m_renderer != null)
        {
            float t = Mathf.PingPong(m_transitionTimer * 6f, 1f);
            m_renderer.color = Color.Lerp(Color.white, m_phase2Color, t);
        }

        if (m_transitionTimer >= m_invincibilityDuration)
        {
            // Passe en phase 2
            Debug.Log("[BossBehavior] Phase 2 active !");
            m_phase        = Phase.Phase2;
            m_isInvincible = false;
            m_health.SetHealth(m_phase2MaxHealth);
            if (m_renderer != null) m_renderer.color = m_phase2Color;
        }
    }

    // ── Phase 2 : s'éloigne et tire ──────────────────────────────────────────

    private float m_debugLogTimer = 0f;

    private void UpdatePhase2()
    {
        // Ne tire que si le joueur est dans la zone
        bool playerInZone = m_bossZone == null || m_bossZone.IsPlayerInside;

        float dist = Vector2.Distance(transform.position, m_player.position);
        Vector2 dir = ((Vector2)m_player.position - (Vector2)transform.position).normalized;

        m_debugLogTimer -= Time.fixedDeltaTime;
        if (m_debugLogTimer <= 0f)
        {
            m_debugLogTimer = 1f;
            Debug.Log($"[BossBehavior] Phase2 — dist={dist:F1}  keepDist={m_keepDistance}  playerInZone={playerInZone}  timeSinceShot={(Time.time - m_lastShootTime):F1}s  prefab={(m_projectilePrefab == null ? "NULL" : m_projectilePrefab.name)}");
        }

        if (!playerInZone)
        {
            m_rb.velocity = Vector2.zero;
            return;
        }

        // Phase 2 : fuit toujours le joueur
        Vector2 move = -dir * m_phase2Speed;

        // Contraindre à la zone
        if (m_bossZone != null && move != Vector2.zero)
        {
            Vector2 nextPos = (Vector2)transform.position + move * Time.fixedDeltaTime;
            if (!m_bossZone.ContainsPosition(nextPos))
                move = Vector2.zero;
        }

        m_rb.velocity = move;

        UpdateAnimator(-dir);

        // Tir en salve
        if (Time.time - m_lastShootTime >= m_shootCooldown)
            ShootBurst(dir);
    }

    private void ShootBurst(Vector2 dir)
    {
        m_lastShootTime = Time.time;
        AudioManager.instance?.PlaySound(m_sfxShoot);
        if (m_projectilePrefab == null)
        {
            Debug.LogWarning("[BossBehavior] m_projectilePrefab non assigné — aucun projectile tiré !", this);
            return;
        }

        Debug.Log($"[BossBehavior] ShootBurst vers {dir}");
        for (int i = 0; i < m_projectilesPerBurst; i++)
        {
            float angle = -m_burstAngle / 2f + m_burstAngle * i / Mathf.Max(1, m_projectilesPerBurst - 1);
            Vector2 shootDir = Quaternion.Euler(0f, 0f, angle) * dir;

            GameObject proj = Instantiate(m_projectilePrefab, transform.position, Quaternion.identity);
            BossProjectile bp = proj.GetComponent<BossProjectile>();
            if (bp != null)
            {
                bp.Launch(shootDir, m_projectileSpeed, m_projectileDamage, m_projectileRange);
                Debug.Log($"[BossBehavior] Projectile {i} lancé — dir={shootDir}  speed={m_projectileSpeed}  range={m_projectileRange}");
            }
            else
            {
                Debug.LogError($"[BossBehavior] BossProjectile script MANQUANT sur le prefab '{m_projectilePrefab.name}' !");
            }
        }
    }

    // ── Mort / transition ─────────────────────────────────────────────────────

    private void OnBossDeath()
    {
        if (m_phase == Phase.Phase1)
        {
            // Pas mort : transition vers phase 2
            StartTransition();
        }
        else if (m_phase == Phase.Phase2)
        {
            AudioManager.instance?.PlaySound(m_sfxDeath);

            if (m_bossZone != null)
                m_bossZone.GetComponent<BossMusicTrigger>()?.OnBossDefeated();

            Destroy(gameObject);
            GameManager.Instance?.TriggerEndGame();
        }
    }

    // ── Dégâts (bloqués pendant la transition) ────────────────────────────────

    public bool IsInvincible() => m_isInvincible;

    // ── Animator ─────────────────────────────────────────────────────────────

    private void UpdateAnimator(Vector2 dir)
    {
        if (m_animator == null || dir == Vector2.zero) return;
        if (HasParameter("Direction"))
        {
            int direction = Mathf.Abs(dir.x) >= Mathf.Abs(dir.y)
                ? (dir.x > 0 ? 3 : 2)
                : (dir.y > 0 ? 1 : 0);
            m_animator.SetInteger("Direction", direction);
        }
    }

    private bool HasParameter(string paramName)
    {
        foreach (AnimatorControllerParameter p in m_animator.parameters)
            if (p.name == paramName) return true;
        return false;
    }
}
