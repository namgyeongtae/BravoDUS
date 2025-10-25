using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UICollectButton : CanvasPanel
{
    [Bind("CollectButton")] private UIButton _collectButton;
    [Bind("ResourceIcon")] private Image _resourceIcon;

    private ResourceCollectHandler _rh;

    private UnityAction<float> _addIngredientAction;

    private readonly string WOOD_PARTICLE_PATH = "UI/Particle/UIWoodParticle";
    private readonly string IRON_PARTICLE_PATH = "UI/Particle/UIIronParticle";

    protected override void Initialize()
    {
        base.Initialize();

        _collectButton.BindEvent(OnClickCollectButton, ClickType.Up);
    }

    public override void SetPanelInfo(object Info)
    {
        _rh = Info as ResourceCollectHandler;
    }

    public override void Close()
    {
        base.Close();

        Managers.Resource.Destroy(this.gameObject);
    }

    void Update()
    {
        AdjustPosition();
    }
    
    private void OnClickCollectButton()
    {
        IngredientType ingreType = _rh.ResourceType;

        UIParticle particle = null;

        switch (ingreType)
        {
            case IngredientType.Wood:
                particle = Managers.Resource.Instantiate(WOOD_PARTICLE_PATH, CanvasManager.Instance.transform).GetComponent<UIParticle>();
                break;
            case IngredientType.Iron:
                particle = Managers.Resource.Instantiate(IRON_PARTICLE_PATH, CanvasManager.Instance.transform).GetComponent<UIParticle>();
                break;
        }

        Managers.UI.GetUI<SceneUI>().AddParticleToAttractor(ingreType, particle);
        particle.GetComponent<RectTransform>().position = Rect.position;
        particle.Play();

        _collectButton.gameObject.SetActive(false);
    }

    private void AdjustPosition()
    {
        if (_rh == null)
            return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(_rh.transform.position);

        Rect.position = screenPos + Vector3.up * 15f;
    }
}
