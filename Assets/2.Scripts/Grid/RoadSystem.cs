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

public enum Dir
{
    Up = 0,
    Right,
    Down,
    Left
}

public class RoadSystem : MonoBehaviour
{
    [SerializeField] private GridHandler _gridHandler;
    [SerializeField] private RoadTileSO _roadTileSO;

    [SerializeField] private RoadType _roadType = RoadType.Dirt;

    private List<RoadData> _roadDataList = new();
    private RoadTileData _roadTileData => _roadTileSO.RoadTileDatas[(int)_roadType];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (!_gridHandler.BuildMode) return;

        InputDetect();
    }

    private void InputDetect() // Road 의 방향이 몇 방향인지 알아야 함
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
            if (Physics.Raycast(ray,out RaycastHit hit, 1000, LayerMask.GetMask("Default")))
            {
                Vector3Int cell = _gridHandler.WorldToCell(hit.point);
                Debug.Log("cell: " + cell);
                if (cell.x >= -_gridHandler.Width / 2 && cell.x < _gridHandler.Width / 2 && cell.y >= -_gridHandler.Height / 2 && cell.y < _gridHandler.Height / 2)
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
}
