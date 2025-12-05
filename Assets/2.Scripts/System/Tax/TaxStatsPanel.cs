using UnityEngine;
using UnityEngine.UI;

public class TaxStatsPanel : MonoBehaviour
{
    [Header("UI Text References")]
    public Text todayTaxText;      // 오늘 세금
    public Text avg7Text;          // 7일 평균
    public Text weeklyDiffText;    // 전주 대비 (이번 7일 - 지난 7일)
    public GameObject Panel;
    private TaxSystem _taxSystem;

    private void Awake()
    {
        // 씬에 있는 TaxSystem 찾아오기 (필요하면 직접 Drag&Drop로 참조해도 됨)
        _taxSystem = FindObjectOfType<TaxSystem>();
    }

    /// <summary>
    /// 패널 켜기 (버튼에서 OnClick으로 연결)
    /// </summary>
    public void Open()
    {
        Panel.gameObject.SetActive(true);
        Refresh();
    }

    /// <summary>
    /// 패널 끄기 (닫기 버튼에서 OnClick으로 연결)
    /// </summary>
    public void Close()
    {
        Panel.gameObject.SetActive(false);
    }

    /// <summary>
    /// 텍스트 갱신
    /// </summary>
    public void Refresh()
    {
        if (_taxSystem == null)
        {
            Debug.LogWarning("[TaxStatsPanel] TaxSystem not found.");
            return;
        }

        int today = _taxSystem.TodayTax;
        float avg7 = _taxSystem.SevenDayAverage;
        int diff = _taxSystem.WeeklyDiff;

        string diffSign = diff > 0 ? "▲" : diff < 0 ? "▼" : "-";

        if (todayTaxText != null)
            todayTaxText.text = $"오늘 세금 : {today:N0}";

        if (avg7Text != null)
            avg7Text.text = $"7일 평균 : {avg7:N0}";

        if (weeklyDiffText != null)
            weeklyDiffText.text = $"전주 대비 : {diffSign} {Mathf.Abs(diff):N0}";
    }
}
