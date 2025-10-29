using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 행복도 시스템
// 0~29 불만 : -10%  / 30~59 보통 : 0%
// 60~89 만족 : +10% / 90~100 행복 : +20%
public class HappinessSystem : MonoBehaviour
{
    [Header("UI")]
    public Image targetImage; // 표시할 아이콘 UI
    
    public int happiness = 0; // 행복도 수치 (0~100)
    float productivityMultiplier = 1.0f;
    float populationChangeRate;

    // 아이콘 캐싱
    Sprite sad, neutral, smile, happy;

    // 현재 표시 중인 아이콘
    Sprite currentSprite;

    void Start()
    {
        // 시작 시 한번만 로드
        sad = Resources.Load<Sprite>("Icon/sad");
        neutral = Resources.Load<Sprite>("Icon/neutral");
        smile = Resources.Load<Sprite>("Icon/smile");
        happy = Resources.Load<Sprite>("Icon/happy");
    }

    void Update()
    {
        // 테스트 입력
        if (Input.GetKeyDown(KeyCode.Alpha1))
            happiness += 10;

        if (Input.GetKeyDown(KeyCode.Alpha2))
            happiness -= 10;

        // 범위 제한
        happiness = Mathf.Clamp(happiness, 0, 100);

        // 아이콘 갱신 (같은 아이콘이면 교체 안함)
        UpdateIcon();
        // 생산성 배율 갱신
        UpdateProductivity();
        // 인구 감소&증가 확률 갱신
        UpdatePopulationChangeMultiplier();
    }

    // 현재 행복도 구간에 맞는 아이콘으로 교체
    void UpdateIcon()
    {
        Sprite newSprite = null;

        if (happiness < 30)         // 불만
            newSprite = sad;
        else if (happiness < 60)    // 보통
            newSprite = neutral;
        else if (happiness < 90)    // 만족
            newSprite = smile;
        else 
            newSprite = happy;      // 행복

        // 새로운 아이콘이 현재 아이콘과 다를 때만 교체
        if (newSprite != currentSprite)
        {
            currentSprite = newSprite;
            targetImage.sprite = currentSprite;
        }
    }

    // 생산성 배율 갱신
    void UpdateProductivity()
    {
        if (happiness < 30)                 // 불만
            productivityMultiplier = 0.9f;
        else if (happiness < 60)            // 보통
            productivityMultiplier = 1.0f;
        else if (happiness < 90)            // 만족
            productivityMultiplier = 1.1f;
        else                                // 행복
            productivityMultiplier = 1.2f;
    }

    // 인구 감소&증가 확률 갱신
    void UpdatePopulationChangeMultiplier()
    {
        int delta;

        if (happiness < 30)                 // 불만
            delta = -5;
        else if (happiness < 60)            // 보통
            delta = 0;
        else if (happiness < 90)            // 만족
            delta = 5;
        else                                // 행복
            delta = 10;
    }

    public float GetProductivityMultiplier()
    {
        return productivityMultiplier;
    }

    public void ApplyHappinessChange(int delta)
    {
        happiness += delta;
    }

    public void ApplyPopulationChange(int delta)
    {
        // 인구 감소 or 증가 확률에 더하기
    }

    public int GetHappiness()
    {
        return happiness;
    }
}
