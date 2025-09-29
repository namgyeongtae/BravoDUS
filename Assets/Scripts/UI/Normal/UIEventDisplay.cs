using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIEventDisplay : CanvasPanel
{
    [Bind("FireTime")] protected Text _fireTime;
    [Bind("SecurityTime")] protected Text _securityTime;
    [Bind("InjureTime")] protected Text _injureTime;
    [Bind("OpenButton")] protected UIButton _openButton;
    [Bind("CloseButton")] protected UIButton _closeButton;

    private Vector2 _originAnchoredPos;

    private Coroutine _animateCoroutine = null;

    protected override void Initialize()
    {
        _openButton.BindEvent(() => 
        {
            if (_animateCoroutine != null)
                StopCoroutine(_animateCoroutine);
            _animateCoroutine = StartCoroutine(AnimateUI(_originAnchoredPos, new Vector2(0, Rect.anchoredPosition.y)));
        }, 
        ClickType.Up);

        _closeButton.BindEvent(() => 
        {
            if (_animateCoroutine != null)
                StopCoroutine(_animateCoroutine);
            _animateCoroutine = StartCoroutine(AnimateUI(new Vector2(0, Rect.anchoredPosition.y), _originAnchoredPos));
        }, 
        ClickType.Up);
    }

    public override void Open()
    {
        base.Open();

        _originAnchoredPos = Rect.anchoredPosition;
    }

    void Update()
    {
        _fireTime.text = Managers.Event.Fire.RemainTime;
        _injureTime.text = Managers.Event.Injure.RemainTime;
        // _securityTime.text = Managers.Event.Security.RemainTime;
    }

    private IEnumerator AnimateUI(Vector3 from, Vector3 to)
    {
        float time = 0f;
        float duration = 0.5f;

        _openButton.gameObject.SetActive(!_openButton.gameObject.activeSelf);
        _closeButton.gameObject.SetActive(!_closeButton.gameObject.activeSelf);

        while (time < duration)
        {
            float t = time / duration;
            float easedT = UIUtils.EaseInOutQuad(t);

            Rect.anchoredPosition = Vector2.Lerp(from, to, easedT);

            time += Time.deltaTime;
            yield return null;
        }
    }
}
