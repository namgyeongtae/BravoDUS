using UnityEngine;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }
    [SerializeField] private List<Building> buildings = new List<Building>(); // 모든 빌딩 객체 목록
    private bool isDragging = false; // 드래그 중 클릭 무시 플래그
    private float lastInputTime = 0f; // 마지막 입력 타임스탬프
    private const float inputCooldown = 0.2f; // 0.2초 내 중복 입력 무시

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 씬 안에 있는 모든 Building 자동 등록
        buildings.AddRange(FindObjectsOfType<Building>());
    }

    void Update()
    {
        HandleTouchInput();
#if UNITY_EDITOR
        HandleMouseInput();
#endif
        HandleKeyInput();
    }

    // ------------------- 📱 터치 처리 -------------------
    private void HandleTouchInput()
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
                    return;
                }
                if (Time.time - lastInputTime < inputCooldown)
                {
                    Debug.Log("Touch input ignored due to cooldown.");
                    return;
                }
                lastInputTime = Time.time;
                Debug.Log($"Touch ended at position: {touch.position}");
                ProcessRaycast(Camera.main.ScreenPointToRay(touch.position));
            }
        }
    }

    // ------------------- 🖱️ 마우스 처리 -------------------
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - lastInputTime < inputCooldown)
            {
                Debug.Log("Mouse input ignored due to cooldown.");
                return;
            }
            lastInputTime = Time.time;
            Debug.Log($"Mouse click at position: {Input.mousePosition}");
            ProcessRaycast(Camera.main.ScreenPointToRay(Input.mousePosition));
        }
    }

    // ------------------- ⌨️ 키 입력 처리 -------------------
    private void HandleKeyInput()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R))
        {
            Building building = GetComponent<Building>(); // 현재 객체의 Building 컴포넌트
            if (building != null)
            {
                Debug.Log($"Key press detected for {gameObject.name} - CurrentState: {building.CurrentState}");
                if (building.CurrentState == Building.State.Ruin)
                {
                    building.StartConstruction();
                }
                else if (building.CurrentState == Building.State.Base)
                {
                    building.Upgrade();
                }
            }
        }
#endif
    }

    // ------------------- 🔍 공통 Raycast 처리 -------------------
    private void ProcessRaycast(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Default")))
        {
            Debug.Log($"Raycast hit object: {hit.transform.name}, Position: {hit.point}, Layer: {LayerMask.LayerToName(hit.transform.gameObject.layer)}");
            Building building = hit.transform.GetComponentInParent<Building>();
            if (building != null)
            {
                Debug.Log($"Raycast hit: {hit.transform.name}, Building: {building.name}");
                if (building.CurrentState == Building.State.Ruin)
                {
                    building.StartConstruction();
                }
                else if (building.CurrentState == Building.State.Base)
                {
                    building.Upgrade();
                }
            }
        }
        else
        {
            Debug.Log("Raycast missed - No hit detected");
        }
    }

    // ------------------- 🏗️ 유틸 함수 -------------------
    public void AddBuilding(Building newBuilding)
    {
        buildings.Add(newBuilding);
    }

    public void StartBuildingConstruction(Building building)
    {
        building.StartConstruction();
    }

    public void UpgradeBuilding(Building building)
    {
        building.Upgrade();
    }

    public bool CheckResources(int wood, int iron)
    {
        return Managers.Commodity.GetIngredient(IngredientType.Wood)?.Amount >= wood &&
               Managers.Commodity.GetIngredient(IngredientType.Iron)?.Amount >= iron;
    }

    public void RefundResources(int wood, int iron)
    {
        Managers.Commodity.AddIngredient(IngredientType.Wood, wood);
        Managers.Commodity.AddIngredient(IngredientType.Iron, iron);
    }

    public int GetGovernmentLevel()
    {
        foreach (var b in buildings)
        {
            if (b.name == "Government") return b.Level;
        }
        return 0;
    }

    public void AddResource(string type, float amount)
    {
        IngredientType ingredientType = (IngredientType)System.Enum.Parse(typeof(IngredientType), type);
        Managers.Commodity.AddIngredient(ingredientType, amount);
    }

    public void HealPopulation(float amount)
    {
        Debug.Log("인구 치료: " + amount);
    }
}