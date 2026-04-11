using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [Header("Slots (3 entrées)")]
    public Button[]          m_slotButtons;
    public TextMeshProUGUI[] m_slotLabels;
    public Button[]          m_deleteButtons;

    [Header("Bouton Retour")]
    public Button m_backButton;

    private MainMenuUI m_mainMenu;

    void Start()
    {
        m_mainMenu = FindObjectOfType<MainMenuUI>();

        for (int i = 0; i < m_slotButtons.Length; i++)
        {
            if (m_slotButtons[i] == null) continue;
            int slot = i;
            m_slotButtons[i].onClick.AddListener(() => SelectSlot(slot));
        }

        for (int i = 0; i < m_deleteButtons.Length; i++)
        {
            if (m_deleteButtons[i] == null) continue;
            int slot = i;
            m_deleteButtons[i].onClick.AddListener(() => DeleteSlot(slot));
        }

        if (m_backButton != null)
            m_backButton.onClick.AddListener(() => m_mainMenu?.ShowMain());

        RefreshSlots();
    }

    void OnEnable() => RefreshSlots();

    private void RefreshSlots()
    {
        if (SaveManager.Instance == null) return;

        for (int i = 0; i < SaveManager.SLOT_COUNT; i++)
        {
            SaveData data = SaveManager.Instance.Load(i);
            bool exists   = !data.isEmpty;

            if (i < m_slotLabels.Length && m_slotLabels[i] != null)
            {
                if (exists)
                {
                    int min = Mathf.FloorToInt(data.playTime / 60f);
                    int sec = Mathf.FloorToInt(data.playTime % 60f);
                    string boss = data.bossDefeated ? "Boss: Oui" : "Boss: Non";
                    m_slotLabels[i].text = $"Slot {i + 1} — {min:00}:{sec:00}  {boss}";
                }
                else
                {
                    m_slotLabels[i].text = $"Slot {i + 1} — VIDE";
                }
            }

            if (i < m_deleteButtons.Length && m_deleteButtons[i] != null)
                m_deleteButtons[i].gameObject.SetActive(exists);
        }
    }

    private void SelectSlot(int slot)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[SaveSlotUI] GameManager introuvable !");
            return;
        }
        GameManager.Instance.StartGame(slot);
    }

    private void DeleteSlot(int slot)
    {
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.Delete(slot);
        RefreshSlots();
    }
}
