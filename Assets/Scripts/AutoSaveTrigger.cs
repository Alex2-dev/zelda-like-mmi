using UnityEngine;

/// <summary>
/// Placer ce script sur un trigger collider dans MainScene.
/// Déclenche une sauvegarde automatique quand le joueur entre dans la zone.
/// Ne sauvegarde qu'une seule fois par zone (m_triggered).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AutoSaveTrigger : MonoBehaviour
{
    private bool m_triggered = false;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (m_triggered) return;
        if (!other.CompareTag("Player")) return;

        m_triggered = true;

        if (GameManager.Instance != null)
            GameManager.Instance.QuickSave();

        Debug.Log("[AutoSave] Sauvegarde automatique déclenchée.");
    }
}
