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
    public TileBase UpTile;
    public TileBase RightTile;
    public TileBase DownTile;
    public TileBase LeftTile;
    public TileBase UpRightTile;
    public TileBase UpLeftTile;
    public TileBase DownRightTile;
    public TileBase DownLeftTile;
    public TileBase UpRightLeftTile;
    public TileBase DownRightLeftTile;
    public TileBase RightLeftTile;
    public TileBase UpDownTile;
    public TileBase RightUpDownTile;
    public TileBase LeftUpDownTile;
    public TileBase LeftRightUpDownTile;
    public TileBase CenterTile;
}

[CreateAssetMenu(fileName = "RoadTileSO", menuName = "ScriptableObjects/RoadTileSO")]
public class RoadTileSO : ScriptableObject
{
    public List<RoadTileData> RoadTileDatas;    
}
