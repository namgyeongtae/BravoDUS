using UnityEngine;
using TMPro;

public class TimeSystem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI clockText;

    [Header("Time Settings")]
    public float realSecondsPerDay = 60f;     // 현실 1분 = 게임 1일
    public float realSecondsPerClockCycle = 300f; // 현실 5분 = 게임 시계 1회전(0~5시)

    private float dayTimer = 0f;
    private float clockTimer = 0f;

    public int currentDay = 1;

    // 날짜가 바뀔 때 알려주는 이벤트
    public event System.Action OnDayChanged;

    void Start()
    {
        UpdateDayUI();
        UpdateClockUI();
    }

    void Update()
    {
        // ■■■ 1) 하루 타이머 계산 (현실 1분마다 Day++) ■■■
        dayTimer += Time.deltaTime;
        if (dayTimer >= realSecondsPerDay)
        {
            dayTimer = 0f;
            currentDay++;

            OnDayChanged?.Invoke(); // 날짜 변화 알림 이벤트
            UpdateDayUI();
        }

        // ■■■ 2) 5분짜리 시계 타이머 계산 (현실 5분 → 게임내 5시간 시계) ■■■
        clockTimer += Time.deltaTime;
        if (clockTimer > realSecondsPerClockCycle)
        {
            clockTimer = 0f;
        }

        UpdateClockUI();
    }

    // -------------------- UI 업데이트 --------------------
    void UpdateDayUI()
    {
        if (dayText != null)
            dayText.text = $"Day {currentDay}";
    }

    void UpdateClockUI()
    {
        if (clockText == null) return;

        float normalized = clockTimer / realSecondsPerClockCycle; // 0~1
        float gameHours = normalized * 5f; // 0~5시

        int hour = Mathf.FloorToInt(gameHours);
        int minute = Mathf.FloorToInt((gameHours - hour) * 60f);

        clockText.text = $"{hour:00}:{minute:00}";
    }
}
