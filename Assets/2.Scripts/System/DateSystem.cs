using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DateSystem : MonoBehaviour
{
    public int currentDay = 1; // 현재 날짜
    public TextMeshProUGUI dateText;

    // 날짜 변경 시 알림: int = 변경 후 날짜
    public event Action OnDayChanged;

    [SerializeField] Button dateButton;

    private void Start()
    {
        dateButton.onClick.AddListener(DateTest);
    }

    void Update()
    {
        UpdateText();
    }

    void UpdateText()
    {
        dateText.text = "Day " + currentDay.ToString();
    }

    void DateTest()
    {
        currentDay++;
        // null 체크 겸용: 구독자가 있을 때만 이벤트 호출
        // (?.Invoke는 구독자가 null이면 호출하지 않음 -> NRE 방지) 
        OnDayChanged?.Invoke(); // <- 하루가 지날 때마다 통지
    }
}
