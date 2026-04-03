// Assets/Scripts/UI/PCScreenBlink.cs
using UnityEngine;

/// <summary>
/// Pulse la luminosité du SpriteRenderer pour simuler un écran de moniteur.
/// </summary>
public class PCScreenBlink : MonoBehaviour
{
    SpriteRenderer m_renderer;
    Color m_baseColor;
    float m_speed = 1.2f;
    float m_minBrightness = 0.7f;
    float m_maxBrightness = 1.0f;

    void Awake()
    {
        m_renderer = GetComponent<SpriteRenderer>();
        if (m_renderer == null)
        {
            Debug.LogError("[PCScreenBlink] No SpriteRenderer found on " + gameObject.name);
            enabled = false;
            return;
        }
        m_baseColor = m_renderer.color;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * m_speed) + 1f) / 2f;
        float brightness = Mathf.Lerp(m_minBrightness, m_maxBrightness, t);
        m_renderer.color = new Color(
            m_baseColor.r * brightness,
            m_baseColor.g * brightness,
            m_baseColor.b * brightness,
            m_baseColor.a
        );
    }
}
