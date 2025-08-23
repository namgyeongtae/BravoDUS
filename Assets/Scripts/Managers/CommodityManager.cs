using System.Collections.Generic;
using UnityEngine;

public class CommodityManager : IManagerBase
{
    private Dictionary<IngredientType, Ingredient> _ingredients;

    public Ingredient GetIngredient(IngredientType type) => _ingredients[type];

    public void Init()
    {
        _ingredients = new Dictionary<IngredientType, Ingredient>();

        LoadIngredientFromDB();
    }

    public void Release()
    {
        _ingredients.Clear();
    }

    private void LoadIngredientFromDB()
    {
        // TODO: Load ingredients from DB (json, SO ....)
    }

    public void AddIngredient(IngredientType type, float amount)
    {
        if (_ingredients.TryGetValue(type, out var ingredient))
        {
            ingredient.Gather(amount);
        }
    }
}
