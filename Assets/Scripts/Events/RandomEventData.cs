// ============================================================
//  RandomEventData.cs — Structure de données d'un événement
// ============================================================
using System;

/// <summary>
/// Décrit un événement aléatoire du système "Code Fracture".
/// Instancier et remplir via les classes Impl (DecorEvents, etc.).
/// </summary>
public class RandomEventData
{
    // ── Identité ─────────────────────────────────────────────
    /// <summary>Identifiant unique (ex: "decor_platform_shuffle").</summary>
    public string Id;

    /// <summary>Famille de l'événement (Decor, GlitchVisuel, Ennemi).</summary>
    public EventCategory Category;

    /// <summary>Niveau de danger — utilisé par les règles de sécurité.</summary>
    public EventDangerLevel DangerLevel;

    // ── Probabilité ──────────────────────────────────────────
    /// <summary>
    /// Poids relatif dans le tirage au sort.
    /// 1.0 = poids normal, 2.0 = deux fois plus probable, 0.5 = moitié moins.
    /// </summary>
    public float BaseProbability = 1f;

    // ── Conditions et callback ────────────────────────────────
    /// <summary>
    /// Retourne true si l'événement est éligible pour la salle/état courant.
    /// Peut être null → toujours éligible.
    /// </summary>
    public Func<RoomContext, bool> CanTrigger;

    /// <summary>
    /// Logique exécutée quand l'événement est déclenché.
    /// </summary>
    public Action<RoomContext> Trigger;
}
