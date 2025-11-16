using System.Collections.Generic;
using UnityEngine;
using System;

// ⚠️ 현재 struct 필드가 전부 private(기본)이라 외부에서 접근이 불가.
// 앞으로 사용할 계획이면 public으로 바꾸거나 프로퍼티/생성자를 추가하자.
struct RoadData
{
    int x;   // 셀 X
    int y;   // 셀 Y
    int dir; // 비트마스크(4방향 막힘정보 등용)
}

// 4방향을 비트로 표현(오=1, 왼=2, 아래=4, 위=8)
// 이 코드에서는 "막힌 방향(도로가 아닌 방향/경계/필드)"을 1로 세팅하는 컨벤션임
public enum RoadDir
{
    None = 0,               // 0000 : 사방이 도로(센터)
    Right = 1,              // 0001 : 오른쪽만 막힘
    Left = 2,               // 0010 : 왼쪽만 막힘
    RightLeft = 3,          // 0011 : 좌우만 막힘
    Down = 4,               // 0100 : 아래만 막힘
    DownRight = 5,          // 0101 : 아래+오른쪽 막힘
    DownLeft = 6,           // 0110 : 아래+왼쪽 막힘
    DownRightLeft = 7,      // 0111 : 아래+좌+우 막힘
    Up = 8,                 // 1000 : 위만 막힘
    UpRight = 9,            // 1001 : 위+오른쪽 막힘
    UpLeft = 10,            // 1010 : 위+왼쪽 막힘
    UpRightLeft = 11,       // 1011 : 위+좌+우 막힘
    UpDown = 12,            // 1100 : 위+아래 막힘
    RightUpDown = 13,       // 1101 : 위+아래+오른쪽 막힘
    LeftUpDown = 14,        // 1110 : 위+아래+왼쪽 막힘
    LeftRightUpDown = 15    // 1111 : 사방 막힘(고립/엔드캡)
}

public enum RoadMode
{
    Install,   // 설치 모드(좌클릭으로 도로 깔기)
    UnInstall  // 제거 모드(좌클릭으로 도로 지우기)
}

/// <summary>
/// 유저 입력을 감지해 도로를 설치하거나 제거하는 시스템.
/// <para> GridHandler를 통해 실제 셀의 TileType을 Road로 변경.</para>
/// <para> Tilemap에 도로 타일을 배치 및 연결부(교차로, 곡선 등) 갱신.</para>
/// <para> 설치/제거 완료 시 RoadGraphFromGrid에 알리기 위해 RoadsChanged 이벤트 발행.</para>
/// 도로 편집의 시각적/논리적 일관성을 유지.
/// </summary>
public class RoadSystem : MonoBehaviour
{
    [SerializeField] private int _size = 1; // 추후 영역 페인트 크기(현재 미사용)
    [SerializeField] private GameObject _constructIndicator; // 설치 미리보기 프리팹
    [SerializeField] private GridHandler _gridHandler;       // 셀 변환/타입 관리
    [SerializeField] private RoadTileSO _roadTileSO;         // 타일 변형 데이터 모음(SO)

    [SerializeField] private RoadType _roadType = RoadType.Dirt; // 흙길/포장 등 종류
    [SerializeField] private RoadMode _roadMode = RoadMode.Install;

    private List<RoadData> _roadDataList = new(); // 추후 영역/경로 기록용(현재 미사용)
    private RoadTileData _roadTileData => _roadTileSO.RoadTileDatas[(int)_roadType]; // 선택형 타일셋
    private GameObject _currentIndicator = null; // 미리보기 인디케이터 인스턴스

    // "도로 변경됨" 방송용 전역 이벤트
    public static event Action RoadsChanged;

    // 도로 설치/제거 후 호출 - 구독자(RoadGraphFromGrid)에게 "변경 알림"
    private void NotifyRoadsChanged()
    {
        RoadsChanged?.Invoke();
    }

    void Start()
    {
        InitRoadTiles();
    }

    void Update()
    {
        // 빌드 모드가 아니면 도로 편집 차단
        if (!_gridHandler.BuildMode) return;

        if (Managers.Construct.ConstructMode != ConstructMode.Road) return;

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (_roadMode == RoadMode.Install)
            {
                _roadMode = RoadMode.UnInstall;
                // DestroyIndicator();
            }
            else
            {
                _roadMode = RoadMode.Install;
                // CreateIndicator();
            }
        }

        // 현재 모드에 따라 입력 처리 분기
        if (_roadMode == RoadMode.Install)
            InstallRoad();
        else
            UnInstallRoad();
    }

    private void InitRoadTiles()
    {
        for (int x = -_gridHandler.Width / 2; x < _gridHandler.Width / 2; x++)
        {
            for (int y = -_gridHandler.Height / 2; y < _gridHandler.Height / 2; y++)
            {
                var tilebase = _gridHandler.RoadTilemap.GetTile(new Vector3Int(x, y, 0));
                if (tilebase != null)
                {
                    _gridHandler.SetGridTileType(x, y, TileType.Road);
                }
            }
        }
    }

    private void InstallRoad() // Road 의 방향이 몇 방향인지 알아야 함
    {
#if !UNITY_EDITOR
        Touch touch = Input.GetTouch(0);

        if (UIUtils.IsPointerOverUIObject(touch.position)) return;

        if (touch.phase == TouchPhase.Moved)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);

            // ⚠️ 레이어 하드코딩(Default). 인스펙터 노출 권장.
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, LayerMask.GetMask("Default")))
            {
                Vector3Int cell = _gridHandler.WorldToCell(hit.point);

                if (cell.x >= -_gridHandler.Width / 2 && cell.x < _gridHandler.Width / 2 
                && cell.y >= -_gridHandler.Height / 2 && cell.y < _gridHandler.Height / 2
                && _gridHandler.GetGridTileType(cell.x, cell.y) == TileType.Field)
                {
                    // 논리 타입을 Road로 변경(먼저 변경해야 주변 계산이 일관됨)
                    _gridHandler.SetGridTileType(cell.x, cell.y, TileType.Road);

                    // 현재 셀 타일 그리기(주변 상태를 보고 변형 선택)
                    DrawRoadTile(cell);

                    // 인접 4셀 중 "이미 도로"인 셀들도 연결 상태가 변하므로 재그리기
                    DrawAdjacentRoadTile(cell);

                    // 그래프에 "변경됨" 알림 (리빌드 디바운드 대기 시작)
                    NotifyRoadsChanged();
                }
                else
                {
                    Debug.Log("cell is out of bounds");
                }
            }
        }
#else
        if (Input.GetMouseButton(0))
        {
            if (UIUtils.IsPointerOverUIObject(Input.mousePosition)) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, LayerMask.GetMask("Default")))
            {
                Vector3Int cell = _gridHandler.WorldToCell(hit.point);

                if (cell.x >= -_gridHandler.Width / 2 && cell.x < _gridHandler.Width / 2
                && cell.y >= -_gridHandler.Height / 2 && cell.y < _gridHandler.Height / 2
                && _gridHandler.GetGridTileType(cell.x, cell.y) == TileType.Field)
                {
                    _gridHandler.SetGridTileType(cell.x, cell.y, TileType.Road);

                    DrawRoadTile(cell);
                    DrawAdjacentRoadTile(cell);
                }
                else
                {
                    Debug.Log("cell is out of bounds");
                }
            }
        }
#endif
    }

    // 도로 제거: 좌클릭한 셀이 도로면 지우고, 인접 4셀 재계산
    private void UnInstallRoad()
    {
#if !UNITY_EDITOR
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Ended)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
            if (Physics.Raycast(ray,out RaycastHit hit, 1000, LayerMask.GetMask("Default")))
            {
                Vector3Int cell = _gridHandler.WorldToCell(hit.point);
                Debug.Log("cell: " + cell);
                if (cell.x >= -_gridHandler.Width / 2 && cell.x < _gridHandler.Width / 2 
                && cell.y >= -_gridHandler.Height / 2 && cell.y < _gridHandler.Height / 2
                && _gridHandler.GetGridTileType(cell.x, cell.y) == TileType.Road)
                {
                    _gridHandler.SetGridTileType(cell.x, cell.y, TileType.Field);

                    RemoveRoadTile(cell);
                    DrawAdjacentRoadTile(cell);
                }
                else
                {
                    Debug.Log("cell is out of bounds");
                }
            }
        }
#endif        
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000, LayerMask.GetMask("Default")))
            {
                Vector3Int cell = _gridHandler.WorldToCell(hit.point);
                Debug.Log("cell: " + cell);

                if (cell.x >= -_gridHandler.Width / 2 && cell.x < _gridHandler.Width / 2
                 && cell.y >= -_gridHandler.Height / 2 && cell.y < _gridHandler.Height / 2
                 && _gridHandler.GetGridTileType(cell.x, cell.y) == TileType.Road)
                {
                    // 논리 타입을 Field로 되돌림
                    _gridHandler.SetGridTileType(cell.x, cell.y, TileType.Field);

                    // 실제 타일맵에서 타일 제거
                    RemoveRoadTile(cell);

                    // 인접 도로들의 연결부를 다시 계산
                    DrawAdjacentRoadTile(cell);

                    // 그래프에 "변경됨" 알림(리빌드 디바운스 대기 시작)
                    NotifyRoadsChanged();
                }
                else
                {
                    Debug.Log("cell is out of bounds");
                }
            }
        }
    }

    // 인접 4방(오,왼,아래,위)에 "이미 도로"가 있으면 해당 셀을 다시 그려 연결부 업데이트
    private void DrawAdjacentRoadTile(Vector3Int cell)
    {
        // Right Left Down Up (비트 순서와 동일하게 유지)
        int[] dirX = { 1, -1, 0, 0 };
        int[] dirY = { 0, 0, -1, 1 };

        for (int i = 0; i < 4; i++)
        {
            int nx = cell.x + dirX[i];
            int ny = cell.y + dirY[i];

            // 내부 + 도로인 경우만 갱신
            if (nx >= -_gridHandler.Width / 2 && nx < _gridHandler.Width / 2
             && ny >= -_gridHandler.Height / 2 && ny < _gridHandler.Height / 2
             && _gridHandler.GetGridTileType(nx, ny) == TileType.Road)
            {
                DrawRoadTile(new Vector3Int(nx, ny, 0));
            }
        }
    }

    // 현재 셀의 "막힌 방향" 비트마스크를 계산해 적절한 변형 타일을 선택
    private void DrawRoadTile(Vector3Int cell)
    {
        // Right Left Down Up (비트 0,1,2,3)
        int[] dirX = { 1, -1, 0, 0 };
        int[] dirY = { 0, 0, -1, 1 };

        int roadState = 0x0000; // 0이면 사방이 도로(센터)

        for (int i = 0; i < 4; i++)
        {
            int nx = cell.x + dirX[i];
            int ny = cell.y + dirY[i];

            // 경계 밖이면 해당 방향은 "막힘"
            if (nx < -_gridHandler.Width / 2 || nx >= _gridHandler.Width / 2
             || ny < -_gridHandler.Height / 2 || ny >= _gridHandler.Height / 2)
            {
                roadState |= 1 << i;
                continue;
            }

            // 내부인데 이웃이 "Field"면, 그 방향은 "막힘"
            // (이웃이 Road면 연결되어 "뚫림" 상태이므로 비트 유지)
            TileType neighbor = _gridHandler.GetGridTileType(nx, ny);
            if (neighbor == TileType.Field)
                roadState |= 1 << i;
        }

        DrawTile(cell, roadState);
    }

    // 계산된 roadState(막힘 비트)에 따라 적절한 변형 타일을 SetTile
    private void DrawTile(Vector3Int cell, int roadState)
    {
        _gridHandler.RoadTilemap.SetTile(cell, _roadTileData.RoadTiles[roadState]);

        var roadRuntimeAPI = _gridHandler.RoadTilemap.GetComponent<RoadRuntimeAPI>();

        if (roadRuntimeAPI != null)
        {
            roadRuntimeAPI.Place(cell, _roadTileData.RoadTiles[roadState]);
        }
    }

    // 특정 셀의 도로 타일 제거
    private void RemoveRoadTile(Vector3Int cell)
    {
        _gridHandler.RoadTilemap.SetTile(cell, null);

        var roadRuntimeAPI = _gridHandler.RoadTilemap.GetComponent<RoadRuntimeAPI>();

        if (roadRuntimeAPI != null)
        {
            roadRuntimeAPI.Remove(cell);
        }
    }

    // 설치 미리보기 오브젝트 생성(설치 모드 전용)
    private void CreateIndicator()
    {
        if (_currentIndicator != null)
            Managers.Resource.Destroy(_currentIndicator);

        _currentIndicator = Instantiate(_constructIndicator);
        _currentIndicator.transform.position = _gridHandler.CellToWorld(0, 0);
    }

    // 설치 미리보기 제거
    private void DestroyIndicator()
    {
        if (_currentIndicator != null)
            Managers.Resource.Destroy(_currentIndicator);
    }

    // 인디케이터 크기/텍스처 스케일/정렬 보정(월드 XZ 격자 가정)
    private void ResizeIndicator(int sizeScale)
    {
        _currentIndicator.transform.localScale = new Vector3(sizeScale * 2, 1, sizeScale * 2);

        var mat = _currentIndicator.GetComponentInChildren<MeshRenderer>().material;
        mat.SetTextureScale("_BaseMap", new Vector2(sizeScale, sizeScale));

        bool isEven = sizeScale % 2 == 0;

        Vector3Int indicatorCell = _gridHandler.WorldToCell(_currentIndicator.transform.position);
        Vector3 indicatorWorld = _gridHandler.CellToWorld(indicatorCell.x, indicatorCell.y);

        // 짝/홀 크기에 따라 셀 중심 정렬 보정
        if (isEven)
        {
            _currentIndicator.transform.position = new Vector3(
                indicatorWorld.x - _gridHandler.CellSize.x / 2,
                0.05f,
                indicatorWorld.z - _gridHandler.CellSize.y / 2);
        }
        else
        {
            _currentIndicator.transform.position = new Vector3(
                indicatorWorld.x,
                0.05f,
                indicatorWorld.z);
        }
    }

    public RoadMode SwitchInstallMode()
    {
        _roadMode = (_roadMode == RoadMode.Install) ? RoadMode.UnInstall : RoadMode.Install;
        return _roadMode;
    }
}
