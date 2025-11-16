using UnityEngine;
using System.Collections;
using System.Linq;

public class UIFireEventWarning : CanvasPanel
{
    private Coroutine _animateCoroutine = null;

    private UIButton _callResolveButton;

    private Building _targetBuilding;
    
    protected override void Initialize()
    {
        base.Initialize();

        _callResolveButton = GetComponent<UIButton>();
        _callResolveButton.BindEvent(OnClickCallResolveButton, ClickType.Up);
    }

    public override void Open()
    {
        base.Open();
    }

    public override void SetPanelInfo(object Info)
    {
        _targetBuilding = Info as Building;
    }

    public override void CallAfterSetting()
    {
        Rect.position = Camera.main.WorldToScreenPoint(_targetBuilding.transform.position) + Vector3.up * 100f;
    }

    void Update()
    {
        if (_animateCoroutine == null)
        {
            _animateCoroutine = StartCoroutine(AnimateShake());
        }

        Rect.position = Camera.main.WorldToScreenPoint(_targetBuilding.transform.position) + Vector3.up * 100f;
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

    private void OnClickCallResolveButton()
    {
        bool isSuccess = false;

        foreach (var fireStationRole in Managers.Event.Fire.FireStationRoles)
        {
            if (fireStationRole.CanProtect(_targetBuilding))
            {
                int wfCount = fireStationRole.GetComponent<Building>().WorkForceList.Where(x => x.HRState == HRState.None).Count();
                if (wfCount >= 0) // 이후 > 0 으로 수정해야 함. Test를 위해 <= 0 으로 설정
                {
                    fireStationRole.DispatchFireTruck(_targetBuilding);
                    isSuccess = true;
                    break;
                }
            }
        }

        if (!isSuccess)
        {
            Managers.UI.OpenToastPopup("소방서에서 소방차를 호출할 수 없습니다. 주변에 소방서가 없거나 인력이 부족합니다.");
        }
    }
}
