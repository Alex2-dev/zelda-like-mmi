using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager m_instance;
    public static GameManager Instance
    {
        get
        {
            if (m_instance == null)
                m_instance = FindObjectOfType<GameManager>();
            return m_instance;
        }
        private set => m_instance = value;
    }

    public bool  BossDefeated { get; private set; } = false;
    public int   CurrentSlot  { get; private set; } = 0;
    public float PlayTime     { get; private set; } = 0f;

    private const string MAIN_SCENE = "MainScene";
    private const string MENU_SCENE = "MenuScene";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        // Compte le temps de jeu uniquement en jeu (pas dans les menus)
        if (SceneManager.GetActiveScene().name == MAIN_SCENE)
            PlayTime += Time.deltaTime;
    }

    public void SetPlayTime(float time)  => PlayTime     = time;
    public void SetBossDefeated(bool v)  => BossDefeated = v;

    public void StartGame(int slot = 0)
    {
        CurrentSlot = slot;

        // Charge le temps de jeu depuis la save pour continuer le compteur
        if (SaveManager.Instance != null)
        {
            SaveData existing = SaveManager.Instance.Load(slot);
            PlayTime = existing.isEmpty ? 0f : existing.playTime;
        }

        if (TransitionManager.Instance != null)
            TransitionManager.Instance.PlayEnterPC(() => SceneManager.LoadScene(MAIN_SCENE));
        else
            SceneManager.LoadScene(MAIN_SCENE);
    }

    public void TriggerEndGame()
    {
        BossDefeated = true;
        PlayerSaveLoader.Instance?.SaveCurrentState();

        if (CreditsUI.Instance != null)
            CreditsUI.Instance.Show();
        else
            SceneManager.LoadScene(MENU_SCENE);
    }

    public void ReturnToMenu()
    {
        // Sauvegarde avant de quitter
        PlayerSaveLoader.Instance?.SaveCurrentState();
        SceneManager.LoadScene(MENU_SCENE);
    }
}
