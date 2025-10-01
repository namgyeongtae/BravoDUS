using System.Collections;
using UnityEngine;

public class UIPopupBase : CanvasPanel
{
    [Bind("CloseButton")] protected UIButton _closeButton;

    protected override void Initialize()
    {
        base.Initialize();

        _closeButton.BindEvent(Close, ClickType.Up);
    }

    public override void Open()
    {
        base.Open();
        StartCoroutine(AnimateOpen());
    }

    public override void Close()
    {
        StartCoroutine(AnimateClose());
    }

    private IEnumerator AnimateOpen()
    {
        float duration = 0.5f;

        float time = 0f;

        // 크기가 잠깐 커졌다 쏙 줄어드는 연출
        while (time < duration)
        {
            float t = time / duration;
            float easedT = UIUtils.EaseInOutQuad(t);
            
            // 처음에는 크기가 커졌다가 (1.2배), 그 다음에 원래 크기로 돌아옴
            float scale = (easedT < 0.5f) ? Mathf.Lerp(0f, 1.2f, easedT * 2f) : Mathf.Lerp(1.2f, 1f, (easedT - 0.5f) * 2f);
            
            transform.localScale = Vector3.one * scale;

            time += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator AnimateClose()
    {
        float duration = 0.5f;

        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            float easedT = UIUtils.EaseInOutQuad(t);
            
            float scale = (easedT < 0.5f) ? Mathf.Lerp(1f, 1.2f, easedT * 2f) : Mathf.Lerp(1.2f, 0f, (easedT - 0.5f) * 2f);
            
            transform.localScale = Vector3.one * scale;

            time += Time.deltaTime;
            yield return null;
        }

        CanvasManager.Instance.ReleasePopup(this);

        OnClose_Event?.Invoke();
        OnClose_Event = null;
    }
}
