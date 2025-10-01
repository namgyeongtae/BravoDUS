using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class UIResourceGather : CanvasPanel
{
    [SerializeField] private Sprite _woodIcon;
    [SerializeField] private Sprite _ironIcon;

    [Bind("ResourceIcon")] private Image _resourceIcon;
    [Bind("AmountText")] private Text _amountText;

    private ResourceCollectHandler _rh;

    protected override void Initialize()
    {
        base.Initialize();
    }

    public override void Close()
    {
        Managers.Resource.Destroy(this.gameObject);
    }

    public override void SetPanelInfo(object Info)
    {
        _rh = Info as ResourceCollectHandler;
    }

    public override void CallAfterSetting()
    {
        SettingUI();
    }

    private void SettingUI()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(_rh.transform.position);
        Rect.position = screenPos;

        _resourceIcon.sprite = _rh.ResourceType == IngredientType.Wood ? _woodIcon : _ironIcon;
        _amountText.text = $"+{_rh.Quantity}";

        StartCoroutine(CoAnimateGather());
    }

    private IEnumerator CoAnimateGather()
    {
        // 투명해지면서 점점 올라가기
        float duration = 0.5f;
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            float easedT = UIUtils.EaseInOutCubic(t);

            _resourceIcon.color = new Color(1, 1, 1, 1 - easedT);
            _amountText.color = new Color(1, 1, 1, 1 - easedT);

            Rect.position = new Vector3(Rect.position.x, Rect.position.y + 1 * t, 0);

            time += Time.deltaTime;
            yield return null;
        }

        Close();
    }
}
