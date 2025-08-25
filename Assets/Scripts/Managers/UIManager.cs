using UnityEngine;

public class UIManager : IManagerBase
{
    public RectTransform BaseCanvas => CanvasManager.Instance.MainRect;

    public void Init()
    {

    }

    public T GetUI<T>(string name) where T : CanvasPanel
    {
        return CanvasManager.Instance.GetPanel<T>(name);
    }

    public CanvasPanel GetUI(string name)
    {
        return CanvasManager.Instance.GetPanel(name);
    }

    public T AddPanel<T>(string name, object param = null) where T : CanvasPanel
    {
        if (name == null) name = typeof(T).Name;

        T panel = CanvasManager.Instance?.AddPanel<T>(name, param);
        if (panel == null)
        {
            Debug.LogError($"Failed to add panel : {name}");
            return null;
        }

        return panel;
    }

    public T AddPanel<T>(object param = null) where T : CanvasPanel
    {
        string name = typeof(T).Name;
        T panel = CanvasManager.Instance?.AddPanel<T>(name, param);
        if (panel == null)
        {
            Debug.LogError($"Failed to add panel : {name}");
            return null;
        }

        return panel;
    }

    public void RemovePanel(CanvasPanel obj)
    {
        CanvasManager.Instance.RemovePanel(obj);
    }

    public void RemovePanel(string panelObj)
    {
        CanvasManager.Instance.RemovePanel(panelObj);
    }

    public void RemoveAllPanel()
    {
        CanvasManager.Instance.RemoveAllPanel();
    }
}
