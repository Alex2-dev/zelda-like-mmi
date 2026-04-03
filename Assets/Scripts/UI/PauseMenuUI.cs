// Assets/Scripts/UI/PauseMenuUI.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gère le menu pause (Échap) dans MainScene.
/// Toggle PausePanel / SettingsPanel, gère Time.timeScale.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject m_pausePanel;
    public GameObject m_settingsPanel;

    [Header("Bouton Sauvegarder — label")]
    public TextMeshProUGUI m_saveLabel;

    private bool m_isPaused = false;

    void Update()
    {
        if (InputManager.Instance != null && InputManager.Instance.PausePressed)
            TogglePause();
    }

    // ── API publique ────────────────────────────────────────────────────────

    public void TogglePause()
    {
        if (m_isPaused)
            Resume();
        else
            Pause();
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

    public void SaveGame()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.QuickSave();
        if (m_saveLabel != null)
        {
            StopCoroutine(nameof(SaveFeedback));
            StartCoroutine(nameof(SaveFeedback));
        }
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        m_isPaused = false;
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMenu();
    }

    // ── Interne ─────────────────────────────────────────────────────────────

    private IEnumerator SaveFeedback()
    {
        m_saveLabel.text = "Sauvegardé !";
        yield return new WaitForSecondsRealtime(1.5f);
        m_saveLabel.text = "Sauvegarder";
    }
}
