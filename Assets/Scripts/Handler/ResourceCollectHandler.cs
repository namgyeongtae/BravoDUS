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

            var resourceInfo = Managers.Commodity.GetIngredient(_resourceType);
            Debug.Log($"{_resourceType} 개수: {resourceInfo.Amount}");
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
