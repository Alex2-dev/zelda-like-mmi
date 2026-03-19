using UnityEngine;

/// <summary>
/// Affiche une barre de vie au-dessus de l'ennemi via deux sprites enfants.
/// SETUP :
/// - Ajouter ce script sur le prefab ennemi (avec EnemyHealth).
/// - La barre se crée automatiquement au démarrage, aucun prefab nécessaire.
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Position")]
    public float m_offsetY = 1.0f;

    [Header("Dimensions")]
    public float m_width  = 1.2f;
    public float m_height = 0.18f;

    [Header("Couleurs")]
    public Color m_backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color m_fillColor       = new Color(0.2f, 0.9f, 0.2f, 0.9f);

    private EnemyHealth m_health;
    private Transform   m_fill;

    void Awake()
    {
        m_health = GetComponent<EnemyHealth>();
        BuildBar();
    }

    void LateUpdate()
    {
        if (m_fill == null || m_health == null) return;

        float ratio = m_health.GetHealthNormalized();
        m_fill.localScale = new Vector3(m_width * ratio, m_height, 1f);
        // Recentre le fill à gauche quand il rétrécit
        m_fill.localPosition = new Vector3(m_width * (ratio - 1f) * 0.5f, m_offsetY, -0.2f);
    }

    private void BuildBar()
    {
        // ── Fond ──────────────────────────────────────────────
        GameObject bg = new GameObject("HealthBar_BG");
        bg.transform.SetParent(transform, false);
        bg.transform.localPosition = new Vector3(0f, m_offsetY, -0.1f);
        bg.transform.localScale    = new Vector3(m_width, m_height, 1f);

        SpriteRenderer bgSr = bg.AddComponent<SpriteRenderer>();
        bgSr.sprite            = GetWhiteSprite();
        bgSr.color             = m_backgroundColor;
        bgSr.sortingLayerName  = "Player";
        bgSr.sortingOrder      = 100;

        // ── Remplissage ───────────────────────────────────────
        GameObject fill = new GameObject("HealthBar_Fill");
        fill.transform.SetParent(transform, false);
        fill.transform.localPosition = new Vector3(0f, m_offsetY, -0.2f);
        fill.transform.localScale    = new Vector3(m_width, m_height, 1f);

        SpriteRenderer fillSr = fill.AddComponent<SpriteRenderer>();
        fillSr.sprite           = GetWhiteSprite();
        fillSr.color            = m_fillColor;
        fillSr.sortingLayerName = "Player";
        fillSr.sortingOrder     = 101;

        m_fill = fill.transform;

        Debug.Log($"[HealthBar] Créée sur {gameObject.name} | BG layer={bgSr.sortingLayerName} order={bgSr.sortingOrder} pos={bg.transform.position}");
    }

    private Sprite GetWhiteSprite()
    {
        return Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }
}
