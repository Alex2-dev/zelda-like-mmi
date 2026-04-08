using UnityEngine;

/// <summary>
/// Effet visuel + sonore pour les objets au sol :
/// - Pulsation de couleur jaune (glow)
/// - Son ambiant en boucle via un AudioSource local
///
/// SETUP :
/// - Ajouter ce script sur le prefab de l'item au sol
/// - Assigner m_ambientClip si tu veux un son en boucle
/// - Le SpriteRenderer est trouvé automatiquement
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class GroundItemFX : MonoBehaviour
{
    [Header("Glow")]
    [SerializeField] Color m_glowColor    = new Color(1f, 0.85f, 0f); // jaune
    [SerializeField] Color m_baseColor    = Color.white;
    [SerializeField] float m_pulseSpeed   = 2f;
    [SerializeField] float m_pulseMin     = 0.3f;
    [SerializeField] float m_pulseMax     = 1f;

    [Header("Son ambiant")]
    [SerializeField] AudioClip m_ambientClip;
    [SerializeField][Range(0f, 1f)] float m_ambientVolume = 0.4f;

    private SpriteRenderer m_renderer;
    private AudioSource    m_audioSource;

    void Awake()
    {
        m_renderer    = GetComponent<SpriteRenderer>();
        m_audioSource = GetComponent<AudioSource>();

        // Configure l'AudioSource en boucle locale (pas via AudioManager)
        m_audioSource.clip        = m_ambientClip;
        m_audioSource.loop        = true;
        m_audioSource.volume      = m_ambientVolume;
        m_audioSource.spatialBlend = 0f;
        m_audioSource.playOnAwake = false;

        if (m_ambientClip != null)
            m_audioSource.Play();
    }

    void Update()
    {
        if (m_renderer == null) return;

        float t = Mathf.Lerp(m_pulseMin, m_pulseMax,
                             (Mathf.Sin(Time.time * m_pulseSpeed) + 1f) * 0.5f);

        m_renderer.color = Color.Lerp(m_baseColor, m_glowColor, t);
    }

    void OnDisable()
    {
        if (m_audioSource != null)
            m_audioSource.Stop();
    }
}
