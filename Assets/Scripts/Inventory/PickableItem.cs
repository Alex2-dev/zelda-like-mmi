using UnityEngine;

/// <summary>
/// Ajoute ce script sur un GameObject au sol pour le rendre ramassable.
/// Le ramassage est déclenché par le PlayerPickup (enfant du Player avec CircleCollider2D).
///
/// SETUP :
/// - Ajouter un Collider2D sur l'objet (Is Trigger = coché)
/// - Assigner l'ItemData voulu dans m_itemData
/// </summary>
public class PickableItem : MonoBehaviour
{
    [Tooltip("L'objet qui sera ajouté à l'inventaire du joueur")]
    public ItemData m_itemData;

    /// <summary>Appelée par PlayerPickup quand le joueur entre dans la zone de ramassage.</summary>
    public void TryPickup(Inventory inventory)
    {
        if (m_itemData == null) return;

        if (inventory.AddItem(m_itemData))
        {
            GetComponent<Collider2D>().enabled = false;

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;

            Destroy(gameObject, 0.05f);
        }
        // Si inventaire plein : l'objet reste au sol
    }
}
