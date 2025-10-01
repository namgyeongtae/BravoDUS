using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIBuildingActionButton : UIBind
{
    [Bind("Icon")] private Image _icon;
    [Bind("Lock")] private Image _lockImage;
    [Bind("ActionName")] private Text _actionName;

    private UIButton _button;

    protected override void Awake()
    {
        base.Awake();
        _button = GetComponent<UIButton>();
    }

    public void SettingUI(BuildingAction buildingAction)
    {
        // TODO
        // 선택된 빌딩의 상태에 따라 버튼이 잠길수도 있고 안 잠겨있을 수 있음
        // ex) buildingAction 이 Upgrade이 경우 현재 빌딩이 업그레이드 하기 위한 조건(Government의 레벨, 자원 등등)이
        // 충족되지 않으면 버튼을 잠금
        //_lockImage.gameObject.SetActive(조건);

        _icon.sprite = buildingAction.icon;
        _actionName.text = buildingAction.actionName;
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
