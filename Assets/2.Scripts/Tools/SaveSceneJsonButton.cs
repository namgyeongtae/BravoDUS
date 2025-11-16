using UnityEngine;

public class SaveSceneJsonButton : CanvasPanel
{
    [SerializeField] private RoadRuntimeLogger _roadRuntimeLogger;
    UIButton _saveButton;

    protected override void Initialize()
    {
        _saveButton = GetComponent<UIButton>();
        _saveButton.BindEvent(OnClickSave, ClickType.Up);
    }

    private void OnClickSave()
    {
        _roadRuntimeLogger.SaveJson();
    }
}
