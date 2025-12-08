using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static GridUtils;

// 🧱 타일의 종류를 정의하는 열거형
// 여러 시스템(Field, Road, Building 등)에서 공통으로 사용 가능하므로 클래스 밖에 둠
public enum TileType
{
    None,
    Field,
    Road,
    Constructed
}

/// <summary>
/// 실제 타일맵의 셀 좌표 ↔ 월드 좌표를 변환하고,  
/// <para>각 셀의 논리적 타일 타입(Field, Road 등)을 관리하는 핵심 시스템.</para>
/// <para> Field, Road, Building 등의 타입 정보를 2차원 배열에 보관.</para>
/// <para> Tilemap을 통해 시각적 타일과 동기화.</para>
/// 좌표 변환: CellToWorld / WorldToCell 등 제공.
/// </summary>
public class GridHandler : MonoBehaviour
{
    // 🧩 그리드 시각화용 오브젝트 (활성/비활성으로 빌드 모드 표시)
    [SerializeField] private GameObject _gridVisualizer;

    // 🧱 실제 타일이 그려지는 Tilemap (Field용, Road용 분리)
    [SerializeField] private Tilemap _fieldTilemap;
    [SerializeField] private Tilemap _roadTilemap;

    private readonly int _width = 40;
    private readonly int _height = 40;

    // Unity Grid Component (좌표 변환용)
    [SerializeField] private Grid _grid;

    // 현재 빌드 모드 상태 (Space로 토글)
    [SerializeField] private bool _buildMode = false;

    [SerializeField] private TileBase _selectedTile;

    private Dictionary<Vector3Int, Building> _buildingByCellDict = new();

    private TileType[,] _gridTileTypes = new TileType[80, 80];

    // 읽기 전용 프로퍼티들 ----------------------------------
    public bool BuildMode => _buildMode;        // 현재 빌드모드 여부
    public int Width => _width;                 // 그리드 너비
    public int Height => _height;               // 그리드 높이
    public Vector3 CellSize => _grid.cellSize;  // 셀 하나의 크기
    public Tilemap FieldTilemap => _fieldTilemap;
    public Tilemap RoadTilemap => _roadTilemap;
    // ------------------------------------------------------

    void Update()
    {
        
    }

    // 셀 좌표를 월드 좌표(중심점)로 변환
    public Vector3 CellToWorld(int x, int y)
    {                                                           // _gird.CellToWorld : Grid 컴포넌트의 내장 함수
        return _grid.CellToWorld(new Vector3Int(x, y, 0))       // 셀 좌표 -> 월드 좌표 변환 (왼쪽 아래 모서리 기준)
             + new Vector3(CellSize.x / 2, 0, CellSize.y / 2);  // 셀 중앙으로 보정
    }                                                           // 반환값 : 셀의 정중앙 월드 좌표

    // 월드 좌표를 셀 좌표로 변환
    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        return _grid.WorldToCell(worldPosition);    // Grid 컴포넌트의 내장 함수
    }

    // 논리 데이터(2D 배열)에 타일 타입 기록
    public void SetGridTileType(int x, int y, TileType tileType)
    {

        if (IsCellOutOfRange(new Vector3Int(x, y, 0)))
        {
            Debug.LogError($"Cell is out of range: ({x}, {y})");
            return;
        }

        int xIndex = x + _width / 2;
        int yIndex = y + _height / 2;
        _gridTileTypes[xIndex, yIndex] = tileType;
    }

    // 특정 셀의 타일 타입을 가져오기
    public TileType GetGridTileType(int x, int y)
    {
        // 배열 범위 벗어나면 Field로 반환 (안전 처리)
        if (IsCellOutOfRange(new Vector3Int(x, y, 0)))
        {
            Debug.LogError($"Cell is out of range: ({x}, {y})");
            return TileType.None;
        }

        int xIndex = x + _width / 2;
        int yIndex = y + _height / 2;

        return _gridTileTypes[xIndex, yIndex];
    }

    // 월드 좌표 기반으로 타일 타입 얻기 (좌표 변환 포함)
    public TileType GetGridTileType(Vector3 worldPosition)
    {
        Vector3Int cell = WorldToCell(worldPosition);
        return GetGridTileType(cell.x, cell.y);
    }

    public List<Vector3Int> GetCellsInRange(Vector3 center, int size)
    {
        List<Vector3Int> cells = new();

        Vector3 startPos = CalcCenterPosition(center, size, CellSize);

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float posX = startPos.x + (CellSize.x * i);
                float posZ = startPos.z + (CellSize.y * j);

                Vector3Int cell = WorldToCell(new Vector3(posX, 0, posZ));

                cells.Add(cell);
            }
        }

        return cells;
    }

    public bool IsCellOutOfRange(Vector3Int cell)
    {
        return cell.x < -_width / 2 || cell.x >= _width / 2 || cell.y < -_height / 2 || cell.y >= _height / 2;
    }

    public Building GetBuilding(Vector3Int cell)
    {
        // cell의 위치에 있는 건물을 찾아서 반환한다.
        if (_buildingByCellDict.ContainsKey(cell)) 
            return _buildingByCellDict[cell];

        return null;
    }
    
    public bool IsConnectedToRoad(Vector3 centerWorldPos, int buildingSize)
    {
        List<Vector3Int> boundaryCells = new List<Vector3Int>();

        Vector3 startPos = CalcCenterPosition(centerWorldPos, buildingSize, CellSize);
        Vector3Int startCell = WorldToCell(startPos);

        for (int i = 0; i < buildingSize; i++)
        {
            for (int j = 0; j < buildingSize; j++)
            {
                Vector3Int currentCell = new Vector3Int(startCell.x + j, startCell.y + i, 0);
                boundaryCells.Add(currentCell);
            }
        }

        int[] dirX = {0, 1, 0, -1};
        int[] dirY = {-1, 0, 1, 0};

        for (int i = 0; i < boundaryCells.Count; i++)
        {
            for (int dir = 0; dir < 4; dir++)
            {
                Vector3Int nextCell = new Vector3Int(boundaryCells[i].x + dirX[dir], boundaryCells[i].y + dirY[dir], 0);

                if (IsCellOutOfRange(nextCell)) continue;

                if (GetGridTileType(nextCell.x, nextCell.y) == TileType.Road)
                    return true;
            }
        }

        return false;
    }
    public void AddBuildngByCell(Vector3Int cell, Building building)
    {
        _buildingByCellDict.Add(cell, building);
    }

    public void EnterBuildMode()
    {
        // Camera.main.GetComponent<MobileCameraPan>().enabled = false;
        _buildMode = true;
        _gridVisualizer.SetActive(_buildMode);
    }

    public void ExitBuildMode()
    {
        // Camera.main.GetComponent<MobileCameraPan>().enabled = true;
        _buildMode = false;
        _gridVisualizer.SetActive(_buildMode);
    }
}
