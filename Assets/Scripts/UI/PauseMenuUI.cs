using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère le menu pause (Échap) dans MainScene.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject m_pausePanel;
    public GameObject m_settingsPanel;

    private bool m_isPaused = false;

    void Update()
    {
        if (InputManager.Instance == null) return;
        if (InputManager.Instance.PausePressed)
            TogglePause();
    }

    public void TogglePause()
    {
        if (m_isPaused) Resume();
        else            Pause();
    }

    public void Pause()
    {
        m_isPaused = true;
        Time.timeScale = 0f;
        m_pausePanel.SetActive(true);
        m_settingsPanel.SetActive(false);
    }

    public void Resume()
    {
        m_isPaused = false;
        Time.timeScale = 1f;
        m_pausePanel.SetActive(false);
        m_settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        m_pausePanel.SetActive(false);
        m_settingsPanel.SetActive(true);
    }

    public void BackFromSettings()
    {
        m_settingsPanel.SetActive(false);
        m_pausePanel.SetActive(true);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        m_isPaused = false;
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMenu();
    }
}
