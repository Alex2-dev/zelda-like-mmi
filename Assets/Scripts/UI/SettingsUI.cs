using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 3 onglets : Touches / Manette / Son.
/// </summary>
public class SettingsUI : MonoBehaviour
{
    [Header("Onglets — Panels")]
    public GameObject m_touchesPanel;
    public GameObject m_manettPanel;
    public GameObject m_sonPanel;

    [Header("Son — Sliders")]
    public Slider m_musicSlider;
    public Slider m_soundSlider;

    [Header("Touches — Rebinding")]
    public Button[]           m_rebindButtons;
    public TextMeshProUGUI[]  m_bindLabels;

    [Header("Bouton Retour")]
    public Button m_backButton;

    // Ordre correspondant aux boutons de rebinding
    private readonly string[] ACTION_NAMES = { "Attack", "Inventory", "Run", "Hide", "Map", "Hotbar1", "Hotbar2" };

    private MainMenuUI m_mainMenu;
    private bool       m_isRebinding = false;

    void Start()
    {
        m_mainMenu = FindObjectOfType<MainMenuUI>();

        // Son
        if (m_musicSlider != null)
        {
            m_musicSlider.value = AudioManager.instance != null ? AudioManager.instance.GetMusicVolume() : 0.8f;
            m_musicSlider.onValueChanged.AddListener(v => AudioManager.instance?.SetMusicVolume(v));
        }
        if (m_soundSlider != null)
        {
            m_soundSlider.value = AudioManager.instance != null ? AudioManager.instance.GetSoundVolume() : 1f;
            m_soundSlider.onValueChanged.AddListener(v => AudioManager.instance?.SetSoundVolume(v));
        }

        // Touches
        RefreshBindLabels();
        for (int i = 0; i < m_rebindButtons.Length && i < ACTION_NAMES.Length; i++)
        {
            int idx = i;
            m_rebindButtons[i].onClick.AddListener(() => StartRebind(idx));
        }

        if (m_backButton != null)
            m_backButton.onClick.AddListener(() => m_mainMenu.ShowMain());

        ShowTouches();
    }

    // ── Onglets ──────────────────────────────────────────────────────────────

    public void ShowTouches()
    {
        m_touchesPanel.SetActive(true);
        m_manettPanel.SetActive(false);
        m_sonPanel.SetActive(false);
    }

    public void ShowManette()
    {
        m_touchesPanel.SetActive(false);
        m_manettPanel.SetActive(true);
        m_sonPanel.SetActive(false);
    }

    public void ShowSon()
    {
        m_touchesPanel.SetActive(false);
        m_manettPanel.SetActive(false);
        m_sonPanel.SetActive(true);
    }

    // ── Rebinding ────────────────────────────────────────────────────────────

    private void StartRebind(int actionIndex)
    {
        if (m_isRebinding || InputManager.Instance == null) return;
        m_isRebinding = true;

        string actionName = ACTION_NAMES[actionIndex];
        m_bindLabels[actionIndex].text = "Appuie sur une touche...";

        InputManager.Instance.StartRebinding(actionName, 0, () =>
        {
            m_isRebinding = false;
            RefreshBindLabels();
        });
    }

    private void RefreshBindLabels()
    {
        if (InputManager.Instance == null) return;
        for (int i = 0; i < m_bindLabels.Length && i < ACTION_NAMES.Length; i++)
            m_bindLabels[i].text = InputManager.Instance.GetBindingDisplayString(ACTION_NAMES[i], 0);
    }
}
