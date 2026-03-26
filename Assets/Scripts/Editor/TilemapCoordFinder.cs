using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapCoordFinder : EditorWindow
{
    [MenuItem("Tools/Afficher coordonnées TilemapDoor")]
    public static void ShowCoords()
    {
        GameObject obj = GameObject.Find("TilemapDoor");
        if (obj == null)
        {
            Debug.LogError("TilemapDoor introuvable dans la scène !");
            return;
        }

        Tilemap tilemap = obj.GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("Pas de Tilemap sur TilemapDoor !");
            return;
        }

        tilemap.CompressBounds();
        BoundsInt bounds = tilemap.cellBounds;
        string result = "=== Coordonnées des tiles dans TilemapDoor ===\n";
        int count = 0;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                result += $"  new Vector3Int({pos.x}, {pos.y}, {pos.z})\n";
                count++;
            }
        }

        result += $"=== Total : {count} tile(s) ===";
        Debug.Log(result);
    }
}
