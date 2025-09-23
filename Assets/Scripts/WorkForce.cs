using System;
using UnityEngine;

public enum JobType
{
    None,
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
    public string Name;
    public float Stamina;
    public JobType JobType;
    public HRState HRState;
}

public class WorkForce
{
    private WorkForceData _data;

    private bool _isAssigned;

    public Sprite Icon { get; private set;}

    public bool isAssigned => _isAssigned;
    public JobType JobType => _data.JobType;

    public WorkForce(WorkForceData data)
    {
        _data = data;
    }

    public void WorkTickUpdate()
    {
        if (_data.HRState != HRState.Work)
            return;

        if (_data.Stamina > 0)
        {
            _data.Stamina -= 1f;
        }
        else
        {
            _data.HRState = HRState.Tired;
        }
    }

    public void Assign()
    {
        _isAssigned = true;

        // Managers.HR.Assign(this);
    }

    public void Unassign()
    {
        _isAssigned = false;

        // Managers.HR.Unassign(this);
    }
}
