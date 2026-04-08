using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CreditsUI : MonoBehaviour
{
    public static CreditsUI Instance { get; private set; }

    // Couleurs
    private static readonly Color COL_TITLE  = new Color(0.85f, 0.72f, 0.40f); // or doux
    private static readonly Color COL_BODY   = new Color(0.92f, 0.92f, 0.92f); // blanc cassé
    private static readonly Color COL_LINE   = new Color(0.85f, 0.72f, 0.40f); // or doux

    private readonly (string title, string body)[] m_credits = new (string, string)[]
    {
        ("",                            "CODE\nFRACTURE"),
        ("",                            "© 2026"),
        ("Développement & Game Design", "Alexandre Bouvy"),
        ("Character Design",            "Inspiré d'assets itch.io\nModifié par Alexandre Bouvy"),
        ("Musiques & Sound Effects",    "Pixabay\nModifiés avec Adobe Audition"),
        ("Remerciements",               "M. Brulin\nMme. Panchetti\nM. Bernard"),
        ("",                            "Merci à Alexis\npour ses tests"),
        ("",                            "Merci d'avoir joué.")
    };

    public float m_fadeInDuration  = 1.4f;
    public float m_lineDuration    = 3.2f;
    public float m_fadeOutDuration = 1.2f;

    private Canvas          m_canvas;
    private Image           m_bg;
    private TextMeshProUGUI m_titleLabel;
    private TextMeshProUGUI m_bodyLabel;
    private Image           m_lineTop;
    private Image           m_lineBot;
    private Image           m_lineAccent;

    private const string MENU_SCENE = "MenuScene";

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ResetInstance() => Instance = null;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
    }

    public void Show()
    {
        StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        if (SceneManager.GetActiveScene().name != MENU_SCENE)
        {
            SceneManager.LoadScene(MENU_SCENE);
            yield return new WaitForSeconds(0.6f);
        }

        m_canvas.gameObject.SetActive(true);

        yield return FadeImage(m_bg, 0f, 1f, m_fadeInDuration);

        bool first = true;
        foreach (var (title, body) in m_credits)
        {
            m_titleLabel.text    = title;
            m_bodyLabel.text     = body;

            bool isHeadline = string.IsNullOrEmpty(title);
            m_bodyLabel.fontSize       = isHeadline ? 72 : 40;
            m_bodyLabel.characterSpacing = isHeadline ? 18f : 2f;
            m_bodyLabel.fontStyle      = isHeadline ? FontStyles.Bold : FontStyles.Normal;
            m_lineTop.gameObject.SetActive(!isHeadline);
            m_lineBot.gameObject.SetActive(!isHeadline);

            // Ligne accent visible uniquement pour le tout premier slide
            m_lineAccent.gameObject.SetActive(first);
            first = false;

            yield return FadeGroup(0f, 1f, 0.7f);
            yield return new WaitForSeconds(m_lineDuration);
            yield return FadeGroup(1f, 0f, 0.5f);
            yield return new WaitForSeconds(0.25f);
        }

        yield return new WaitForSeconds(0.4f);
        yield return FadeImage(m_bg, 1f, 0f, m_fadeOutDuration);

        m_canvas.gameObject.SetActive(false);
    }

    // ── Helpers fade ────────────────────────────────────────────────────────────

    private IEnumerator FadeImage(Image img, float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            Color c = img.color;
            c.a = Mathf.Lerp(from, to, t / dur);
            img.color = c;
            yield return null;
        }
        Color cf = img.color; cf.a = to; img.color = cf;
    }

    private IEnumerator FadeGroup(float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / dur);
            SetAlpha(m_titleLabel, a);
            SetAlpha(m_bodyLabel,  a);
            SetImageAlpha(m_lineTop,    a * 0.5f);
            SetImageAlpha(m_lineBot,    a * 0.5f);
            SetImageAlpha(m_lineAccent, a * 0.6f);
            yield return null;
        }
        SetAlpha(m_titleLabel, to);
        SetAlpha(m_bodyLabel,  to);
        SetImageAlpha(m_lineTop,    to * 0.5f);
        SetImageAlpha(m_lineBot,    to * 0.5f);
        SetImageAlpha(m_lineAccent, to * 0.6f);
    }

    private void SetAlpha(TextMeshProUGUI t, float a)
        { Color c = t.color; c.a = a; t.color = c; }

    private void SetImageAlpha(Image img, float a)
        { Color c = img.color; c.a = a; img.color = c; }

    // ── Build UI ────────────────────────────────────────────────────────────────

    private void BuildUI()
    {
        // Canvas
        var canvasGO = new GameObject("CreditsCanvas");
        canvasGO.transform.SetParent(transform, false);
        m_canvas = canvasGO.AddComponent<Canvas>();
        m_canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        m_canvas.sortingOrder = 1000;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Fond noir ──────────────────────────────────────────────────────────
        var bgGO = new GameObject("BG");
        bgGO.transform.SetParent(canvasGO.transform, false);
        m_bg = bgGO.AddComponent<Image>();
        m_bg.color = new Color(0.04f, 0.04f, 0.06f, 0f); // quasi noir avec légère teinte bleue
        Stretch(bgGO);

        // ── Ligne accent (petite, or, centrée en haut du texte) ───────────────
        m_lineAccent = MakeLine(canvasGO.transform,
            new Vector2(0.42f, 0.68f), new Vector2(0.58f, 0.68f), 3f, COL_LINE);

        // ── Ligne séparatrice haut ─────────────────────────────────────────────
        m_lineTop = MakeLine(canvasGO.transform,
            new Vector2(0.20f, 0.635f), new Vector2(0.80f, 0.635f), 1f, COL_LINE);

        // ── Ligne séparatrice bas ──────────────────────────────────────────────
        m_lineBot = MakeLine(canvasGO.transform,
            new Vector2(0.20f, 0.365f), new Vector2(0.80f, 0.365f), 1f, COL_LINE);

        // ── Titre (catégorie) ──────────────────────────────────────────────────
        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(canvasGO.transform, false);
        m_titleLabel                  = titleGO.AddComponent<TextMeshProUGUI>();
        m_titleLabel.fontSize         = 18;
        m_titleLabel.color            = WithAlpha(COL_TITLE, 0f);
        m_titleLabel.alignment        = TextAlignmentOptions.Center;
        m_titleLabel.characterSpacing = 8f;
        m_titleLabel.lineSpacing      = 6f;
        m_titleLabel.fontStyle        = FontStyles.UpperCase;
        var tRect = m_titleLabel.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0.15f, 0.635f);
        tRect.anchorMax = new Vector2(0.85f, 0.72f);
        tRect.offsetMin = tRect.offsetMax = Vector2.zero;

        // ── Corps ──────────────────────────────────────────────────────────────
        var bodyGO = new GameObject("Body");
        bodyGO.transform.SetParent(canvasGO.transform, false);
        m_bodyLabel             = bodyGO.AddComponent<TextMeshProUGUI>();
        m_bodyLabel.fontSize    = 40;
        m_bodyLabel.color       = WithAlpha(COL_BODY, 0f);
        m_bodyLabel.alignment   = TextAlignmentOptions.Center;
        m_bodyLabel.lineSpacing = 16f;
        var bRect = m_bodyLabel.GetComponent<RectTransform>();
        bRect.anchorMin = new Vector2(0.15f, 0.365f);
        bRect.anchorMax = new Vector2(0.85f, 0.635f);
        bRect.offsetMin = bRect.offsetMax = Vector2.zero;

        m_canvas.gameObject.SetActive(false);
    }

    private Image MakeLine(Transform parent, Vector2 aMin, Vector2 aMax, float height, Color col)
    {
        var go  = new GameObject("Line");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = WithAlpha(col, 0f);
        var r   = go.GetComponent<RectTransform>();
        r.anchorMin = aMin;
        r.anchorMax = aMax;
        r.offsetMin = Vector2.zero;
        r.offsetMax = new Vector2(0f, height);
        return img;
    }

    private void Stretch(GameObject go)
    {
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    private Color WithAlpha(Color c, float a) { c.a = a; return c; }
}
