using UnityEngine;

/// <summary>
/// Équipe une arme dans le WeaponManager du joueur.
/// SETUP : Assets > Create > Inventory > Effects > Equip Weapon Effect
/// </summary>
[CreateAssetMenu(fileName = "EquipWeaponEffect", menuName = "Inventory/Effects/Equip Weapon Effect")]
public class EquipWeaponEffect : ItemEffect
{
    [Tooltip("L'arme à équiper")]
    public WeaponData m_weaponData;

    public override void ApplyEffect(GameObject player)
    {
        WeaponManager wm = player.GetComponent<WeaponManager>();
        if (wm != null && m_weaponData != null)
            wm.EquipWeapon(m_weaponData);
    }
}
