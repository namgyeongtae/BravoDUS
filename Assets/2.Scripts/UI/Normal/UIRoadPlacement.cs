using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRoadPlacement : CanvasPanel
{
    [SerializeField] private List<Sprite> _installModeSprites;

    [Bind("ExitButton")] private UIButton _exitButton;
    [Bind("SwitchModeButton")] private UIButton _switchModeButton;

    protected override void Initialize()
    {
        _exitButton.BindEvent(OnClickExitButton, ClickType.Up);
        _switchModeButton.BindEvent(OnClickSwitchModeButton, ClickType.Up);
    }

    private void OnClickSwitchModeButton()
    {
        RoadMode mode = Managers.Construct.RoadSystem.SwitchInstallMode();
        _switchModeButton.GetComponent<Image>().sprite = _installModeSprites[(int)mode];
    }

    private void OnClickExitButton()
    {
        Managers.Construct.SwitchConstructMode(ConstructMode.None);
        Close();
    }
}
