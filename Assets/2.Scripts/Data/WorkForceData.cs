using System;
using System.Collections.Generic;
using UnityEngine;

public enum JobType
{
    None = -1,
    WoodWorker,
    IronWorker,
    Doctor,
    PoliceOfficer,
    FireFighter
}

public enum HRState
{
    None,
    Work,
    Rest,
    Tired,
    Recovering,
    Injured
}

[Serializable]
public class WorkForceData
{
    public int Id;
    public string Name;
    public float Stamina;
    public string JobType;
    public string HRState;
    public string Icon;
}

[Serializable]
public class WorkForceDataWrapper : List<WorkForceData>
{
    
}
