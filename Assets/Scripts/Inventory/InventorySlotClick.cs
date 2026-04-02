using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// À mettre sur chaque slot de l'inventaire à la place du Button.
/// Glisse l'InventoryUI et l'index du slot dans l'Inspector.
/// </summary>
public class InventorySlotClick : MonoBehaviour, IPointerClickHandler
{
    public InventoryUI m_inventoryUI;
    public int m_slotIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[InventorySlotClick] Slot {m_slotIndex} cliqué");
        m_inventoryUI?.OnSlotClickedPublic(m_slotIndex);
    }
}
