using UnityEngine;

/// <summary>
/// Place un piège à la position du joueur.
/// SETUP : Assets > Create > Inventory > Effects > Place Trap Effect
/// </summary>
[CreateAssetMenu(fileName = "PlaceTrapEffect", menuName = "Inventory/Effects/Place Trap Effect")]
public class PlaceTrapEffect : ItemEffect
{
    [Tooltip("Les données du piège à poser")]
    public TrapData m_trapData;

    public override void ApplyEffect(GameObject player)
    {
        if (m_trapData == null || m_trapData.m_trapPrefab == null) return;

        GameObject trap = Instantiate(m_trapData.m_trapPrefab, player.transform.position, Quaternion.identity);
        TrapBehavior tb = trap.GetComponent<TrapBehavior>();
        if (tb != null)
            tb.m_trapData = m_trapData;
    }
}
