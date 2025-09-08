using UnityEngine;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }

    [SerializeField] private List<Building> buildings = new List<Building>(); // 모든 빌딩 객체 목록 (에디터 할당 or 자동 로드)

    private bool isDragging = false; // 드래그 중 클릭 무시 플래그

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        // 수정: 싱글톤 초기화 – GameManager 원본 돌림으로 별도 매니저로 동작
    }

    void Start()
    {
        buildings.AddRange(FindObjectsOfType<Building>()); // 자동 초기화 – 씬에 배치된 Building 객체 찾기 (논리: 동적 로드 용이)
    }

    void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved && touch.deltaPosition.magnitude > 10f)
            {
                isDragging = true; // 드래그 중 플래그 – 카메라 Pan과 터치 충돌 방지
            }
            if (touch.phase == TouchPhase.Ended)
            {
                if (isDragging)
                {
                    isDragging = false;
                    return; // 드래그 끝, 클릭 무시 – 모바일 터치 안전성 강화
                }
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Building building = hit.transform.GetComponent<Building>();
                    if (building != null && Vector3.Distance(hit.point, building.FixedPosition) < 0.01f) // iso 오차 방지
                    {
                        // Game Dev 코드
                        /* if (building.CurrentState == Building.State.Ruin) StartBuildingConstruction(building);
                        else if (building.CurrentState == Building.State.Base) UpgradeBuilding(building); */

                        // 샷댕이 코드 
                        if (building.CurrentState == Building.State.Ruin)
                        {
                            Managers.UI.AddPanel<UIBuildButtonGroup>(building);
                        }
                        else if (building.CurrentState == Building.State.Base)
                        {
                            // Managers.UI.GetUI<SceneUI>("SceneUI").ToggleBuildingSelection(building.BuildingType);
                        }
                    }
                }
            }
        }
    }

    public void AddBuilding(Building newBuilding)
    {
        buildings.Add(newBuilding); // 동적 추가 – 나중 확장 (레벨업 해금) 용이
    }

    public void StartBuildingConstruction(Building building)
    {   
        building.StartConstruction(); // 빌딩 객체 호출 – 논리 분리 (SOLID 준수)
    }

    public void UpgradeBuilding(Building building)
    {
        building.Upgrade(); // 빌딩 객체 호출
    }

    // 수정: 자원 체크 CommodityManager 직접 – GameManager 원본 돌림으로 호환
    public bool CheckResources(int wood, int iron)
    {
        return Managers.Commodity.GetIngredient(IngredientType.Wood)?.Amount >= wood &&
               Managers.Commodity.GetIngredient(IngredientType.Iron)?.Amount >= iron;
    }

    // 수정: 환불 CommodityManager 직접
    public void RefundResources(int wood, int iron)
    {
        Managers.Commodity.AddIngredient(IngredientType.Wood, wood);
        Managers.Commodity.AddIngredient(IngredientType.Iron, iron);
    }

    // 수정: 정부 레벨 placeholder – 정부 빌딩 찾기 (이름 기반, 나중 ID로 개선)
    public int GetGovernmentLevel()
    {
        foreach (var b in buildings)
        {
            if (b.name == "Government") return b.Level;
        }
        return 0;
    }

    // 수정: 자원 추가 CommodityManager 직접
    public void AddResource(string type, float amount)
    {
        IngredientType ingredientType = (IngredientType)System.Enum.Parse(typeof(IngredientType), type);
        Managers.Commodity.AddIngredient(ingredientType, amount);
    }

    // placeholder (인구 시스템 연동)
    public void HealPopulation(float amount)
    {
        Debug.Log("인구 치료: " + amount);
    }
}