using UnityEngine;

/// <summary>
/// À attacher sur le GameObject Player dans MainScene.
/// - Au démarrage : applique les données de la save au joueur.
/// - Fournit SaveCurrentState() appelé par GameManager et les checkpoints.
/// </summary>
[RequireComponent(typeof(PlayerStats))]
public class PlayerSaveLoader : MonoBehaviour
{
    public static PlayerSaveLoader Instance { get; private set; }

    private PlayerStats m_stats;
    private Inventory   m_inventory;

    void Awake()
    {
        Instance  = this;
        m_stats   = GetComponent<PlayerStats>();
        m_inventory = GetComponent<Inventory>();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        ApplySave();
    }

    // ── Chargement ────────────────────────────────────────────────────────────

    private void ApplySave()
    {
        if (GameManager.Instance == null || SaveManager.Instance == null) return;

        int      slot = GameManager.Instance.CurrentSlot;
        SaveData data = SaveManager.Instance.Load(slot);

        if (data.isEmpty) return; // Nouvelle partie — valeurs par défaut

        // Position du joueur
        transform.position = new Vector3(data.posX, data.posY, transform.position.z);

        // Santé
        if (m_stats != null)
            m_stats.RestoreHealth(data.currentHP, data.maxHP);

        // Boss déjà vaincu
        if (data.bossDefeated)
            GameManager.Instance.SetBossDefeated(true);

        Debug.Log($"[SaveLoader] Save slot {slot} chargée — pos({data.posX:F1},{data.posY:F1}) HP:{data.currentHP}/{data.maxHP}");
    }

    // ── Sauvegarde ────────────────────────────────────────────────────────────

    public void SaveCurrentState()
    {
        if (GameManager.Instance == null || SaveManager.Instance == null) return;

        int slot = GameManager.Instance.CurrentSlot;

        var data = new SaveData
        {
            slotIndex    = slot,
            posX         = transform.position.x,
            posY         = transform.position.y,
            currentHP    = m_stats != null ? m_stats.CurrentHealth : 0f,
            maxHP        = m_stats != null ? m_stats.MaxHealth     : 0f,
            bossDefeated = GameManager.Instance.BossDefeated,
            playTime     = GameManager.Instance.PlayTime,
            isEmpty      = false
        };

        SaveManager.Instance.Save(slot, data);
        Debug.Log($"[SaveLoader] Jeu sauvegardé — slot {slot}, temps {data.playTime:F0}s");
    }
}
