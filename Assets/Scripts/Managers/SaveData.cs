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
