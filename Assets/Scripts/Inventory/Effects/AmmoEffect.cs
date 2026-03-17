using UnityEngine;

/// <summary>
/// Ajoute des munitions au WeaponManager du joueur.
/// SETUP : Assets > Create > Inventory > Effects > Ammo Effect
/// </summary>
[CreateAssetMenu(fileName = "AmmoEffect", menuName = "Inventory/Effects/Ammo Effect")]
public class AmmoEffect : ItemEffect
{
    [Tooltip("Type de munitions à ajouter")]
    public AmmoType m_ammoType = AmmoType.Pistol;

    [Tooltip("Nombre de munitions ajoutées")]
    public int m_amount = 10;

    public override void ApplyEffect(GameObject player)
    {
        WeaponManager wm = player.GetComponent<WeaponManager>();
        if (wm != null)
            wm.AddAmmo(m_ammoType, m_amount);
    }
}
