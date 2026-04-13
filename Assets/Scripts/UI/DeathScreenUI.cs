using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Écran de mort — s'affiche quand le joueur tombe à 0 HP.
/// Placer ce GameObject dans la MainScene avec un Canvas en overlay.
/// </summary>
public class DeathScreenUI : MonoBehaviour
{
    public static DeathScreenUI Instance { get; private set; }

    [Header("Références UI")]
    public CanvasGroup m_canvasGroup;
    public Button      m_menuButton;
    public Button      m_retryButton;

    [Header("Fondu")]
    public float m_fadeDuration = 1.5f;

    void Awake()
    {
        Instance = this;
        m_canvasGroup.alpha          = 0f;
        m_canvasGroup.interactable   = false;
        m_canvasGroup.blocksRaycasts = false;
    }

    void Start()
    {
        m_menuButton.onClick.AddListener(OnMenu);
        m_retryButton.onClick.AddListener(OnRetry);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ── API publique ─────────────────────────────────────────────────────────

    public void Show()
    {
        Time.timeScale = 0f;           // gèle le jeu
        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    // ── Boutons ──────────────────────────────────────────────────────────────

    private void OnMenu()
    {
        Time.timeScale = 1f;
        GameManager.Instance.ReturnToMenu();
    }

    private void OnRetry()
    {
        Time.timeScale = 1f;
        GameManager.Instance.StartGame(GameManager.Instance.CurrentSlot);
    }

    // ── Fondu ────────────────────────────────────────────────────────────────

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < m_fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            m_canvasGroup.alpha = Mathf.Clamp01(elapsed / m_fadeDuration);
            yield return null;
        }
        m_canvasGroup.alpha          = 1f;
        m_canvasGroup.interactable   = true;
        m_canvasGroup.blocksRaycasts = true;

        // Sélectionne "Réessayer" par défaut pour la navigation manette
        UINavigationSetup.SelectFirst(new[] { m_retryButton, m_menuButton });
    }
}
