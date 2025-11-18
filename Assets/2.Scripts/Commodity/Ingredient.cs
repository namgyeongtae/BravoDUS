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
    public float Amount { get; private set; }

    public void Gather(float amount) { Amount += amount; }
    public void Consume(float amount) { Amount -= amount; }

    // 🔥 세이브/로드용: 강제 세팅 함수
    public void SetAmount(float amount)
    {
        Amount = Mathf.Max(0, amount);
    }
    private IngredientType _type;

    public IngredientType Type => _type;

    public Ingredient(IngredientType type)
    {
        _type = type;
        _resourceName = type.ToString();
        _amount = 0;
    }

    protected override void OnAmountChanged(float amount, bool isAdd)
    {
        
    }
}
