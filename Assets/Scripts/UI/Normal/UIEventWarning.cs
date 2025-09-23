using UnityEngine;
using System.Collections;

public class UIEventWarning : CanvasPanel
{
    private Coroutine _animateCoroutine = null;

    private MonoBehaviour _targetObject;
    
    protected override void Initialize()
    {
        base.Initialize();
    }

    public override void Open()
    {
        base.Open();
    }

    public override void SetPanelInfo(object Info)
    {
        _targetObject = Info as MonoBehaviour;
    }

    public override void CallAfterSetting()
    {
        Rect.position = Camera.main.WorldToScreenPoint(_targetObject.transform.position) + Vector3.up * 100f;
    }

    void Update()
    {
        if (_animateCoroutine == null)
        {
            _animateCoroutine = StartCoroutine(AnimateShake());
        }
    }

    private IEnumerator AnimateShake()
    {
        float duration = 0.8f;
        float shakeAmount = 15f; // 회전 각도
        float time = 0f;
        Quaternion originalRotation = Rect.rotation;

        // 좌우 회전 흔들림 연출
        while (time < duration)
        {
            float t = time / duration;
            float easedT = UIUtils.EaseInOutQuad(t);
            
            // Z축 회전으로 좌우 기울어짐
            float shakeZ = shakeAmount * Mathf.Sin(time * Mathf.PI * 6f) * (1f - easedT);
            
            Rect.rotation = originalRotation * Quaternion.Euler(0, 0, shakeZ);

            time += Time.deltaTime;
            yield return null;
        }

        // 원래 회전으로 복원
        Rect.rotation = originalRotation;

        yield return new WaitForSeconds(0.3f);

        StopCoroutine(_animateCoroutine);
        _animateCoroutine = null;
    }
}
