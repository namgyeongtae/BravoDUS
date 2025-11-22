using UnityEngine;
using TMPro;
using System;

public class TaxSystem : MonoBehaviour
{
    public TextMeshProUGUI taxRateText;

    int taxAmount; // 하루 세금 금액 // 인구수 x 세금 단가 x 세금률 
    int taxRate = 10; // 세율 (세금 비율)
    int taxUnit = 10; // // 세금 단가 : 10 (임시)

    MoneySystem moneySystem;
    DateSystem dateSystem;
    PopulationSystem populationSystem;
    HappinessSystem happinessSystem;

    private void Awake()
    {
        moneySystem = FindFirstObjectByType<MoneySystem>();
        dateSystem = FindFirstObjectByType<DateSystem>();
        populationSystem = FindFirstObjectByType<PopulationSystem>();
        happinessSystem = FindFirstObjectByType<HappinessSystem>();
    }

    private void OnEnable()
    {
        // 날짜 이벤트 구독 : 하루가 지날 때마다 적용
        if (dateSystem != null)
            dateSystem.OnDayChanged += OnDayChanged;
    }

    private void OnDisable()
    {
        if (dateSystem != null)
            dateSystem.OnDayChanged -= OnDayChanged;
    }

    void Update()
    {
        RecalculateTaxAmount();
        UpdateText();
    }

    void OnDayChanged()
    {
        // 세금 징수
        moneySystem.CollectTax(taxAmount);
        // 행복도 변화량 적용
        ApplyHappinessByTaxRate();
    }

    // 하루 세금 금액 계산
    void RecalculateTaxAmount()
    {
        // RoundToInt : 소수점 첫번째 자리에서 반올림
        taxAmount = Mathf.RoundToInt(populationSystem.GetCurrentPopulation() * taxUnit * (float)taxRate / 100);
    }

    // 현재 세율 구간에 따른 행복도 변화량 적용
    void ApplyHappinessByTaxRate()
    {
        // 행복도 변화량
        int delta =
            taxRate < 5 ? 2 :   // 0 ~ 4%   : 세금 거의 없음 -> 시민 기쁨
            taxRate < 10 ? 0 :  // 5 ~ 9%   : 적정선 -> 변화 없음
            taxRate < 15 ? -1 : // 10 ~ 14% : 약간 부담 느낌
            taxRate < 20 ? -3 : // 15 ~ 19% : 부담이 커짐
                            -5; // 20% 이상 : 세금 폭탄 느낌 -> 불만 크게 증가

        // 실제 행복도 변화 적용
        happinessSystem.ApplyHappinessChange(delta);
    }

    void UpdateText()
    {
        taxRateText.text = taxRate.ToString() + "%";
    }
}
