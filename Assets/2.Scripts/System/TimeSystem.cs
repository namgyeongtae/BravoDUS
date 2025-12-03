using UnityEngine;
using TMPro;

public class TimeSystem : MonoBehaviour
{
    public int hour = 0;
    public int minute = 0;

    public float gameMinuteInterval = 1f;
    // 현실 1초마다 게임 5분 증가를 원하면 1f (1초)
    // 현실 5초마다 게임 5분 증가를 원하면 5f
    // 원하는 속도로 조절 가능

    private float timer = 0f;

    public TextMeshProUGUI clockText;
    public DateSystem dateSystem; // 날짜 증가를 위해 참조 필요

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= gameMinuteInterval)
        {
            timer -= gameMinuteInterval;
            AddGameMinutes(5);
        }

        UpdateClockUI();
    }

    private void AddGameMinutes(int minutes)
    {
        minute += minutes;

        if (minute >= 60)
        {
            minute -= 60;
            hour++;

            if (hour >= 24)
            {
                hour = 0;

                if (dateSystem != null)
                    dateSystem.AdvanceDay();   // 날짜 증가
            }
        }
    }

    private void UpdateClockUI()
    {
        if (clockText != null)
        {
            clockText.text = $"{hour:00}:{minute:00}";
        }
    }
}
