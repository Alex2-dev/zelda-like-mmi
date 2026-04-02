using UnityEngine;

/// <summary>
/// Projectile tiré par le boss en phase 2. Touche uniquement le joueur.
/// SETUP du prefab :
/// - Rigidbody2D : Body Type = Kinematic, Gravity Scale = 0
/// - CircleCollider2D : Is Trigger = coché
/// - Layer : "BossProjectile" (désactiver collision avec "Enemy" dans Physics 2D)
/// </summary>
public class BossProjectile : MonoBehaviour
{
    private float   m_damage;
    private float   m_range;
    private float   m_speed;
    private Vector2 m_direction;
    private float   m_traveled;
    private bool    m_launched;

    public void Launch(Vector2 direction, float speed, float damage, float range)
    {
        m_direction = direction.normalized;
        m_speed     = speed;
        m_damage    = damage;
        m_range     = range;
        m_launched  = true;

        // Oriente le sprite selon la direction
        float angle = Mathf.Atan2(m_direction.y, m_direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
        if (!m_launched) return;
        float step = m_speed * Time.deltaTime;
        transform.Translate(m_direction * step, Space.World);
        m_traveled += step;
        if (m_traveled >= m_range) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats != null) stats.TakeDamage(m_damage);
        Destroy(gameObject);
    }
}
