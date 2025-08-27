using UnityEngine;

public class Managers
{
    private static CommodityManager _commodity = new CommodityManager();
    private static ResourceManager _resource = new ResourceManager();
    private static UIManager _ui = new UIManager();
    private static EventManager _event = new EventManager();
    // 수정: _game new 제거 (씬 배치 GameManager.Instance 사용)

    public static CommodityManager Commodity => _commodity;
    public static ResourceManager Resource => _resource;
    public static UIManager UI => _ui;
    public static EventManager Event => _event;
    public static GameManager Game => GameManager.Instance; // 수정: Instance 접근

    public void Init()
    {
        _commodity.Init();
        _ui.Init();
        _event.Init();
        // 수정: GameManager.Instance.Init() 호출 (new 안 함)
        GameManager.Instance.Init();
    }

    public void Update()
    {
        _event.Update();
        // 수정: GameManager.Instance.Update() 호출
        GameManager.Instance.Update();
    }

    public void Release()
    {
        _commodity.Release();
        _event.Release();
        // 수정: GameManager.Instance.Release() 호출
        GameManager.Instance.Release();
    }
}