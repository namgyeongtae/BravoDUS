using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWorkForce : CanvasPanel
{
    [Bind("Background")] private Image _background;
    [Bind("WorkForceGroup")] private GridLayoutGroup _workForceGroup;

    private Building _building;
    private int _maxWorkForceCount;
    private int _validWorkForceCount;
    private List<UIWorkForceSlot> _workForceSlots = new();

    protected override void Initialize()
    {

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

    public override void SetPanelInfo(object Info)
    {
        try
        {
            _building = Info as Building;

            // TODO
            // _maxWorkForceCount = (빌딩 데이터에 따라 결정);
            // _validWorkForceCount = (정부 레벨에 따라 결정 혹은 정부 레벨 도달 시 저장된 유저 데이터로부터 로드); -> 후자 유력

            SpawnWorkForceSlot();
        }
        catch (Exception e)
        {
            Debug.LogError($"UIWorkForce SetPanelInfo Error : {e.Message}");
        }
    }

    private void SpawnWorkForceSlot()
    {
        int filledCount = 0;

        for (int i = 0; i < _maxWorkForceCount; i++)
        {
            var slot = Managers.Resource.Instantiate("UI/UIWorkForceSlot").GetComponent<UIWorkForceSlot>();
            slot.transform.SetParent(_workForceGroup.transform);
            slot.transform.localScale = Vector3.one;
            _workForceSlots.Add(slot);

            if (i < _building.WorkForceList.Count)
            {
                slot.State = WorkForceSlotState.Assigned;
                slot.SetSlot(_building.WorkForceList[i]);
                filledCount++;
            }
            else
            {
                // 남은 인덱스는 validWorkForceCount 만큼 빈 슬롯으로 채우기
                int remainValidCount = _validWorkForceCount - filledCount;
                if (i <= remainValidCount)
                {
                    slot.State = WorkForceSlotState.Unassigned;
                }
                else
                {
                    slot.State = WorkForceSlotState.Locked;
                }
            }
        }
    }

    private IEnumerator AnimateOpen()
    {
        Vector3 startPos = Rect.position + Vector3.down * _background.rectTransform.sizeDelta.y;
        Vector3 midPos = Rect.position + Vector3.up * 50f;
        Vector3 endPos = Rect.position;
        
        Rect.position = startPos;

        float midDuration = 0.2f;
        float endDuration = 0.1f;
        float time = 0f;

        while (time < midDuration)
        {
            float t = time / midDuration;
            float easedT = UIUtils.EaseInOutQuad(t);

            Rect.position = Vector3.Lerp(startPos, midPos, easedT);

            time += Time.deltaTime;
            yield return null;
        }

        time = 0f;

        while (time < endDuration)
        {
            float t = time / endDuration;
            float easedT = UIUtils.EaseInOutQuad(t);

            Rect.position = Vector3.Lerp(midPos, endPos, easedT);

            time += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator AnimateClose()
    {
        Vector3 startPos = Rect.position;
        Vector3 midPos = Rect.position + Vector3.up * 50f;
        Vector3 endPos = Rect.position + Vector3.down * _background.rectTransform.sizeDelta.y;

        Rect.position = startPos;

        float midDuration = 0.1f;
        float endDuration = 0.2f;
        float time = 0f;

        while (time < midDuration)
        {
            float t = time / midDuration;
            float easedT = UIUtils.EaseInOutQuad(t);

            Rect.position = Vector3.Lerp(startPos, midPos, easedT);

            time += Time.deltaTime;
            yield return null;
        }

        time = 0f;

        while (time < endDuration)
        {
            float t = time / endDuration;
            float easedT = UIUtils.EaseInOutQuad(t);

            Rect.position = Vector3.Lerp(midPos, endPos, easedT);

            time += Time.deltaTime;
            yield return null;
        }

        base.Close();
    }
}
