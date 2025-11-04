using UnityEngine;

public class UIRoadPlacement : CanvasPanel
{
    [Bind("ExitButton")] private UIButton _exitButton;

    protected override void Initialize()
    {
        _exitButton.BindEvent(OnClickExitButton, ClickType.Up);
    }

    private void OnClickExitButton()
    {
        Managers.Construct.SwitchConstructMode(ConstructMode.None);
        Close();
    }
}
