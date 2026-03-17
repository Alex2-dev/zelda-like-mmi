using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Met à jour les barres de vie, d'intégrité et de faim (Sliders Unity).
///
/// SETUP :
/// - Glisser les Sliders HealthBar, IntegrityBar et HungerBar dans les champs ci-dessous
/// - Glisser le Player dans m_playerStats
/// </summary>
public class PlayerStatsUI : MonoBehaviour
{
    [Header("Barres de vie, d'intégrité et de faim")]
    public Slider m_healthBar;
    public Slider m_integrityBar;
    public Slider m_hungerBar;

    [Header("Référence joueur")]
    public PlayerStats m_playerStats;

    void Update()
    {
        if (m_playerStats == null) return;

        if (m_healthBar != null)
            m_healthBar.value = m_playerStats.GetHealthNormalized();

        if (m_integrityBar != null)
            m_integrityBar.value = m_playerStats.GetIntegrityNormalized();

        if (m_hungerBar != null)
            m_hungerBar.value = m_playerStats.GetHungerNormalized();
    }
}
