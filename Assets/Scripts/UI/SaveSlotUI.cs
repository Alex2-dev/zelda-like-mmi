using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Affiche les 3 slots de sauvegarde et gère la sélection.
/// </summary>
public class SaveSlotUI : MonoBehaviour
{
    [Header("Slots (3 boutons)")]
    public Button[]           m_slotButtons;
    public TextMeshProUGUI[]  m_slotLabels;

    [Header("Bouton Retour")]
    public Button m_backButton;

    private MainMenuUI m_mainMenu;

    void Start()
    {
        m_mainMenu = FindObjectOfType<MainMenuUI>();

        for (int i = 0; i < m_slotButtons.Length; i++)
        {
            if (m_slotButtons[i] == null)
            {
                Debug.LogError($"[SaveSlotUI] m_slotButtons[{i}] est null ! Vérifie les références dans l'Inspector.");
                continue;
            }
            int slot = i;
            m_slotButtons[i].onClick.AddListener(() => SelectSlot(slot));
        }

        if (m_backButton != null)
            m_backButton.onClick.AddListener(() => m_mainMenu?.ShowMain());

        RefreshSlots();
    }

    private void RefreshSlots()
    {
        if (SaveManager.Instance == null) return;
        SaveData[] saves = SaveManager.Instance.LoadAll();

        for (int i = 0; i < saves.Length && i < m_slotLabels.Length; i++)
        {
            if (saves[i].isEmpty)
            {
                m_slotLabels[i].text = $"Slot {i + 1} — VIDE";
            }
            else
            {
                int minutes = Mathf.FloorToInt(saves[i].playTime / 60f);
                int seconds = Mathf.FloorToInt(saves[i].playTime % 60f);
                string boss = saves[i].bossDefeated ? "Boss: Oui" : "Boss: Non";
                m_slotLabels[i].text = $"Slot {i + 1} — {minutes:00}:{seconds:00}  {boss}";
            }
        }
    }

    private void SelectSlot(int slot)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[SaveSlotUI] GameManager.Instance est null ! Vérifie que le GameObject Managers est dans la scène MenuScene.");
            return;
        }
        Debug.Log($"[SaveSlotUI] Démarrage slot {slot}");
        GameManager.Instance.StartGame(slot);
    }
}
