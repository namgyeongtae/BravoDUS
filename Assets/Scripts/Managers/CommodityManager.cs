using System.Collections.Generic;
using UnityEngine;

public class CommodityManager : IManagerBase
{
    private Dictionary<IngredientType, Ingredient> _ingredients = new Dictionary<IngredientType, Ingredient>();

    public Ingredient GetIngredient(IngredientType type) => _ingredients.TryGetValue(type, out var ingredient) ? ingredient : null;

    public void Init()
    {
        LoadIngredientFromDB();
    }

    public void Release()
    {
        _ingredients.Clear();
    }

    private void LoadIngredientFromDB()
    {
        // TODO: Load ingredients from DB (json, SO ....)
        // 예시 초기화 (placeholder)
        _ingredients.Add(IngredientType.Wood, new Ingredient(IngredientType.Wood));
        _ingredients.Add(IngredientType.Iron, new Ingredient(IngredientType.Iron));
        // 초기 Amount 0 설정 + OnAmountChanged 호출 (Gather로 통일)
        _ingredients[IngredientType.Wood].Gather(0f);
        _ingredients[IngredientType.Iron].Gather(0f);
    }

    public void AddIngredient(IngredientType type, float amount)
    {
        if (_ingredients.TryGetValue(type, out var ingredient))
        {
            ingredient.Gather(amount);
        }
        else
        {
            Debug.LogWarning($"Ingredient {type} not found");
        }
    }
}