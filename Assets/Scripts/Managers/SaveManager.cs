using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int    slotIndex;
    public float  posX;
    public float  posY;
    public float  currentHP;
    public float  maxHP;
    public string inventoryJson;
    public string hotbarJson;
    public string killedEnemyIds;
    public bool   bossDefeated;
    public string completedDialogs;
    public string openedDoors;
    public float  playTime;
    public bool   isEmpty = true;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public const int SLOT_COUNT = 3;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Chemins ─────────────────────────────────────────────────────────────

    private string GetPath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");

    // ── Lecture / Écriture ───────────────────────────────────────────────────

    public void Save(int slot, SaveData data)
    {
        data.slotIndex = slot;
        data.isEmpty   = false;
        File.WriteAllText(GetPath(slot), JsonUtility.ToJson(data, true));
    }

    public SaveData Load(int slot)
    {
        string path = GetPath(slot);
        if (!File.Exists(path)) return new SaveData { slotIndex = slot, isEmpty = true };
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(path)) ?? new SaveData { slotIndex = slot, isEmpty = true };
    }

    public void Delete(int slot)
    {
        string path = GetPath(slot);
        if (File.Exists(path)) File.Delete(path);
    }

    public bool SlotExists(int slot) => File.Exists(GetPath(slot));

    public SaveData[] LoadAll()
    {
        var saves = new SaveData[SLOT_COUNT];
        for (int i = 0; i < SLOT_COUNT; i++)
            saves[i] = Load(i);
        return saves;
    }

    // ── Snapshot du joueur ───────────────────────────────────────────────────

    public SaveData TakeSnapshot(int slot, GameObject player)
    {
        var data = Load(slot);
        data.slotIndex = slot;
        data.isEmpty   = false;

        data.posX = player.transform.position.x;
        data.posY = player.transform.position.y;

        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            data.currentHP = stats.CurrentHealth;
            data.maxHP     = stats.MaxHealth;
        }

        data.playTime += Time.timeSinceLevelLoad;

        return data;
    }
}
