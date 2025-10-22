using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ClickType
{
    Down,
    Up,
    Click
}

public class UIButton : Button
{
    private static readonly Vector3 SMALL_SCALE = new Vector3(0.9f, 0.9f, 0.9f);
    private static readonly float SCALE_DURATION = 0.1f;
    private Vector3 _initScale = Vector3.one;

    public UnityAction onClickDown;
    public UnityAction onClickUp;

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

        _scaleCoroutine = StartCoroutine(CoScaleDown(SMALL_SCALE, SCALE_DURATION));

        onClickDown?.Invoke();
    }

    public override void OnPointerUp(PointerEventData eventData)
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

        _scaleCoroutine = StartCoroutine(CoScaleInit(SCALE_DURATION));

        // 드래그한 상태일 때는 Invoke 하지 않음
        if (eventData.dragging)
            return;

        onClickUp?.Invoke();
        Debug.Log("OnPointerUp");
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
        Debug.Log($"OnPointerClick : {onClick}");
    }

    public void BindEvent(UnityAction action, ClickType clickType)
    {
        switch (clickType)
        {
            case ClickType.Down:
                onClickDown += action;
                break;
            case ClickType.Up:
                onClickUp += action;
                break;
            case ClickType.Click:
                onClick.RemoveAllListeners();
                onClick.AddListener(action);
                break;
        }
    }

    IEnumerator CoScaleInit(float duration)
    {
        Vector3 initialScale = _initScale;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float progress = Mathf.PingPong(t, duration) / duration;
            transform.localScale = Vector3.Lerp(transform.localScale, _initScale, progress);
            yield return null;
        }
        transform.localScale = _initScale;
    }

    IEnumerator CoScaleDown(Vector3 downScale, float duration)
    {
        Vector3 initialScale = _initScale;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float progress = Mathf.PingPong(t, duration) / duration;
            transform.localScale = Vector3.Lerp(initialScale, downScale, progress);
            yield return null;
        }
        // transform.localScale = initialScale;
    }
}
