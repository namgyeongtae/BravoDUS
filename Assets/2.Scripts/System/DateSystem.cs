using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DateSystem : MonoBehaviour
{
    [Header("Game Day")]
    public int currentDay = 1;      // 현재 날짜 (Day 1부터 시작)

    [Header("Time Scale")]
    [Tooltip("현실 몇 초마다 게임 하루(Day)가 지나가는가? 기본 60초 = 1 Day")]
    public float realSecondsPerGameDay = 60f;

    [Header("UI")]
    public TextMeshProUGUI dateText;   // 레거시 UI Text

    public Button dateButton;

    // 날짜 변경 시 알림
    public event Action OnDayChanged;

    private float _timeAcc; // 누적 현실 시간(초)

    private void Start()
    {
        dateButton.onClick.AddListener(IncreaseDay);
    }

    void Update()
    {
        // 현실 시간 누적
        _timeAcc += Time.deltaTime;

        // 누적이 1일 분(기본 60초)을 넘으면 Day++
        while (_timeAcc >= realSecondsPerGameDay)
        {
            _timeAcc -= realSecondsPerGameDay;
            IncreaseDay();
        }

        UpdateText();
    }

    private void IncreaseDay()
    {
        currentDay++;
        OnDayChanged?.Invoke();
        Debug.Log($"[DateSystem] Day changed: {currentDay}");
    }

    private void UpdateText()
    {
        if (dateText == null) return;
        dateText.text = $"Day {currentDay}";
    }
}
