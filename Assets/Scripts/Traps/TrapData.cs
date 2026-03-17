using UnityEngine;

/// <summary>
/// Définit les statistiques d'un piège.
/// SETUP : Assets > Create > Traps > Trap Data
/// </summary>
[CreateAssetMenu(fileName = "NewTrap", menuName = "Traps/Trap Data")]
public class TrapData : ScriptableObject
{
    [Header("Présentation")]
    public string m_trapName = "Piège";

    [Header("Statistiques")]
    public float m_damage = 50f;

    [Tooltip("Rayon de déclenchement — doit correspondre au CircleCollider2D.radius du prefab")]
    public float m_triggerRadius = 0.5f;

    [Tooltip("Secondes avant que le piège soit armé après pose")]
    public float m_armDelay = 1f;

    [Tooltip("Si vrai, le piège se détruit après le premier déclenchement")]
    public bool m_destroyOnUse = false;

    [Tooltip("Secondes avant réarmement (ignoré si destroyOnUse = true)")]
    public float m_rearmDelay = 3f;

    [Header("Prefab")]
    [Tooltip("Prefab avec TrapBehavior.cs et CircleCollider2D (Is Trigger)")]
    public GameObject m_trapPrefab;
}
