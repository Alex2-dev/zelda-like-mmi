// ============================================================
//  GlitchOverlay.cs — Overlay glitch écran via Image UI
// ============================================================
//  SETUP Unity :
//  1. Dans le Canvas principal, créer une Image plein écran
//     (couleur de ta texture glitch ou une couleur unie avec alpha).
//  2. Attacher ce script sur cette Image.
//  3. Glisser la référence dans EventManager.m_glitchOverlay.
//
//  OPTIONNEL : remplacer m_overlayImage par un RawImage avec
//  une texture de distorsion/chromatic aberration animée en sprite sheet.
// ============================================================
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GlitchOverlay : MonoBehaviour
{
    [Header("Paramètres de l'effet")]
    [Tooltip("Intensité maximale de l'overlay (alpha de l'image).")]
    [Range(0f, 1f)] public float m_maxAlpha = 0.75f;

    [Tooltip("Couleur de l'overlay de glitch.")]
    public Color m_glitchColor = new Color(0.8f, 0.1f, 0.9f); // violet/cyan glitchy

    private Image   m_overlayImage;
    private bool    m_isGlitching = false;

    void Awake()
    {
        m_overlayImage = GetComponent<Image>();
        SetAlpha(0f); // invisible au démarrage
    }

    /// <summary>Déclenche l'effet glitch pendant <paramref name="duration"/> secondes.</summary>
    public void TriggerGlitch(float duration)
    {
        if (m_isGlitching)
            StopAllCoroutines(); // si déjà actif, on repart proprement

        m_overlayImage.color = m_glitchColor;
        StartCoroutine(GlitchRoutine(duration));
    }

    private IEnumerator GlitchRoutine(float duration)
    {
        m_isGlitching = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Clignotement irrégulier pour simuler la distorsion
            float noise = Mathf.PerlinNoise(elapsed * 15f, 0f); // bruit pseudo-aléatoire
            SetAlpha(noise * m_maxAlpha);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Fade out rapide sur 0.15s
        float fadeTime = 0.15f;
        float start    = m_overlayImage.color.a;
        elapsed        = 0f;
        while (elapsed < fadeTime)
        {
            SetAlpha(Mathf.Lerp(start, 0f, elapsed / fadeTime));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        SetAlpha(0f);
        m_isGlitching = false;
    }

    private void SetAlpha(float alpha)
    {
        Color c = m_overlayImage.color;
        c.a = alpha;
        m_overlayImage.color = c;
    }
}
