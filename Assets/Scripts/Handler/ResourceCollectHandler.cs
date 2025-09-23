using UnityEngine;

public class ResourceCollectHandler : RoleHandler
{
    [SerializeField] private IngredientType _resourceType;

    private float _lastCollectTime = 0f;
    private float _intervalTime = 3f;
    private float _quantity = 4f;

    public IngredientType ResourceType => _resourceType;
    public float Quantity => _quantity;

    public override void HandleEvent(string eventType)
    {
        float now = Time.time;
        if (now - _lastCollectTime >= _intervalTime)
        {
            CollectResource();
            _lastCollectTime = now;
        }
    }

    void Update()
    {
        HandleEvent("ResourceCollect");
    }   

    public override void OnUpgrade(int newLevel)
    {
        base.OnUpgrade(newLevel);
    }

    public override void OnDeBuff()
    {
        if (debuffCount >= MAX_DEBUFF_COUNT)
        {
            return;
        }
        debuffCount++;

        _quantity *= 0.5f;  // 추후에 디버프는 변경될 수 있음음
    }

    public override void OnResolved()
    {
        _quantity = 4f;
    }

    private void CollectResource()
    {
        switch (_resourceType)
        {
            case IngredientType.Wood:
                Managers.Commodity.AddIngredient(IngredientType.Wood, _quantity);
                break;
            case IngredientType.Iron:
                Managers.Commodity.AddIngredient(IngredientType.Iron, _quantity);
                break;
        }

        Managers.UI.AddPanel<UIResourceGather>(this, true);
    }
}
