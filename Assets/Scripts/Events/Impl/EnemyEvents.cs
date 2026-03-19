using System.Collections.Generic;
using UnityEngine;

public static class EnemyEvents
{
    public static List<RandomEventData> CreateAll(EventManager mgr)
    {
        return new List<RandomEventData>
        {
            CreateBugHunter(mgr),
            CreateMiniBoss(mgr),
        };
    }

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
                && !room.hasStalkerActive
                && mgr.m_stalkerEnemyPrefab != null,

            Trigger = (room) =>
            {
                GameObject stalker = Object.Instantiate(
                    mgr.m_stalkerEnemyPrefab,
                    room.transform.position,
                    Quaternion.identity);

                StalkerBehavior sb = stalker.GetComponent<StalkerBehavior>();
                if (sb != null)
                    sb.SetTargetRoom(room);

                room.hasStalkerActive = true;

                if (AudioManager.instance != null && mgr.m_glitchSound != null)
                    AudioManager.instance.PlaySound(mgr.m_glitchSound, 1.0f, 0.4f);

                Debug.Log($"[EnemyEvents] Chasseur de bugs spawn dans '{room.roomId}'.");
            }
        };
    }

    private static RandomEventData CreateMiniBoss(EventManager mgr)
    {
        return new RandomEventData
        {
            Id              = "enemy_mini_boss",
            Category        = EventCategory.Ennemi,
            DangerLevel     = EventDangerLevel.Dangereux,
            BaseProbability = 0.4f,

            CanTrigger = (room) =>
                !room.isBossRoom
                && mgr.m_miniBossPrefab != null,

            Trigger = (room) =>
            {
                GameObject boss = Object.Instantiate(
                    mgr.m_miniBossPrefab,
                    room.transform.position,
                    Quaternion.identity);

                EnemyHealth health = boss.GetComponent<EnemyHealth>();
                if (health != null)
                    health.m_maxHealth = 300f;

                if (AudioManager.instance != null && mgr.m_glitchSound != null)
                    AudioManager.instance.PlaySound(mgr.m_glitchSound, 1.0f, 0.3f);

                Debug.Log($"[EnemyEvents] Mini-boss spawn dans '{room.roomId}'.");
            }
        };
    }
}
