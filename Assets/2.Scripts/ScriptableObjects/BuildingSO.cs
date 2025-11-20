using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingData
{
    public string buildingName;
    public Sprite buildingIcon;
    public BuildingType buildingType;
    public GameObject buildingPrefab;
    public int Happiness;
    public int Population;
    public int buildingSize;
}

[CreateAssetMenu(fileName = "BuildingSO", menuName = "ScriptableObjects/BuildingSO")]
public class BuildingSO : ScriptableObject
{
    public List<BuildingData> buildingDatas;
}
