using UnityEngine;

public class Managers
{
    private static CommodityManager _commodity = new CommodityManager();
    private static ResourceManager _resource = new ResourceManager();
    private static UIManager _ui = new UIManager();

    public static CommodityManager Commodity => _commodity;
    public static ResourceManager Resource => _resource;
    public static UIManager UI => _ui;
    public void Init()
    {
        _commodity.Init();
    }

    public void Update()
    {
        
    }

    public void Release()
    {
        _commodity.Release();
    }
}
