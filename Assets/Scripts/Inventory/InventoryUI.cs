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

    [Tooltip("Les 8 Images de fond des slots (pour le highlight de sélection)")]
    public Image[] m_slotFrames = new Image[8];

    [Tooltip("Couleur du slot sélectionné dans l'inventaire")]
    public Color m_slotSelectedColor = Color.cyan;

    [Tooltip("Couleur normale des slots inventaire")]
    public Color m_slotNormalColor = Color.white;

    [Tooltip("Couleur des slots hotbar (0 et 1) dans le panel inventaire")]
    public Color m_hotbarSlotColor = new Color(1f, 0.85f, 0.3f, 1f);

    [Header("Référence")]
    public Inventory m_inventory;

    public static bool IsOpen { get; private set; } = false;

    private bool m_inventoryOpen = false;
    private int m_selectedSlot = -1;

    void Awake()
    {
        // Wiring des clics avant tout
        for (int i = 0; i < m_slotButtons.Length; i++)
        {
            int index = i;
            if (m_slotButtons[i] != null)
                m_slotButtons[i].onClick.AddListener(() => OnSlotClicked(index));
        }
    }

    void Start()
    {
        if (m_inventoryPanel != null)
            m_inventoryPanel.SetActive(false);

        if (m_inventory != null)
            m_inventory.OnInventoryChanged += RefreshUI;

        RefreshUI();
    }

    private void AssignToHotbar(int hotbarIndex)
    {
        if (m_selectedSlot == hotbarIndex) { m_selectedSlot = -1; RefreshUI(); return; }
        m_inventory.SwapSlots(m_selectedSlot, hotbarIndex);
        m_selectedSlot = -1;
        RefreshUI();
    }

    public void OnSlotClickedPublic(int index) => OnSlotClicked(index);

    private void OnSlotClicked(int index)
    {
        Debug.Log($"[InventoryUI] Slot cliqué : {index} | inventory null : {m_inventory == null}");
        if (m_inventory == null) return;

        if (m_selectedSlot == -1)
        {
            // Sélectionne uniquement si le slot n'est pas vide
            if (!m_inventory.GetSlots()[index].IsEmpty())
                m_selectedSlot = index;
        }
        else if (m_selectedSlot == index)
        {
            // Clic sur le même slot → annule la sélection
            m_selectedSlot = -1;
        }
        else
        {
            // Échange et désélectionne
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
            IsOpen = m_inventoryOpen;
            if (m_inventoryPanel != null)
                m_inventoryPanel.SetActive(m_inventoryOpen);

            if (!m_inventoryOpen)
            {
                m_selectedSlot = -1;
                RefreshUI();
            }
        }

        // Échap annule la sélection
        if (Input.GetKeyDown(KeyCode.Escape) && m_selectedSlot != -1)
        {
            m_selectedSlot = -1;
            RefreshUI();
        }

        // Si un slot est sélectionné : 1 ou 2 l'assigne à la hotbar correspondante
        if (m_selectedSlot != -1)
        {
            // Supporte clavier AZERTY (&/é) et QWERTY (1/2)
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Ampersand))
                AssignToHotbar(0);
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.At))
                AssignToHotbar(1);
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

            // Couleur du cadre : sélectionné > hotbar > normal
            if (i < m_slotFrames.Length && m_slotFrames[i] != null)
            {
                if (i == m_selectedSlot)
                    m_slotFrames[i].color = m_slotSelectedColor;
                else if (i < Inventory.HOTBAR_SLOTS)
                    m_slotFrames[i].color = m_hotbarSlotColor;
                else
                    m_slotFrames[i].color = m_slotNormalColor;
            }
        }
    }

    void OnDestroy()
    {
        if (m_inventory != null)
            m_inventory.OnInventoryChanged -= RefreshUI;
    }
}
