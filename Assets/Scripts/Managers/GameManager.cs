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

    public bool BossDefeated { get; private set; } = false;
    public int  CurrentSlot  { get; private set; } = 0;

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

    public void StartGame(int slot = 0)
    {
        CurrentSlot = slot;
        if (TransitionManager.Instance != null)
            TransitionManager.Instance.PlayEnterPC(() => SceneManager.LoadScene(MAIN_SCENE));
        else
            SceneManager.LoadScene(MAIN_SCENE);
    }

    public void TriggerEndGame()
    {
        BossDefeated = true;

        if (CreditsUI.Instance != null)
            CreditsUI.Instance.Show();
        else
            SceneManager.LoadScene(MENU_SCENE);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(MENU_SCENE);
    }
}
