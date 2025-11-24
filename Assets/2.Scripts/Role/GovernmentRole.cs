using System.Collections.Generic;
using UnityEngine;

public class GovernmentRole : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitTiles();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitTiles()
    {
        Building building = GetComponent<Building>();
        building.SetCurrentState(Building.State.Base);

        List<Vector3Int> cells = Managers.Construct.GridHandler.GetCellsInRange(transform.position, building.BuildingSize);

        foreach (var cell in cells)
            Managers.Construct.GridHandler.SetGridTileType(cell.x, cell.y, TileType.Constructed);
    }
}
