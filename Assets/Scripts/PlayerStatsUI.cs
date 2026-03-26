using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Met à jour les barres de vie, d'intégrité et de faim (Sliders Unity).
///
/// SETUP :
/// - Glisser les Sliders HealthBar et IntegrityBar dans les champs ci-dessous
/// - Glisser le Player dans m_playerStats
/// </summary>
public class PlayerStatsUI : MonoBehaviour
{
    [Header("Barres de vie et d'intégrité")]
    public Slider m_healthBar;
    public Slider m_integrityBar;

    [Header("Référence joueur")]
    public PlayerStats m_playerStats;

    void Update()
    {
        if (m_playerStats == null) return;

        if (m_healthBar != null)
            m_healthBar.value = m_playerStats.GetHealthNormalized();

        if (m_integrityBar != null)
            m_integrityBar.value = m_playerStats.GetIntegrityNormalized();
    }
}
