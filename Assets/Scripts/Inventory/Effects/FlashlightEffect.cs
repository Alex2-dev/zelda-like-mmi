using UnityEngine;

/// <summary>
/// Double le rayon de détection du joueur pendant 30 secondes.
/// SETUP : Assets > Create > Inventory > Effects > Flashlight Effect
/// </summary>
[CreateAssetMenu(fileName = "FlashlightEffect", menuName = "Inventory/Effects/Flashlight Effect")]
public class FlashlightEffect : ItemEffect
{
    public override void ApplyEffect(GameObject player)
    {
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
            stats.StartRadiusEffect(2f, 30f);
    }
}
