using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType
{
    Field,
    Road
}

public enum BrushMode
{
    None,
    Field,
    Road,
    Building
}

public class GridHandler : MonoBehaviour
{
    [SerializeField] private GameObject _gridVisualizer;    
    [SerializeField] private Tilemap _fieldTilemap;
    [SerializeField] private Tilemap _roadTilemap;
    [SerializeField] private TileBase _selectedTile;        // 이후 고도화 (더 다양한 TileBase를 선택할 수 있는 UX 제공)

    [SerializeField] private BrushMode _curBrushMode = BrushMode.Field;

    private readonly int _width = 20;
    private readonly int _height = 20;

    [SerializeField] private Grid _grid;

    [SerializeField] private bool _buildMode = false;

    private TileType[,] _gridTileTypes = new TileType[20, 20];

    public bool BuildMode => _buildMode;

    public int Width => _width;
    public int Height => _height;

    public Vector3 CellSize => _grid.cellSize;

    public Tilemap FieldTilemap => _fieldTilemap;
    public Tilemap RoadTilemap => _roadTilemap;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Camera.main.GetComponent<MobileCameraPan>().enabled = !Camera.main.GetComponent<MobileCameraPan>().enabled;
            _buildMode = !_buildMode;
            _gridVisualizer.SetActive(_buildMode);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _curBrushMode = (BrushMode)(((int)_curBrushMode + 1) % Enum.GetValues(typeof(BrushMode)).Length);
        }

        if (_curBrushMode == BrushMode.Field)
        {
            DrawFieldTile();
        }
        else if (_curBrushMode == BrushMode.Road)
        {

        }
        else if (_curBrushMode == BrushMode.Building)
        {
            
        }
    }
    public void DrawFieldTile()
    {
        if (!_buildMode) return;

        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
            if (Physics.Raycast(ray,out RaycastHit hit, 1000, LayerMask.GetMask("Default")))
            {
                Vector3Int cell = WorldToCell(hit.point);
                Debug.Log("cell: " + cell);
                if (cell.x >= -_width / 2 && cell.x < _width / 2 && cell.y >= -_height / 2 && cell.y < _height / 2)
                {
                    _fieldTilemap.SetTile(cell, _selectedTile);
                    SetGridTileType(cell.x, cell.y, TileType.Field);
                }
            }
        }
    }

    public Vector3 CellToWorld(int x, int y)
    {
        return _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(CellSize.x / 2, 0, CellSize.y / 2);
    }

    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        return _grid.WorldToCell(worldPosition);
    }

    public void SetGridTileType(int x, int y, TileType tileType)
    {
        int xIndex = x + _width / 2;
        int yIndex = y + _height / 2;

        _gridTileTypes[xIndex, yIndex] = tileType;
    }

    public TileType GetGridTileType(int x, int y)
    {
        int xIndex = x + _width / 2;
        int yIndex = y + _height / 2;

        if (xIndex < 0 || xIndex >= _width || yIndex < 0 || yIndex >= _height)
        {
            return TileType.Field;
        }

        return _gridTileTypes[xIndex, yIndex];
    }

    public TileType GetGridTileType(Vector3 worldPosition)
    {
        Vector3Int cell = WorldToCell(worldPosition);

        return GetGridTileType(cell.x, cell.y);
    }
}
