using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Swap le sprite d'un bouton au survol de la souris.
/// Attacher sur le même GameObject que le composant Image du bouton.
/// </summary>
[RequireComponent(typeof(Image))]
public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Sprite m_normalSprite;
    [SerializeField] private Sprite m_hoverSprite;

    private Image m_image;

    void Awake()
    {
        m_image = GetComponent<Image>();
        if (m_normalSprite == null)
            m_normalSprite = m_image.sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (m_hoverSprite != null)
            m_image.sprite = m_hoverSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_image.sprite = m_normalSprite;
    }
}
