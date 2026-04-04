using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Passage bloqué par des tiles qui s'ouvre quand le joueur possède
/// un certain objet en quantité suffisante dans son inventaire.
///
/// SETUP :
/// 1. Créer un GameObject "PassageBlocker" avec un Collider2D (Is Trigger = true)
///    positionné devant le passage.
/// 2. Attacher ce script au même GameObject.
/// 3. Dans m_doorTilemap : glisser le Tilemap qui contient les tiles de blocage.
/// 4. Dans m_doorTilePositions : lister les coordonnées des tiles à effacer
///    (en coordonnées Tilemap, lisibles dans la fenêtre Tilemap Editor).
/// 5. Dans m_requiredItem : glisser l'ItemData requis.
/// 6. Régler m_requiredQuantity (ex: 3).
/// 7. Cocher m_consumeItems si les objets doivent être retirés de l'inventaire.
/// </summary>
public class ItemLockedPassage : MonoBehaviour
{
    [Header("Tiles à effacer (la porte)")]
    [Tooltip("Le Tilemap sur lequel les tiles de blocage sont placées.")]
    [SerializeField] private Tilemap m_doorTilemap;

    [Tooltip("Liste des positions (en coordonnées Tilemap) des tiles à effacer.")]
    [SerializeField] private List<Vector3Int> m_doorTilePositions = new List<Vector3Int>();

    [Header("Condition d'ouverture")]
    [Tooltip("L'objet que le joueur doit posséder.")]
    [SerializeField] private ItemData m_requiredItem;

    [Tooltip("Quantité minimale requise dans l'inventaire.")]
    [SerializeField] [Min(1)] private int m_requiredQuantity = 1;

    [Tooltip("Si vrai, retire les objets de l'inventaire lors de l'ouverture.")]
    [SerializeField] private bool m_consumeItems = false;

    [Header("Feedback visuel (optionnel)")]
    [Tooltip("GameObject affiché si la condition N'est PAS remplie (ex: panneau 'Clé requise').")]
    [SerializeField] private GameObject m_lockedFeedback;

    private bool m_isOpen = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (m_isOpen) return;
        if (!other.CompareTag("Player")) return;

        Inventory inventory = other.GetComponent<Inventory>();
        if (inventory == null)
            inventory = other.GetComponentInChildren<Inventory>();
        if (inventory == null)
            inventory = other.GetComponentInParent<Inventory>();
        if (inventory == null) return;

        int total = CountItem(inventory, m_requiredItem);

        if (total >= m_requiredQuantity)
        {
            OpenPassage(inventory);
        }
        else
        {
            ShowLockedFeedback();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (m_lockedFeedback != null && other.CompareTag("Player"))
            m_lockedFeedback.SetActive(false);
    }

    // -------------------------------------------------------------------------

    /// <summary>Compte le total d'un item dans tous les slots de l'inventaire.</summary>
    private int CountItem(Inventory inventory, ItemData item)
    {
        if (item == null) return 0;
        int count = 0;
        foreach (var slot in inventory.GetSlots())
        {
            if (!slot.IsEmpty() && slot.m_item == item)
                count += slot.m_quantity;
        }
        return count;
    }

    /// <summary>Ouvre le passage : efface les tiles et consomme les objets si demandé.</summary>
    private void OpenPassage(Inventory inventory)
    {
        m_isOpen = true;

        // Effacer les tiles de blocage
        if (m_doorTilemap != null)
        {
            foreach (Vector3Int pos in m_doorTilePositions)
                m_doorTilemap.SetTile(pos, null);
        }

        // Consommer les objets si nécessaire
        if (m_consumeItems)
            RemoveItems(inventory, m_requiredItem, m_requiredQuantity);

        // Désactiver le feedback de verrouillage
        if (m_lockedFeedback != null)
            m_lockedFeedback.SetActive(false);

        // Désactiver ce collider (le passage est définitivement ouvert)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    /// <summary>Retire exactement 'amount' exemplaires de 'item' de l'inventaire.</summary>
    private void RemoveItems(Inventory inventory, ItemData item, int amount)
    {
        int remaining = amount;
        foreach (var slot in inventory.GetSlots())
        {
            if (remaining <= 0) break;
            if (slot.IsEmpty() || slot.m_item != item) continue;

            int toRemove = Mathf.Min(remaining, slot.m_quantity);
            slot.m_quantity -= toRemove;
            remaining -= toRemove;

            if (slot.m_quantity <= 0)
                slot.Clear();
        }
        // Notifier l'UI
        inventory.OnInventoryChanged?.Invoke();
    }

    /// <summary>Affiche le feedback de verrouillage si assigné.</summary>
    private void ShowLockedFeedback()
    {
        if (m_lockedFeedback != null)
            m_lockedFeedback.SetActive(true);
    }

#if UNITY_EDITOR
    /// <summary>Affiche les positions des tiles dans la Scene view.</summary>
    private void OnDrawGizmos()
    {
        if (m_doorTilemap == null) return;
        Gizmos.color = m_isOpen ? Color.green : Color.red;
        foreach (Vector3Int pos in m_doorTilePositions)
        {
            Vector3 world = m_doorTilemap.CellToWorld(pos) + m_doorTilemap.cellSize * 0.5f;
            Gizmos.DrawWireCube(world, m_doorTilemap.cellSize);
        }
    }
#endif
}
