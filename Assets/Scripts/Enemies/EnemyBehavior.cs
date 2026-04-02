using UnityEngine;

/// <summary>
/// IA d'ennemi basique avec états Patrol / Chase / Return.
/// - Patrol  : se déplace aléatoirement dans sa zone de spawn.
/// - Chase   : pourchasse le joueur uniquement si le joueur est dans la zone de spawn.
/// - Return  : retourne vers le centre de la zone si le joueur en sort.
/// SETUP :
/// - Ajouter sur le prefab ennemi avec SpriteRenderer, Rigidbody2D, Collider2D, EnemyHealth.
/// - Cocher "Is Trigger" sur le Collider2D pour la détection de contact.
/// - Ajouter un second Collider2D (non-trigger) pour la physique.
/// - Optionnel : ajouter un Animator avec paramètre Direction (int: 0=S,1=N,2=W,3=E).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyBehavior : MonoBehaviour
{
    [Header("Déplacement")]
    public float m_speed = 2f;
    public float m_separationRadius = 0.6f;
    public float m_separationForce  = 3f;

    [Header("Détection")]
    public float m_detectionRange = 6f;

    [Header("Patrol")]
    public float m_patrolSpeed   = 1f;
    public float m_patrolWaitMin = 1f;
    public float m_patrolWaitMax = 3f;


    [Header("Dégâts")]
    public float m_damage         = 10f;
    public float m_damageCooldown = 1f;

    // Zone assignée par l'EnemySpawner (optionnel)
    [HideInInspector] public SpawnZone m_homeZone;

    [Header("Zone de patrouille")]
    [Tooltip("Rayon max autour du point de spawn — l'ennemi ne s'éloigne pas au-delà")]
    public float m_patrolRadius = 8f;


    private Vector2 m_spawnPoint;

    private enum State { Patrol, Chase, Return }
    private State     m_state       = State.Patrol;
    private Vector2   m_patrolTarget;
    private float     m_patrolTimer;

    private Rigidbody2D m_rb;
    private Transform   m_player;
    private float       m_lastDamageTime = -999f;
    private Animator    m_animator;

    // Gestion des murs
    private Vector2 m_wallNormal    = Vector2.zero;
    private float   m_wallSteerTime = 0f;


    // Cache des voisins
    private static readonly System.Collections.Generic.List<EnemyBehavior> s_allEnemies =
        new System.Collections.Generic.List<EnemyBehavior>();

    void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_rb.gravityScale = 0f;
        m_rb.freezeRotation = true;
        m_animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            m_player = playerObj.transform;

        m_patrolTarget = transform.position;
    }

    void OnEnable()
    {
        s_allEnemies.Add(this);
        m_spawnPoint   = transform.position;
        m_patrolTarget = transform.position;

    }
    void OnDisable() { s_allEnemies.Remove(this); }

    void FixedUpdate()
    {
        if (m_player == null) return;

        UpdateState();

        Vector2 moveDir = ComputeMoveDirection();
        Vector2 desiredVelocity = ApplySeparation(moveDir);
        desiredVelocity = ApplyExclusionZones(desiredVelocity);

        m_rb.velocity = desiredVelocity;
        UpdateAnimator(desiredVelocity.normalized);
    }

    // ── État ────────────────────────────────────────────────────────────────

    private void UpdateState()
    {
        float distToPlayer  = Vector2.Distance(transform.position, m_player.position);
        float distToSpawn   = Vector2.Distance(transform.position, m_spawnPoint);
        bool  playerNear    = distToPlayer <= m_detectionRange;
        bool  tooFarFromHome = distToSpawn  > m_patrolRadius;

        switch (m_state)
        {
            case State.Patrol:
                if (playerNear)
                    m_state = State.Chase;
                break;

            case State.Chase:
                // Arrête la poursuite si le joueur est trop loin du point de spawn
                if (tooFarFromHome || distToPlayer > m_detectionRange * 1.5f)
                    m_state = State.Return;
                break;

            case State.Return:
                if (distToSpawn < m_separationRadius * 2f)
                    m_state = State.Patrol;
                else if (playerNear && !tooFarFromHome)
                    m_state = State.Chase;
                break;
        }
    }

    // ── Direction de déplacement ─────────────────────────────────────────────

    private Vector2 ComputeMoveDirection()
    {
        switch (m_state)
        {
            case State.Chase:
            {
                Vector2 toPlayer = ((Vector2)m_player.position - (Vector2)transform.position).normalized;
                if (m_wallSteerTime > 0f)
                {
                    m_wallSteerTime -= Time.fixedDeltaTime;
                    // Glisse le long du mur en combinant direction joueur + normale du mur
                    Vector2 slide = Vector2.Perpendicular(m_wallNormal);
                    if (Vector2.Dot(slide, toPlayer) < 0f) slide = -slide;
                    toPlayer = (toPlayer + slide).normalized;
                }
                return toPlayer * m_speed;
            }

            case State.Return:
                return (m_spawnPoint - (Vector2)transform.position).normalized * m_patrolSpeed;

            default: // Patrol
                return ComputePatrolDirection();
        }
    }


    private Vector2 ComputePatrolDirection()
    {
        m_patrolTimer -= Time.fixedDeltaTime;
        if (m_patrolTimer <= 0f || Vector2.Distance(transform.position, m_patrolTarget) < m_separationRadius)
        {
            m_patrolTimer = Random.Range(m_patrolWaitMin, m_patrolWaitMax);
            m_patrolTarget = PickPatrolTarget();
        }
        Vector2 dir = m_patrolTarget - (Vector2)transform.position;
        if (dir.magnitude < m_separationRadius) return Vector2.zero;
        return dir.normalized * m_patrolSpeed;
    }

    private Vector2 PickPatrolTarget()
    {
        for (int i = 0; i < 15; i++)
        {
            Vector2 candidate = m_spawnPoint + Random.insideUnitCircle * m_patrolRadius;
            if (Vector2.Distance(candidate, transform.position) > 1.5f)
                return candidate;
        }
        return m_spawnPoint;
    }

    // ── Séparation ───────────────────────────────────────────────────────────

    private Vector2 ApplySeparation(Vector2 baseVelocity)
    {
        Vector2 separation = Vector2.zero;
        foreach (EnemyBehavior other in s_allEnemies)
        {
            if (other == this) continue;
            Vector2 diff = (Vector2)transform.position - (Vector2)other.transform.position;
            float dist = diff.magnitude;
            if (dist < m_separationRadius && dist > 0.001f)
                separation += diff.normalized * ((m_separationRadius - dist) / m_separationRadius) * m_separationForce;
        }
        return baseVelocity + separation;
    }

    // ── Zones d'exclusion ────────────────────────────────────────────────────

    private Vector2 ApplyExclusionZones(Vector2 desiredVelocity)
    {
        Vector2 nextPos = (Vector2)transform.position + desiredVelocity * Time.fixedDeltaTime;
        foreach (SpawnZone ex in SpawnZone.ExclusionZones)
        {
            if (!ex.ContainsPosition(nextPos)) continue;

            Vector2 slideX = new Vector2(desiredVelocity.x, 0f);
            Vector2 slideY = new Vector2(0f, desiredVelocity.y);

            if (!ex.ContainsPosition((Vector2)transform.position + slideX * Time.fixedDeltaTime))
                return slideX;
            if (!ex.ContainsPosition((Vector2)transform.position + slideY * Time.fixedDeltaTime))
                return slideY;
            return Vector2.zero;
        }
        return desiredVelocity;
    }

    // ── Animator ─────────────────────────────────────────────────────────────

    private void UpdateAnimator(Vector2 dir)
    {
        if (m_animator == null || dir.magnitude < 0.2f) return;

        int direction;
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            direction = dir.x > 0 ? 3 : 2;
        else
            direction = dir.y > 0 ? 1 : 0;
        m_animator.SetInteger("Direction", direction);
    }

    // ── Dégâts ───────────────────────────────────────────────────────────────

    void OnCollisionEnter2D(Collision2D col)
    {
        m_wallNormal    = col.contacts[0].normal;
        m_wallSteerTime = 0.4f;

        // Dans tous les cas : nouveau point de patrol
        m_patrolTimer  = 0f;
        m_patrolTarget = PickPatrolTarget();
    }

    void OnTriggerEnter2D(Collider2D other) { TryDamagePlayer(other); }
    void OnTriggerStay2D(Collider2D other)  { TryDamagePlayer(other); }

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
