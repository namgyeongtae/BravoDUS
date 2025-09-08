using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class UIResourceGather : CanvasPanel
{
    [SerializeField] 

    [Bind("ResourceIcon")] private Image _resourceIcon;
    [Bind("AmountText")] private Text _amountText;

    protected override void Initialize()
    {
        base.Initialize();
    }

    public override void SetPanelInfo(object Info)
    {
        var building = Info as Building;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(building.transform.position);
        Rect.position = screenPos;
    }

    /* private IEnumerator CoAnimateGather(Building building)
    {

    } */
}
