// ============================================================
//  GlitchEvents.cs — Événements de glitch visuel/sonore (Catégorie 2)
// ============================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlitchEvents
{
    /// <summary>Retourne la liste de tous les événements de glitch visuel/sonore.</summary>
    public static List<RandomEventData> CreateAll(EventManager mgr)
    {
        return new List<RandomEventData>
        {
            CreateGlitchScreen(mgr),
            CreateSlowMotion(mgr),
            CreateCorruptedMessage(mgr),
        };
    }

    // ============================================================
    //  Event 1 : Glitch global écran / UI
    //  → Active le GlitchOverlay (distorsion, chromatic aberration…)
    //    pendant 2–3 secondes avec son de glitch.
    // ============================================================
    private static RandomEventData CreateGlitchScreen(EventManager mgr)
    {
        return new RandomEventData
        {
            Id              = "glitch_screen",
            Category        = EventCategory.GlitchVisuel,
            DangerLevel     = EventDangerLevel.Visuel,
            BaseProbability = 1.5f,

            CanTrigger = (room) => mgr.m_glitchOverlay != null,

            Trigger = (room) =>
            {
                float duration = Random.Range(2f, 3.5f);
                mgr.m_glitchOverlay.TriggerGlitch(duration);

                // Pitch aléatoire pour varier le son
                if (AudioManager.instance != null && mgr.m_glitchSound != null)
                    AudioManager.instance.PlaySound(mgr.m_glitchSound, 0.7f, Random.Range(0.8f, 1.2f));
            }
        };
    }

    // ============================================================
    //  Event 2 : Lag / Slow-motion volontaire
    //  → Abaisse Time.timeScale à 0.3 pendant 2.5s.
    //    Note : la lecture des inputs (Input.GetAxis) n'est pas
    //    affectée par timeScale, mais le mouvement physique l'est.
    // ============================================================
    private static RandomEventData CreateSlowMotion(EventManager mgr)
    {
        return new RandomEventData
        {
            Id              = "glitch_slowmo",
            Category        = EventCategory.GlitchVisuel,
            DangerLevel     = EventDangerLevel.Visuel,
            BaseProbability = 1.0f,

            // Jamais en salle de boss (risque de frustration excessive)
            CanTrigger = (room) => !room.isBossRoom,

            Trigger = (room) =>
            {
                mgr.StartCoroutine(SlowMotionCoroutine(
                    slowDuration  : 2.5f,
                    targetScale   : 0.3f,
                    transitionTime: 0.2f,
                    glitchSound   : mgr.m_glitchSound,
                    glitchOverlay : mgr.m_glitchOverlay
                ));
            }
        };
    }

    private static IEnumerator SlowMotionCoroutine(
        float     slowDuration,
        float     targetScale,
        float     transitionTime,
        AudioClip glitchSound,
        GlitchOverlay glitchOverlay)
    {
        // Son de déclenchement
        if (AudioManager.instance != null && glitchSound != null)
            AudioManager.instance.PlaySound(glitchSound, 0.6f, 0.5f); // pitch bas = effet lourd

        // Flash glitch au début
        if (glitchOverlay != null)
            glitchOverlay.TriggerGlitch(0.3f);

        // Transition vers le ralenti
        float elapsed = 0f;
        while (elapsed < transitionTime)
        {
            Time.timeScale = Mathf.Lerp(1f, targetScale, elapsed / transitionTime);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        Time.timeScale = targetScale;

        // Maintien du ralenti
        yield return new WaitForSecondsRealtime(slowDuration);

        // Flash glitch au retour à la normale
        if (glitchOverlay != null)
            glitchOverlay.TriggerGlitch(0.3f);

        // Retour progressif à la normale
        elapsed = 0f;
        while (elapsed < transitionTime)
        {
            Time.timeScale = Mathf.Lerp(targetScale, 1f, elapsed / transitionTime);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        Time.timeScale = 1f;

        // Resynchronise le pas physique (sécurité)
        Time.fixedDeltaTime = 0.02f;
    }

    // ============================================================
    //  Event 3 : Message système corrompu
    //  → Affiche une DialogPage avec texte partiellement corrompu
    //    (caractères de substitution, blocs, erreurs système fictives).
    // ============================================================
    private static RandomEventData CreateCorruptedMessage(EventManager mgr)
    {
        // Banque de messages (thème "Code Fracture" — jeu d'arcade glitché)
        string[] messages = new string[]
        {
            "ERR0R : S3GM3NT F4ULT\n" +
            "█▓░ C0R3 DU MP d3t3ct3d ░▓█\n" +
            "App. pr0t0c0le d3 r3p4r4t10n...",

            "SY5T3M W4RN1NG\n" +
            "INT3GR1T3 du n1v34u : [C0RRUPT]\n" +
            ">_ r3d3m4rr4g3 3n c0urs...",

            "D3B0GG3UR 4ct1f\n" +
            "Su1v1 du j0u3ur d3t3ct3\n" +
            "████████░░ 80%  ██▓░░░",

            "M3M01R3 fr4gm3nt33 d3t3ct33\n" +
            "S3ct3ur [0x4A2F] : l1s1bl3\n" +
            "S3ct3ur [0x4A30] : ████",

            "W4RN1NG : 3nt1t3 3xt3rn3\n" +
            "d4ns l3 buff3r d3 c0ll1s10n\n" +
            "S0urc3 : INK0NN███",
        };

        return new RandomEventData
        {
            Id              = "glitch_corrupt_message",
            Category        = EventCategory.GlitchVisuel,
            DangerLevel     = EventDangerLevel.Visuel,
            BaseProbability = 0.8f,

            CanTrigger = (room) => mgr.m_dialogManager != null,

            Trigger = (room) =>
            {
                string msg = messages[Random.Range(0, messages.Length)];

                // Crée une DialogPage avec couleur rouge-glitch
                var pages = new List<DialogPage>
                {
                    new DialogPage
                    {
                        text  = msg,
                        color = new Color(1f, 0.15f, 0.15f) // rouge vif
                    }
                };

                mgr.m_dialogManager.SetDialog(pages);

                // Son grave et distordu
                if (AudioManager.instance != null && mgr.m_glitchSound != null)
                    AudioManager.instance.PlaySound(mgr.m_glitchSound, 0.5f, 0.55f);
            }
        };
    }
}
