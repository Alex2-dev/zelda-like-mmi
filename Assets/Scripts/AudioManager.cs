using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance = null;

    public AudioSource m_soundStream;
    public AudioSource m_musicStream;

    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const string SOUND_VOL_KEY = "SoundVolume";

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ResetInstance() => instance = null;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(this); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        LoadVolumes();
    }

    // ── Lecture ─────────────────────────────────────────────────────────────

    public void PlaySound(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
    {
        m_soundStream.pitch  = pitch;
        m_soundStream.volume = volume * GetSoundVolume();
        m_soundStream.clip   = clip;
        m_soundStream.Play();
    }

    public void StopSound() => m_soundStream.Stop();

    public void PlayMusic(AudioClip clip, bool loop, float volume = 1.0f, float pitch = 1.0f)
    {
        m_musicStream.pitch  = pitch;
        m_musicStream.volume = volume * GetMusicVolume();
        m_musicStream.loop   = loop;
        m_musicStream.clip   = clip;
        m_musicStream.Play();
    }

    public void StopMusic() => m_musicStream.Stop();

    // ── Volume ───────────────────────────────────────────────────────────────

    public float GetMusicVolume() => PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 0.8f);
    public float GetSoundVolume() => PlayerPrefs.GetFloat(SOUND_VOL_KEY, 1.0f);

    public void SetMusicVolume(float value)
    {
        value = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, value);
        m_musicStream.volume = value;
    }

    public void SetSoundVolume(float value)
    {
        value = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SOUND_VOL_KEY, value);
        m_soundStream.volume = value;
    }

    private void LoadVolumes()
    {
        if (m_musicStream != null) m_musicStream.volume = GetMusicVolume();
        if (m_soundStream != null) m_soundStream.volume = GetSoundVolume();
    }
}
