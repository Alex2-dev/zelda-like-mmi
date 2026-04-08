using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gère l'arme équipée, les munitions et le tir.
/// Toujours présent sur le Player, même sans arme équipée.
///
/// SETUP :
/// - Attacher ce script au GameObject "Player"
/// - Glisser ce composant dans le champ m_weaponManager de PlayerBehavior
/// </summary>
public class WeaponManager : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] AudioClip m_sfxShoot;
    [SerializeField] AudioClip m_sfxNoAmmo;

    private WeaponData m_equippedWeapon;
    private Dictionary<AmmoType, int> m_ammoStorage;
    private float m_shootCooldown = 0f;
    private Vector2 m_currentDirection = Vector2.up;

    // Mis à false par PlayerBehavior quand un dialogue ou la carte est affiché
    public bool m_canFire = true;

    void Awake()
    {
        m_ammoStorage = new Dictionary<AmmoType, int>();
        foreach (AmmoType type in System.Enum.GetValues(typeof(AmmoType)))
            m_ammoStorage[type] = 0;
    }

    void Update()
    {
        if (m_shootCooldown > 0f)
            m_shootCooldown -= Time.deltaTime;

        // Les armes automatiques gèrent leur propre tir ici
        bool fireHeld = InputManager.Instance != null ? InputManager.Instance.AttackHeld : Input.GetKey(KeyCode.Space);
        if (m_canFire && m_equippedWeapon != null && m_equippedWeapon.m_isAutomatic && fireHeld)
            TryShoot(m_currentDirection);
    }

    /// <summary>Appelée par PlayerBehavior chaque frame pour synchroniser la direction.</summary>
    public void SetDirection(Vector2 direction)
    {
        m_currentDirection = direction;
    }

    public bool HasWeapon() => m_equippedWeapon != null;

    public bool IsAutomatic() => m_equippedWeapon != null && m_equippedWeapon.m_isAutomatic;

    public void EquipWeapon(WeaponData weapon)
    {
        m_equippedWeapon = weapon;
    }

    public void AddAmmo(AmmoType type, int amount)
    {
        if (m_ammoStorage.ContainsKey(type))
            m_ammoStorage[type] += amount;
    }

    public int GetAmmo(AmmoType type)
    {
        return m_ammoStorage.TryGetValue(type, out int val) ? val : 0;
    }

    /// <summary>Tente de tirer. Retourne false si impossible (cooldown, munitions, pas d'arme).</summary>
    public bool TryShoot(Vector2 direction)
    {
        if (m_equippedWeapon == null) return false;
        if (m_shootCooldown > 0f) return false;
        if (m_ammoStorage[m_equippedWeapon.m_ammoType] < m_equippedWeapon.m_ammoPerShot)
        {
            AudioManager.instance?.PlaySound(m_sfxNoAmmo);
            return false;
        }
        if (m_equippedWeapon.m_projectilePrefab == null) return false;

        m_ammoStorage[m_equippedWeapon.m_ammoType] -= m_equippedWeapon.m_ammoPerShot;
        m_shootCooldown = 1f / m_equippedWeapon.m_fireRate;

        // Calcul de la dispersion par rotation angulaire
        float angle = Random.Range(-m_equippedWeapon.m_spread / 2f, m_equippedWeapon.m_spread / 2f);
        Vector2 spreadDir = Quaternion.Euler(0f, 0f, angle) * direction.normalized;

        GameObject projObj = Instantiate(
            m_equippedWeapon.m_projectilePrefab,
            transform.position,
            Quaternion.identity
        );

        Projectile proj = projObj.GetComponent<Projectile>();
        if (proj != null)
            proj.Launch(spreadDir, m_equippedWeapon.m_projectileSpeed, m_equippedWeapon.m_damage, m_equippedWeapon.m_range);

        AudioManager.instance?.PlaySound(m_sfxShoot);
        return true;
    }
}
