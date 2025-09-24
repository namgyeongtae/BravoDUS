using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum WorkForceSlotState
{
    Locked,
    Unassigned,
    Assigned
}

public class UIWorkForceSlot : UIBind
{
    [SerializeField] private Sprite[] _stateSprites;    // 나중에는 Resources.Load를 하든 Addressable을 사용하든 해야할듯?
    [Bind("Icon")] private Image _icon;
    [Bind("Stamina")] private Image _stamina;

    private UIButton _slotButton;
    private UIWorkForce _parentUIWorkForce;
    private WorkForce _assignedWorkForce;

    private WorkForceSlotState _state = WorkForceSlotState.Locked;

    public WorkForceSlotState State 
    { 
        get { return _state; } 
        set 
        { 
            _state = value;
            
            switch (_state)
            {
                case WorkForceSlotState.Locked:
                    _icon.sprite = _stateSprites[(int)WorkForceSlotState.Locked]; // TODO 잠금 아이콘 설정
                    _stamina.gameObject.SetActive(false);
                    break;
                case WorkForceSlotState.Unassigned:
                    _icon.sprite = _stateSprites[(int)WorkForceSlotState.Unassigned]; // TODO 빈 아이콘 설정
                    _stamina.gameObject.SetActive(false);
                    break;
            } 
        } 
    }

    public override void Open()
    {
        base.Open();
        _slotButton = GetComponent<UIButton>();
        _parentUIWorkForce = GetComponentInParent<UIWorkForce>();

        if (_state == WorkForceSlotState.Locked)
            _slotButton.interactable = false;
        else
            _slotButton.interactable = true;

        _slotButton.BindEvent(OnClickSlotButton, ClickType.Up);
    }

    void Update()
    {
        if (_state == WorkForceSlotState.Assigned)
            _stamina.fillAmount = _assignedWorkForce.Stamina / 100f;
    }

    public void SetSlot(WorkForce workForce)
    {
        if (workForce == null)
        {
            _icon.sprite = null;
            _assignedWorkForce = null;
            return;
        }

        // TODO
        // _icon.sprite = workForce.Icon;
        var subName = workForce.Icon.Split('/').Last();
        _icon.sprite = AtlasController.GetSprite(workForce.Icon, subName + $"_{(int)workForce.JobType}");

        _assignedWorkForce = workForce;
        
        _stamina.gameObject.SetActive(true);
        _stamina.fillAmount = workForce.Stamina / 100f;
        _state = WorkForceSlotState.Assigned;
    }

    private void OnClickSlotButton()
    {
        if (State == WorkForceSlotState.Assigned)
            return;

        var jobType = GetJobTypeByBuilding();
        var workForce = Managers.HR.HoldResources.Find(x => x.JobType == jobType && x.HRState == HRState.None);
        if (workForce != null)
        {
            Managers.HR.AssignWorkForce(_parentUIWorkForce.Building, workForce);
            State = WorkForceSlotState.Assigned;
            SetSlot(workForce); 
        }
    }

    private JobType GetJobTypeByBuilding()
    {
        var buildingType = _parentUIWorkForce.Building.BuildingType;
        switch (buildingType)
        {
            case BuildingType.ResourceCollector:
                return JobType.WoodWorker;
            case BuildingType.Hospital:
                return JobType.Doctor;
            case BuildingType.PoliceStation:
                return JobType.PoliceOfficer;
            case BuildingType.FireStation:
                return JobType.FireFighter;
        }

        return JobType.None;
    }
}
