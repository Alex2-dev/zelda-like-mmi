using TMPro;
using UnityEngine;

/// <summary>
/// Objet du monde représentant un message posé par un joueur (style Dark Souls).
///
/// Setup du préfab :
///   - SpriteRenderer  : sprite lumineux (ex. cercle blanc/doré)
///   - CircleCollider2D: isTrigger = true, radius ≈ 1
///   - (optionnel) enfant "Prompt" avec un TMP_Text "[ Espace ] Lire"
///   - Ce script en composant
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class MessageObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_glowRenderer;
    [SerializeField] private GameObject     m_promptObject; // ex. "[ Espace ] Lire"

    private string m_text;
    private bool   m_playerNearby;

    // ── Initialisation ────────────────────────────────────────────────────────

    private void Awake()
    {
        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;

        if (m_promptObject != null)
            m_promptObject.SetActive(false);
    }

    /// <summary>Appelé par MessageManager après Instantiate.</summary>
    public void Init(string text)
    {
        m_text = text;
    }

    // ── Mise à jour ───────────────────────────────────────────────────────────

    private void Update()
    {
        if (!m_playerNearby || MessageUI.IsOpen) return;

        bool readPressed = InputManager.Instance != null
            ? InputManager.Instance.AttackPressed
            : Input.GetKeyDown(KeyCode.Space);

        if (readPressed)
            MessageManager.Instance?.ShowReadUI(m_text);
    }

    /// <summary>Animation de pulsation lumineuse (utilise unscaledTime pour résister à timeScale = 0).</summary>
    private void LateUpdate()
    {
        if (m_glowRenderer == null) return;
        float alpha = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 2.5f);
        Color c = m_glowRenderer.color;
        c.a = alpha;
        m_glowRenderer.color = c;
    }

    // ── Trigger ───────────────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        m_playerNearby = true;
        if (m_promptObject != null)
            m_promptObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        m_playerNearby = false;
        if (m_promptObject != null)
            m_promptObject.SetActive(false);
    }
}
