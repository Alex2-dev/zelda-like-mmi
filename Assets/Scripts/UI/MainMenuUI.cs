using UnityEngine;

/// <summary>
/// Controller du menu principal.
/// 3 panels : MainPanel (boutons principaux), SlotPanel (sélection save), SettingsPanel.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject m_mainPanel;
    public GameObject m_slotPanel;
    public GameObject m_settingsPanel;

    void Start()
    {
        ShowMain();
    }

    public void OnPlayClicked()
    {
        m_mainPanel.SetActive(false);
        m_slotPanel.SetActive(true);
    }

    public void OnSettingsClicked()
    {
        m_mainPanel.SetActive(false);
        m_settingsPanel.SetActive(true);
    }

    public void OnCreditsClicked()
    {
        CreditsUI.Instance?.Show();
    }

    public void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ShowMain()
    {
        m_mainPanel.SetActive(true);
        m_slotPanel.SetActive(false);
        m_settingsPanel.SetActive(false);
    }
}
