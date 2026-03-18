// ============================================================
//  GravityZoneTrigger.cs — Zone d'attraction anormale locale
// ============================================================
//  SETUP Unity :
//  1. Créer un GameObject avec un Collider2D en mode "Is Trigger".
//  2. Attacher ce script.
//  3. Ajouter ce Collider2D dans la liste gravityZones du RoomContext.
//
//  L'événement "decor_gravity_zone" appellera Activate() sur ce script.
//  La zone attire le joueur vers son centre pendant la durée définie.
// ============================================================
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GravityZoneTrigger : MonoBehaviour
{
    [Header("Visuels (optionnel)")]
    [Tooltip("SpriteRenderer ou particules indiquant la zone active.")]
    public GameObject m_visualFX;

    // ── État runtime ──────────────────────────────────────────
    private bool         m_isActive      = false;
    private float        m_attractForce  = 4f;
    private Rigidbody2D  m_playerRb      = null;

    void Awake()
    {
        if (m_visualFX != null) m_visualFX.SetActive(false);
    }

    /// <summary>
    /// Active la zone d'attraction pendant <paramref name="duration"/> secondes.
    /// </summary>
    /// <param name="duration">Durée en secondes.</param>
    /// <param name="attractForce">Force d'attraction vers le centre (unités/s²).</param>
    public void Activate(float duration, float attractForce)
    {
        if (m_isActive) return; // déjà active

        m_attractForce = attractForce;
        m_isActive     = true;

        if (m_visualFX != null) m_visualFX.SetActive(true);

        StartCoroutine(DeactivateAfter(duration));
    }

    private IEnumerator DeactivateAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        m_isActive  = false;
        m_playerRb  = null;

        if (m_visualFX != null) m_visualFX.SetActive(false);
    }

    void FixedUpdate()
    {
        if (!m_isActive || m_playerRb == null) return;

        // Attire le joueur vers le centre de la zone
        Vector2 toCenter = (Vector2)transform.position - m_playerRb.position;

        // Normalise pour garder une force constante quelle que soit la distance
        if (toCenter.sqrMagnitude > 0.01f)
            m_playerRb.AddForce(toCenter.normalized * m_attractForce, ForceMode2D.Force);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (m_isActive && other.CompareTag("Player"))
            m_playerRb = other.GetComponent<Rigidbody2D>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            m_playerRb = null;
    }
}
