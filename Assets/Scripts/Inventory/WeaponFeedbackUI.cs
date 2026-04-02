using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Affiche le cercle de rechargement et le message "pas de munitions".
/// SETUP dans Unity :
/// - Créer un enfant du Canvas nommé "WeaponFeedback"
///   - ReloadCircle : Image circulaire, Fill Type = Radial 360, Fill Origin = Top, désactivée par défaut
///   - NoAmmoText   : Text "Consomme des balles d'abord !", désactivé par défaut
/// - Glisser les références dans cet Inspector.
/// - Glisser l'Inventory du joueur dans m_inventory.
/// </summary>
public class WeaponFeedbackUI : MonoBehaviour
{
    [Header("Références")]
    public Inventory m_inventory;
    public InventoryUI m_inventoryUI;

    [Tooltip("Image circulaire (Fill Type = Radial 360)")]
    public Image m_reloadCircle;

    [Tooltip("Text affiché quand pas de munitions")]
    public Text m_noAmmoText;

    [Tooltip("Durée d'affichage du message no-ammo (secondes)")]
    public float m_messageDuration = 2f;

    [Header("Fade")]
    public float m_fadeDuration = 0.3f;

    private float m_reloadDuration;
    private float m_reloadTimer;
    private bool  m_isReloading;
    private float m_messageTimer;
    private float m_fadeTimer;
    private bool  m_fadingIn;

    void Start()
    {
        if (m_reloadCircle != null) m_reloadCircle.gameObject.SetActive(false);
        if (m_noAmmoText   != null) m_noAmmoText.gameObject.SetActive(false);

        if (m_inventory != null)
        {
            m_inventory.OnWeaponEquipped += StartReload;
            m_inventory.OnShowMessage    += ShowNoAmmo;
        }
    }

    void OnDestroy()
    {
        if (m_inventory != null)
        {
            m_inventory.OnWeaponEquipped -= StartReload;
            m_inventory.OnShowMessage    -= ShowNoAmmo;
        }
    }

    void Update()
    {
        // Cercle de rechargement
        if (m_isReloading)
        {
            m_reloadTimer += Time.deltaTime;
            float fill = Mathf.Clamp01(m_reloadTimer / m_reloadDuration);
            if (m_reloadCircle != null) m_reloadCircle.fillAmount = fill;

            if (m_reloadTimer >= m_reloadDuration)
            {
                m_isReloading = false;
                if (m_reloadCircle != null) m_reloadCircle.gameObject.SetActive(false);
            }
        }

        // Fade du message no-ammo
        if (m_noAmmoText != null && m_noAmmoText.gameObject.activeSelf)
        {
            m_fadeTimer += Time.deltaTime;
            float alpha;

            if (m_fadingIn)
            {
                alpha = Mathf.Clamp01(m_fadeTimer / m_fadeDuration);
                if (m_fadeTimer >= m_fadeDuration) { m_fadingIn = false; m_fadeTimer = 0f; }
            }
            else if (m_messageTimer > m_fadeDuration)
            {
                alpha = 1f;
            }
            else
            {
                alpha = Mathf.Clamp01(m_messageTimer / m_fadeDuration);
            }

            Color c = m_noAmmoText.color;
            c.a = alpha;
            m_noAmmoText.color = c;

            m_messageTimer -= Time.deltaTime;
            if (m_messageTimer <= 0f)
            {
                m_noAmmoText.gameObject.SetActive(false);
                m_messageTimer = 0f;
            }
        }
    }

    private void StartReload(float duration)
    {
        m_reloadDuration = duration;
        m_reloadTimer    = 0f;
        m_isReloading    = true;

        if (m_reloadCircle != null)
        {
            // Positionne le cercle sur le slot hotbar actif
            if (m_inventoryUI != null && m_inventory != null)
            {
                int activeSlot = m_inventory.SelectedHotbarIndex;
                if (activeSlot < m_inventoryUI.m_hotbarFrames.Length && m_inventoryUI.m_hotbarFrames[activeSlot] != null)
                {
                    m_reloadCircle.rectTransform.position =
                        m_inventoryUI.m_hotbarFrames[activeSlot].rectTransform.position;
                }
            }

            m_reloadCircle.fillAmount = 0f;
            m_reloadCircle.gameObject.SetActive(true);
        }
    }

    private void ShowNoAmmo(string message)
    {
        if (m_noAmmoText != null)
        {
            m_noAmmoText.text = message;
            Color c = m_noAmmoText.color;
            c.a = 0f;
            m_noAmmoText.color = c;
            m_noAmmoText.gameObject.SetActive(true);
        }
        m_messageTimer = m_messageDuration;
        m_fadeTimer    = 0f;
        m_fadingIn     = true;
    }
}
