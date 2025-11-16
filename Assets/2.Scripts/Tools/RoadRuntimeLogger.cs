// Runtime/RoadRuntimeLogger.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

[System.Serializable]
public struct RoadCell {
    public Vector3Int pos;
    public string tileName; // 간단히 이름 매핑
}

[System.Serializable]
public class RoadEditLog {
    public string tilemapPathInScene;
    public List<RoadCell> placed = new();
    public List<Vector3Int> removed = new();
}

public class RoadRuntimeLogger : MonoBehaviour
{
    public Tilemap target;
    public string fileName = "road_edits.json";
    private RoadEditLog _log = new();

    void Awake() {
        _log.tilemapPathInScene = GetHierarchyPath(target.transform);
    }

    public void PlaceTile(Vector3Int cell, TileBase tile) {
        target.SetTile(cell, tile);
        _log.placed.Add(new RoadCell{ pos = cell, tileName = tile ? tile.name : "" });
    }

    public void RemoveTile(Vector3Int cell) {
        target.SetTile(cell, null);
        _log.removed.Add(cell);
    }

    // 플레이 종료 전에 한 번 호출 (예: OnApplicationQuit, 버튼 등)
    public void SaveJson() {
        var path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(path, JsonUtility.ToJson(_log, true));
        Debug.Log($"[RoadRuntimeLogger] Saved: {path}");
    }

    // void OnApplicationQuit() { SaveJson(); } // 편의상 자동 저장

    private string GetHierarchyPath(Transform t) {
        var stack = new Stack<string>();
        while (t != null) { stack.Push(t.name); t = t.parent; }
        return string.Join("/", stack);
    }
}
