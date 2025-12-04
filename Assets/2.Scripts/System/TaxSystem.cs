using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.InputSystem.Controls; // 안 쓰면 지워도 됨

public class TaxSystem : MonoBehaviour
{
    [SerializeField] GameObject taxRatePanel;

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

    // ===== 통계용 필드 =====
    const int HISTORY_DAYS = 14;              // 최근 14일 기억 (이번 7일 + 지난 7일)
    int[] _taxHistory = new int[HISTORY_DAYS];
    int _historyCount = 0;                    // 지금까지 기록된 일수 (최대 14)
    int _todayTax = 0;                        // 마지막으로 징수된 하루 세금

    /// <summary>오늘 세금(가장 최근 하루 세금)</summary>
    public int TodayTax => _todayTax;

    /// <summary>최근 7일 평균 세금</summary>
    public float SevenDayAverage
    {
        get
        {
            if (_historyCount == 0) return 0f;

            int days = Mathf.Min(7, _historyCount);
            int sum = 0;

            // 가장 최근 days일 합
            for (int i = _historyCount - days; i < _historyCount; i++)
                sum += _taxHistory[i];

            return (float)sum / days;
        }
    }

    /// <summary>
    /// 전주 대비 차이.
    /// (최근 7일 합 - 그 이전 7일 합 / 데이터가 8일 미만이면 0)
    /// </summary>
    public int WeeklyDiff
    {
        get
        {
            if (_historyCount < 8) return 0; // 최소 8일은 지나야 비교 가능

            int recentDays = Mathf.Min(7, _historyCount);
            int prevDays = Mathf.Min(7, _historyCount - recentDays);

            int recentSum = 0;
            int prevSum = 0;

            // 최근 7일
            int recentStart = _historyCount - recentDays;
            for (int i = recentStart; i < _historyCount; i++)
                recentSum += _taxHistory[i];

            // 그 이전 7일
            int prevStart = Mathf.Max(0, recentStart - prevDays);
            for (int i = prevStart; i < recentStart; i++)
                prevSum += _taxHistory[i];

            return recentSum - prevSum;
        }
    }

    // ========================

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
        // 하루 세금 다시 계산 (인구 변동 등을 반영)
        RecalculateTaxAmount();

        // 오늘 세금 기록
        _todayTax = tax;

        // 세금 징수
        

        // 히스토리에 저장
        RecordTodayTax(_todayTax);

        // 행복도 변화량 적용
        ApplyHappinessByTaxRate();
    }

    // 하루 세금 금액 계산
    void RecalculateTaxAmount()
    {
        tax = populationSystem.GetCurrentPopulation() * taxRate;
    }

    // 히스토리 배열에 오늘 세금 push
    void RecordTodayTax(int amount)
    {
        if (_historyCount < HISTORY_DAYS)
        {
            _taxHistory[_historyCount] = amount;
            _historyCount++;
        }
        else
        {
            // 14일 꽉 찼으면 한 칸씩 당기고 마지막에 오늘 값 삽입
            for (int i = 1; i < HISTORY_DAYS; i++)
                _taxHistory[i - 1] = _taxHistory[i];

            _taxHistory[HISTORY_DAYS - 1] = amount;
        }
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
        happinessSystem.ApplyHappinessChange(expected_Happiness_Change);
    }

    void UpdateText()
    {
        if (taxText != null)
            taxText.text = "예상 세금 : " + tax;

        if (expected_Happiness_Change_Text != null)
            expected_Happiness_Change_Text.text = "예상 행복도 변화량 : " + expected_Happiness_Change;
    }

    public int GetTaxRate() => taxRate;

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
