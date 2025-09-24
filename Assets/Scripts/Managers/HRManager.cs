using System.Collections.Generic;
using UnityEngine;

public class HRManager : IManagerBase
{
    private List<WorkForce> _holdResources = new(); // 보유중인 인력

    public List<WorkForce> HoldResources => _holdResources;

    public void Init()
    {
        // TODO:
        // Load Hold Resources From Json Data

        // TEMP CODE
        for (int i = 0; i < 4; i++)
        {
            WorkForce workForce = new WorkForce(new WorkForceData()
            {
                Name = "John Doe",
                JobType = JobType.WoodWorker,
                HRState = HRState.None,
                Icon = "Atlas/PersonIcon",
                Stamina = 100f,
            });
            _holdResources.Add(workForce);
        }
    }

    public void Update()
    {
        foreach (var workForce in _holdResources)
        {
            workForce.Update();
        }
    }

    public void Release()
    {

    }

    public void AssignWorkForce(Building building, WorkForce workForce)
    {
        if (workForce.Assign())
            building.AssignWorkForce(workForce);
    }
}
