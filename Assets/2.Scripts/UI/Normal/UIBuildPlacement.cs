using UnityEngine;
using UnityEngine.UI;

public class UIBuildPlacement : CanvasPanel
{
    [Bind("ConfirmButton")] private UIButton _confirmButton;
    [Bind("RotateButton")] private UIButton _rotateButton;
    [Bind("CancelButton")] private UIButton _cancelButton;

    [Bind("Name")] private Text _nameText;

    private GameObject _buildingObject;

    protected override void Initialize()
    {
        base.Initialize();

        _confirmButton.BindEvent(OnClickConfirm, ClickType.Up);
        _rotateButton.BindEvent(OnClickRotate, ClickType.Up);
        _cancelButton.BindEvent(OnClickCancel, ClickType.Up);
    }

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }

    public override void SetPanelInfo(object Info)
    {
        base.SetPanelInfo(Info);

        _buildingObject= Info as GameObject;
        _nameText.text = _buildingObject.name;
    }

    private void OnClickConfirm()
    {
        bool isSuccess = Managers.Construct.PlacementSystem.EndPlacement();
        if (!isSuccess)
        {
            return;
        }
        Managers.Construct.SwitchConstructMode(ConstructMode.None);
        Close();
    }

    private void OnClickRotate()
    {
        Managers.Construct.PlacementSystem.RotateBuilding();
    }

    private void OnClickCancel()
    {
        Managers.Construct.PlacementSystem.EndPlacement(isCancel: true);
        Managers.Construct.SwitchConstructMode(ConstructMode.None);
        
        _buildingObject = null;

        Close();
    }
}
