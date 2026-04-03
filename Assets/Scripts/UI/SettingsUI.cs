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

    [Header("Bouton Sauvegarder")]
    public Button              m_saveButton;
    public TextMeshProUGUI     m_saveLabel;

    private struct ActionBinding
    {
        public string action;
        public int    bindingIndex;
        public ActionBinding(string a, int b) { action = a; bindingIndex = b; }
    }

    // Ordre correspondant aux boutons de rebinding
    // Move : binding 0 = composite, 1 = Haut, 2 = Bas, 3 = Gauche, 4 = Droite
    private static readonly ActionBinding[] ACTIONS =
    {
        new ActionBinding("Attack",    0),
        new ActionBinding("Inventory", 0),
        new ActionBinding("Run",       0),
        new ActionBinding("AlterEgo",  0),
        new ActionBinding("Dash",      0),
        new ActionBinding("Hotbar1",   0),
        new ActionBinding("Hotbar2",   0),
        new ActionBinding("Move",      1), // Haut
        new ActionBinding("Move",      2), // Bas
        new ActionBinding("Move",      3), // Gauche
        new ActionBinding("Move",      4), // Droite
    };

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
        for (int i = 0; i < m_rebindButtons.Length && i < ACTIONS.Length; i++)
        {
            int idx = i;
            m_rebindButtons[i].onClick.AddListener(() => StartRebind(idx));
        }

        if (m_backButton != null)
            m_backButton.onClick.AddListener(() => m_mainMenu.ShowMain());

        if (m_saveButton != null)
            m_saveButton.onClick.AddListener(OnSaveClicked);

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

        m_bindLabels[actionIndex].text = "Appuie sur une touche...";

        InputManager.Instance.StartRebinding(ACTIONS[actionIndex].action, ACTIONS[actionIndex].bindingIndex, () =>
        {
            m_isRebinding = false;
            RefreshBindLabels();
        });
    }

    private void OnSaveClicked()
    {
        if (InputManager.Instance == null) return;
        InputManager.Instance.SaveBindings();
        if (m_saveLabel != null)
            StartCoroutine(ShowSavedFeedback());
    }

    private System.Collections.IEnumerator ShowSavedFeedback()
    {
        m_saveLabel.text = "Sauvegardé !";
        yield return new WaitForSeconds(1.5f);
        m_saveLabel.text = "Sauvegarder";
    }

    private void RefreshBindLabels()
    {
        if (InputManager.Instance == null) return;
        for (int i = 0; i < m_bindLabels.Length && i < ACTIONS.Length; i++)
            m_bindLabels[i].text = InputManager.Instance.GetBindingDisplayString(ACTIONS[i].action, ACTIONS[i].bindingIndex);
    }
}
