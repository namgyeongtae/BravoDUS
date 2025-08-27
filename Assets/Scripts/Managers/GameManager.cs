using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour, IManagerBase
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private List<Building> buildings = new List<Building>();

    private bool isDragging = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void Init()
    {
        buildings.AddRange(FindObjectsOfType<Building>()); // 자동 초기화
    }

    public void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved && touch.deltaPosition.magnitude > 10f)
            {
                isDragging = true;
            }
            if (touch.phase == TouchPhase.Ended)
            {
                if (isDragging)
                {
                    isDragging = false;
                    return; // 드래그 끝, 클릭 무시
                }
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Building building = hit.transform.GetComponent<Building>();
                    if (building != null && Vector3.Distance(hit.point, building.FixedPosition) < 0.01f)
                    {
                        if (building.CurrentState == Building.State.Ruin) BuildBuilding(building);
                        else if (building.CurrentState == Building.State.Base) UpgradeBuilding(building);
                    }
                }
            }
        }
    }

    public void Release()
    {
        buildings.Clear();
    }

    public void AddBuilding(Building newBuilding)
    {
        buildings.Add(newBuilding); // 동적 추가
    }

    public void BuildBuilding(Building building)
    {
        building.StartConstruction();
    }

    public void UpgradeBuilding(Building building)
    {
        building.Upgrade();
    }

    public bool CheckResources(int wood, int iron)
    {
        // 수정: CommodityManager 연동 (팀원 코드 호환)
        return Managers.Commodity.GetIngredient(IngredientType.Wood)?.Amount >= wood &&
               Managers.Commodity.GetIngredient(IngredientType.Iron)?.Amount >= iron;
    }

    public void RefundResources(int wood, int iron)
    {
        // 수정: CommodityManager 연동 (Gather로 환불)
        Managers.Commodity.AddIngredient(IngredientType.Wood, wood);
        Managers.Commodity.AddIngredient(IngredientType.Iron, iron);
    }

    public int GetGovernmentLevel()
    {
        // 수정: 정부 Building 찾기 (buildings List에서 이름 기반 검색, 나중 ID로 개선)
        foreach (var b in buildings)
        {
            if (b.name == "Government") return b.Level;
        }
        return 0;
    }

    public void AddResource(string type, float amount)
    {
        // 수정: CommodityManager 연동 (IngredientType 파싱)
        IngredientType ingredientType = (IngredientType)System.Enum.Parse(typeof(IngredientType), type);
        Managers.Commodity.AddIngredient(ingredientType, amount);
    }

    public void HealPopulation(float amount)
    {
        // placeholder (인구 시스템 연동)
        Debug.Log("인구 치료: " + amount);
    }
}