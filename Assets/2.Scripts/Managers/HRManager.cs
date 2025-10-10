using System;
using System.Collections.Generic;
using UnityEngine;

public class HRManager : IManagerBase
{
    private List<WorkForce> _holdResources = new(); // 보유중인 인력
    private Dictionary<JobType, List<WorkForce>> _workForceDictionary = new();
    public List<WorkForce> HoldResources => _holdResources;
    public Dictionary<JobType, List<WorkForce>> WorkForceDictionary => _workForceDictionary;

    public void Init()
    {
        // TODO:
        // Load Hold Resources From Json Data

        // TEMP CODE
        /* for (int i = 0; i < 4; i++)
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
        } */

        LoadWorkForceFromDB();
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

    public void HireWorkForce(WorkForce workForce)
    {
        _holdResources.Add(workForce);
    }

    public void AssignWorkForce(Building building, WorkForce workForce)
    {
        if (workForce.Assign(building))
            building.AssignWorkForce(workForce);
    }

    public void UnassignWorkForce(WorkForce workForce)
    {
        workForce.Unassign();
    }

    private void LoadWorkForceFromDB()
    {
        for (int i = 0; i < Enum.GetValues(typeof(JobType)).Length; i++)
        {
            _workForceDictionary.Add((JobType)i, new List<WorkForce>());
        }

        var list = JsonUtils.SerializeList<WorkForceData>("WorkforceDatabase");

        foreach (var wf in list)
        {
            Debug.Log($"wf.JobType: {wf.JobType}, name: {wf.Name}");
            WorkForce workForce = new WorkForce(wf);
            _workForceDictionary[workForce.JobType].Add(workForce);
        }
    }
}
