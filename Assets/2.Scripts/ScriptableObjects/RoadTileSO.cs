using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum RoadType
{
    Dirt,
    Brick,
    Concrete
}

[Serializable]
public class RoadTileData
{
    public RoadType RoadType;
    public List<TileBase> RoadTiles;
}

[CreateAssetMenu(fileName = "RoadTileSO", menuName = "ScriptableObjects/RoadTileSO")]
public class RoadTileSO : ScriptableObject
{
    public List<RoadTileData> RoadTileDatas;    
}
