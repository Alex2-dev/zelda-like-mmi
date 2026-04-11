using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public const int SLOT_COUNT = 3;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Chemins ──────────────────────────────────────────────────────────────

    private string GetPath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");

    // ── API publique ─────────────────────────────────────────────────────────

    public SaveData Load(int slot)
    {
        string path = GetPath(slot);
        if (!File.Exists(path)) return new SaveData { slotIndex = slot, isEmpty = true };
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(path))
               ?? new SaveData { slotIndex = slot, isEmpty = true };
    }

    public SaveData[] LoadAll()
    {
        var saves = new SaveData[SLOT_COUNT];
        for (int i = 0; i < SLOT_COUNT; i++)
            saves[i] = Load(i);
        return saves;
    }

    public void Save(int slot, SaveData data)
    {
        data.slotIndex = slot;
        data.isEmpty   = false;
        File.WriteAllText(GetPath(slot), JsonUtility.ToJson(data, true));
    }

    public void Delete(int slot)
    {
        string path = GetPath(slot);
        if (File.Exists(path)) File.Delete(path);
    }

    public bool SlotExists(int slot) => File.Exists(GetPath(slot));
}
