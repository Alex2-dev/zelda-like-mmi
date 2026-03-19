using UnityEngine;

public class RoomContext : MonoBehaviour
{
    [Header("Identité de la salle")]
    public string roomId = "salle_01";

    [Header("Restrictions")]
    public bool isBossRoom = false;
    public bool isCinematicRoom = false;
    public int maxEventsPerRoom = 3;

    // ── État runtime (géré par EventManager) ─────────────────
    [HideInInspector] public int eventsTriggeredCount = 0;
    [HideInInspector] public bool hasStalkerActive = false;

    void OnEnable()
    {
        eventsTriggeredCount = 0;
        hasStalkerActive = false;

        if (EventManager.Instance != null)
            EventManager.Instance.OnRoomEntered(this);
    }

    void OnDisable()
    {
        if (EventManager.Instance != null)
            EventManager.Instance.OnRoomLeft(this);
    }
}
