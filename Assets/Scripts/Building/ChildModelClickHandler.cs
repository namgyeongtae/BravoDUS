using UnityEngine;

public class ChildModelClickHandler : MonoBehaviour
{
    private Renderer rend;
    private Color originalColor;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalColor = rend.material.color;
        }
    }

    void OnMouseEnter()
    {
        // 마우스가 올라갔을 때 하이라이트
        if (rend != null)
        {
            rend.material.color = Color.yellow;
        }
    }

    void OnMouseExit()
    {
        // 마우스가 나갔을 때 원래 색 복원
        if (rend != null)
        {
            rend.material.color = originalColor;
        }
    }
}
