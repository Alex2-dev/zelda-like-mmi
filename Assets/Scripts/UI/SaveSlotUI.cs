using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sélection du slot de démarrage — plus de sauvegarde, juste le choix du slot visuel.
/// </summary>
public class SaveSlotUI : MonoBehaviour
{
    [Header("Bouton Retour")]
    public Button m_backButton;

    private MainMenuUI m_mainMenu;

    void Start()
    {
        m_mainMenu = FindObjectOfType<MainMenuUI>();

        if (m_backButton != null)
            m_backButton.onClick.AddListener(() => m_mainMenu?.ShowMain());
    }

    public void SelectSlot()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }
}
