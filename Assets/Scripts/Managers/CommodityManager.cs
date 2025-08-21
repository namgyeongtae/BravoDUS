using System.Collections.Generic;
using UnityEngine;

public class CommodityManager : IManagerBase
{
    private Dictionary<IngredientType, Ingredient> _ingredients;

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

    public Ingredient GetIngredient(IngredientType type)
    {
        if (_ingredients.TryGetValue(type, out var ingredient))
        {
            return ingredient;
        }
        return null;
    }

    public void AddIngredient(IngredientType type, int amount)
    {
        if (_ingredients.TryGetValue(type, out var ingredient))
        {
            ingredient.Gather(amount);
        }
    }
}
