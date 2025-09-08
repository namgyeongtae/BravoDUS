using UnityEngine;
using System;

public enum IngredientType
{
    None,
    Wood,
    Iron
}

public class Ingredient : BaseResource
{
    private IngredientType _type;

    public IngredientType Type => _type;

    public Ingredient(IngredientType type)
    {
        _type = type;
        _resourceName = type.ToString();
        _amount = 0;
    }

    protected override void OnAmountChanged(float amount)
    {
        // TODO: Apply amount to UI
        // 재화별로 UI 매칭 후 해당 UI에 수치 적용

        // SceneUI.AddAmount(this, amount);
        Debug.Log($"{_resourceName} 개수: {amount}");
        Managers.UI.GetUI<SceneUI>("SceneUI").AddCommodity(this, amount);
    }
}
