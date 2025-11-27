using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem.Controls;

public class TaxSystem : MonoBehaviour
{
    public Text taxText;
    public Text expected_Happiness_Change_Text;

    int taxRate = 5; // 세율 (세금 비율)
    int taxRate_Min = 5;
    int taxRate_Max = 15;

    int tax; // 세금 금액 (인구수 x 세금률) 
    int expected_Happiness_Change;

    DateSystem dateSystem;
    PopulationSystem populationSystem;
    HappinessSystem happinessSystem;

    private void Start()
    {
        dateSystem = CityManager.Instance.dateSystem;
        populationSystem = CityManager.Instance.populationSystem;
        happinessSystem = CityManager.Instance.happinessSystem;

        // 날짜 이벤트 구독 : 하루가 지날 때마다 적용
        if (dateSystem != null)
            dateSystem.OnDayChanged += OnDayChanged;

        taxRate = Mathf.Clamp(taxRate, taxRate_Min, taxRate_Max);
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
        Update_Expected_Happiness_Change();
    }

    void OnDayChanged()
    {
        // 세금 징수
        // moneySystem.CollectTax(tax);
        // 행복도 변화량 적용
        ApplyHappinessByTaxRate();
    }

    // 하루 세금 금액 계산
    void RecalculateTaxAmount()
    {
        tax = populationSystem.GetCurrentPopulation() * taxRate;
    }

    void Update_Expected_Happiness_Change()
    {
        expected_Happiness_Change =
            taxRate == 5 ? 2 :   // 5%  : 세금 거의 없음 -> 시민 기쁨
            taxRate == 10 ? 0 :  // 10% : 적정선 -> 변화 없음
            taxRate == 15 ? -2 : // 15% : 부담 느낌
                                0;
    }

    // 현재 세율 구간에 따른 행복도 변화량 적용
    void ApplyHappinessByTaxRate()
    {
        // 행복도 변화량
        //int delta =
        //    taxRate < 5 ? 2 :   // 0 ~ 4%   : 세금 거의 없음 -> 시민 기쁨
        //    taxRate < 10 ? 0 :  // 5 ~ 9%   : 적정선 -> 변화 없음
        //    taxRate < 15 ? -1 : // 10 ~ 14% : 약간 부담 느낌
        //    taxRate < 20 ? -3 : // 15 ~ 19% : 부담이 커짐
        //                    -5; // 20% 이상 : 세금 폭탄 느낌 -> 불만 크게 증가
        
        // 실제 행복도 변화 적용
        happinessSystem.ApplyHappinessChange(expected_Happiness_Change);
    }

    void UpdateText()
    {
        taxText.text = "예상 세금 : " + tax;
        ;
        expected_Happiness_Change_Text.text = "예상 행복도 변화량 : " + expected_Happiness_Change;
    }

    public int GetTaxRate()
    {
        return taxRate;
    }

    public void IncreaseTaxRate()
    {
        taxRate += 5;
        taxRate = Mathf.Clamp(taxRate, taxRate_Min, taxRate_Max);
    }

    public void DecreaseTaxRate()
    {
        taxRate -= 5;
        taxRate = Mathf.Clamp(taxRate, taxRate_Min, taxRate_Max);
    }
}
