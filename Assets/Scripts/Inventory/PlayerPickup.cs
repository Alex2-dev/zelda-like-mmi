using UnityEngine;

/// <summary>
/// Gère le ramassage des objets au sol via un CircleCollider2D dédié.
///
/// SETUP :
/// 1. Sur le Player, clic droit > Create Empty > renommer "PickupZone"
/// 2. Sur "PickupZone", Add Component > Circle Collider 2D
///    - Régler le Radius selon la distance de ramassage souhaitée
///    - Cocher "Is Trigger"
/// 3. Sur "PickupZone", Add Component > Player Pickup
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class PlayerPickup : MonoBehaviour
{
    private Inventory m_inventory;

    void Start()
    {
        // Récupère l'Inventory sur le parent (le Player)
        m_inventory = GetComponentInParent<Inventory>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (m_inventory == null) return;

        PickableItem item = other.GetComponent<PickableItem>();
        if (item != null)
            item.TryPickup(m_inventory);
    }
}
