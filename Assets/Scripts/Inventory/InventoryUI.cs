using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Gère l'affichage de la hotbar (toujours visible) et de l'inventaire complet (touche E).
///
/// SETUP dans Unity — structure UI à créer dans le Canvas :
///
///   Canvas
///    ├── Hotbar                    (objet vide, ancré en haut à droite)
///    │    ├── Slot0                (Image fond, 50x50)
///    │    │    ├── Icon0           (Image icône, 40x40, centré)
///    │    │    └── Quantity0       (Text, coin bas-droit, taille 12)
///    │    └── Slot1                (Image fond, 50x50)
///    │         ├── Icon1
///    │         └── Quantity1
///    └── InventoryPanel            (Panel, centré, désactivé par défaut)
///         ├── Slot0..Slot7         (8 cases identiques)
///              ├── Icon0..Icon7
///              └── Quantity0..Quantity7
///
/// Puis glisser toutes les références dans l'Inspector de ce script.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Hotbar (toujours visible, 2 slots)")]
    [Tooltip("Les 2 Images icônes de la hotbar (Icon0, Icon1)")]
    public Image[] m_hotbarIcons = new Image[2];

    [Tooltip("Les 2 Texts de quantité de la hotbar")]
    public Text[] m_hotbarQuantities = new Text[2];

    [Tooltip("Les 2 Images de fond/cadre (changent de couleur selon la sélection)")]
    public Image[] m_hotbarFrames = new Image[2];

    [Tooltip("Couleur du cadre quand le slot est sélectionné")]
    public Color m_selectedColor = Color.yellow;

    [Tooltip("Couleur du cadre quand le slot n'est pas sélectionné")]
    public Color m_normalColor = Color.white;

    [Header("Inventaire complet (touche E, 8 slots)")]
    [Tooltip("Le Panel qui contient les 8 slots — désactivé par défaut")]
    public GameObject m_inventoryPanel;

    [Tooltip("Les 8 Images icônes des slots")]
    public Image[] m_slotIcons = new Image[8];

    [Tooltip("Les 8 Texts de quantité des slots")]
    public Text[] m_slotQuantities = new Text[8];

    [Header("Boutons des slots (pour le clic — ajouter Button sur chaque slot)")]
    [Tooltip("Les 8 boutons des slots de l'inventaire complet (dans l'ordre 0 à 7)")]
    public Button[] m_slotButtons = new Button[8];

    [Header("Référence")]
    public Inventory m_inventory;

    private bool m_inventoryOpen = false;
    private int m_selectedSlot = -1; // slot en attente de déplacement (-1 = aucun)

    void Start()
    {
        if (m_inventoryPanel != null)
            m_inventoryPanel.SetActive(false);

        if (m_inventory != null)
            m_inventory.OnInventoryChanged += RefreshUI;

        // Wiring des clics sur les slots
        for (int i = 0; i < m_slotButtons.Length; i++)
        {
            int index = i;
            if (m_slotButtons[i] != null)
                m_slotButtons[i].onClick.AddListener(() => OnSlotClicked(index));
        }

        RefreshUI();
    }

    /// <summary>
    /// Premier clic : sélectionne le slot source.
    /// Deuxième clic : échange le slot source avec le slot cible.
    /// </summary>
    private void OnSlotClicked(int index)
    {
        if (m_inventory == null) return;

        if (m_selectedSlot == -1)
        {
            // Sélectionne le slot si non vide
            if (!m_inventory.GetSlots()[index].IsEmpty())
                m_selectedSlot = index;
        }
        else
        {
            // Échange avec le slot cible
            m_inventory.SwapSlots(m_selectedSlot, index);
            m_selectedSlot = -1;
        }
        RefreshUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            m_inventoryOpen = !m_inventoryOpen;
            if (m_inventoryPanel != null)
                m_inventoryPanel.SetActive(m_inventoryOpen);
        }
    }

    void RefreshUI()
    {
        if (m_inventory == null) return;
        var slots = m_inventory.GetSlots();

        // --- Hotbar (slots 0 et 1) ---
        for (int i = 0; i < Inventory.HOTBAR_SLOTS; i++)
        {
            if (i >= m_hotbarIcons.Length) break;

            bool empty = slots[i].IsEmpty();

            if (m_hotbarIcons[i] != null)
            {
                m_hotbarIcons[i].sprite = empty ? null : slots[i].m_item.m_icon;
                m_hotbarIcons[i].color = empty ? new Color(1, 1, 1, 0) : Color.white;
            }

            if (m_hotbarQuantities[i] != null)
                m_hotbarQuantities[i].text = (!empty && slots[i].m_quantity > 1) ? slots[i].m_quantity.ToString() : "";

            if (m_hotbarFrames[i] != null)
                m_hotbarFrames[i].color = (i == m_inventory.SelectedHotbarIndex) ? m_selectedColor : m_normalColor;
        }

        // --- Inventaire complet (8 slots) ---
        for (int i = 0; i < Inventory.TOTAL_SLOTS; i++)
        {
            if (i >= m_slotIcons.Length) break;

            bool empty = slots[i].IsEmpty();

            if (m_slotIcons[i] != null)
            {
                m_slotIcons[i].sprite = empty ? null : slots[i].m_item.m_icon;
                m_slotIcons[i].color = empty ? new Color(1, 1, 1, 0) : Color.white;
            }

            if (m_slotQuantities[i] != null)
                m_slotQuantities[i].text = (!empty && slots[i].m_quantity > 1) ? slots[i].m_quantity.ToString() : "";
        }
    }

    void OnDestroy()
    {
        if (m_inventory != null)
            m_inventory.OnInventoryChanged -= RefreshUI;
    }
}
