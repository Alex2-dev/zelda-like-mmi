using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Met à jour la barre de vie (Slider Unity).
///
/// SETUP :
/// - Glisser le Slider HealthBar dans m_healthBar
/// - Glisser le Player dans m_playerStats
/// </summary>
public class PlayerStatsUI : MonoBehaviour
{
    [Header("Barre de vie")]
    public Slider m_healthBar;

    [Header("Référence joueur")]
    public PlayerStats m_playerStats;

    void Update()
    {
        if (m_playerStats == null) return;

        if (m_healthBar != null)
            m_healthBar.value = m_playerStats.GetHealthNormalized();
    }
}
