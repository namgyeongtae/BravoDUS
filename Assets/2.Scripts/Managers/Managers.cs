using UnityEngine;

public class Managers
{
    private static CDNManager _cdn = new CDNManager();
    private static CommodityManager _commodity = new CommodityManager();
    private static ResourceManager _resource = new ResourceManager();
    private static UIManager _ui = new UIManager();
    private static EventManager _event = new EventManager();
    private static HRManager _hr = new HRManager();
    private static ItemManager _item = new ItemManager();
    private static ConstructManager _construct = new ConstructManager();
    private static SOManager _so = new SOManager();
    private static LevelManager _level = new LevelManager();

    public static CDNManager CDN => _cdn;
    public static CommodityManager Commodity => _commodity;
    public static ResourceManager Resource => _resource;
    public static UIManager UI => _ui;
    public static EventManager Event => _event;
    public static HRManager HR => _hr;
    public static ItemManager Item => _item;
    public static ConstructManager Construct => _construct;
    public static SOManager SO => _so;
    public static LevelManager Level => _level;
    public void Init()
    {
        _cdn.Init();
        _commodity.Init();
        _ui.Init();
        _event.Init();
        _hr.Init();
        _item.Init();
        _construct.Init();
        _so.Init();
        _level.Init();
    }

    public void Update()
    {
        _event.Update();
        _hr.Update();
    }

    public void Release()
    {
        _commodity.Release();
        _event.Release();
    }
}