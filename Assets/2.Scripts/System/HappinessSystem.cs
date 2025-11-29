using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 행복도 시스템
// 0~29 불만 : -10%  / 30~59 보통 : 0%
// 60~89 만족 : +10% / 90~100 행복 : +20%
public class HappinessSystem : MonoBehaviour
{  
    int happiness = 10; // 행복도 수치
    float productivityMultiplier = 1.0f;

    public float GetProductivityMultiplier()
    {
        return productivityMultiplier;
    }

    public void ApplyHappinessChange(int delta)
    {
        happiness += delta;
    }

    public int GetHappiness()
    {
        return happiness;
    }
}
