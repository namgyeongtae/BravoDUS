using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


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
        if (dateText != null)
            dateText.text = $"Day {currentDay}";
    }
    public void AdvanceDay()
    {
        currentDay++;
        OnDayChanged?.Invoke();
    }
}
