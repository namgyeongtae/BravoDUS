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
        _amount = 0f; // 초기 0 유지
    }

    public void Gather(float amount) // 추가: 자원 수집 메서드
    {
        float oldAmount = _amount;
        _amount += amount;
        OnAmountChanged(_amount - oldAmount); // 변경 delta 전달
        Debug.Log($"Gather called for {Type}: +{amount}, new Amount: {_amount}"); // 추가: Gather 추적 로그
    }

    public void Consume(float amount) // 추가: 자원 소모 메서드
    {
        float oldAmount = _amount;
        _amount -= amount;
        if (_amount < 0) _amount = 0f;
        OnAmountChanged(_amount - oldAmount); // 변경 delta 전달
        Debug.Log($"Consume called for {Type}: -{amount}, new Amount: {_amount}"); // 추가: Consume 추적 로그
    }

    protected override void OnAmountChanged(float delta) // 수정: delta로 변경 (증감량)
    {
        // TODO: Apply amount to UI
        // 재화별로 UI 매칭 후 해당 UI에 수치 적용
        // SceneUI.AddAmount(this, delta);
        Debug.Log($"{_resourceName} 개수 변경: {delta} (총 {_amount})"); // 수정: delta와 총량 로그
        // Managers.UI.GetUI<SceneUI>("SceneUI").AddCommodity(this, delta);
    }
}