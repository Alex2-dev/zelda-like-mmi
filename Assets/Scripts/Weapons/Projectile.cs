using UnityEngine;

/// <summary>
/// Balle générique tirée par les armes du joueur.
/// Utilise Transform.Translate (Rigidbody2D Kinematic) pour un déplacement cohérent
/// avec le suivi de distance par accumulation de vitesse*deltaTime.
///
/// SETUP du prefab :
/// - Ajouter Rigidbody2D (Body Type = Kinematic, Gravity Scale = 0)
/// - Ajouter CircleCollider2D (Is Trigger = coché, radius petit ex: 0.1)
/// - Assigner la layer "Projectile" au GameObject
/// - Dans Edit > Project Settings > Physics 2D : désactiver la collision entre "Projectile" et "Player"
/// </summary>
public class Projectile : MonoBehaviour
{
    private float m_damage;
    private float m_range;
    private float m_speed;
    private float m_traveledDistance;
    private Vector2 m_direction;
    private bool m_launched = false;

    /// <summary>Lance le projectile dans une direction donnée.</summary>
    public void Launch(Vector2 direction, float speed, float damage, float range)
    {
        m_direction = direction.normalized;
        m_damage = damage;
        m_range = range;
        m_speed = speed;
        m_traveledDistance = 0f;
        m_launched = true;
    }

    void Update()
    {
        if (!m_launched) return;
        float step = m_speed * Time.deltaTime;
        transform.Translate(m_direction * step, Space.World);
        m_traveledDistance += step;

        if (m_traveledDistance >= m_range)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        EnemyHealth health = other.GetComponent<EnemyHealth>();
        if (health != null)
            health.TakeDamage(m_damage);

        Destroy(gameObject);
    }
}
