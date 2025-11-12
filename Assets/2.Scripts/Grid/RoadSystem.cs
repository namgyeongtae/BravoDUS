using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

struct RoadData
{
    int x;
    int y;
    int dir;
}

public enum RoadDir
{
    None = 0,               // 0000
    Right = 1,              // 0001
    Left = 2,               // 0010
    RightLeft = 3,          // 0011
    Down = 4,               // 0100
    DownRight = 5,          // 0101
    DownLeft = 6,           // 0110
    DownRightLeft = 7,      // 0111
    Up = 8,                 // 1000
    UpRight = 9,            // 1001
    UpLeft = 10,            // 1010
    UpRightLeft = 11,       // 1011
    UpDown = 12,
    RightUpDown = 13,
    LeftUpDown = 14,
    LeftRightUpDown = 15
}

public enum RoadMode
{
    Install,
    UnInstall
}

public class RoadSystem : MonoBehaviour
{
    [SerializeField] private int _size = 1;
    [SerializeField] private GameObject _constructIndicator;
    [SerializeField] private GridHandler _gridHandler;
    [SerializeField] private RoadTileSO _roadTileSO;

    [SerializeField] private RoadType _roadType = RoadType.Dirt;
    [SerializeField] private RoadMode _roadMode = RoadMode.Install;

    private List<RoadData> _roadDataList = new();
    private RoadTileData _roadTileData => _roadTileSO.RoadTileDatas[(int)_roadType];
    private GameObject _currentIndicator = null;

    private bool _canInstall = false;
    [SerializeField] private int _limitedDeltaTouch = 10; // 도로 설치 시 허용되는 Touch 움직임 거리의 제한 -> 이거 넘긴 상태의 Touch는 도로 설치 못함

    void Start()
    {
        InitRoadTiles();
    }

    // Update is called once per frame
    void Update()
    {
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
// #if !UNITY_EDITOR
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (UIUtils.IsPointerOverUIObject(touch.position)) return;

            if (touch.phase == TouchPhase.Began)
            {
                _canInstall = true;
            }
            if (touch.phase == TouchPhase.Moved)
            {
                _canInstall = touch.deltaPosition.sqrMagnitude <= _limitedDeltaTouch * _limitedDeltaTouch;

                Debug.Log("Can Install : " + _canInstall);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                if (!_canInstall)
                    return;

                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);

                // TODO:
                // 중간에 다른 Layer의 오브젝트를 무시하고 통과하는 Raycast 구현 필요
                if (Physics.Raycast(ray,out RaycastHit hit, 1000, LayerMask.GetMask("Ground")))
                {
                    Vector3Int cell = _gridHandler.WorldToCell(hit.point);
                    
                    //if (cell.x >= -_gridHandler.Width / 2 && cell.x < _gridHandler.Width / 2 
                    //&& cell.y >= -_gridHandler.Height / 2 && cell.y < _gridHandler.Height / 2
                    if (!_gridHandler.IsCellOutOfRange(cell)
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

                _canInstall = true;
            }
        }
/* #else
        if (Input.GetMouseButton(0))
        {
            if (UIUtils.IsPointerOverUIObject(Input.mousePosition)) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
            if (Physics.Raycast(ray,out RaycastHit hit, 1000, LayerMask.GetMask("Default")))
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
        } */
// #endif 
    }

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
    }
    private void DrawAdjacentRoadTile(Vector3Int cell)
    {
        // Right Left Down Up
        int[] dirX = {1, -1, 0, 0};
        int[] dirY = {0, 0, -1, 1};

        for (int i = 0; i < 4; i++)
        {
            int nx = cell.x + dirX[i];
            int ny = cell.y + dirY[i];

            if (nx >= -_gridHandler.Width / 2 && nx < _gridHandler.Width / 2 && ny >= -_gridHandler.Height / 2 && ny < _gridHandler.Height / 2 && _gridHandler.GetGridTileType(nx, ny) == TileType.Road)
            {
                TileType tileType = _gridHandler.GetGridTileType(nx, ny);

                if (tileType == TileType.Road)
                {
                    DrawRoadTile(new Vector3Int(nx, ny, 0));
                }
            }
        }
    }
    
    private void DrawRoadTile(Vector3Int cell)
    {
        // Right Left Down Up
        int[] dirX = {1, -1, 0, 0};
        int[] dirY = {0, 0, -1, 1};

        int roadState = 0x0000;

        for (int i = 0; i < 4; i++)
        {
            int nx = cell.x + dirX[i];
            int ny = cell.y + dirY[i];

            if (nx >= -_gridHandler.Width / 2 && nx < _gridHandler.Width / 2 
             && ny >= -_gridHandler.Height / 2 && ny < _gridHandler.Height / 2)
            {
                TileType tileType = _gridHandler.GetGridTileType(nx, ny);
                if (tileType == TileType.Field)
                {
                    roadState |= 1 << i;
                }
            }
            else
            {
                roadState |= 1 << i;
            }
        }

        DrawTile(cell, roadState);
    }

    private void DrawTile(Vector3Int cell, int roadState)
    {
        _gridHandler.RoadTilemap.SetTile(cell, _roadTileData.RoadTiles[roadState]);

        var roadRuntimeAPI = _gridHandler.RoadTilemap.GetComponent<RoadRuntimeAPI>();

        if (roadRuntimeAPI != null)
        {
            roadRuntimeAPI.Place(cell, _roadTileData.RoadTiles[roadState]);
        }
    }
    
    private void RemoveRoadTile(Vector3Int cell)
    {
        _gridHandler.RoadTilemap.SetTile(cell, null);

        var roadRuntimeAPI = _gridHandler.RoadTilemap.GetComponent<RoadRuntimeAPI>();

        if (roadRuntimeAPI != null)
        {
            roadRuntimeAPI.Remove(cell);
        }
    }

    private void CreateIndicator()
    {
        if (_currentIndicator != null)
        {
            Managers.Resource.Destroy(_currentIndicator);
        }

        _currentIndicator = Instantiate(_constructIndicator);
        _currentIndicator.transform.position = _gridHandler.CellToWorld(0, 0);
    }

    private void DestroyIndicator()
    {
        if (_currentIndicator != null)
        {
            Managers.Resource.Destroy(_currentIndicator);
        }
    }

    private void ResizeIndicator(int sizeScale)
    {
        _currentIndicator.transform.localScale = new Vector3(sizeScale * 2, 1, sizeScale * 2);
        
        var mat = _currentIndicator.GetComponentInChildren<MeshRenderer>().material;
        mat.SetTextureScale("_BaseMap", new Vector2(sizeScale, sizeScale));

        bool isEven = sizeScale % 2 == 0;
        Vector3Int indicatorCell = _gridHandler.WorldToCell(_currentIndicator.transform.position);
        Vector3 indicatorWorld = _gridHandler.CellToWorld(indicatorCell.x, indicatorCell.y);

        if (isEven)
        {
            _currentIndicator.transform.position = new Vector3(indicatorWorld.x - _gridHandler.CellSize.x / 2, 0.05f, indicatorWorld.z - _gridHandler.CellSize.y / 2);
        }
        else
        {
            _currentIndicator.transform.position = new Vector3(indicatorWorld.x, 0.05f, indicatorWorld.z);
        }
    }

    public RoadMode SwitchInstallMode()
    {
        _roadMode = (_roadMode == RoadMode.Install) ? RoadMode.UnInstall : RoadMode.Install;
        return _roadMode;
    }
}
