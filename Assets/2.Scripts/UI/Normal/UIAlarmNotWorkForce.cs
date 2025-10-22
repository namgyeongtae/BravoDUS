using UnityEngine;

public class UIAlarmNotWorkForce : CanvasPanel
{
    [Bind("AssignButton")] private UIButton _assignButton;

    private Building _building;

    protected override void Initialize()
    {
        _assignButton.BindEvent(OnClickAssignButton, ClickType.Up);
    }

    public override void SetPanelInfo(object Info)
    {
        _building = Info as Building;
    }

    public override void Close()
    {
        Managers.Resource.Destroy(this.gameObject);
    }

    void Update()
    {
        if (_building != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(_building.transform.position + Vector3.up * 15f);

            Rect.position = screenPos;
        }
    }

    private void OnClickAssignButton()
    {
        Managers.UI.AddPanel<UIWorkForce>(_building);
    }
}
