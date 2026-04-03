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

    public int  ActiveSlot   { get; private set; } = 0;
    public bool BossDefeated { get; private set; } = false;

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

    // ── Démarrer une partie ──────────────────────────────────────────────────

    public void StartGame(int slot)
    {
        ActiveSlot = slot;
        if (TransitionManager.Instance != null)
            TransitionManager.Instance.PlayEnterPC(() => SceneManager.LoadScene(MAIN_SCENE));
        else
            SceneManager.LoadScene(MAIN_SCENE);
    }

    // ── Fin de jeu (boss tué) ────────────────────────────────────────────────

    public void TriggerEndGame()
    {
        BossDefeated = true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && SaveManager.Instance != null)
        {
            SaveData data = SaveManager.Instance.TakeSnapshot(ActiveSlot, player);
            data.bossDefeated = true;
            SaveManager.Instance.Save(ActiveSlot, data);
        }

        if (TransitionManager.Instance != null)
            TransitionManager.Instance.PlayExitPC(() => SceneManager.LoadScene(MENU_SCENE));
        else
            SceneManager.LoadScene(MENU_SCENE);
    }

    // ── Retour menu ──────────────────────────────────────────────────────────

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(MENU_SCENE);
    }

    // ── Quick Save ───────────────────────────────────────────────────────────

    public void QuickSave()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || SaveManager.Instance == null) return;
        SaveData data = SaveManager.Instance.TakeSnapshot(ActiveSlot, player);
        SaveManager.Instance.Save(ActiveSlot, data);
    }
}
