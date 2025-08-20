using UnityEngine;

public class Managers
{
    private static CommodityManager _commodity = new CommodityManager();

    public static CommodityManager Commodity => _commodity;

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
