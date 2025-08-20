using UnityEngine;

public enum IngredientType
{
    None,
    Wood,
    Iron
}

public class Ingredient : BaseResource
{
    public Ingredient(IngredientType type)
    {
        _resourceName = type.ToString();
        _amount = 0;
    }

    protected override void OnAmountChanged()
    {
        // TODO: Apply amount to UI
        // 재화별로 UI 매칭 후 해당 UI에 수치 적용
    }
}
