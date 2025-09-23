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

    private UIButton _slotButton;

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
                    break;
                case WorkForceSlotState.Unassigned:
                    _icon.sprite = _stateSprites[(int)WorkForceSlotState.Unassigned]; // TODO 빈 아이콘 설정
                    break;
            } 
        } 
    }

    public override void Open()
    {
        base.Open();
        _slotButton = GetComponent<UIButton>();

        if (_state == WorkForceSlotState.Locked)
            _slotButton.interactable = false;
        else
            _slotButton.interactable = true;

        _slotButton.BindEvent(OnClickSlotButton, ClickType.Up);
    }

    public void SetSlot(WorkForce workForce)
    {
        /* if (workForce == null)
        {
            // TODO
            // icon은 잠금 아이콘 혹은 추가 버튼 아이콘으로 표시시
            _icon.sprite = null;
            return;
        } */

        // TODO
        // _icon.sprite = workForce.Icon;
    }

    private void OnClickSlotButton()
    {
        Debug.Log("OnClickSlotButton");
    }
}
