using System.Collections.Generic;
using System.Linq;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    private Dictionary<string, CanvasPanel> panelList = new();

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

    public T AddPanel<T>(string name = null, object info = null) where T : CanvasPanel
    {
        if (name == null) name = typeof(T).Name;

        // 이미 켜져 있으면
        if (panelList.TryGetValue(name, out var panel))
        {
            return null;
        }

        GameObject obj = Managers.Resource.Instantiate(name, transform);
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
}
