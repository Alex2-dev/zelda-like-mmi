// ============================================================
//  EnemyEvents.cs — Événements d'ennemis / tension (Catégorie 3)
// ============================================================
using System.Collections.Generic;
using UnityEngine;

public static class EnemyEvents
{
    /// <summary>Retourne la liste de tous les événements d'ennemis.</summary>
    public static List<RandomEventData> CreateAll(EventManager mgr)
    {
        return new List<RandomEventData>
        {
            CreateBugHunter(mgr),
            CreateMiniBoss(mgr),
            // Note : la variante corrompue est gérée séparément via
            // CorruptedEnemyModifier.TryApply() au moment du spawn d'un ennemi.
        };
    }

    // ============================================================
    //  Event 1 : Chasseur de bugs (enemy "stalker")
    //  → Spawn d'un ennemi spécial qui poursuit le joueur jusqu'à
    //    ce que le joueur quitte la salle ou le détruise.
    //    Un seul stalker peut être actif par salle à la fois.
    // ============================================================
    private static RandomEventData CreateBugHunter(EventManager mgr)
    {
        return new RandomEventData
        {
            Id              = "enemy_bug_hunter",
            Category        = EventCategory.Ennemi,
            DangerLevel     = EventDangerLevel.Dangereux,
            BaseProbability = 1.0f,

            CanTrigger = (room) =>
                !room.isBossRoom
                && !room.hasStalkerActive                       // un seul stalker à la fois
                && room.enemySpawnPoints.Count > 0
                && (mgr.m_stalkerEnemyPrefab != null || room.stalkerEnemyPrefab != null),

            Trigger = (room) =>
            {
                // Préfère le prefab spécifique à la salle, sinon celui global
                GameObject prefab = room.stalkerEnemyPrefab != null
                    ? room.stalkerEnemyPrefab
                    : mgr.m_stalkerEnemyPrefab;

                // Spawn à un point aléatoire de la salle
                Transform spawnPoint = room.enemySpawnPoints[Random.Range(0, room.enemySpawnPoints.Count)];
                GameObject stalker   = Object.Instantiate(prefab, spawnPoint.position, Quaternion.identity);

                // Configure le stalker pour qu'il sache dans quelle salle il est
                StalkerBehavior sb = stalker.GetComponent<StalkerBehavior>();
                if (sb != null)
                    sb.SetTargetRoom(room);
                else
                    Debug.LogWarning("[EnemyEvents] StalkerBehavior manquant sur le prefab stalker.");

                room.hasStalkerActive = true;

                // Son d'alerte grave
                if (AudioManager.instance != null && mgr.m_glitchSound != null)
                    AudioManager.instance.PlaySound(mgr.m_glitchSound, 1.0f, 0.4f);

                Debug.Log($"[EnemyEvents] Chasseur de bugs spawn dans '{room.roomId}'.");
            }
        };
    }

    // ============================================================
    //  Event 2 : Mini-boss glitch rare
    //  → Remplace un spawn standard par un mini-boss avec plus de
    //    HP et une récompense spéciale. Seulement dans les salles
    //    non-boss avec des points de spawn définis.
    //    HP mini-boss : 3× un ennemi standard (ex: 300 HP).
    // ============================================================
    private static RandomEventData CreateMiniBoss(EventManager mgr)
    {
        return new RandomEventData
        {
            Id              = "enemy_mini_boss",
            Category        = EventCategory.Ennemi,
            DangerLevel     = EventDangerLevel.Dangereux,
            BaseProbability = 0.4f, // rare : ~2× moins probable que le bug hunter

            CanTrigger = (room) =>
                !room.isBossRoom
                && room.enemySpawnPoints.Count > 0
                && (mgr.m_miniBossPrefab != null || room.miniBossPrefab != null),

            Trigger = (room) =>
            {
                GameObject prefab = room.miniBossPrefab != null
                    ? room.miniBossPrefab
                    : mgr.m_miniBossPrefab;

                // Spawn au premier point déclaré (point principal de la salle)
                Transform spawnPoint = room.enemySpawnPoints[0];
                GameObject boss      = Object.Instantiate(prefab, spawnPoint.position, Quaternion.identity);

                // Surcharge les HP si l'EnemyHealth est présent
                EnemyHealth health = boss.GetComponent<EnemyHealth>();
                if (health != null)
                    health.m_maxHealth = 300f; // 3× un ennemi standard (100 HP)

                // Son d'apparition dramatique (pitch très bas)
                if (AudioManager.instance != null && mgr.m_glitchSound != null)
                    AudioManager.instance.PlaySound(mgr.m_glitchSound, 1.0f, 0.3f);

                Debug.Log($"[EnemyEvents] Mini-boss spawn dans '{room.roomId}'.");
            }
        };
    }
}
