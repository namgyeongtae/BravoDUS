using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildProgress : CanvasPanel
{
    [Bind("ImmediateButton")] private UIButton _immediateButton;    
    [Bind("BuildProgress")] private Slider _buildProgress;

    private Building _selectedBuilding;

    public override void Open()
    {
        base.Open();

        _buildProgress.value = 0;
    }

    public override void Close()
    {
        base.Close();

        Managers.Resource.Destroy(gameObject);
    }

    public override void SetPanelInfo(object Info)
    {
        _selectedBuilding = Info as Building;
        StartCoroutine(UpdateBuildProgress());
    }

    protected override void Initialize()
    {
        _immediateButton.BindEvent(OnClickImmediateButton, ClickType.Up);
    }

    void Update()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(_selectedBuilding.transform.position + Vector3.up * 5f);

        Rect.position = screenPos;
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
        _immediateButton.gameObject.SetActive(false);

        Close();
    }

    private void OnClickImmediateButton()
    {
        // TODO
        // 즉시완료 아이템 효과 적용
        Debug.Log("즉시완료 아이템 효과 적용");
    }
}
