using UnityEngine;

/// <summary>
/// À attacher sur un NPC (même GameObject que Dialog.cs).
/// Désactive m_zoneToUnlock une fois que le joueur a terminé LE dialogue
/// de CE PNJ précisément (fonctionne avec plusieurs PNJ en scène).
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

    private Dialog m_myDialog;
    private bool m_dialogWasActive = false;
    private bool m_hasUnlocked = false;

    void Awake()
    {
        m_myDialog = GetComponent<Dialog>();
    }

    void Update()
    {
        if (m_hasUnlocked) return;
        if (m_dialogManager == null || m_zoneToUnlock == null || m_myDialog == null) return;

        // On ne réagit que si C'EST notre dialogue qui est actif
        bool isMyDialogActive = m_dialogManager.IsOnScreen()
                                && m_dialogManager.ActiveDialog == m_myDialog;

        if (isMyDialogActive)
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
