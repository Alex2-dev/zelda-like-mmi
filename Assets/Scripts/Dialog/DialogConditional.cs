using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Variante de Dialog qui affiche un texte différent selon si le joueur
/// possède un item précis dans son inventaire.
///
/// SETUP :
/// 1. Remplacer le composant Dialog par DialogConditional sur le NPC.
/// 2. Dans m_requiredItem, glisser le ScriptableObject ItemData de la clé.
/// 3. Remplir m_dialogWithItem  (joueur a la clé).
/// 4. Remplir m_dialogWithoutItem (joueur n'a pas la clé).
/// </summary>
public class DialogConditional : Dialog
{
    [Header("Condition")]
    [Tooltip("L'item que le joueur doit posséder (ex. ItemData de la clé).")]
    public ItemData m_requiredItem;

    [Header("Dialogue si le joueur A l'item")]
    public List<DialogPage> m_dialogWithItem;

    [Header("Dialogue si le joueur N'A PAS l'item")]
    public List<DialogPage> m_dialogWithoutItem;

    public override List<DialogPage> GetDialog()
    {
        if (m_requiredItem == null)
            return m_dialogWithoutItem;

        Inventory inventory = FindObjectOfType<Inventory>();
        if (inventory != null && HasItem(inventory, m_requiredItem))
            return m_dialogWithItem;

        return m_dialogWithoutItem;
    }

    private bool HasItem(Inventory inventory, ItemData item)
    {
        foreach (var slot in inventory.GetSlots())
        {
            if (!slot.IsEmpty() && slot.m_item == item)
                return true;
        }
        return false;
    }
}
