// ============================================================
//  EventManager.cs — Gestionnaire global d'événements aléatoires
// ============================================================
//  SETUP Unity :
//  1. Créer un GameObject vide "EventManager" dans la scène persistante.
//  2. Attacher ce script.
//  3. Glisser les références (prefabs, AudioClip, overlays) dans l'inspecteur.
//  4. Placer un RoomContext sur chaque parent de salle.
// ============================================================
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────
    public static EventManager Instance { get; private set; }

    // ── Paramètres de cooldown ────────────────────────────────
    [Header("Cooldown entre événements")]
    [Tooltip("Durée minimale (secondes de jeu actif) entre deux événements.")]
    public float m_cooldownMin = 30f;

    [Tooltip("Durée maximale (secondes de jeu actif) entre deux événements.")]
    public float m_cooldownMax = 45f;

    [Tooltip("Délai minimum entre deux événements de niveau Dangereux.")]
    public float m_dangerousCooldown = 60f;

    // ── Probabilités par catégorie ────────────────────────────
    [Header("Probabilités par catégorie (doivent sommer à 1)")]
    [Range(0f, 1f)] public float m_probDecor    = 0.60f;
    [Range(0f, 1f)] public float m_probGlitch   = 0.25f;
    [Range(0f, 1f)] public float m_probEnnemi   = 0.15f;

    // ── Références scène ──────────────────────────────────────
    [Header("Références scène globales")]
    [Tooltip("Prefab de plateforme glitchée temporaire (event plateforme secrète).")]
    public GameObject m_glitchPlatformPrefab;

    [Tooltip("Prefab de l'ennemi chasseur/stalker (fallback si non défini sur la salle).")]
    public GameObject m_stalkerEnemyPrefab;

    [Tooltip("Prefab du mini-boss (fallback si non défini sur la salle).")]
    public GameObject m_miniBossPrefab;

    [Tooltip("DialogManager de la scène principale (pour les messages corrompus).")]
    public DialogManager m_dialogManager;

    [Tooltip("Overlay glitch écran (GlitchOverlay sur un Canvas).")]
    public GlitchOverlay m_glitchOverlay;

    [Tooltip("Son de glitch joué par AudioManager.instance.")]
    public AudioClip m_glitchSound;

    // ── État interne ──────────────────────────────────────────
    // Timer de jeu actif (s'arrête si Time.timeScale == 0)
    private float m_gameTimer = 0f;
    // Moment du dernier événement déclenché
    private float m_lastEventTime = -999f;
    // Moment du dernier événement de niveau Dangereux
    private float m_lastDangerousEventTime = -999f;
    // Cooldown tiré aléatoirement pour le prochain événement
    private float m_nextCooldown = 35f;
    // Salle actuellement active
    private RoomContext m_activeRoom = null;
    // Registre de tous les événements disponibles
    private List<RandomEventData> m_events = new List<RandomEventData>();

    // ── Lifecycle Unity ───────────────────────────────────────
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Enregistre tous les événements au démarrage
        RegisterEvents();
        m_nextCooldown = Random.Range(m_cooldownMin, m_cooldownMax);
    }

    void Update()
    {
        // Avance le timer uniquement quand le jeu tourne (pas pendant les pauses)
        if (Time.timeScale > 0f)
            m_gameTimer += Time.unscaledDeltaTime;

        // Pas de salle active → rien à faire
        if (m_activeRoom == null) return;

        // Vérifie si le cooldown est écoulé
        if (m_gameTimer - m_lastEventTime >= m_nextCooldown)
            TryTriggerRandomEvent(m_activeRoom);
    }

    // ── API publique ──────────────────────────────────────────

    /// <summary>Appelé par RoomContext.OnEnable() à chaque entrée dans une salle.</summary>
    public void OnRoomEntered(RoomContext room)
    {
        m_activeRoom = room;
        Debug.Log($"[EventManager] Salle active : {room.roomId}");
    }

    /// <summary>Appelé par RoomContext.OnDisable() à chaque sortie de salle.</summary>
    public void OnRoomLeft(RoomContext room)
    {
        if (m_activeRoom == room)
            m_activeRoom = null;
    }

    /// <summary>
    /// Tente de déclencher un événement aléatoire dans la salle donnée.
    /// Applique toutes les règles de sécurité avant le tirage.
    /// </summary>
    public void TryTriggerRandomEvent(RoomContext room)
    {
        // ── Règles de sécurité ─────────────────────────────
        // Pas d'événement pendant les cinématiques
        if (room.isCinematicRoom) return;

        // Vérifie si un dialogue est à l'écran (DialogManager bloque)
        if (m_dialogManager != null && m_dialogManager.IsOnScreen()) return;

        // Limite d'événements par salle atteinte
        if (room.eventsTriggeredCount >= room.maxEventsPerRoom) return;

        // ── Choix de la catégorie ──────────────────────────
        EventCategory category = PickCategory();

        // Les événements dangereux sont bloqués en salle de boss
        // ou si le cooldown dangereux n'est pas écoulé
        bool dangerousAllowed = !room.isBossRoom
                                && (m_gameTimer - m_lastDangerousEventTime) >= m_dangerousCooldown;

        // ── Filtrage des événements éligibles ──────────────
        List<RandomEventData> eligible = new List<RandomEventData>();
        foreach (RandomEventData ev in m_events)
        {
            if (ev.Category != category) continue;
            if (!dangerousAllowed && ev.DangerLevel == EventDangerLevel.Dangereux) continue;
            if (ev.CanTrigger != null && !ev.CanTrigger(room)) continue;
            eligible.Add(ev);
        }

        // Aucun événement disponible → on reporte (reset le timer de moitié)
        if (eligible.Count == 0)
        {
            m_lastEventTime = m_gameTimer - m_nextCooldown * 0.5f;
            return;
        }

        // ── Tirage pondéré ─────────────────────────────────
        RandomEventData chosen = WeightedRandom(eligible);
        if (chosen == null) return;

        // ── Déclenchement ──────────────────────────────────
        Debug.Log($"[EventManager] ▶ {chosen.Id} (catégorie:{chosen.Category} danger:{chosen.DangerLevel}) dans '{room.roomId}'");
        chosen.Trigger?.Invoke(room);

        // Mise à jour de l'historique
        room.eventsTriggeredCount++;
        m_lastEventTime = m_gameTimer;
        m_nextCooldown = Random.Range(m_cooldownMin, m_cooldownMax);

        if (chosen.DangerLevel == EventDangerLevel.Dangereux)
            m_lastDangerousEventTime = m_gameTimer;
    }

    // ── Internals ─────────────────────────────────────────────

    /// <summary>Choisit une catégorie au hasard selon les probabilités configurées.</summary>
    private EventCategory PickCategory()
    {
        float total = m_probDecor + m_probGlitch + m_probEnnemi;
        float roll  = Random.value * total;

        if (roll < m_probDecor)                          return EventCategory.Decor;
        if (roll < m_probDecor + m_probGlitch)           return EventCategory.GlitchVisuel;
        return EventCategory.Ennemi;
    }

    /// <summary>Tirage au sort pondéré par BaseProbability dans un pool filtré.</summary>
    private RandomEventData WeightedRandom(List<RandomEventData> pool)
    {
        float total = 0f;
        foreach (RandomEventData ev in pool) total += ev.BaseProbability;

        float roll  = Random.value * total;
        float cumul = 0f;
        foreach (RandomEventData ev in pool)
        {
            cumul += ev.BaseProbability;
            if (roll <= cumul) return ev;
        }
        return pool[pool.Count - 1]; // fallback
    }

    /// <summary>Enregistre tous les événements disponibles au démarrage.</summary>
    private void RegisterEvents()
    {
        m_events.AddRange(DecorEvents.CreateAll(this));
        m_events.AddRange(GlitchEvents.CreateAll(this));
        m_events.AddRange(EnemyEvents.CreateAll(this));

        Debug.Log($"[EventManager] {m_events.Count} événements enregistrés.");
    }
}
