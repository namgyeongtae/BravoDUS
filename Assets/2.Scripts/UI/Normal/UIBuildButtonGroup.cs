using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildButtonGroup : CanvasPanel
{
    [Bind("Build")] private UIButton _buildButton;
    [Bind("Cancel")] private UIButton _cancelButton;
    [Bind("ImmediateButton")] private UIButton _immediateButton;
    [Bind("BuildProgress")] private Slider _buildProgress;

    private Building _selectedBuilding = null;

    protected override void Initialize()
    {
        _buildButton.BindEvent(OnClickBuildButton, ClickType.Up);
        _cancelButton.BindEvent(OnClickCancelButton, ClickType.Up);
        _immediateButton.BindEvent(OnClickImmediateButton, ClickType.Up);
    }

    public override void SetPanelInfo(object Info)
    {
        base.SetPanelInfo(Info);

        _selectedBuilding =  Info as Building;

        AdjustPosition();
    }

    void Update()
    {
        if (_selectedBuilding != null)
        {
            AdjustPosition();
        }
    }

    public override void Open()
    {
        base.Open();
        StartCoroutine(AnimateOpen());
    }

    public override void Close()
    {
        StartCoroutine(AnimateClose(true));
    }

    private void OnClickImmediateButton()
    {
        // TODO
        // 즉시완료 아이템 효과 적용
        Debug.Log("즉시완료 아이템 효과 적용");
    }
    private void OnClickBuildButton()
    {
        bool isSuccess = CraftingManager.Instance.StartBuildingConstruction(_selectedBuilding);
        if (!isSuccess)
        {
            var toast = Managers.UI.AddPanel<UIToastPopup>();
            toast.SettingPopup("자원이 부족해서 실패하였습니다.");
            OnClickCancelButton();
            return;
        }

        _immediateButton.gameObject.SetActive(true);
        _buildProgress.gameObject.SetActive(true);
        _buildProgress.value = 0;

        StartCoroutine(UpdateBuildProgress());

        _buildButton.gameObject.SetActive(false);
        _cancelButton.gameObject.SetActive(false);
    }

    private IEnumerator UpdateBuildProgress()
    {
        float duration = _selectedBuilding.constructionTime;
        float time = 0;

        // 코루틴이 시작되고 duration 동안 _buildProgress.value를 0에서 1로 증가시킴
        while (time < duration)
        {
            float t = time / duration;
            _buildProgress.value = t;
            time += Time.deltaTime;
            yield return null;
        }

        _buildProgress.gameObject.SetActive(false);

        base.Close();
    }

    private void OnClickCancelButton()
    {
        Close();
    }

    private void AdjustPosition()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(_selectedBuilding.transform.position + Vector3.up * 5f);

        Rect.position = screenPos;
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
            
            _buildButton.GetComponent<RectTransform>().localScale = Vector3.one * scale;
            _cancelButton.GetComponent<RectTransform>().localScale = Vector3.one * scale;

            time += Time.deltaTime;
            yield return null;
        }
    }
    private IEnumerator AnimateClose(bool isClose)
    {
        float duration = 0.5f;

        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            float easedT = UIUtils.EaseInOutQuad(t);

            float scale = (easedT < 0.5f) ? Mathf.Lerp(1f, 1.2f, easedT * 2f) : Mathf.Lerp(1.2f, 0f, (easedT - 0.5f) * 2f);

            _buildButton.GetComponent<RectTransform>().localScale = Vector3.one * scale;
            _cancelButton.GetComponent<RectTransform>().localScale = Vector3.one * scale;

            time += Time.deltaTime;
            yield return null;
        }

        if (isClose)
        {
            base.Close();
        }
    }
}
