using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIUtils
{
    public static float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
    }

    public static float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f;
    }

    public static float EaseInOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c2 = c1 * 1.525f;
        
        if (t < 0.5f)
            return (2f * t * t * ((c2 + 1f) * 2f * t - c2)) / 2f;
        else
            return (2f * t * t * ((c2 + 1f) * 2f * t - c2) + 2f) / 2f;
    }

    public static float EaseInOutElastic(float t)
    {
        const float c5 = (2f * Mathf.PI) / 4.5f;
        
        if (t == 0f) return 0f;
        if (t == 1f) return 1f;
        
        if (t < 0.5f)
            return -(Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) / 2f;
        else
            return (Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) / 2f + 1f;
    }

    public static bool IsPointerOverUIObject(Vector2 touchPos)
    {
        PointerEventData eventDataCurrentPosition
        = new PointerEventData(EventSystem.current);

        eventDataCurrentPosition.position = touchPos;

        List<RaycastResult> results = new List<RaycastResult>();


        EventSystem.current
        .RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0;
    }
}
