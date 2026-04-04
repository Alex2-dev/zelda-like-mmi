using UnityEngine;

/// <summary>
/// À attacher sur un NPC (même GameObject que Dialog.cs).
/// Désactive m_zoneToUnlock une fois que le joueur a terminé le dialogue.
///
/// SETUP :
/// 1. Attacher ce script au GameObject NPC.
/// 2. Glisser le DialogManager de la scène dans m_dialogManager.
/// 3. Glisser la zone à désactiver dans m_zoneToUnlock.
/// </summary>
public class NPCDialogUnlock : MonoBehaviour
{
    [Tooltip("Le DialogManager de la scène.")]
    public DialogManager m_dialogManager;

    [Tooltip("Le GameObject à désactiver quand le dialogue est terminé.")]
    public GameObject m_zoneToUnlock;

    private bool m_dialogWasActive = false;
    private bool m_hasUnlocked = false;

    void Update()
    {
        if (m_hasUnlocked) return;
        if (m_dialogManager == null || m_zoneToUnlock == null) return;

        bool isActive = m_dialogManager.IsOnScreen();

        if (isActive)
        {
            m_dialogWasActive = true;
        }
        else if (m_dialogWasActive)
        {
            m_zoneToUnlock.SetActive(false);
            m_hasUnlocked = true;
            m_dialogWasActive = false;
        }
    }
}
