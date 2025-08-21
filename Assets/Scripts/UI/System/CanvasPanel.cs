using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
public class CanvasPanel : UIBind
{
    public int CanvasPanelDepth { get; private set; } = 0;
    private Canvas _canvasComponent;

    protected override void Start()
    {
        base.Start();
        Initialize();
    }

    protected void SetCanvasComponent()
    {
        if (_canvasComponent == null)
        {
            _canvasComponent = GetComponent<Canvas>();
        }
    }

    public virtual void SetPanelInfo(object Info) { }

    public void SetPanelDepth(int depth)
    {
        SetCanvasComponent();
        if (_canvasComponent == null)
        {
            Debug.LogError("CanvasComponent is null");
            return;
        }

        _canvasComponent.overrideSorting = true;
        _canvasComponent.sortingOrder = depth;
        CanvasPanelDepth = depth;
    }

    public virtual void CallAfterSetting() {}
}
