// Assets/Scripts/Inventory/Effects/HealOverTimeEffect.cs
using UnityEngine;

/// <summary>
/// Soigne le joueur progressivement sur la durée.
/// Note : le soin se déclenche toutes les secondes entières. Une durée de 5.5s
/// produit 5 ticks (à t=1s, 2s, 3s, 4s, 5s) — le demi-tick final est ignoré.
/// SETUP : Assets > Create > Inventory > Effects > Heal Over Time Effect
/// </summary>
[CreateAssetMenu(fileName = "HealOverTimeEffect", menuName = "Inventory/Effects/Heal Over Time Effect")]
public class HealOverTimeEffect : ItemEffect
{
    [Tooltip("Points de vie restaurés par seconde")]
    public float m_healPerSecond = 5f;

    [Tooltip("Durée totale en secondes (arrondie au tick inférieur)")]
    public float m_duration = 10f;

    public override void ApplyEffect(GameObject player)
    {
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
            stats.StartHealOverTime(m_healPerSecond, m_duration);
    }
}
