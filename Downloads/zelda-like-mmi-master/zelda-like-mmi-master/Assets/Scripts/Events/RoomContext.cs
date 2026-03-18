// ============================================================
//  RoomContext.cs — Composant à placer sur chaque GameObject-salle
// ============================================================
//  SETUP Unity :
//  1. Glisser ce script sur le parent de chaque salle.
//  2. Remplir l'inspecteur : roomId, flags, listes de plateformes,
//     points de spawn, prefabs optionnels.
//  3. Les champs HideInInspector sont gérés automatiquement
//     par l'EventManager au runtime.
// ============================================================
using System.Collections.Generic;
using UnityEngine;

public class RoomContext : MonoBehaviour
{
    // ── Identité ─────────────────────────────────────────────
    [Header("Identité de la salle")]
    [Tooltip("Nom/ID unique — utilisé dans les logs et les conditions d'événement.")]
    public string roomId = "salle_01";

    // ── Restrictions d'événements ─────────────────────────────
    [Header("Restrictions")]
    [Tooltip("Aucun événement dangereux ne se déclenchera ici (boss, arène finale…).")]
    public bool isBossRoom = false;

    [Tooltip("Aucun événement ne se déclenchera pendant une cinématique dans cette salle.")]
    public bool isCinematicRoom = false;

    [Tooltip("Nombre maximum d'événements déclenchables dans cette salle par visite.")]
    public int maxEventsPerRoom = 3;

    // ── Décor : plateformes mobiles ───────────────────────────
    [Header("Décor – Plateformes mobiles/toggleables")]
    [Tooltip("GameObjects de plateformes pouvant être togglées/déplacées par un événement.")]
    public List<GameObject> movablePlatforms = new List<GameObject>();

    // ── Décor : salle secrète ─────────────────────────────────
    [Header("Décor – Plateforme secrète")]
    [Tooltip("Point où la plateforme glitchée temporaire sera instanciée.")]
    public Transform secretPlatformSpawnPoint;

    [Tooltip("(Optionnel) Entrée de salle secrète à révéler quand la plateforme apparaît.")]
    public GameObject secretRoomEntrance;

    // ── Décor : zones de gravité ──────────────────────────────
    [Header("Décor – Zones de gravité anormale")]
    [Tooltip("Colliders 2D Trigger portant un composant GravityZoneTrigger.")]
    public List<Collider2D> gravityZones = new List<Collider2D>();

    // ── Ennemis ───────────────────────────────────────────────
    [Header("Ennemis – Points de spawn")]
    [Tooltip("Transforms utilisés pour faire apparaître ennemis spéciaux.")]
    public List<Transform> enemySpawnPoints = new List<Transform>();

    [Header("Ennemis – Prefabs spécifiques à cette salle (optionnel)")]
    [Tooltip("Remplace le prefab global de l'EventManager si renseigné.")]
    public GameObject stalkerEnemyPrefab;

    [Tooltip("Remplace le prefab global de l'EventManager si renseigné.")]
    public GameObject miniBossPrefab;

    // ── État runtime (géré par EventManager) ─────────────────
    [HideInInspector] public int eventsTriggeredCount = 0;
    [HideInInspector] public bool hasStalkerActive = false;

    // ── Cycle de vie ──────────────────────────────────────────
    void OnEnable()
    {
        // Réinitialise le compteur à chaque entrée dans la salle
        eventsTriggeredCount = 0;
        hasStalkerActive = false;

        // Révèle l'entrée secrète si elle existe (off par défaut)
        if (secretRoomEntrance != null)
            secretRoomEntrance.SetActive(false);

        if (EventManager.Instance != null)
            EventManager.Instance.OnRoomEntered(this);
    }

    void OnDisable()
    {
        if (EventManager.Instance != null)
            EventManager.Instance.OnRoomLeft(this);
    }
}
