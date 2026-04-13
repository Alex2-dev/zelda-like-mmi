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

    void Update()
    {
        if (InputManager.Instance == null) return;
        if (!InputManager.Instance.UIBackPressed) return;

        // B manette = retour au menu principal depuis les sous-panels
        if (m_slotPanel.activeSelf || m_settingsPanel.activeSelf)
            ShowMain();
    }

    public void OnPlayClicked()
    {
        m_mainPanel.SetActive(false);
        m_slotPanel.SetActive(true);
        UINavigationSetup.SelectFirstInPanel(m_slotPanel);
    }

    public void OnSettingsClicked()
    {
        m_mainPanel.SetActive(false);
        m_settingsPanel.SetActive(true);
        UINavigationSetup.SelectFirstInPanel(m_settingsPanel);
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
        UINavigationSetup.SelectFirstInPanel(m_mainPanel);
    }
}
