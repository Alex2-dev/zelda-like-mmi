/// <summary>Un slot de l'inventaire : quel objet et combien.</summary>
[System.Serializable]
public class InventorySlot
{
    public ItemData m_item;
    public int m_quantity;

    public bool IsEmpty() => m_item == null || m_quantity <= 0;

    public bool CanStack(ItemData item)
    {
        return m_item == item && m_quantity < m_item.m_maxStack;
    }

    public void Clear()
    {
        m_item = null;
        m_quantity = 0;
    }
}
