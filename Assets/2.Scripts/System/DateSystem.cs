using System;
using UnityEngine;
<<<<<<< Updated upstream
using TMPro;
using UnityEngine.UI;
=======
using TMPro;  // ← TextMeshPro 추가
>>>>>>> Stashed changes

public class DateSystem : MonoBehaviour
{
    [Header("Game Day")]
    public int currentDay = 1;

    [Header("Time Scale")]
    [Tooltip("현실 몇 초마다 게임 하루(Day)가 지나가는가? 기본 60초 = 1 Day")]
    public float realSecondsPerGameDay = 60f;

    [Header("UI (TextMeshPro)")]
    public TextMeshProUGUI dateText;

    public event Action OnDayChanged;

<<<<<<< Updated upstream
    [SerializeField] Button dateButton;

    private void Start()
    {
        dateButton.onClick.AddListener(DateTest);
    }

    void Update()
    {
=======
    private float _timeAcc = 0f; // 현실 시간 누적

    void Update()
    {
        // 현실 시간 누적
        _timeAcc += Time.deltaTime;

        // 누적 시간이 하루 분량을 넘으면 Day 증가
        while (_timeAcc >= realSecondsPerGameDay)
        {
            _timeAcc -= realSecondsPerGameDay;
            IncreaseDay();
        }

#if UNITY_EDITOR
        // 테스트용: 스페이스로 강제 하루 증가
        if (Input.GetKeyDown(KeyCode.Space))
        {
            IncreaseDay();

        }
#endif

>>>>>>> Stashed changes
        UpdateText();
    }

    private void IncreaseDay()
    {
        currentDay++;
        OnDayChanged?.Invoke();
        Debug.Log($"[DateSystem] Day changed: {currentDay}");
    }

<<<<<<< Updated upstream
    void DateTest()
    {
        currentDay++;
        // null 체크 겸용: 구독자가 있을 때만 이벤트 호출
        // (?.Invoke는 구독자가 null이면 호출하지 않음 -> NRE 방지) 
        OnDayChanged?.Invoke(); // <- 하루가 지날 때마다 통지
    }
=======
    private void UpdateText()
    {
        if (dateText != null)
            dateText.text = $"Day {currentDay}";
    }
    public void AdvanceDay()
    {
        currentDay++;
        OnDayChanged?.Invoke();
    }

>>>>>>> Stashed changes
}
