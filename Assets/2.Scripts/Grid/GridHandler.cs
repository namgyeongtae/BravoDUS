using System;
using UnityEngine;
using UnityEngine.Tilemaps;

// 🧱 타일의 종류를 정의하는 열거형
// 여러 시스템(Field, Road, Building 등)에서 공통으로 사용 가능하므로 클래스 밖에 둠
public enum TileType
{
    Field,  // 일반 땅
    Road    // 도로
}

// 🎨 브러시 모드 (사용자가 어떤 종류의 타일을 칠할지)
public enum BrushMode
{
    None,
    Field,
    Road,
    Building
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

    // 현재 선택된 타일 (Field/Road 등)
    [SerializeField] private TileBase _selectedTile;

    // 현재 브러시 모드 (기본: Field)
    [SerializeField] private BrushMode _curBrushMode = BrushMode.Field;

    // 그리드 크기
    private readonly int _width = 20;
    private readonly int _height = 20;

    // Unity Grid Component (좌표 변환용)
    [SerializeField] private Grid _grid;

    // 현재 빌드 모드 상태 (Space로 토글)
    [SerializeField] private bool _buildMode = false;

    // 각 셀의 논리적 타일 타입 저장용 배열
    private TileType[,] _gridTileTypes = new TileType[20, 20];

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
        // 🟢 Space: 빌드 모드 토글
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 카메라 팬 스크립트(MobileCameraPan) 활성/비활성 반전
            Camera.main.GetComponent<MobileCameraPan>().enabled =
                !Camera.main.GetComponent<MobileCameraPan>().enabled;

            // 빌드 모드 on/off
            _buildMode = !_buildMode;

            // 격자 시각화 on/off
            _gridVisualizer.SetActive(_buildMode);
        }

        // 🟣 Tab: 브러시 모드 순환 (None→Field→Road→Building→None)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _curBrushMode = (BrushMode)
                (((int)_curBrushMode + 1) % Enum.GetValues(typeof(BrushMode)).Length);
        }

        // 🖌️ 현재 브러시 모드에 따라 작업 실행
        if (_curBrushMode == BrushMode.Field)
        {
            DrawFieldTile();
        }
        else if (_curBrushMode == BrushMode.Road)
        {
            // 도로 타일 페인트 로직 (아직 비워둠)
        }
        else if (_curBrushMode == BrushMode.Building)
        {
            // 건물 배치 로직 (아직 비워둠)
        }
    }

    // 🖌️ Field 브러시로 타일 칠하기
    public void DrawFieldTile()
    {
        // 빌드 모드 아닐 때는 무시
        if (!_buildMode) return;

        // 마우스 왼쪽 버튼 누르고 있는 동안만 작동
        if (Input.GetMouseButton(0))
        {
            // 마우스 위치에서 레이 쏘기
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);

            // Default 레이어만 대상으로 Raycast
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, LayerMask.GetMask("Default")))
            {
                // 맞은 지점을 셀 좌표로 변환
                Vector3Int cell = WorldToCell(hit.point); // hit.point = 실제 월드 좌표
                Debug.Log("cell: " + cell);

                // 셀이 그리드 범위 안에 있으면 타일 세팅
                if (cell.x >= -_width / 2 && cell.x < _width / 2 &&
                    cell.y >= -_height / 2 && cell.y < _height / 2)
                {
                    // 실제 타일맵에 타일 그리기
                    _fieldTilemap.SetTile(cell, _selectedTile);

                    // 논리 데이터(타일 타입) 갱신
                    SetGridTileType(cell.x, cell.y, TileType.Field);
                }
            }
        }
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
        int xIndex = x + _width / 2;
        int yIndex = y + _height / 2;
        _gridTileTypes[xIndex, yIndex] = tileType;
    }

    // 특정 셀의 타일 타입을 가져오기
    public TileType GetGridTileType(int x, int y)
    {
        int xIndex = x + _width / 2;
        int yIndex = y + _height / 2;

        // 배열 범위 벗어나면 Field로 반환 (안전 처리)
        if (xIndex < 0 || xIndex >= _width || yIndex < 0 || yIndex >= _height)
        {
            return TileType.Field;
        }

        return _gridTileTypes[xIndex, yIndex];
    }

    // 월드 좌표 기반으로 타일 타입 얻기 (좌표 변환 포함)
    public TileType GetGridTileType(Vector3 worldPosition)
    {
        Vector3Int cell = WorldToCell(worldPosition);
        return GetGridTileType(cell.x, cell.y);
    }
}
