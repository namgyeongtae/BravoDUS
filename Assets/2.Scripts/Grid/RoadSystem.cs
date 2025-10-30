using System.Collections.Generic;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (!_gridHandler.BuildMode) return;

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (_roadMode == RoadMode.Install)
            {
                _roadMode = RoadMode.UnInstall;
                DestroyIndicator();
            }
            else
            {
                _roadMode = RoadMode.Install;
                CreateIndicator();
            }
        }

        if (_roadMode == RoadMode.Install)
            InputDetect();
        else
            UnInstallRoad();

        /* if (_roadMode == RoadMode.Install && _currentIndicator != null)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ResizeIndicator(++_size);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ResizeIndicator(--_size);
            }
        } */
    }

    void OnEnable()
    {
        // CreateIndicator();
    }

    void OnDisable()
    {
        // DestroyIndicator();
    }

    private void InputDetect() // Road 의 방향이 몇 방향인지 알아야 함
    {
#if !UNITY_EDITOR
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Moved)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
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
        }
#else
        if (Input.GetMouseButtonUp(0))
        {
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
        }
#endif
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
        switch (roadState)
        {
            case (int)RoadDir.None:
                Debug.Log("Tile: Center");
                _gridHandler.RoadTilemap.SetTile(cell, _roadTileData.CenterTile);
                break;
            case (int)RoadDir.Right:
                Debug.Log("Tile: Right");
                _gridHandler.RoadTilemap.SetTile(cell, _roadTileData.RightTile);
                break;
            case (int)RoadDir.Left:
                Debug.Log("Tile: Left");
                _gridHandler.RoadTilemap.SetTile(cell, _roadTileData.LeftTile);
                break;
            case (int)RoadDir.Down:
                Debug.Log("Tile: Down");
                _gridHandler.RoadTilemap.SetTile(cell, _roadTileData.DownTile);
                break;
            case (int)RoadDir.Up:
                Debug.Log("Tile: Up");
                _gridHandler.RoadTilemap.SetTile(cell, _roadTileData.UpTile);
                break;
            case (int)RoadDir.UpRight:
                Debug.Log("Tile: UpRight");
                _gridHandler.RoadTilemap.SetTile(cell, _roadTileData.UpRightTile);
                break;
            case (int)RoadDir.UpLeft:
                Debug.Log("Tile: UpLeft");
                _gridHandler.RoadTilemap.SetTile(cell, _roadTileData.UpLeftTile);
                break;
            case (int)RoadDir.DownRight:
                Debug.Log("Tile: DownRight");
                _gridHandler.RoadTilemap.SetTile(cell, _roadTileData.DownRightTile);
                break;
            case (int)RoadDir.DownLeft:
                Debug.Log("Tile: DownLeft");
                _gridHandler.RoadTilemap.SetTile(cell, _roadTileData.DownLeftTile);
                break;
            case (int)RoadDir.UpRightLeft:
                Debug.Log("Tile: UpRightLeft");
                _gridHandler.RoadTilemap.SetTile(cell, _roadTileData.UpRightLeftTile);
                break;
            case (int)RoadDir.RightLeft:
                Debug.Log("Tile: RightLeft");
                _gridHandler.RoadTilemap.SetTile(cell, _roadTileData.RightLeftTile);
                break;
            case (int)RoadDir.DownRightLeft:
                Debug.Log("Tile: DownRightLeft");
                _gridHandler.RoadTilemap.SetTile(cell, _roadTileData.DownRightLeftTile);
                break;
            case (int)RoadDir.UpDown:
                Debug.Log("Tile: UpDown");
                _gridHandler.RoadTilemap.SetTile(cell, _roadTileData.UpDownTile);
                break;
            case (int)RoadDir.RightUpDown:
                Debug.Log("Tile: RightUpDown");
                _gridHandler.RoadTilemap.SetTile(cell, _roadTileData.RightUpDownTile);
                break;
            case (int)RoadDir.LeftUpDown:
                Debug.Log("Tile: LeftUpDown");
                _gridHandler.RoadTilemap.SetTile(cell, _roadTileData.LeftUpDownTile);
                break;
            case (int)RoadDir.LeftRightUpDown:
                Debug.Log("Tile: LeftRightUpDown");
                _gridHandler.RoadTilemap.SetTile(cell, _roadTileData.LeftRightUpDownTile);
                break;
        }
    }
    
    private void RemoveRoadTile(Vector3Int cell)
    {
        _gridHandler.RoadTilemap.SetTile(cell, null);
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
}
