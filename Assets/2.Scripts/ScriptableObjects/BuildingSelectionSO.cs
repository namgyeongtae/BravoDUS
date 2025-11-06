using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum BuildingType
{
    None,
    Government,
    Hospital,
    PoliceStation,
    FireStation,
    ConvenienceStore,
    ResourceCollector
}

public enum BuildingActionType
{
    None,
    Info,
    Upgrade,
    Destroy,
    HumanResource,
    PatientManage,
    Hire
}

[Serializable]
public class BuildingAction
{
    public BuildingActionType actionType;
    public string actionName;
    public Sprite icon;
}

[System.Serializable]
public class BuildingActionSet
{
    public BuildingType buildingType;
    public List<BuildingAction> availableActions;
}

[CreateAssetMenu(fileName = "BuildingSelectionSO", menuName = "ScriptableObjects/BuildingSelectionSO")]
public class BuildingSelectionSO : ScriptableObject
{
    public List<BuildingActionSet> actionSets;
}
