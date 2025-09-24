using System;
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
    public string Name;
    public float Stamina;
    public JobType JobType;
    public HRState HRState;
    public string Icon;
}

public class WorkForce
{
    private WorkForceData _data;

    private bool _isAssigned;
    private float _stamina;
    private HRState _hrState;

    public bool isAssigned => _isAssigned;
    public JobType JobType => _data.JobType;
    public HRState HRState => _hrState;
    public string Icon => _data.Icon;
    public float Stamina => _stamina;

    public WorkForce(WorkForceData data)
    {
        _data = data;
        _stamina = data.Stamina;
        _hrState = data.HRState;
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

    public void WorkTickUpdate()
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

    public void RestTickUpdate()
    {
        if (_hrState != HRState.Rest)
            return;
        
        if (_stamina < 100)
        {
            _stamina += 2f;
        }
    }

    public bool Assign()
    {
        if (_stamina > 0.1f)
        {
            _hrState = HRState.Work;
            _isAssigned = true;
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
        _isAssigned = false;
        _hrState = HRState.None;
    }
}
