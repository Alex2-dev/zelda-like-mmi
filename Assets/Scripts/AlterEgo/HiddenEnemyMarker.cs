using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Marque un ennemi comme "caché" — invisible en forme normale, bleu en alter ego.
///
/// SETUP :
/// - Ajouter ce script sur le GameObject ennemi voulu
/// - L'ennemi doit avoir un SpriteRenderer
/// </summary>
public class HiddenEnemyMarker : MonoBehaviour
{
    private static readonly List<HiddenEnemyMarker> s_all = new List<HiddenEnemyMarker>();
    private static readonly Color k_alterEgoColor = new Color(0.3f, 0.6f, 1f, 0.7f);

    private SpriteRenderer m_spriteRenderer;

    void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        s_all.Add(this);
    }

    void OnDisable()
    {
        s_all.Remove(this);
    }

    void Start()
    {
        // Applique l'état courant au moment du spawn
        // Par défaut (pas d'AlterEgoManager en scène) : invisible
        AlterEgoManager am = FindObjectOfType<AlterEgoManager>();
        ApplyMode(am != null && am.IsAlterEgo);
    }

    /// <summary>Met à jour la visibilité de tous les ennemis cachés en scène.</summary>
    public static void SetAlterEgoMode(bool isAlterEgo)
    {
        foreach (HiddenEnemyMarker marker in s_all)
        {
            if (marker != null)
                marker.ApplyMode(isAlterEgo);
        }
    }

    private void ApplyMode(bool isAlterEgo)
    {
        if (m_spriteRenderer == null) return;

        if (isAlterEgo)
        {
            m_spriteRenderer.enabled = true;
            m_spriteRenderer.color = k_alterEgoColor;
        }
        else
        {
            m_spriteRenderer.enabled = false;
        }
    }
}
