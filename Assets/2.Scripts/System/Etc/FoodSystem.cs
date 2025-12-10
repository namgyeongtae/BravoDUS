using UnityEngine;
using TMPro;

public class FoodSystem : MonoBehaviour
{
    public TextMeshProUGUI foodText;

    int food = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha5))
            food++;
        if (Input.GetKeyDown(KeyCode.Alpha6))
            food--;

        // 범위 제한
        food = Mathf.Max(food, 0);

        UpdateText();
    }

    void UpdateText()
    {
        foodText.text = food.ToString();
    }

    public int GetFood()
    {
        return food;
    }
}
