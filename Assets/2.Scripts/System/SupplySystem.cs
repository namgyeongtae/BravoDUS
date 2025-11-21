using UnityEngine;
using TMPro;

public class SupplySystem : MonoBehaviour
{
    FoodSystem foodSystem;
    PopulationSystem populationSystem;
    HappinessSystem happinessSystem;
    DateSystem dateSystem;

    public TextMeshProUGUI supplyText;

    int supplyRate;
    int food;
    int population;

    void Awake()
    {
        foodSystem = FindFirstObjectByType<FoodSystem>();
        populationSystem = FindFirstObjectByType<PopulationSystem>();
        happinessSystem = FindFirstObjectByType<HappinessSystem>();
        dateSystem = FindFirstObjectByType<DateSystem>();
    }

    private void OnEnable()
    {
        // 날짜 이벤트 구독: 하루가 지날 때마다 적용
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
        //RecalculateSupply();
        UpdateText();
    }

    // 날짜가 '하루' 지날 때마다 호출됨
    void OnDayChanged()
    {
        // 이 시점의 최신 보급률(supplyRate)을 기준으로 행복도 변화 1회 적용
        UpdateHappinessBySupply();
    }

    void UpdateText()
    {
        supplyText.text = supplyRate.ToString() + "%";
    }

    //void RecalculateSupply()
    //{
    //    food = foodSystem.GetFood();
    //    population = populationSystem.GetPopulation();

    //    // 보급률 = (식량 / 인구 수) X 100% // RoundToInt : 소수점 첫번째 자리에서 반올림
    //    supplyRate = (population > 0) ? Mathf.RoundToInt((float)food / population * 100) : 0;

    //    // 범위 제한
    //    supplyRate = Mathf.Max(supplyRate, 0);
    //}

    // 현재 보급률 구간에 따른 행복도 변화량 적용
    void UpdateHappinessBySupply()
    {
        // 현재 보급률이 몇 번째 구간인지 계산 // delta : 행복도 변화량
        int delta =
            supplyRate < 50 ? -5 :  // 0 ~ 49%    : 부족
            supplyRate < 100 ? -2 : // 50 ~ 99%   : 불안정
            supplyRate < 200 ? 0 :  // 100 ~ 199% : 안정
                                2;  // 200% 이상  : 풍족

        // 실제 행복도 변화 적용
        happinessSystem.ApplyHappinessChange(delta);
    }
}
