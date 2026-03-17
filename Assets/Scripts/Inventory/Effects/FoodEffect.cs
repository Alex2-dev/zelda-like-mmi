// Assets/Scripts/Inventory/Effects/FoodEffect.cs
using UnityEngine;

/// <summary>
/// Restaure la faim du joueur et optionnellement un peu de vie.
/// SETUP : Assets > Create > Inventory > Effects > Food Effect
/// </summary>
[CreateAssetMenu(fileName = "FoodEffect", menuName = "Inventory/Effects/Food Effect")]
public class FoodEffect : ItemEffect
{
    [Tooltip("Points de faim restaurés")]
    public float m_hungerRestore = 20f;

    [Tooltip("Points de vie bonus (0 = aucun)")]
    public float m_healBonus = 0f;

    public override void ApplyEffect(GameObject player)
    {
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;

        stats.FillHunger(m_hungerRestore);

        if (m_healBonus > 0f)
            stats.Heal(m_healBonus);
    }
}
