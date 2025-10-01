using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIToastPopup : CanvasPanel
{
    [Bind("Background")] private Image _background;
    [Bind("Message")] private Text _message;

    private Coroutine _animateCoroutine = null;

    public override void Open()
    {
        _animateCoroutine = StartCoroutine(AnimateToast());
    }

    public void SettingPopup(string message)
    {
        _message.text = message;
    }

    private IEnumerator AnimateToast()
    {
        Color originalColor = _background.color;
        Color originalMessageColor = _message.color;

        _background.color = new Color(_background.color.r, _background.color.g, _background.color.b, 0);
        _message.color = new Color(1, 1, 1, 0);

        float duration = 0.5f;
        float time = 0f;

        // 선명해지면서 올라가기
        while (time < duration)
        {
            float t = time / duration;
            float easedT = UIUtils.EaseInOutQuad(t);
            
            float startPos = _background.transform.localPosition.y;
            float endPos = startPos + 2;
            _background.transform.localPosition = new Vector3(_background.transform.localPosition.x, startPos + (endPos - startPos) * easedT, _background.transform.localPosition.z);
            
            // 원래 색상으로 복원 (투명도도 복원)
            _background.color = new Color(originalColor.r, originalColor.g, originalColor.b, easedT);
            _message.color = new Color(originalMessageColor.r, originalMessageColor.g, originalMessageColor.b, easedT);

            time += Time.deltaTime;
            yield return null;
        }

        time = 0f;
        yield return new WaitForSeconds(1f);

        // 투명해지면서 올라가기
        while (time < duration)
        {
            float t = time / duration;
            float easedT = UIUtils.EaseInOutQuad(t);

            float startPos = _background.transform.localPosition.y;
            float endPos = startPos + 2;

            _background.transform.localPosition = new Vector3(_background.transform.localPosition.x, startPos + (endPos - startPos) * easedT, _background.transform.localPosition.z);
            _background.color = new Color(_background.color.r, _background.color.g, _background.color.b, 1 - easedT);
            _message.color = new Color(1, 1, 1, 1 - easedT);

            time += Time.deltaTime;
            yield return null;
        }

        StopCoroutine(_animateCoroutine);
        _animateCoroutine = null;

        base.Close();
    }
}
