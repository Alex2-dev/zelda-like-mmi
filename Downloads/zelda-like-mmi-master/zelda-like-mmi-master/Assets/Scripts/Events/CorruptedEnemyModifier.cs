// ============================================================
//  CorruptedEnemyModifier.cs — Variante corrompue d'un ennemi
// ============================================================
//  Utilisation : appeler TryApply() juste après l'Instantiate d'un ennemi.
//  Exemple dans un script de spawn d'ennemis :
//
//      GameObject enemy = Instantiate(enemyPrefab, pos, rot);
//      CorruptedEnemyModifier.TryApply(enemy, corruptChance: 0.15f);
//
//  Effets de la corruption :
//  - HP ×1.5 (ennemi plus résistant)
//  - Vitesse ×1.3 (si composant gérant la vitesse présent)
//  - Couleur violette/glitch sur le SpriteRenderer
//  - Tag "CorruptedEnemy" pour identifier le loot spécial à la mort
// ============================================================
using UnityEngine;

public static class CorruptedEnemyModifier
{
    // ── Paramètres de tuning ──────────────────────────────────
    /// <summary>Multiplicateur de HP appliqué à la version corrompue.</summary>
    public const float HP_MULTIPLIER    = 1.5f;

    /// <summary>Couleur de teinte du sprite pour indiquer la corruption.</summary>
    public static readonly Color CORRUPT_COLOR = new Color(0.7f, 0.1f, 1.0f); // violet glitch

    // ── API publique ──────────────────────────────────────────

    /// <summary>
    /// Tente de transformer l'ennemi <paramref name="enemy"/> en version corrompue.
    /// </summary>
    /// <param name="enemy">GameObject de l'ennemi instancié.</param>
    /// <param name="corruptChance">Probabilité de corruption (0–1). Défaut 15%.</param>
    /// <returns>true si la corruption a été appliquée.</returns>
    public static bool TryApply(GameObject enemy, float corruptChance = 0.15f)
    {
        if (enemy == null) return false;
        if (Random.value > corruptChance) return false;

        Apply(enemy);
        return true;
    }

    /// <summary>Applique directement la corruption (sans tirage aléatoire).</summary>
    public static void Apply(GameObject enemy)
    {
        if (enemy == null) return;

        // ── HP ×1.5 ───────────────────────────────────────────
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
            health.m_maxHealth *= HP_MULTIPLIER;

        // ── Couleur glitch ────────────────────────────────────
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = CORRUPT_COLOR;

        // ── Tag de reconnaissance ─────────────────────────────
        // Utile pour ton système de loot (récompense améliorée à la mort)
        // Attention : le tag "CorruptedEnemy" doit être déclaré dans
        // Edit > Project Settings > Tags and Layers.
        if (TagExists("CorruptedEnemy"))
            enemy.tag = "CorruptedEnemy";

        // ── Composant marqueur pour identifier au runtime ──────
        enemy.AddComponent<CorruptedEnemyMarker>();

        Debug.Log($"[CorruptedEnemyModifier] '{enemy.name}' est maintenant corrompu (HP×{HP_MULTIPLIER}).");
    }

    // ── Utilitaire ────────────────────────────────────────────
    private static bool TagExists(string tag)
    {
        try
        {
            // GameObject.FindGameObjectsWithTag lance une exception si le tag n'existe pas
            GameObject.FindGameObjectsWithTag(tag);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

// ── Marqueur vide pour identifier les ennemis corrompus ──────────
/// <summary>
/// Composant marqueur ajouté aux ennemis corrompus.
/// Utiliser GetComponent&lt;CorruptedEnemyMarker&gt;() dans le loot drop
/// pour vérifier si l'ennemi était corrompu.
/// </summary>
public class CorruptedEnemyMarker : MonoBehaviour { }
