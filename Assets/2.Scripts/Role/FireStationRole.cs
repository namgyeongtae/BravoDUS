using System.Collections.Generic;
using UnityEngine;

public class FireStationRole : RoleHandler
{
    [SerializeField] private int _handleTerritorySize = 5;

    [SerializeField] private List<Building> _handleBuildings = new List<Building>();

    private float _suppressionRate = 0.3f;

    public float SuppressionRate => _suppressionRate;

    public List<Building> HandleBuildings => _handleBuildings;

    public override void Initialize()
    {
        Managers.Event.Fire.AddFireStationRole(this);

        SearchBuildingsInTerritory();
    }
    public override void HandleEvent(string eventType)
    {
        
    }

    private void SearchBuildingsInTerritory()
    {
        var gridHandler = Managers.Construct.GridHandler;

        Debug.Log("Fire Station: " + Managers.Construct.GridHandler.WorldToCell(transform.position));

        // 반경 _handleTerritorySize 만큼의 셀을 찾는다.
        List<Vector3Int> cells = new List<Vector3Int>();
        Vector3Int centerCell = Managers.Construct.GridHandler.WorldToCell(transform.position);

        int startX = centerCell.x - _handleTerritorySize;
        int startY = centerCell.y - _handleTerritorySize;

        for (int x = startX; x < centerCell.x + _handleTerritorySize; x++) 
        {
            for (int y = startY; y < centerCell.y + _handleTerritorySize; y++)
            {
                cells.Add(new Vector3Int(x, y, 0));
            }
        }

        // cells 중에서 건물이 있는 셀을 찾아 해당 셀에 있는 건물을 _handleBuildings에 추가한다.
        foreach (var cell in cells)
        {
            var building = gridHandler.GetBuilding(cell);
            if (building != null && !building.gameObject.Equals(this.gameObject))
            {
                _handleBuildings.Add(building);
            }
        }
    }

    public void RegisterBuilding(Building building)
    {
        Vector3Int centerCell = Managers.Construct.GridHandler.WorldToCell(transform.position);
        Vector3Int cell = Managers.Construct.GridHandler.WorldToCell(building.transform.position);

        int distanceX = Mathf.Abs(centerCell.x - cell.x);
        int distanceY = Mathf.Abs(centerCell.y - cell.y);

        if (distanceX <= _handleTerritorySize && distanceY <= _handleTerritorySize)
        {
            _handleBuildings.Add(building);
        }
    }

    public void DispatchFireTruck(Building targetBuilding)
    {
        int buildingSize = GetComponent<Building>().BuildingSize;

        // Truck이 출동 시 첫 포지션 잡기기
        // 중심 셀에서 건물 기준 앞 뒤 왼쪽 오른쪽 순서로 경계에서 한 칸 떨어져있는는 셀 중 TileType이 Field 이거나 Road 인 셀을 찾는다.
        int[] dirX = {0, 1, 0, -1};
        int[] dirY = {-1, 0, 1, 0};
        
        List<Vector3Int> cells = Managers.Construct.GridHandler.GetCellsInRange(transform.position, buildingSize);

        bool isFound = false;
        Vector3Int spawnCell = Vector3Int.zero;

        foreach (var cell in cells)
        {
            for (int i = 0; i < 4; i++)
            {
                int nextX = cell.x + dirX[i];
                int nextY = cell.y + dirY[i];

                TileType tileType = Managers.Construct.GridHandler.GetGridTileType(nextX, nextY);
                if (tileType == TileType.Road)
                {
                    isFound = true;
                    spawnCell = new Vector3Int(nextX, nextY, 0);
                    break;
                }
            }

            if (isFound) break;
        }

        if (!isFound) 
        {
            Managers.UI.OpenToastPopup("길이 막혀 소방차가 출동할 수 없습니다.");
        }
        else
        {
            Vector3 spawnPos = Managers.Construct.GridHandler.CellToWorld(spawnCell.x, spawnCell.y);
            FireTruck truck = Managers.Resource.Instantiate("FireTruck").GetComponent<FireTruck>();
            truck.transform.position = spawnPos;

            Vector3Int truckCell = Managers.Construct.GridHandler.WorldToCell(truck.transform.position);

            bool isSuccess = truck.SetDestination(truckCell, targetBuilding);
            if (!isSuccess)
            {
                Managers.Resource.Destroy(truck.gameObject);
            }
        }
    }

    public bool CanProtect(Building building)
    {
        return _handleBuildings.Contains(building);
    }
}
