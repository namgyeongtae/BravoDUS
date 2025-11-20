using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkForce
{
    private WorkForceData _data;

    private int _id;
    private string _name;
    private float _stamina;
    private JobType _jobType;
    private HRState _hrState;
    private string _icon;
    
    private bool _isAssigned;
    private Building _assignedBuilding;

    public string Name => _data.Name;
    public bool isAssigned => _isAssigned;
    public JobType JobType => _jobType;
    public HRState HRState => _hrState;
    public string Icon => _data.Icon;
    public float Stamina => _stamina;
    public WorkForce(WorkForceData data)
    {
        _data = data;
        _id = data.Id;
        _name = data.Name;
        _stamina = data.Stamina;
        _jobType = Enum.TryParse<JobType>(data.JobType, out var jobType) ? jobType : JobType.None;
        _hrState = Enum.TryParse<HRState>(data.HRState, out var hrState) ? hrState : HRState.None;
        _icon = data.Icon;
    }

    public void Update()
    {
        switch (_hrState)
        {
            case HRState.Work:
                WorkTickUpdate();
                break;
            case HRState.Rest:
                RestTickUpdate();
                break;
        }
    }

    float _timer = 0f;
    float _intervalTime = 3f;

    private void WorkTickUpdate()
    {
        if (_hrState != HRState.Work)
                return;

        _timer += Time.deltaTime;
        if (_timer >= _intervalTime)
        {
            _timer = 0f;

            if (_stamina > 0)
            {
                _stamina -= 2f;
            }
            else
            {
                _hrState = HRState.Tired;
            }
        }
        
    }

    private void RestTickUpdate()
    {
        if (_hrState != HRState.Rest)
            return;
        
        if (_stamina < 100)
        {
            _stamina += 2f;
        }
    }

    private void InjuredTickUpdate()
    {
        
    }

    public void OnInjured()
    {
        _hrState = HRState.Injured;

        var workForceUI = Managers.UI.GetUI<UIWorkForce>();

        if (workForceUI != null)
        {
            var slot = workForceUI.GetComponentsInChildren<UIWorkForceSlot>().First(x => x.WorkForce == this);
            if (slot != null) slot.DisplayWarning();
        }
    }
    public void OnHealed()
    {
        _hrState = HRState.None;
        _stamina = 100f;
    }
    public bool Assign(Building building)
    {
        if (_stamina > 0.1f)
        {
            _hrState = HRState.Work;
            _isAssigned = true;
            _assignedBuilding = building;
        }
        else
        {
            var toast = Managers.UI.AddPanel<UIToastPopup>();
            toast.SettingPopup("스태미나가 부족해서 일을 할 수 없습니다.");

            _isAssigned = false;
        }

        return _isAssigned;
    }

    public void Unassign()
    {
        if (_isAssigned == false)
            return;

        _isAssigned = false;
        _hrState = HRState.None;
        _assignedBuilding.UnassignWorkForce(this);
        _assignedBuilding = null;
    }

    public void SetHRState(HRState state)
    {
        _hrState = state;
    }
}
