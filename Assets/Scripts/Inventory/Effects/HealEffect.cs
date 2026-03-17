using UnityEngine;

/// <summary>
/// Effet : soigne le joueur d'un certain montant.
///
/// SETUP :
/// - Assets > Create > Inventory > Effects > Heal Effect
/// - Régler m_healAmount
/// - Assigner dans le champ "Effect" d'un ItemData
/// </summary>
[CreateAssetMenu(fileName = "HealEffect", menuName = "Inventory/Effects/Heal Effect")]
public class HealEffect : ItemEffect
{
    [Tooltip("Points de vie restaurés quand l'objet est utilisé")]
    public float m_healAmount = 30f;

    public override void ApplyEffect(GameObject player)
    {
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
            stats.Heal(m_healAmount);
    }
}
