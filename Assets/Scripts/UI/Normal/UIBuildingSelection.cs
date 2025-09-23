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

    private BuildingType _selectedBuildingType = BuildingType.None;
    private Building _selectedBuilding = null;

    private Coroutine _animateCoroutine = null;

    public bool IsOpen => _isOpen;

    protected override void Initialize()
    {
        InitActionDict();
    }
    public void SetSelectedBuilding(Building building)
    {
        _selectedBuilding = building;
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

        _selectedBuildingType = BuildingType.None;
    }

    public void SpawnButtons()
    {
        if (_isOpen)
            return;

        var buildingActionSet = GetActionSetForBuilding(_selectedBuilding.BuildingType);

        var actions = buildingActionSet.availableActions;

        foreach (var action in actions)
        {
            var button = SpawnActionButton(_selectedBuilding, action);
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

        _selectedBuildingType = _selectedBuilding.BuildingType;
    }

    private UIBuildingActionButton SpawnActionButton(Building building, BuildingAction buildingAction)
    {
        UIBuildingActionButton button = Managers.Resource.Instantiate("UI/Buttons/BuildingActionButton")
                                                .GetComponent<UIBuildingActionButton>();
        button.SettingUI(buildingAction);
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
        Debug.Log($"GetActionSetForBuilding: {type}");
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
        _actionDict.Add(BuildingActionType.HumanResource, Action_HumanResource);
    }

    private void Action_ShowInfo()
    {
        Debug.Log("Show Info");
        
        // TODO
        // _selectedBuildingType 에 따라 정보 팝업 표시
        switch (_selectedBuildingType)
        {
            case BuildingType.Government:
                Debug.Log("Show Government Info");
                break;
            case BuildingType.Hospital:
                Debug.Log("Show Hospital Info");
                break;
            case BuildingType.PoliceStation:
                Debug.Log("Show Police Station Info");
                break;
            case BuildingType.FireStation:
                Debug.Log("Show Fire Station Info");
                break;
            case BuildingType.ConvenienceStore:
                Debug.Log("Show Convenience Store Info");
                break;
        }
    }

    private void Action_Upgrade()
    {
        Debug.Log("Start Upgrade");

        // TODO
        // 선택된 빌딩을 어떻게든 가져와서 Upgrade 함수 호출출
    }

    private void Action_HumanResource()
    {
        Debug.Log("Start Human Resource");

        // TODO 
        // 선택된 빌딩의 인력 정보를 나타내는 UI 표시
        var uiWorkForce = Managers.UI.AddPanel<UIWorkForce>(_selectedBuilding);

        // 인력 정보 세팅
        // uiWorkForce.SetWorkForceInfo(_selectedBuilding);
    }
    #endregion
}
