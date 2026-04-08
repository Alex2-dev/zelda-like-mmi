using UnityEngine;

/// <summary>
/// Déclenche un son via AudioManager selon l'événement choisi.
/// Place ce script sur n'importe quel GameObject et assigne un clip.
///
/// SETUP :
/// - Ajouter ce script sur un GameObject
/// - Assigner m_clip dans l'Inspector
/// - Choisir le mode de déclenchement (m_trigger)
/// - Pour "Manual" : appeler PlaySound() depuis un autre script ou un UnityEvent
/// </summary>
public class SoundTrigger : MonoBehaviour
{
    public enum TriggerMode
    {
        OnStart,            // joue au démarrage de la scène
        OnEnable,           // joue chaque fois que le GameObject est activé
        OnPlayerEnter,      // joue quand le joueur entre dans le trigger
        OnPlayerExit,       // joue quand le joueur quitte le trigger
        Manual              // appel explicite à PlaySound()
    }

    [Header("Son")]
    [SerializeField] AudioClip m_clip;
    [SerializeField][Range(0f, 1f)] float m_volume = 1f;
    [SerializeField][Range(0.5f, 2f)] float m_pitch = 1f;

    [Header("Déclenchement")]
    [SerializeField] TriggerMode m_trigger = TriggerMode.OnPlayerEnter;

    [Tooltip("Joue une seule fois (ignore les appels suivants)")]
    [SerializeField] bool m_playOnce = false;

    private bool m_hasPlayed = false;

    void Start()
    {
        if (m_trigger == TriggerMode.OnStart)
            PlaySound();
    }

    void OnEnable()
    {
        if (m_trigger == TriggerMode.OnEnable)
            PlaySound();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (m_trigger == TriggerMode.OnPlayerEnter && other.CompareTag("Player"))
            PlaySound();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (m_trigger == TriggerMode.OnPlayerExit && other.CompareTag("Player"))
            PlaySound();
    }

    public void PlaySound()
    {
        if (m_clip == null) return;
        if (m_playOnce && m_hasPlayed) return;

        AudioManager.instance?.PlaySound(m_clip, m_volume, m_pitch);
        m_hasPlayed = true;
    }
}
