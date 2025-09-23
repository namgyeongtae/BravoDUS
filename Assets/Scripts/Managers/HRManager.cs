using System.Collections.Generic;
using UnityEngine;

public class HRManager : IManagerBase
{
    private List<WorkForce> _holdResources = new(); // 보유중인 인력
    private List<WorkForce> _workResources = new(); // 일하고 있는 인력

    public List<WorkForce> HoldResources => _holdResources;
    public List<WorkForce> WorkResources => _workResources;

    public void Init()
    {
        // TODO:
        // Load Hold Resources From Json Data
    }

    public void Update()
    {
        foreach (var workForce in _workResources)
        {
            workForce.WorkTickUpdate();
        }
    }

    public void Release()
    {

    }
}
