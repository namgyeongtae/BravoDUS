using UnityEngine;

public class Managers
{
    private static CommodityManager _commodity = new CommodityManager();
    private static ResourceManager _resource = new ResourceManager();
    private static UIManager _ui = new UIManager();
    private static EventManager _event = new EventManager();
    private static HRManager _hr = new HRManager();
    private static ItemManager _item = new ItemManager();
    // ����: _game new ���� (�� ��ġ GameManager.Instance ���)

    public static CommodityManager Commodity => _commodity;
    public static ResourceManager Resource => _resource;
    public static UIManager UI => _ui;
    public static EventManager Event => _event;
    public static HRManager HR => _hr;
    public static ItemManager Item => _item;
    public static GameManager Game => GameManager.Instance; // ����: Instance ����

    public void Init()
    {
        _commodity.Init();
        _ui.Init();
        _event.Init();
        _hr.Init();
        _item.Init();
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