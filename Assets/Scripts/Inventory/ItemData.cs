using UnityEngine;

/// <summary>
/// Définit un type d'objet (potion, clé, arme...).
///
/// SETUP :
/// - Assets > Create > Inventory > Item Data
/// - Remplir le nom, l'icône, le stack max et l'effet
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Présentation")]
    public string m_itemName = "Nouvel objet";
    public Sprite m_icon;

    [Header("Stack")]
    [Tooltip("Nombre maximum du même objet par case (1 à 3)")]
    [Range(1, 3)]
    public int m_maxStack = 1;

    [Header("Effet")]
    [Tooltip("Laisser vide si l'objet n'a pas d'effet (objet de quête, clé...)")]
    public ItemEffect m_effect;
}
