// Assets/Scripts/UI/PCScreenBlink.cs
using UnityEngine;

public class PCScreenBlink : MonoBehaviour
{
    SpriteRenderer m_renderer;
    float m_speed = 1.2f;
    float m_minBrightness = 0.7f;
    float m_maxBrightness = 1.0f;

    void Awake()
    {
        m_renderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * m_speed) + 1f) / 2f;
        float brightness = Mathf.Lerp(m_minBrightness, m_maxBrightness, t);
        Color c = m_renderer.color;
        c.r = brightness;
        c.g = brightness;
        c.b = brightness;
        m_renderer.color = c;
    }
}
