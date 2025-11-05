// Runtime/RoadRuntimeAPI.cs
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class RoadRuntimeAPI : MonoBehaviour
{
    public RoadRuntimeLogger logger; // 같은 타일맵을 가리켜야 함
    private Tilemap _tm;

    void Awake()
    {
        _tm = GetComponent<Tilemap>();
        if (!logger) logger = FindAnyObjectByType<RoadRuntimeLogger>();
        // 로거의 target이 나(=같은 Tilemap)인지 확인
        if (logger && logger.target != _tm)
        {
            Debug.LogWarning($"[RoadRuntimeAPI] logger.target이 다른 타일맵을 가리킵니다. ({logger.target?.name} != {_tm.name})");
        }
    }

    public void Place(Vector3Int cell, TileBase tile)
    {
        _tm.SetTile(cell, tile);
        logger?.PlaceTile(cell, tile); // 로거에 기록
        Debug.Log($"[RoadRuntimeAPI] Place {cell} {tile?.name}");
    }

    public void Remove(Vector3Int cell)
    {
        _tm.SetTile(cell, null);
        logger?.RemoveTile(cell);      // 로거에 기록
        Debug.Log($"[RoadRuntimeAPI] Remove {cell}");
    }
}
