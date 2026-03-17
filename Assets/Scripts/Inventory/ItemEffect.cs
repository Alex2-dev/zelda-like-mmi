using UnityEngine;

/// <summary>
/// Classe de base pour tous les effets d'objets.
/// Crée une classe héritant de ItemEffect pour chaque type d'effet.
///
/// Exemple :
///   [CreateAssetMenu(menuName = "Inventory/Effects/Mon Effet")]
///   public class MonEffet : ItemEffect
///   {
///       public override void ApplyEffect(GameObject player) { ... }
///   }
/// </summary>
public abstract class ItemEffect : ScriptableObject
{
    /// <summary>Appelée quand le joueur utilise l'objet.</summary>
    public abstract void ApplyEffect(GameObject player);
}
