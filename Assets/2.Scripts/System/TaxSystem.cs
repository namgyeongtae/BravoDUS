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

<<<<<<< Updated upstream:Assets/2.Scripts/System/TaxSystem.cs
=======
    MoneySystem moneySystem;
    DateSystem dateSystem;
    PopulationSystem populationSystem;
    HappinessSystem happinessSystem;

    // ================================
    // 🔥 통계용 필드 추가
    // ================================
    int todayCollectedTax = 0;          // 오늘 하루 동안 실제로 징수된 세금 합계
    private readonly System.Collections.Generic.List<int> dailyHistory
        = new System.Collections.Generic.List<int>(); // 최근 최대 14일 기록 (전주 비교용)

    // 👉 패널에서 읽어 쓸 프로퍼티
    public int TodayTax => todayCollectedTax;

    public float SevenDayAverage
    {
        get
        {
            if (dailyHistory.Count == 0) return 0f;

            int count = Mathf.Min(7, dailyHistory.Count);
            int sum = 0;
            // 최근 count일 평균
            for (int i = dailyHistory.Count - count; i < dailyHistory.Count; i++)
                sum += dailyHistory[i];

            return (float)sum / count;
        }
    }

    public int WeeklyDiff
    {
        get
        {
            // 전주 데이터까지 있으려면 최소 14일 필요
            if (dailyHistory.Count < 14) return 0;

            int last7Sum = 0;
            for (int i = dailyHistory.Count - 7; i < dailyHistory.Count; i++)
                last7Sum += dailyHistory[i];

            int prev7Sum = 0;
            for (int i = dailyHistory.Count - 14; i < dailyHistory.Count - 7; i++)
                prev7Sum += dailyHistory[i];

            return last7Sum - prev7Sum; // 양수면 이번 주가 더 많이 번 것
        }
    }
    // ================================

>>>>>>> Stashed changes:Assets/2.Scripts/System/Tax/TaxSystem.cs
    private void Start()
    {
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

    // ✅ 하루가 지날 때마다 호출되는 로직
    void OnDayChanged()
    {
        // 최신 인구/세율 기준으로 세금 다시 계산
        RecalculateTaxAmount();

        // 세금 징수
<<<<<<< Updated upstream:Assets/2.Scripts/System/TaxSystem.cs
        Managers.Commodity.AddMoney(tax);
        // 텍스트 갱신 함수 호출 (임시)
        sceneUI.UpdateMoneyText();
=======
        moneySystem.CollectTax(tax);

        // 오늘 세금 누적 (하루에 1번만이긴 하지만, 혹시 나중에 추가 수입 있을 걸 대비해서 += 로)
        todayCollectedTax += tax;

>>>>>>> Stashed changes:Assets/2.Scripts/System/Tax/TaxSystem.cs
        // 행복도 변화량 적용
        ApplyHappinessByTaxRate();

        // 🔥 하루 마감 처리 (통계 업데이트)
        EndOfDay();
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

<<<<<<< Updated upstream:Assets/2.Scripts/System/TaxSystem.cs
    public void ActivatePanel()
    {
        taxRatePanel.SetActive(true);
    }

    public void DeActivatePanel()
    {
        taxRatePanel.SetActive(false);
=======
    // ================================
    // 🔥 하루가 끝날 때 통계 정리
    // ================================
    void EndOfDay()
    {
        // 오늘 징수된 세금 기록
        dailyHistory.Add(todayCollectedTax);

        // 기록은 최대 14일만 유지 (최근 2주)
        if (dailyHistory.Count > 14)
            dailyHistory.RemoveAt(0);

        // 다음 날을 위해 오늘 값 리셋
        todayCollectedTax = 0;
>>>>>>> Stashed changes:Assets/2.Scripts/System/Tax/TaxSystem.cs
    }
}
