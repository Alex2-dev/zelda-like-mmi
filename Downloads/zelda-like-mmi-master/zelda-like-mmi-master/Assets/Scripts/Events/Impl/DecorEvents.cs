// ============================================================
//  DecorEvents.cs — Événements de décor / level (Catégorie 1)
// ============================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DecorEvents
{
    /// <summary>Retourne la liste de tous les événements de décor.</summary>
    public static List<RandomEventData> CreateAll(EventManager mgr)
    {
        return new List<RandomEventData>
        {
            CreatePlatformShuffle(mgr),
            CreateSecretPlatform(mgr),
            CreateGravityZone(mgr),
        };
    }

    // ============================================================
    //  Event 1 : Recompilation locale du décor
    //  → Une plateforme marquée se toggle (active/inactive) avec
    //    un effet de clignotement glitch.
    // ============================================================
    private static RandomEventData CreatePlatformShuffle(EventManager mgr)
    {
        return new RandomEventData
        {
            Id              = "decor_platform_shuffle",
            Category        = EventCategory.Decor,
            DangerLevel     = EventDangerLevel.Leger,
            BaseProbability = 1.5f,

            // Eligible uniquement si la salle a des plateformes déclarées
            CanTrigger = (room) => room.movablePlatforms.Count > 0,

            Trigger = (room) =>
            {
                // Choisit une plateforme au hasard parmi celles de la salle
                int idx = Random.Range(0, room.movablePlatforms.Count);
                GameObject platform = room.movablePlatforms[idx];
                if (platform == null) return;

                mgr.StartCoroutine(GlitchTogglePlatform(platform));
            }
        };
    }

    /// <summary>Clignotement glitch puis toggle actif/inactif de la plateforme.</summary>
    private static IEnumerator GlitchTogglePlatform(GameObject platform)
    {
        SpriteRenderer sr = platform.GetComponent<SpriteRenderer>();

        // 5 flashs rapides (0.1s chacun) pour l'effet glitch
        for (int i = 0; i < 5; i++)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
        }

        // S'assure que le sprite est visible avant le toggle final
        if (sr != null) sr.enabled = true;

        // Toggle : actif → inactif ou vice-versa
        platform.SetActive(!platform.activeSelf);
    }

    // ============================================================
    //  Event 2 : Plateforme temporaire vers micro-salle secrète
    //  → Spawn d'une plateforme glitchée pendant 8 secondes,
    //    révèle l'entrée de la salle secrète si elle est assignée.
    // ============================================================
    private static RandomEventData CreateSecretPlatform(EventManager mgr)
    {
        return new RandomEventData
        {
            Id              = "decor_secret_platform",
            Category        = EventCategory.Decor,
            DangerLevel     = EventDangerLevel.Safe,
            BaseProbability = 0.8f,

            // Nécessite un point de spawn ET un prefab de plateforme glitchée
            CanTrigger = (room) => room.secretPlatformSpawnPoint != null
                                && mgr.m_glitchPlatformPrefab != null,

            Trigger = (room) =>
            {
                // Instancie la plateforme au point désigné
                GameObject platform = Object.Instantiate(
                    mgr.m_glitchPlatformPrefab,
                    room.secretPlatformSpawnPoint.position,
                    Quaternion.identity
                );

                // Révèle l'entrée secrète si elle est assignée dans la salle
                if (room.secretRoomEntrance != null)
                    room.secretRoomEntrance.SetActive(true);

                // La plateforme disparaît après 8s avec un avertissement visuel
                mgr.StartCoroutine(FadeAndDestroyPlatform(platform, room, 8f));
            }
        };
    }

    /// <summary>Clignotement d'avertissement puis destruction de la plateforme.</summary>
    private static IEnumerator FadeAndDestroyPlatform(GameObject platform, RoomContext room, float lifetime)
    {
        // Attend jusqu'aux 2 dernières secondes
        yield return new WaitForSeconds(lifetime - 2f);

        // Clignotement d'avertissement rapide pendant 2s
        SpriteRenderer sr = platform != null ? platform.GetComponent<SpriteRenderer>() : null;
        for (int i = 0; i < 8; i++)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.25f);
        }

        // Destruction de la plateforme
        if (platform != null) Object.Destroy(platform);

        // Cache à nouveau l'entrée secrète
        if (room.secretRoomEntrance != null)
            room.secretRoomEntrance.SetActive(false);
    }

    // ============================================================
    //  Event 3 : Zone de gravité anormale locale
    //  → Active une GravityZoneTrigger dans la salle pendant 6s,
    //    qui attire le joueur vers son centre.
    // ============================================================
    private static RandomEventData CreateGravityZone(EventManager mgr)
    {
        return new RandomEventData
        {
            Id              = "decor_gravity_zone",
            Category        = EventCategory.Decor,
            DangerLevel     = EventDangerLevel.Leger,
            BaseProbability = 1.0f,

            // Eligible si la salle a au moins une zone de gravité déclarée
            CanTrigger = (room) => room.gravityZones.Count > 0,

            Trigger = (room) =>
            {
                // Choisit une zone au hasard
                int idx = Random.Range(0, room.gravityZones.Count);
                Collider2D col = room.gravityZones[idx];
                if (col == null) return;

                GravityZoneTrigger zone = col.GetComponent<GravityZoneTrigger>();
                if (zone == null)
                {
                    Debug.LogWarning($"[DecorEvents] GravityZoneTrigger manquant sur '{col.name}'");
                    return;
                }

                // 6 secondes d'anomalie, force d'attraction à 4 unités
                zone.Activate(duration: 6f, attractForce: 4f);

                // Effet sonore glitch discret
                if (AudioManager.instance != null && mgr.m_glitchSound != null)
                    AudioManager.instance.PlaySound(mgr.m_glitchSound, 0.4f, 1.4f);
            }
        };
    }
}
