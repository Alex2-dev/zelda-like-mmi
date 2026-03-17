using System.Collections;
using UnityEngine;

/// <summary>
/// Comportement d'un piège posé par le joueur.
/// Démarre désarmé, s'arme après m_armDelay secondes.
/// Se déclenche uniquement sur les ennemis (tag "Enemy").
///
/// SETUP du prefab :
/// - Ajouter ce script au GameObject du piège
/// - Ajouter un CircleCollider2D (Is Trigger = coché)
/// - Le radius du CircleCollider2D doit correspondre à TrapData.m_triggerRadius
/// </summary>
public class TrapBehavior : MonoBehaviour
{
    public TrapData m_trapData;

    private bool m_isArmed = false;

    void Start()
    {
        if (m_trapData == null)
        {
            Debug.LogError("TrapBehavior : m_trapData non assigné sur " + gameObject.name, this);
            return;
        }
        m_isArmed = false;
        StartCoroutine(ArmAfterDelay(m_trapData.m_armDelay));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!m_isArmed) return;
        if (!other.CompareTag("Enemy")) return;

        EnemyHealth health = other.GetComponent<EnemyHealth>();
        if (health != null)
            health.TakeDamage(m_trapData.m_damage);

        if (m_trapData.m_destroyOnUse)
        {
            Destroy(gameObject);
        }
        else
        {
            m_isArmed = false;
            StartCoroutine(ArmAfterDelay(m_trapData.m_rearmDelay));
        }
    }

    private IEnumerator ArmAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        m_isArmed = true;
    }
}
