using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButton : Button
{
    private static readonly Vector3 SMALL_SCALE = new Vector3(0.9f, 0.9f, 0.9f);
    private static readonly float SCALE_DURATION = 0.1f;
    private Vector3 _initScale = Vector3.one;

    protected Coroutine _scaleCoroutine = null;

    protected override void Awake()
    {
        base.Awake();

        _initScale = transform.localScale;
    }

    protected override void OnDisable()
    {
        if (_scaleCoroutine != null)
        {
            StopCoroutine(_scaleCoroutine);
            _scaleCoroutine = null;
            transform.localScale = _initScale;
        }

        base.OnDisable();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!isActiveAndEnabled)
            return;

        if (_scaleCoroutine != null)
        {
            StopCoroutine(_scaleCoroutine);
            _scaleCoroutine = null;
        }

        if (!interactable)
            return;

        _scaleCoroutine = StartCoroutine(CoScaleUpAndDown(SMALL_SCALE, SCALE_DURATION));
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!isActiveAndEnabled)
            return;
        
        PressEvent();
    }

    private void PressEvent()
    {
        if (!IsActive() || !IsInteractable())
            return;
        
        onClick?.Invoke();
    }

    public void BindEvent(UnityAction action)
    {
        onClick.RemoveAllListeners();
        onClick.AddListener(action);
    }

    IEnumerator CoScaleUpAndDown(Vector3 upScale, float duration)
    {
        Vector3 initialScale = _initScale;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float progress = Mathf.PingPong(t, duration) / duration;
            transform.localScale = Vector3.Lerp(initialScale, upScale, progress);
            yield return null;
        }
        transform.localScale = initialScale;
    }
}
