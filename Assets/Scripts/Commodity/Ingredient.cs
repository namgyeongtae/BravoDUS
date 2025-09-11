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
        if (amount > 0)
        {
            Managers.UI.GetUI<SceneUI>("SceneUI").AddCommodity(this, amount);
        }
        else if (amount < 0)
        {
            Managers.UI.GetUI<SceneUI>("SceneUI").SubCommodity(this, amount);
        }
    }
}
