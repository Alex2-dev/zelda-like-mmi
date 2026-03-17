using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Applique les effets visuels d'intégrité sur la caméra et l'UI :
///   - Vignette noire qui s'assombrit avec l'intégrité
///   - Environnement qui passe en niveaux de gris
///
/// SETUP dans Unity :
/// 1. Attacher ce script à la Main Camera
/// 2. Créer un Material avec le shader "Custom/GrayscaleEffect" (Assets > Create > Material)
///    et le glisser dans le champ m_grayscaleMaterial
/// 3. Dans le Canvas, créer une Image plein écran noire (couleur #000000) :
///    - Ancre : stretch sur toute la zone (Alt+clic sur le préréglage pour tout étirer)
///    - Alpha de départ : 0
///    - Désactiver "Raycast Target"
///    - La glisser dans le champ m_vignetteImage
/// 4. Glisser le Player (PlayerStats) dans m_playerStats
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraEffects : MonoBehaviour
{
    [Header("Vignette (Image UI noire plein écran)")]
    [Tooltip("Image noire plein écran dans le Canvas - son alpha augmente avec l'intégrité")]
    public Image m_vignetteImage;

    [Tooltip("Transparence maximale de la vignette quand l'intégrité est au max (0 = invisible, 1 = opaque)")]
    [Range(0f, 1f)]
    public float m_maxVignetteAlpha = 0.65f;

    [Header("Niveaux de gris (Material avec shader GrayscaleEffect)")]
    [Tooltip("Material créé depuis le shader Custom/GrayscaleEffect")]
    public Material m_grayscaleMaterial;

    [Header("Référence joueur")]
    public PlayerStats m_playerStats;

    [Header("Vitesse de transition")]
    [Tooltip("Vitesse à laquelle le grayscale s'applique/s'enlève (plus grand = plus rapide)")]
    public float m_transitionSpeed = 3f;

    private float m_currentGrayscale = 0f;

    void Update()
    {
        if (m_playerStats == null) return;

        float integrityNorm = m_playerStats.GetIntegrityNormalized();

        // Vignette : alpha proportionnel à l'intégrité
        if (m_vignetteImage != null)
        {
            Color c = m_vignetteImage.color;
            c.a = Mathf.Lerp(0f, m_maxVignetteAlpha, integrityNorm);
            m_vignetteImage.color = c;
        }

        // Grayscale : transition progressive pour éviter un changement brutal
        m_currentGrayscale = Mathf.Lerp(m_currentGrayscale, integrityNorm, Time.deltaTime * m_transitionSpeed);

        if (m_grayscaleMaterial != null)
        {
            m_grayscaleMaterial.SetFloat("_GrayscaleAmount", m_currentGrayscale);
        }
    }

    // Applique le shader grayscale sur le rendu de la caméra
    // IMPORTANT : ne fonctionne qu'avec le Built-in Render Pipeline.
    // Si tu utilises URP, utilise un Renderer Feature à la place.
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (m_grayscaleMaterial != null && m_currentGrayscale > 0.01f)
        {
            Graphics.Blit(src, dest, m_grayscaleMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
