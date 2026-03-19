// ============================================================
//  EventEnums.cs — Enums partagés du système d'événements
// ============================================================

/// <summary>Les trois grandes familles d'événements.</summary>
public enum EventCategory
{
    Decor,        // Modifications de l'environnement / level
    GlitchVisuel, // Effets glitch visuels et sonores
    Ennemi        // Apparition ou modification d'ennemis
}

/// <summary>
/// Niveau de danger de l'événement.
/// Utilisé par les règles de sécurité de l'EventManager.
/// </summary>
public enum EventDangerLevel
{
    Safe,      // Jamais dangereux (plateforme secrète, message)
    Visuel,    // Seulement perturbant visuellement (glitch écran, slow-mo)
    Leger,     // Léger inconfort gameplay (plateforme qui disparaît, gravité)
    Dangereux  // Peut menacer le joueur (ennemi chasseur, mini-boss)
}
