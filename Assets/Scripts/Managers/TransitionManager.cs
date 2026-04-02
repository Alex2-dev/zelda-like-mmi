using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gère les animations de transition "Entrée PC" et "Sortie PC".
/// Canvas créée par code, DontDestroyOnLoad.
/// </summary>
public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    private Canvas          m_canvas;
    private CanvasGroup     m_fadeGroup;
    private TextMeshProUGUI m_label;
    private RectTransform   m_pcScreen;

    public float m_fadeDuration  = 0.8f;
    public float m_zoomDuration  = 0.6f;
    public float m_typeSpeed     = 0.05f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
    }

    // ── API publique ─────────────────────────────────────────────────────────

    public void PlayEnterPC(System.Action onComplete)
    {
        StartCoroutine(EnterPCRoutine(onComplete));
    }

    public void PlayExitPC(System.Action onComplete)
    {
        StartCoroutine(ExitPCRoutine(onComplete));
    }

    // ── Coroutines ───────────────────────────────────────────────────────────

    private IEnumerator EnterPCRoutine(System.Action onComplete)
    {
        m_canvas.gameObject.SetActive(true);
        yield return ZoomIn();
        yield return Fade(0f, 1f, m_fadeDuration);
        yield return TypeText("Connexion en cours...");
        onComplete?.Invoke();
        yield return new WaitForSeconds(0.5f);
        yield return Fade(1f, 0f, m_fadeDuration);
        ClearText();
        m_canvas.gameObject.SetActive(false);
    }

    private IEnumerator ExitPCRoutine(System.Action onComplete)
    {
        m_canvas.gameObject.SetActive(true);
        yield return Fade(0f, 1f, m_fadeDuration);
        yield return TypeText("Déconnexion...");
        onComplete?.Invoke();
        yield return new WaitForSeconds(0.5f);
        yield return ZoomOut();
        yield return Fade(1f, 0f, m_fadeDuration);
        ClearText();
        m_canvas.gameObject.SetActive(false);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            m_fadeGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        m_fadeGroup.alpha = to;
    }

    private IEnumerator TypeText(string text)
    {
        m_label.text = "";
        foreach (char c in text)
        {
            m_label.text += c;
            yield return new WaitForSeconds(m_typeSpeed);
        }
        yield return new WaitForSeconds(0.5f);
    }

    private void ClearText() => m_label.text = "";

    private IEnumerator ZoomIn()
    {
        float t = 0f;
        Vector3 start = Vector3.one * 0.1f;
        Vector3 end   = Vector3.one;
        while (t < m_zoomDuration)
        {
            t += Time.deltaTime;
            m_pcScreen.localScale = Vector3.Lerp(start, end, t / m_zoomDuration);
            yield return null;
        }
        m_pcScreen.localScale = end;
    }

    private IEnumerator ZoomOut()
    {
        float t = 0f;
        Vector3 start = Vector3.one;
        Vector3 end   = Vector3.one * 0.1f;
        while (t < m_zoomDuration)
        {
            t += Time.deltaTime;
            m_pcScreen.localScale = Vector3.Lerp(start, end, t / m_zoomDuration);
            yield return null;
        }
        m_pcScreen.localScale = end;
    }

    // ── Création UI par code ─────────────────────────────────────────────────

    private void BuildUI()
    {
        m_canvas = gameObject.AddComponent<Canvas>();
        m_canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        m_canvas.sortingOrder = 999;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        // Fond noir
        var bg = new GameObject("Background");
        bg.transform.SetParent(transform, false);
        m_fadeGroup = bg.AddComponent<CanvasGroup>();
        var bgImg  = bg.AddComponent<Image>();
        bgImg.color = Color.black;
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;

        // Icône écran PC
        var pc = new GameObject("PCScreen");
        pc.transform.SetParent(bg.transform, false);
        m_pcScreen = pc.AddComponent<RectTransform>();
        m_pcScreen.sizeDelta       = new Vector2(200, 150);
        m_pcScreen.anchoredPosition = Vector2.zero;
        var pcImg  = pc.AddComponent<Image>();
        pcImg.color = new Color(0.1f, 0.8f, 0.1f, 0.8f);

        // Texte
        var textObj = new GameObject("Label");
        textObj.transform.SetParent(bg.transform, false);
        m_label           = textObj.AddComponent<TextMeshProUGUI>();
        m_label.text      = "";
        m_label.fontSize  = 24;
        m_label.color     = new Color(0.1f, 1f, 0.1f);
        m_label.alignment = TextAlignmentOptions.Center;
        var labelRect = m_label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.1f, 0.4f);
        labelRect.anchorMax = new Vector2(0.9f, 0.6f);
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;

        m_canvas.gameObject.SetActive(false);
    }
}
