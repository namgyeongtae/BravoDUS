using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingData
{
    public string buildingName;
    public Sprite buildingIcon;
    public BuildingType buildingType;
    public GameObject buildingPrefab;
}

[CreateAssetMenu(fileName = "BuildingSO", menuName = "ScriptableObjects/BuildingSO")]
public class BuildingSO : ScriptableObject
{
    public List<BuildingData> buildingDatas;
}
