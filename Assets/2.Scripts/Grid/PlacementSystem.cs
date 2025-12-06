using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public enum PlacementMode
{
    Install,
    UnInstall
}

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private Material _previewMaterial;
    [SerializeField] private GridHandler _gridHandler;
    [SerializeField] private PlacementMode _placementMode = PlacementMode.Install;

    private bool _canMove = false;
    private bool _canBuild = true;
    private GameObject _currentBuilding = null;
    private int _buildingSize = 0;
    private List<Material> _cachedOriginMaterials = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < _gridHandler.Width; i++)
        {
            for (int j = 0; j < _gridHandler.Height; j++)
            {
                if (_gridHandler.GetGridTileType(i, j) == TileType.Road)
                    Debug.Log("Road: " + i + ", " + j);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Managers.Construct.ConstructMode != ConstructMode.Placement) return;

        /* if (Input.GetKeyDown(KeyCode.I))
            _placementMode = (_placementMode == PlacementMode.Install) ? PlacementMode.UnInstall : PlacementMode.Install; */

        UpdatePlacement();

        /* if (_placementMode == PlacementMode.Install)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                if (_currentBuilding == null)
                {
                    StartPlacement(_testBuildingPrefab);
                }
            }
            UpdatePlacement();
        }
        else
            UnInstallPlacement(); */
    }

    public void StartPlacement(GameObject buildingPrefab, int buildingSize)
    {
        if (_currentBuilding != null)
        {
            Managers.Resource.Destroy(_currentBuilding);
        }

        // _currentBuilding = Instantiate(buildingPrefab);
        _buildingSize = buildingSize;

        if (CalcDefaultPosition(_buildingSize, out Vector3 defaultPos))
        {
            _currentBuilding = Managers.Resource.InstantiateAddressable(buildingPrefab.GetComponent<Building>().BuildingType.ToString(), Vector3.zero, Quaternion.identity);
            _currentBuilding.transform.position = defaultPos;
        }
        else
        {
            Managers.UI.AddPanel<UIToastPopup>().SettingPopup("지을 수 있는 곳이 없습니다.");
            return;
        }

        // 프리뷰 머터리얼 설정
        var materials = _currentBuilding.GetComponentInChildren<MeshRenderer>().materials;

        _cachedOriginMaterials.Clear();
        foreach (var mat in materials)
        {
            _cachedOriginMaterials.Add(mat);
        }

        materials = new Material[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = _previewMaterial;
        }

        _currentBuilding.GetComponentInChildren<MeshRenderer>().materials = materials;
    }

    private void UpdatePlacement()
    {
        if (_currentBuilding == null) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Rotate Building");
            RotateBuilding();
        }

        // #if UNITY_EDITOR
        /* Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray,out RaycastHit hit, 1000, LayerMask.GetMask("Default")))
        {
            Vector3Int cell = _gridHandler.WorldToCell(hit.point);
            if (cell.x >= -_gridHandler.Width / 2 && cell.x < _gridHandler.Width / 2 && cell.y >= -_gridHandler.Height / 2 && cell.y < _gridHandler.Height / 2)
            {
                UpdateBuildingPosition(cell, isEven: true);
            }
        } */
        // #elif UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit, 1000, LayerMask.GetMask("Building")))
                {
                    var hitObject = hit.collider.transform.FindHighestParent().gameObject;

                    if (hitObject == _currentBuilding)
                    {
                        _canMove = true;
                        Camera.main.GetComponent<MobileCameraPan>().SetMoveMode(false);
                    }
                }
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                if (_canMove)
                {
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    if (Physics.Raycast(ray, out RaycastHit hit, 1000, LayerMask.GetMask("Default") | LayerMask.GetMask("Building")))
                    {
                        Vector3Int cell = _gridHandler.WorldToCell(hit.point);

                        if (!UIUtils.IsPointerOverUIObject(touch.position))
                        {
                            UpdateBuildingPosition(cell);
                        }
                    }
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                _canMove = false;
                Camera.main.GetComponent<MobileCameraPan>().SetMoveMode(true);
            }
        }
        // #endif


        // TODO:
        // 이후 모바일에서는 그냥 터치로 끝내는 게 아닌 중간 과정의 UI/UX가 필요해 보임(정책 결정 필요)
        /* #if UNITY_EDITOR
        if (Input.GetMouseButtonUp(0))
        {
            EndPlacement();
        }
        #elif UNITY_ANDROID || UNITY_IOS
        if (touch.phase == TouchPhase.Ended)
        {
            EndPlacement();
        }
        #endif */
    }

    private void UnInstallPlacement()
    {
        // TODO:
        // 마찬가지로 이후 모바일에서 그냥 터치한다고 파괴되는 게 아니라 UI/UX 정책이 필요하다.
        // 현재는 테스트 용으로 PC 기준 입력만 고려
#if UNITY_EDITOR
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000, LayerMask.GetMask("Building")))
            {
                Managers.Resource.Destroy(hit.collider.gameObject);
                _currentBuilding = null;
            }
        }
#elif UNITY_ANDROID || UNITY_IOS
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Ended)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, LayerMask.GetMask("Building")))
            {
                Managers.Resource.Destroy(hit.collider.gameObject);
                _currentBuilding = null;
            }
        }
#endif
    }

    private void UpdateBuildingPosition(Vector3Int cell)
    {
        bool isEven = _buildingSize % 2 == 0;

        if (isEven)
        {
            Vector3 cellToWorld = _gridHandler.CellToWorld(cell.x, cell.y);
            Vector3 buildPos = new Vector3(cellToWorld.x + _gridHandler.CellSize.x / 2, 0, cellToWorld.z + _gridHandler.CellSize.y / 2);

            if (!IsCellsOutOfRange(_gridHandler.GetCellsInRange(buildPos, _buildingSize)))
            {
                _currentBuilding.transform.position = buildPos;
            }
            else
            {
                float minX = (-_gridHandler.Width / 2) * _gridHandler.CellSize.x + (_buildingSize / 2) * _gridHandler.CellSize.x;
                float maxX = (_gridHandler.Width / 2) * _gridHandler.CellSize.x - (_buildingSize / 2) * _gridHandler.CellSize.x;
                float minZ = (-_gridHandler.Height / 2) * _gridHandler.CellSize.y + (_buildingSize / 2) * _gridHandler.CellSize.y;
                float maxZ = (_gridHandler.Height / 2) * _gridHandler.CellSize.y - (_buildingSize / 2) * _gridHandler.CellSize.y;

                float clampedX = Mathf.Clamp(buildPos.x, minX, maxX);
                float clampedZ = Mathf.Clamp(buildPos.z, minZ, maxZ);

                _currentBuilding.transform.position = new Vector3(clampedX, buildPos.y, clampedZ);

                Debug.Log("minX: " + minX + ", maxX: " + maxX + ", minZ: " + minZ + ", maxZ: " + maxZ);
                Debug.Log("clampedX: " + clampedX + ", clampedZ: " + clampedZ);
            }
        }
        else
        {
            Vector3 buildPos = _gridHandler.CellToWorld(cell.x, cell.y);
            
            if (!IsCellsOutOfRange(_gridHandler.GetCellsInRange(buildPos, _buildingSize)))
            {
                _currentBuilding.transform.position = buildPos;
            }
            else
            {
                float minX = (-_gridHandler.Width / 2) * _gridHandler.CellSize.x + (_buildingSize / 2) * _gridHandler.CellSize.x + _gridHandler.CellSize.x / 2;
                float maxX = (_gridHandler.Width / 2) * _gridHandler.CellSize.x - (_buildingSize / 2) * _gridHandler.CellSize.x - _gridHandler.CellSize.x / 2;
                float minZ = (-_gridHandler.Height / 2) * _gridHandler.CellSize.y + (_buildingSize / 2) * _gridHandler.CellSize.y + _gridHandler.CellSize.y / 2;
                float maxZ = (_gridHandler.Height / 2) * _gridHandler.CellSize.y - (_buildingSize / 2) * _gridHandler.CellSize.y - _gridHandler.CellSize.y / 2;

                float clampedX = Mathf.Clamp(buildPos.x, minX, maxX);
                float clampedZ = Mathf.Clamp(buildPos.z, minZ, maxZ);

                _currentBuilding.transform.position = new Vector3(clampedX, buildPos.y, clampedZ);

                Debug.Log("minX: " + minX + ", maxX: " + maxX + ", minZ: " + minZ + ", maxZ: " + maxZ);
                Debug.Log("clampedX: " + clampedX + ", clampedZ: " + clampedZ);
            }
        }

        _canBuild = !DetectCollision(_currentBuilding.transform.position, _buildingSize);
        SetMaterialColor(_canBuild);
    }

    private bool DetectCollision(Vector3 position, int buildingSize)
    {
        bool isEven = buildingSize % 2 == 0;

        Vector3 startPos;

        if (!isEven)
        {
            float startPosX = position.x - (_gridHandler.CellSize.x * (buildingSize / 2));
            float startPosZ = position.z - (_gridHandler.CellSize.y * (buildingSize / 2));

            startPos = new Vector3(startPosX, 0, startPosZ);
        }
        else
        {
            float startPosX = position.x - _gridHandler.CellSize.x / 2 - (_gridHandler.CellSize.x * (buildingSize / 2 - 1));
            float startPosZ = position.z - _gridHandler.CellSize.y / 2 - (_gridHandler.CellSize.y * (buildingSize / 2 - 1));

            startPos = new Vector3(startPosX, 0, startPosZ);
        }

        bool isCollide = false;

        for (int i = 0; i < buildingSize; i++)
        {
            for (int j = 0; j < buildingSize; j++)
            {
                float posX = startPos.x + (_gridHandler.CellSize.x * i);
                float posZ = startPos.z + (_gridHandler.CellSize.y * j);

                TileType tileType = _gridHandler.GetGridTileType(new Vector3(posX, 0, posZ));

                if (tileType != TileType.Field)
                {
                    isCollide = true;
                    break;
                }
            }

            if (isCollide)
                break;
        }

        return isCollide;
    }

    public bool EndPlacement(bool isCancel = false)
    {
        if (!_canBuild && !isCancel)
        {
            Managers.UI.AddPanel<UIToastPopup>().SettingPopup("그쪽은 못짓겠는데요?");
            return false;
        }

        _gridHandler.ExitBuildMode();

        if (isCancel)
        {
            Managers.Resource.Destroy(_currentBuilding);
            _currentBuilding = null;
            return isCancel;
        }

        var materials = new Material[_cachedOriginMaterials.Count];

        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = _cachedOriginMaterials[i];
            Debug.Log("materials[i]: " + materials[i].name);
        }

        _currentBuilding.GetComponentInChildren<MeshRenderer>().materials = materials;
        _currentBuilding.GetComponent<Building>().StartConstruction();

        SetTileTypeConstructed();

        _currentBuilding = null;

        _gridHandler.ExitBuildMode();

        return true;
    }

    private void SetMaterialColor(bool canBuild)
    {
        if (_currentBuilding == null) return;

        Color matColor = canBuild ? new Color(1f, 1f, 1f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);

        var materials = _currentBuilding.GetComponentInChildren<MeshRenderer>().materials;

        foreach (var mat in materials)
        {
            Debug.Log("Set Color: " + matColor);
            mat.SetColor("_Color", matColor);
        }
    }

    private void SetTileTypeConstructed()
    {
        if (_currentBuilding == null)
            return;

        Vector3 startPos;

        bool isEven = _buildingSize % 2 == 0;

        if (!isEven)
        {
            float startPosX = _currentBuilding.transform.position.x - (_gridHandler.CellSize.x * (_buildingSize / 2));
            float startPosZ = _currentBuilding.transform.position.z - (_gridHandler.CellSize.y * (_buildingSize / 2));

            startPos = new Vector3(startPosX, 0, startPosZ);
        }
        else
        {
            float startPosX = _currentBuilding.transform.position.x - _gridHandler.CellSize.x / 2 - (_gridHandler.CellSize.x * (_buildingSize / 2 - 1));
            float startPosZ = _currentBuilding.transform.position.z - _gridHandler.CellSize.y / 2 - (_gridHandler.CellSize.y * (_buildingSize / 2 - 1));

            startPos = new Vector3(startPosX, 0, startPosZ);
        }

        for (int i = 0; i < _buildingSize; i++)
        {
            for (int j = 0; j < _buildingSize; j++)
            {
                float posX = startPos.x + (_gridHandler.CellSize.x * i);
                float posZ = startPos.z + (_gridHandler.CellSize.y * j);

                Vector3Int cell = _gridHandler.WorldToCell(new Vector3(posX, 0, posZ));

                _gridHandler.SetGridTileType(cell.x, cell.y, TileType.Constructed);
            }
        }
    }

    public void RotateBuilding()
    {
        if (_currentBuilding == null) return;

        float currentY = _currentBuilding.transform.rotation.eulerAngles.y;
        // float rotationY = (currentY <= 0) ? 270 : 0;
        float rotationY = Mathf.Approximately(currentY, 0) ? 270 : 0;

        _currentBuilding.transform.rotation = Quaternion.Euler(0, rotationY, 0);
    }

    private bool IsCellsOutOfRange(List<Vector3Int> cells)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].x < -_gridHandler.Width / 2 || cells[i].x > _gridHandler.Width / 2 || cells[i].y < -_gridHandler.Height / 2 || cells[i].y > _gridHandler.Height / 2)
                return true;
        }

        return false;
    }

    private bool CalcDefaultPosition(int size, out Vector3 defaultPos)
    {
        Vector3Int startCell = Vector3Int.zero;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 1000, LayerMask.GetMask("Ground")))
        {
            startCell = _gridHandler.WorldToCell(hit.point);
        }

        int[] dirX = {0, 1, 0, -1};
        int[] dirY = {-1, 0, 1, 0};

        // BFS 탐색
        Queue<Vector3Int> cellQueue = new();
        HashSet<Vector3Int> visitedCells = new();

        cellQueue.Enqueue(startCell);
        visitedCells.Add(startCell);

        while (cellQueue.Count > 0)
        {
            Vector3Int currentCell = cellQueue.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                int nextX = (size % 2 == 0) ? currentCell.x + dirX[i] * (size / 2) : currentCell.x + dirX[i] * (size / 2 + 1);
                int nextY = (size % 2 == 0) ? currentCell.y + dirY[i] * (size / 2) : currentCell.y + dirY[i] * (size / 2 + 1);

                Vector3Int nextCell = new Vector3Int(nextX, nextY, 0);

                if (_gridHandler.IsCellOutOfRange(nextCell) || visitedCells.Contains(nextCell)) continue;

                Vector3 nextPos = _buildingSize % 2 != 0 ? 
                _gridHandler.CellToWorld(nextCell.x, nextCell.y) 
              : _gridHandler.CellToWorld(nextCell.x, nextCell.y) + new Vector3(_gridHandler.CellSize.x / 2, 0, _gridHandler.CellSize.y / 2);

                bool isCollide = DetectCollision(nextPos, size);

                if (!isCollide)
                {
                    defaultPos = nextPos;
                    return true;
                }
                else
                {
                    cellQueue.Enqueue(nextCell);
                    visitedCells.Add(nextCell);
                }
            }
        }

        defaultPos = Vector3Int.zero;
        return false;
    }
}
