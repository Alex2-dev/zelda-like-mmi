using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère deux panneaux :
///   - ComposePanel : l'utilisateur tape un message et l'envoie
///   - ReadPanel    : affiche le message d'un autre joueur
///
/// Expose IsOpen (static) pour que PlayerBehavior bloque le mouvement.
/// Doit être placé sur le Canvas de l'UI de messages.
/// </summary>
public class MessageUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    [Header("Panneau — Rédaction")]
    [SerializeField] private GameObject     m_composePanel;
    [SerializeField] private TMP_InputField m_inputField;
    [SerializeField] private Button         m_sendButton;
    [SerializeField] private Button         m_cancelButton;

    [Header("Panneau — Lecture")]
    [SerializeField] private GameObject m_readPanel;
    [SerializeField] private TMP_Text   m_readText;
    [SerializeField] private Button     m_closeReadButton;

    private Vector3 m_pendingPosition;

    // ── Initialisation ────────────────────────────────────────────────────────

    private void Awake()
    {
        m_composePanel.SetActive(false);
        m_readPanel.SetActive(false);

        m_sendButton.onClick.AddListener(OnSend);
        m_cancelButton.onClick.AddListener(CloseAll);
        m_closeReadButton.onClick.AddListener(CloseAll);
    }

    // ── Rédaction ─────────────────────────────────────────────────────────────

    public void OpenCompose(Vector3 playerPos)
    {
        m_pendingPosition = playerPos;
        m_inputField.text = "";
        m_composePanel.SetActive(true);
        m_readPanel.SetActive(false);
        IsOpen = true;
        Time.timeScale = 0f;

        m_inputField.Select();
        m_inputField.ActivateInputField();
        // Sur manette : sélectionne Envoyer (le joueur peut aussi naviguer vers Annuler)
        UINavigationSetup.SelectFirst(new[] { m_sendButton, m_cancelButton });
    }

    private void OnSend()
    {
        string text = m_inputField.text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        MessageManager.Instance.SendMessage(text, m_pendingPosition);
        CloseAll();
    }

    // ── Lecture ───────────────────────────────────────────────────────────────

    public void OpenRead(string text)
    {
        m_readText.text = text;
        m_readPanel.SetActive(true);
        m_composePanel.SetActive(false);
        IsOpen = true;
        Time.timeScale = 0f;
        // Sélectionne le bouton Fermer pour la navigation manette
        UINavigationSetup.SelectFirst(new[] { m_closeReadButton });
    }

    // ── Fermeture ─────────────────────────────────────────────────────────────

    public void CloseAll()
    {
        m_composePanel.SetActive(false);
        m_readPanel.SetActive(false);
        IsOpen = false;
        Time.timeScale = 1f;
        UINavigationSetup.Deselect();
    }
}
