using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem.Controls;

public class TaxSystem : MonoBehaviour
{
    [SerializeField] SceneUI sceneUI;
    public Text expected_Happiness_Change_Text;

    [Header("Tax")]
    public Text taxText;
    [SerializeField] Button taxRateButton;
    [SerializeField] Button closeButton;
    [SerializeField] Button upButton;
    [SerializeField] Button downButton;
    [SerializeField] GameObject taxRatePanel;

    int taxRate = 5; // 세율 (세금 비율)
    int taxRate_Min = 5;
    int taxRate_Max = 15;

    int tax; // 세금 금액 (인구수 x 세금률) 
    int expected_Happiness_Change;

    private void Start()
    {
        taxRatePanel.SetActive(false);

        // 날짜 이벤트 구독 : 하루가 지날 때마다 적용
        CityManager.Instance.dateSystem.OnDayChanged += OnDayChanged;

        taxRate = Mathf.Clamp(taxRate, taxRate_Min, taxRate_Max);

        taxRateButton.onClick.AddListener(ActivatePanel);
        closeButton.onClick.AddListener(DeActivatePanel);
        upButton.onClick.AddListener(IncreaseTaxRate);
        downButton.onClick.AddListener(DecreaseTaxRate);
    }

    private void OnDisable()
    {
        CityManager.Instance.dateSystem.OnDayChanged -= OnDayChanged;
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
        Managers.Commodity.AddMoney(tax);
        // 텍스트 갱신 함수 호출 (임시)
        sceneUI.UpdateMoneyText();
        // 행복도 변화량 적용
        ApplyHappinessByTaxRate();
    }

    // 하루 세금 금액 계산
    void RecalculateTaxAmount()
    {
        tax = CityManager.Instance.populationSystem.GetCurrentPopulation() * taxRate;
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
        // 실제 행복도 변화 적용
        CityManager.Instance.happinessSystem.ApplyHappinessChange(expected_Happiness_Change);
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

    public void ActivatePanel()
    {
        taxRatePanel.SetActive(true);
    }

    public void DeActivatePanel()
    {
        taxRatePanel.SetActive(false);
    }
}
