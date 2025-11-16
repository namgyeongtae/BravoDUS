using UnityEngine;

public class UIManager : IManagerBase
{
    public RectTransform BaseCanvas => CanvasManager.Instance.MainRect;

    public void Init()
    {
        
    }

    public void OpenToastPopup(string message)
    {
        var toast = AddPanel<UIToastPopup>();
        toast.SettingPopup(message);
    }

    public T GetUI<T>(string name = null) where T : CanvasPanel
    {
        if (name == null) name = typeof(T).Name;

        return CanvasManager.Instance.GetPanel<T>(name);
    }

    public T GetPopup<T>(string name) where T : UIPopupBase
    {
        return CanvasManager.Instance.GetPopup<T>(name);
    }

    public CanvasPanel GetUI(string name)
    {
        return CanvasManager.Instance.GetPanel(name);
    }

    public UIPopupBase GetPopup(string name)
    {
        return CanvasManager.Instance.GetPopup(name);
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

    public T AddPanel<T>(object param = null, bool isStackable = false) where T : CanvasPanel
    {
        string name = typeof(T).Name;
        T panel = CanvasManager.Instance?.AddPanel<T>(name, param, isStackable);
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

    public T ShowPopup<T>(string name = null, object param = null) where T : UIPopupBase
    {
        if (name == null) name = typeof(T).Name;

        T popup = CanvasManager.Instance?.ShowPopup<T>(name, param);
        if (popup == null)
        {
            Debug.LogError($"Failed to show popup : {name}");
            return null;
        }

        return popup;
    }
    public void ClosePopup(string popupName)
    {
        CanvasManager.Instance.ClosePopup(popupName);
    }
    public void ClosePopup(UIPopupBase popup)
    {
        CanvasManager.Instance.ClosePopup(popup);
    }
}
