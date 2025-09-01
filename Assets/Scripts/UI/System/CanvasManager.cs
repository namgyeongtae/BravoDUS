using System.Collections.Generic;
using System.Linq;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    private Dictionary<string, CanvasPanel> panelList = new();
    private Dictionary<string, UIPopupBase> popupList = new();

    private Canvas _canvas;

    private static CanvasManager _instance;
    public static CanvasManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("CanvasManager is not found");
            
            return _instance;
        }
    }

    [SerializeField] private int _baseDepth = 100;
    [SerializeField] private int _gap = 10;
    public RectTransform MainRect;

    protected virtual void Awake()
    {
        MainRect = GetComponent<RectTransform>();
        _canvas = GetComponent<Canvas>();

        CanvasPanel[] panels = GetComponentsInChildren<CanvasPanel>();
        foreach (CanvasPanel panel in panels)
        {
            panelList.Add(panel.name, panel);
        }

        _instance = this;
    }

    public T ShowPopup<T>(string name, object info = null) where T : UIPopupBase
    {
        if (name == null) name = typeof(T).Name;

        /* // 이미 켜져 있으면
        if (popupList.TryGetValue(name, out var popup))
        {

        } */

        GameObject obj = Managers.Resource.Instantiate($"UI/Popup/{name}", transform);
        if (obj == null)
        {
            Debug.LogError($"Failed to load prefab: {name}");
            return null;
        }
        
        obj.name = name;
        UIPopupBase popup = obj.GetComponent<UIPopupBase>();
        popup.Open();

        RectTransform rect = obj.GetComponent<RectTransform>();

        rect.SetParent(transform);

        int depth = _baseDepth + _gap * popupList.Count;

        popup.SetPanelDepth(depth);

        popupList.Add(name, popup);

        if (info != null)
        {
            popup.SetPanelInfo(info);
        }

        rect.localScale = Vector3.one;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        obj.SetActive(true);
        popup.CallAfterSetting();

        return popup as T;
    }
    public void ClosePopup(UIPopupBase popup)
    {
        if (popupList.Count == 0)
        {
            return;
        }

        popup?.Close();
    }
    public void ClosePopup(string popupName)
    {
        if (popupList.Count == 0)
            return;

        if (popupList.TryGetValue(popupName, out var popup))
        {
            popup?.Close();
        }
    }
    public void CloseAllPopup()
    {
        List<UIPopupBase> popups = new List<UIPopupBase>(popupList.Values);
        foreach (UIPopupBase popup in popups)
        {
            popup?.Close();
        }
        popupList.Clear();
    }

    public T AddPanel<T>(string name = null, object info = null) where T : CanvasPanel
    {
        if (name == null) name = typeof(T).Name;

        // 이미 켜져 있으면
        if (panelList.TryGetValue(name, out var panel))
        {
            return null;
        }

        GameObject obj = Managers.Resource.Instantiate($"UI/{name}", transform);
        if (obj == null)
        {
            Debug.LogError($"Failed to load prefab: {name}");
            return null;
        }
        
        obj.name = name;
        CanvasPanel canvasPanel = obj.GetComponent<CanvasPanel>();
        canvasPanel.Open();

        RectTransform rect = obj.GetComponent<RectTransform>();

        rect.SetParent(transform);

        int depth = _baseDepth + _gap * panelList.Count;

        canvasPanel.SetPanelDepth(depth);

        panelList.Add(name, canvasPanel);

        if (info != null)
        {
            canvasPanel.SetPanelInfo(info);
        }

        rect.localScale = Vector3.one;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        obj.SetActive(true);
        canvasPanel.CallAfterSetting();

        return canvasPanel as T;
    }
    public void RemovePanel(CanvasPanel obj)
    {
        if (panelList.Count == 0)
        {
            return;
        }

        obj?.Close();
    }
    public void RemovePanel(string panelObj)
    {
        if (panelList.Count == 0)
            return;

        if (panelList.TryGetValue(panelObj, out var data))
        {
            data?.Close();
        }
    }
    public void RemoveAllPanel()
    {
        List<CanvasPanel> panels = new List<CanvasPanel>(panelList.Values);
        foreach (CanvasPanel panel in panels)
        {
            panel.Close();
        }
        panelList.Clear();
    }

    public void ReleaseUI(UIBind uibase)
    {
        if (uibase == null)
            return;

        var data = panelList.FirstOrDefault(x => x.Value.Equals(uibase));
        
        if (data.Key == null)
        {
            Debug.LogWarning($"Failed to find panel : {uibase.name}");
            return;
        }

        panelList.Remove(data.Key);

        foreach (var panel in panelList)
        {
            if (panel.Value.CanvasPanelDepth > data.Value.CanvasPanelDepth)
                panel.Value.SetPanelDepth(panel.Value.CanvasPanelDepth - _gap);
        }

        Managers.Resource.Destroy(uibase.gameObject);
    }

    public void ReleasePopup(UIPopupBase popup)
    {
        if (popup == null)
            return;

        var data = popupList.FirstOrDefault(x => x.Value.Equals(popup));
        
        if (data.Key == null)
        {
            Debug.LogWarning($"Failed to find popup : {popup.name}");
            return;
        }

        popupList.Remove(data.Key);

        foreach (var p in popupList)
        {
            if (p.Value.CanvasPanelDepth > data.Value.CanvasPanelDepth)
                p.Value.SetPanelDepth(p.Value.CanvasPanelDepth - _gap);
        }

        Managers.Resource.Destroy(popup.gameObject);
    }

    public T GetPanel<T>(string name = null) where T : CanvasPanel
    {
        if (name == null) name = typeof(T).Name;

        if (panelList.TryGetValue(name, out var panel))
            return panel.GetComponent<T>();

        return null;
    }

    public CanvasPanel GetPanel(string name)
    {
        if (panelList.TryGetValue(name, out var panel))
            return panel.GetComponent<CanvasPanel>();
        
        return null;
    }

    public T GetPopup<T>(string name) where T : UIPopupBase
    {
        if (name == null) name = typeof(T).Name;

        if (popupList.TryGetValue(name, out var popup))
            return popup.GetComponent<T>();

        return null;
    }

    public UIPopupBase GetPopup(string name)
    {
        if (popupList.TryGetValue(name, out var popup))
            return popup.GetComponent<UIPopupBase>();

        return null;
    }
}
