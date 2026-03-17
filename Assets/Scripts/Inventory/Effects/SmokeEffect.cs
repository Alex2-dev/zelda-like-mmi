using UnityEngine;

/// <summary>
/// Désactive le rayon de détection du joueur pendant 15 secondes.
/// Effet de bord intentionnel : coupe aussi la détection des PNJ et des buissons.
/// SETUP : Assets > Create > Inventory > Effects > Smoke Effect
/// </summary>
[CreateAssetMenu(fileName = "SmokeEffect", menuName = "Inventory/Effects/Smoke Effect")]
public class SmokeEffect : ItemEffect
{
    public override void ApplyEffect(GameObject player)
    {
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
            stats.StartRadiusEffect(0f, 15f);
    }
}
