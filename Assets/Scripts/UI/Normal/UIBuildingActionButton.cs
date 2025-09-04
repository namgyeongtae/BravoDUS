using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIBuildingActionButton : UIBind
{
    [Bind("Icon")] private Image _icon;
    [Bind("ActionName")] private Text _actionName;

    private UIButton _button;

    protected override void Awake()
    {
        base.Awake();
        _button = GetComponent<UIButton>();
    }

    public void SettingUI(Sprite icon, string actionName)
    {
        _icon.sprite = icon;
        _actionName.text = actionName;
    }

    public void BindEvent(UnityAction onClick)
    {
        _button.BindEvent(onClick, ClickType.Up);
    }

    public void StartActiveButton(Vector3 startPos, Vector3 endPos, float duration)
    {
        StartCoroutine(ActiveButton(startPos, endPos, duration));
    }

    public void StartDeactiveButton(Vector3 startPos, Vector3 endPos, float duration)
    {
        StartCoroutine(DeactiveButton(startPos, endPos, duration));
    }

    private IEnumerator ActiveButton(Vector3 startPos, Vector3 endPos, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            float easedT = UIUtils.EaseInOutQuad(t);

            transform.localPosition = Vector3.Lerp(startPos, endPos, easedT);
            _icon.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 1), easedT);
            _actionName.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 1), easedT);
            GetComponent<Image>().color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 1), easedT);

            time += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator DeactiveButton(Vector3 startPos, Vector3 endPos, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            float easedT = UIUtils.EaseInOutQuad(t);

            transform.localPosition = Vector3.Lerp(startPos, endPos, easedT);
            _icon.color = Color.Lerp(new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), easedT);
            _actionName.color = Color.Lerp(new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), easedT);
            GetComponent<Image>().color = Color.Lerp(new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), easedT);

            time += Time.deltaTime;
            yield return null;
        }

        Managers.Resource.Destroy(gameObject);
    }
}
