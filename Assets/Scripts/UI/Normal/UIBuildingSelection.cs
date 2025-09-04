using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIBuildingSelection : CanvasPanel
{
    [SerializeField] private BuildingSelectionSO _buildingSelectionSO;
    [SerializeField] private float _spacing = 10f;

    private bool _isOpen = false; // 빌딩 선택 창 열려있는지 여부 (토글용)

    private Dictionary<BuildingActionType, UnityAction> _actionDict = new();
    private List<UIBuildingActionButton> _actionButtons = new();

    private Coroutine _animateCoroutine = null;

    public bool IsOpen => _isOpen;

    protected override void Initialize()
    {
        InitActionDict();
    }

    public void DespawnButtons()
    {
        if (!_isOpen)
            return;

        if (_animateCoroutine != null)
        {
            StopCoroutine(_animateCoroutine);
            _animateCoroutine = null;
        }

        _isOpen = false;
        _animateCoroutine = StartCoroutine(DeactiveActionButtons());
    }

    public void SpawnButtons(BuildingType type)
    {
        if (_isOpen)
            return;

        var buildingActionSet = GetActionSetForBuilding(type);

        var actions = buildingActionSet.availableActions;

        foreach (var action in actions)
        {
            var button = SpawnActionButton(action);
            _actionButtons.Add(button);
        }

        AdjustActionButtonPosition();

        if (_animateCoroutine != null)
        {
            StopCoroutine(_animateCoroutine);
            _animateCoroutine = null;
        }

        _isOpen = true;
        _animateCoroutine = StartCoroutine(ActiveActionButtons());
    }

    private UIBuildingActionButton SpawnActionButton(BuildingAction buildingAction)
    {
        UIBuildingActionButton button = Managers.Resource.Instantiate("UI/Buttons/BuildingActionButton")
                                                .GetComponent<UIBuildingActionButton>();
        button.SettingUI(buildingAction.icon, buildingAction.actionName);
        button.BindEvent(_actionDict[buildingAction.actionType]);
        button.transform.SetParent(transform);

        return button;
    }

    private void AdjustActionButtonPosition()
    {
        int buttonCount = _actionButtons.Count;

        // 1. 버튼 간격 조정
        float buttonWidth = _actionButtons[0].GetComponent<RectTransform>().rect.width;
        float buttonSpacing = _spacing;
        float totalWidth = buttonWidth * buttonCount + buttonSpacing * (buttonCount - 1);

        float startX = -totalWidth / 2 + buttonWidth / 2;

        // 2. 버튼 위치 조정
        for (int i = 0; i < buttonCount; i++)
        {
            _actionButtons[i].transform.localPosition = new Vector3(startX + i * (buttonWidth + buttonSpacing), -150f, 0);
        }   
    }

    private BuildingActionSet GetActionSetForBuilding(BuildingType type)
    {
        return _buildingSelectionSO.actionSets.Find(set => set.buildingType == type);
    }

    private IEnumerator ActiveActionButtons()
    {
        float duration = 0.5f;
        float delay = 0.1f;

        // 첫 번째 actionButton 부터 차례대로 현재 위치에서 duration 동안 localPosition을 0, 0, 0 으로 smooth하게 이동
        for (int i = 0; i < _actionButtons.Count; i++)
        {
            Vector3 startPos = _actionButtons[i].transform.localPosition;
            Vector3 endPos = new Vector3(_actionButtons[i].transform.localPosition.x, 0, _actionButtons[i].transform.localPosition.z);

            _actionButtons[i].StartActiveButton(startPos, endPos, duration);

            yield return new WaitForSeconds(delay);
        }
    }

    private IEnumerator DeactiveActionButtons()
    {
        float duration = 0.5f;
        float delay = 0.1f;

        for (int i = _actionButtons.Count - 1; i >= 0; i--)
        {
            Vector3 startPos = _actionButtons[i].transform.localPosition;
            Vector3 endPos = new Vector3(_actionButtons[i].transform.localPosition.x, -150f, _actionButtons[i].transform.localPosition.z);
        
            _actionButtons[i].StartDeactiveButton(startPos, endPos, duration);

            yield return new WaitForSeconds(delay);
        }

        _actionButtons.Clear();
    }

    #region Action Func

    private void InitActionDict()
    {
        Debug.Log("InitActionDict");
        _actionDict.Add(BuildingActionType.Info, Action_ShowInfo);
        _actionDict.Add(BuildingActionType.Upgrade, Action_Upgrade);
    }

    private void Action_ShowInfo()
    {
        Debug.Log("Show Info");
    }

    private void Action_Upgrade()
    {
        Debug.Log("Start Upgrade");
    }

    #endregion
}
