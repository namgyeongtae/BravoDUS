using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private Material _previewMaterial;
    [SerializeField] private GridHandler _gridHandler;

    [SerializeField] private GameObject _testBuildingPrefab;

    private GameObject _currentBuilding = null;
    private List<Material> _cachedOriginMaterials = new();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_currentBuilding == null)
            {
                StartPlacement(_testBuildingPrefab);
            }
        }

        UpdatePlacement();
    }

    public void StartPlacement(GameObject buildingPrefab)
    {
        if (_currentBuilding != null)
        {
            Managers.Resource.Destroy(_currentBuilding);
        }

        _currentBuilding = Instantiate(buildingPrefab);

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

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray,out RaycastHit hit, 1000, LayerMask.GetMask("Default")))
        {
            Vector3Int cell = _gridHandler.WorldToCell(hit.point);
            if (cell.x >= -_gridHandler.Width / 2 && cell.x < _gridHandler.Width / 2 && cell.y >= -_gridHandler.Height / 2 && cell.y < _gridHandler.Height / 2)
            {
                UpdateBuildingPosition(cell, isEven: true);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            EndPlacement();
        }
    }

    private void UpdateBuildingPosition(Vector3Int cell,bool isEven)
    {
        if (!isEven)
        {
            Vector3 cellToWorld = _gridHandler.CellToWorld(cell.x, cell.y);
            Vector3 buildPos = new Vector3(cellToWorld.x + _gridHandler.CellSize.x / 2, 0, cellToWorld.z + _gridHandler.CellSize.y / 2);

            _currentBuilding.transform.position = buildPos;
        }
        else
        {
            _currentBuilding.transform.position = _gridHandler.CellToWorld(cell.x, cell.y);
        }

        DetectCollision(buildingSize: 4);
    }

    private void DetectCollision(int buildingSize)
    {
        bool isEven = buildingSize % 2 == 0;

        Vector3 startPos;

        if (!isEven)
        {
            float startPosX = _currentBuilding.transform.position.x - (_gridHandler.CellSize.x * (buildingSize / 2 - 1));
            float startPosZ = _currentBuilding.transform.position.z - (_gridHandler.CellSize.y * (buildingSize / 2 - 1));

            startPos = new Vector3(startPosX, 0, startPosZ);
        }
        else
        {
            float startPosX = _currentBuilding.transform.position.x - _gridHandler.CellSize.x / 2 - (_gridHandler.CellSize.x * (buildingSize / 2 - 1));
            float startPosZ = _currentBuilding.transform.position.z - _gridHandler.CellSize.y / 2 - (_gridHandler.CellSize.y * (buildingSize / 2 - 1));

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

        Color matColor = isCollide ? new Color(1f, 0f, 0f, 0.5f) : new Color(1f, 1f, 1f, 0.5f);

        var materials = _currentBuilding.GetComponentInChildren<MeshRenderer>().materials;

        foreach (var mat in materials)
        {
            mat.SetColor("_Color", matColor);
        }
    }

    private void EndPlacement()
    {
        var materials = new Material[_cachedOriginMaterials.Count];

        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = _cachedOriginMaterials[i];
            Debug.Log("materials[i]: " + materials[i].name);
        }

        _currentBuilding.GetComponentInChildren<MeshRenderer>().materials = materials;
        _currentBuilding = null;
    }

    private void OnDrawGizmos()
    {
        if (_currentBuilding == null) return;

        Vector3 startPos;

        bool isEven = true;
        int buildingSize = 4;

        if (!isEven)
        {
            float startPosX = _currentBuilding.transform.position.x - (_gridHandler.CellSize.x * (buildingSize / 2 - 1));
            float startPosZ = _currentBuilding.transform.position.z - (_gridHandler.CellSize.y * (buildingSize / 2 - 1));

            startPos = new Vector3(startPosX, 0, startPosZ);
        }
        else
        {
            float startPosX = _currentBuilding.transform.position.x - _gridHandler.CellSize.x / 2 - (_gridHandler.CellSize.x * (buildingSize / 2 - 1));
            float startPosZ = _currentBuilding.transform.position.z - _gridHandler.CellSize.y / 2 - (_gridHandler.CellSize.y * (buildingSize / 2 - 1));

            startPos = new Vector3(startPosX, 0, startPosZ);
        }

        // Gizmos.DrawCube(startPos, new Vector3(_gridHandler.CellSize.x, _gridHandler.CellSize.x, _gridHandler.CellSize.x));

        for (int i = 0; i < buildingSize; i++)
        {
            for (int j = 0; j < buildingSize; j++)
            {
                float posX = startPos.x + (_gridHandler.CellSize.x * i);
                float posZ = startPos.z + (_gridHandler.CellSize.y * j);

                Gizmos.DrawCube(new Vector3(posX, 0, posZ), new Vector3(_gridHandler.CellSize.x, _gridHandler.CellSize.x, _gridHandler.CellSize.x));
            }
        }
    }
}
