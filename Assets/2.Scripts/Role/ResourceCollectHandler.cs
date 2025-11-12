using Unity.VisualScripting;
using UnityEngine;

public class ResourceCollectHandler : RoleHandler
{
    [SerializeField] private IngredientType _resourceType;

    private float _lastCollectTime = 0f;
    private float _intervalTime = 3f;
    private float _quantity = 4f;
    private float _cumulativeQuantity = 0f;
    private Building _building;

    private UIAlarmNotWorkForce _alarmNotWorkForce = null;
    private UICollectButton _collectButton = null;

    public IngredientType ResourceType => _resourceType;
    public float Quantity => _quantity;
    public float CumulativeQuantity => _cumulativeQuantity;

    void Start()
    {
        _building = GetComponent<Building>();
        _building.OnWorkForceChanged += OnWorkForceChanged;

        if (_resourceType == IngredientType.Wood)
        {
            Managers.UI.GetUI<SceneUI>().WoodParticleAttractor.OnAttractedCompleted.AddListener(AddIngredient);
        }
        /* else if (_resourceType == IngredientType.Iron)
        {
            Managers.UI.GetUI<SceneUI>().IronParticleAttractor.OnAttractedCompleted.AddListener(AddIngredient);
        } */
    }

    void OnDestroy()
    {
        _building.OnWorkForceChanged -= OnWorkForceChanged;

        if (_resourceType == IngredientType.Wood)
        {
            Managers.UI.GetUI<SceneUI>().WoodParticleAttractor.OnAttractedCompleted.RemoveListener(AddIngredient);
        }
        /* else if (_resourceType == IngredientType.Iron)
        {
            Managers.UI.GetUI<SceneUI>().IronParticleAttractor.OnAttractedCompleted.RemoveListener(AddIngredient);
        } */
    }

    public override void HandleEvent(string eventType)
    {
        float now = Time.time;
        if (now - _lastCollectTime >= _intervalTime)
        {
            CollectResource();
            _lastCollectTime = now;
        }
    }

    void Update()
    {
        HandleEvent("ResourceCollect");
    }   

    public override void OnUpgrade(int newLevel)
    {
        base.OnUpgrade(newLevel);
    }

    public override void OnDeBuff()
    {
        if (debuffCount >= MAX_DEBUFF_COUNT)
        {
            return;
        }
        debuffCount++;

        _quantity *= 0.5f;  // 추후에 디버프는 변경될 수 있음음
    }

    public override void OnResolved()
    {
        _quantity = 4f;
    }

    private void CreateCollectButton()
    {
        if (_collectButton == null && _cumulativeQuantity >= 20f)
            _collectButton = Managers.UI.AddPanel<UICollectButton>(this, true);
    }

    public void DestroyCollectButton()
    {
        if (_collectButton != null)
        {
            _collectButton.Close();
            _collectButton = null;
        }
    }

    private void CollectResource()
    {
        var building = GetComponentInParent<Building>();
        if (building.CurrentState == Building.State.Ruin || building.CurrentState == Building.State.Constructing)
            return;

        if (building.WorkForceList.Count <= 0)
            return;

        _cumulativeQuantity += _quantity;

        CreateCollectButton();

        // Managers.Commodity.AddIngredient(_resourceType, _quantity);

        // Managers.UI.AddPanel<UIResourceGather>(this, true);
    }

    public void AddIngredient()
    {
        Managers.Commodity.AddIngredient(_resourceType, _cumulativeQuantity);
        _cumulativeQuantity = 0f;
        DestroyCollectButton();
    }

    private void OnWorkForceChanged()
    {
        if (_building.CurrentState == Building.State.Ruin)
            return;
        
        if (_building.WorkForceList.Count <= 0)
        {
            // 인력 없음 Alarm UI 띄우기
            _alarmNotWorkForce = Managers.UI.AddPanel<UIAlarmNotWorkForce>(_building, true);
        }
        else
        {
            if (_alarmNotWorkForce != null)
            {
                _alarmNotWorkForce?.Close();
                _alarmNotWorkForce = null;
            }
        }
    }
}
