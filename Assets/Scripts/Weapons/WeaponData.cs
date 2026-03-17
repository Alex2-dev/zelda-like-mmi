using UnityEngine;

/// <summary>Types de munitions disponibles.</summary>
public enum AmmoType { Pistol, SMG, AssaultRifle, Sniper }

/// <summary>
/// Définit les statistiques d'une arme.
/// SETUP : Assets > Create > Weapons > Weapon Data
/// </summary>
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Présentation")]
    public string m_weaponName = "Arme";

    [Header("Statistiques")]
    public float m_damage = 25f;

    [Tooltip("Coups par seconde — cooldown interne = 1 / fireRate")]
    public float m_fireRate = 2f;

    public float m_projectileSpeed = 15f;

    [Tooltip("Distance max en unités Unity avant destruction du projectile")]
    public float m_range = 12f;

    [Tooltip("Dispersion en degrés (0 = parfaitement précis)")]
    [Range(0f, 45f)]
    public float m_spread = 0f;

    public bool m_isAutomatic = false;

    [Header("Munitions")]
    public AmmoType m_ammoType = AmmoType.Pistol;

    [Tooltip("Munitions consommées par tir")]
    public int m_ammoPerShot = 1;

    [Header("Prefab projectile")]
    [Tooltip("Prefab avec Projectile.cs, Rigidbody2D Kinematic, CircleCollider2D (Is Trigger), layer Projectile")]
    public GameObject m_projectilePrefab;
}
