using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gère la logique de l'inventaire : 8 slots dont les 2 premiers = hotbar.
///
/// SETUP :
/// - Attacher au Player
/// - Glisser le Player dans le champ m_inventory de PlayerBehavior
/// </summary>
public class Inventory : MonoBehaviour
{
    public const int TOTAL_SLOTS = 8;
    public const int HOTBAR_SLOTS = 2;

    [Header("Contenu de départ (drag & drop dans l'Inspector)")]
    [SerializeField] private InventorySlot[] m_startItems = new InventorySlot[TOTAL_SLOTS];

    private List<InventorySlot> m_slots = new List<InventorySlot>();
    private int m_selectedHotbarIndex = 0;

    public int SelectedHotbarIndex => m_selectedHotbarIndex;

    /// <summary>Déclenché à chaque modification (pour rafraîchir l'UI).</summary>
    public System.Action OnInventoryChanged;

    void Awake()
    {
        for (int i = 0; i < TOTAL_SLOTS; i++)
            m_slots.Add(new InventorySlot());
    }

    void Start()
    {
        for (int i = 0; i < m_startItems.Length && i < TOTAL_SLOTS; i++)
        {
            if (m_startItems[i] != null && !m_startItems[i].IsEmpty())
            {
                m_slots[i].m_item = m_startItems[i].m_item;
                m_slots[i].m_quantity = m_startItems[i].m_quantity;
            }
        }
        OnInventoryChanged?.Invoke();
    }

    /// <summary>Retourne tous les slots (lecture seule).</summary>
    public IReadOnlyList<InventorySlot> GetSlots() => m_slots;

    /// <summary>Tente d'ajouter un objet. Retourne true si réussi.</summary>
    public bool AddItem(ItemData item)
    {
        // 1. Essaye d'empiler sur un slot existant du même type
        for (int i = 0; i < TOTAL_SLOTS; i++)
        {
            if (m_slots[i].CanStack(item))
            {
                m_slots[i].m_quantity++;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        // 2. Cherche un slot vide
        for (int i = 0; i < TOTAL_SLOTS; i++)
        {
            if (m_slots[i].IsEmpty())
            {
                m_slots[i].m_item = item;
                m_slots[i].m_quantity = 1;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        // Inventaire plein
        return false;
    }

    /// Déclenché quand une arme est activée : float = durée de rechargement
    public System.Action<float> OnWeaponEquipped;
    /// Déclenché quand il n'y a pas assez de munitions
    public System.Action<string> OnShowMessage;

    /// <summary>Utilise l'objet dans le slot hotbar sélectionné.</summary>
    public void UseSelectedItem()
    {
        InventorySlot slot = m_slots[m_selectedHotbarIndex];
        if (slot.IsEmpty()) return;
        if (slot.m_item.m_effect == null) return;

        // Cas arme : ne pas consommer l'item, vérifier les munitions
        if (slot.m_item.m_effect is EquipWeaponEffect weaponEffect && weaponEffect.m_weaponData != null)
        {
            WeaponManager wm = GetComponent<WeaponManager>();
            if (wm != null)
            {
                int ammo = wm.GetAmmo(weaponEffect.m_weaponData.m_ammoType);
                if (ammo <= 0)
                {
                    OnShowMessage?.Invoke("Consomme des balles d'abord !");
                    return;
                }
                // A des munitions : équipe sans supprimer l'item
                weaponEffect.ApplyEffect(gameObject);
                OnWeaponEquipped?.Invoke(weaponEffect.m_weaponData.m_equipTime);
                OnInventoryChanged?.Invoke();
                return;
            }
        }

        // Comportement normal pour les non-armes
        slot.m_item.m_effect.ApplyEffect(gameObject);
        slot.m_quantity--;
        if (slot.m_quantity <= 0)
            slot.Clear();
        OnInventoryChanged?.Invoke();
    }

    /// <summary>Utilise l'objet dans un slot hotbar précis (0 = &, 1 = é).</summary>
    public void UseHotbarSlot(int index)
    {
        if (index < 0 || index >= HOTBAR_SLOTS) return;
        m_selectedHotbarIndex = index;
        UseSelectedItem();
    }

    /// <summary>Échange le contenu de deux slots.</summary>
    public void SwapSlots(int a, int b)
    {
        if (a < 0 || a >= TOTAL_SLOTS || b < 0 || b >= TOTAL_SLOTS) return;
        ItemData tmpItem = m_slots[a].m_item;
        int tmpQty = m_slots[a].m_quantity;
        m_slots[a].m_item = m_slots[b].m_item;
        m_slots[a].m_quantity = m_slots[b].m_quantity;
        m_slots[b].m_item = tmpItem;
        m_slots[b].m_quantity = tmpQty;
        OnInventoryChanged?.Invoke();
    }

    /// <summary>Change le slot hotbar sélectionné avec la molette.</summary>
    public void ScrollHotbar(float scrollDelta)
    {
        if (scrollDelta == 0) return;
        m_selectedHotbarIndex = (m_selectedHotbarIndex + (scrollDelta > 0 ? -1 : 1) + HOTBAR_SLOTS) % HOTBAR_SLOTS;
        OnInventoryChanged?.Invoke();
    }
}
