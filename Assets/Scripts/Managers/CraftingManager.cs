using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;
using System.Text.RegularExpressions; // RemoveComments에서 Regex 사용

[System.Serializable]
public class BuildingRequirement
{
    public string buildingName;
    public int level;
    public List<RequiredResource> requiredResources; // type(int), amount(int)
    public int requiredGovernmentLevel;
    public int constructionTime; // 초 단위
    public float productionRate; // 생산량
    public int cycleSeconds; // 주기 초
    public string description; // 비고
}

[System.Serializable]
public class BuildingRequirementsWrapper
{
    public List<BuildingRequirement> requirements;
}

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }

    [SerializeField] private List<Building> buildings = new List<Building>(); // 모든 빌딩 객체 목록 (에디터 할당 or 자동 로드)

    private bool isDragging = false; // 드래그 중 클릭 무시 플래그
    private float lastInputTime = 0f; // 마지막 입력 타임스탬프
    private const float inputCooldown = 0.2f; // 0.2초 내 중복 입력 무시

    private Dictionary<string, List<BuildingRequirement>> _buildingReqs = new Dictionary<string, List<BuildingRequirement>>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        buildings.AddRange(FindObjectsOfType<Building>()); // 자동 초기화 – 씬에 배치된 Building 객체 찾기 (논리: 동적 로드 용이)
        LoadBuildingRequirements(); // 추가: JSON 로드
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
                    StartBuildingConstruction(building); // 수정: 체크 추가된 메서드 호출
                }
                else if (building.CurrentState == Building.State.Base)
                {
                    UpgradeBuilding(building); // 수정: 체크 추가된 메서드 호출
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
        buildings.Add(newBuilding); // 동적 추가 – 나중 확장 (레벨업 해금) 용이
    }

    public void StartBuildingConstruction(Building building)
    {
        // 수정: JSON 기반 체크 (level 1 = 건설)
        if (!CheckResources(building, 1))
        {
            Debug.LogWarning($"Construction failed for {building.name}: Insufficient resources or government level.");
            return;
        }

        // 자원 소모
        ConsumeResources(building, 1);

        building.StartConstruction(); // 빌딩 객체 호출 – 논리 분리 (SOLID 준수)
    }

    public void UpgradeBuilding(Building building)
    {
        int nextLevel = building.Level + 1;

        // 수정: maxLevel 하드코딩 대신 10으로 (Building.cs private라 접근 대신)
        if (nextLevel > 10 || !CheckResources(building, nextLevel))
        {
            Debug.LogWarning($"Upgrade failed for {building.name}: Max level reached or insufficient resources/government level.");
            return;
        }

        // 자원 소모
        ConsumeResources(building, nextLevel);

        building.Upgrade(); // 빌딩 객체 호출

        // 수정: OnUpgrade 인수 에러 fix – 추가 인수 제거, productionRate 로그만 (ResourceProducer 확장 필요 시 별도)
        var req = GetRequirement(building.name, nextLevel);
        if (req != null)
        {
            // ResourceProducer.OnUpgrade(nextLevel); // 기존처럼 level만 호출 (Building.cs 내부에서 호출됨)
            Debug.Log($"Applying productionRate {req.productionRate} and cycle {req.cycleSeconds} for {building.name} level {nextLevel}. Update ResourceProducer if needed.");
            // TODO: ResourceProducer.cs에 SetProduction(float rate, int cycle) 추가 추천
        }
    }

    // 수정: CheckResources를 JSON 기반으로 변경 (정부 레벨 + 자원 체크)
    public bool CheckResources(Building building, int targetLevel)
    {
        if (!_buildingReqs.TryGetValue(building.name, out var reqs))
        {
            Debug.LogError($"No requirements found for building: {building.name}");
            return false;
        }

        var req = reqs.Find(r => r.level == targetLevel);
        if (req == null)
        {
            Debug.LogError($"No requirement for level {targetLevel} in {building.name}");
            return false;
        }

        // 정부 레벨 체크
        if (GetGovernmentLevel() < req.requiredGovernmentLevel)
        {
            Debug.LogWarning($"Government level too low: Required {req.requiredGovernmentLevel}, Current {GetGovernmentLevel()}");
            return false;
        }

        // 자원 체크
        foreach (var res in req.requiredResources)
        {
            IngredientType type = (IngredientType)res.type;
            var ingredient = Managers.Commodity.GetIngredient(type);
            if (ingredient == null || ingredient.Amount < res.amount)
            {
                Debug.LogWarning($"Insufficient {type}: Required {res.amount}, Available {ingredient?.Amount ?? 0}");
                return false;
            }
        }

        return true;
    }

    // 추가: 자원 소모 헬퍼
    private void ConsumeResources(Building building, int targetLevel)
    {
        if (!_buildingReqs.TryGetValue(building.name, out var reqs)) return;

        var req = reqs.Find(r => r.level == targetLevel);
        if (req == null) return;

        foreach (var res in req.requiredResources)
        {
            IngredientType type = (IngredientType)res.type;
            Managers.Commodity.GetIngredient(type)?.Consume(res.amount);
        }
    }

    // 추가: 요구사항 getter (productionRate 등에 사용)
    private BuildingRequirement GetRequirement(string buildingName, int level)
    {
        if (_buildingReqs.TryGetValue(buildingName, out var reqs))
        {
            return reqs.Find(r => r.level == level);
        }
        return null;
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

    // 추가: JSON 로드 메서드
    private void LoadBuildingRequirements()
    {
        Debug.Log("=== LoadBuildingRequirements Start ===");
        TextAsset[] allTextsInJson = Resources.LoadAll<TextAsset>("Json");
        TextAsset jsonAsset = null;
        foreach (var asset in allTextsInJson)
        {
            if (asset.name.Contains("BuildingRequirements"))
            {
                jsonAsset = asset;
                break;
            }
        }
        if (jsonAsset == null)
        {
            Debug.LogError("BuildingRequirements.json not found in Resources/Json! Using hardcoded if available.");
            return; // fallback: 하드코딩 없으니 에러 로그만
        }
        Debug.Log($"Loaded: {jsonAsset.name} (length: {jsonAsset.text.Length})");

        string cleanJson = RemoveComments(jsonAsset.text);

        try
        {
            BuildingRequirementsWrapper wrapper = JsonUtility.FromJson<BuildingRequirementsWrapper>(cleanJson);
            if (wrapper == null || wrapper.requirements == null || wrapper.requirements.Count == 0)
            {
                Debug.LogError("Invalid/empty JSON! Falling back.");
                return;
            }
            _buildingReqs.Clear();
            foreach (var req in wrapper.requirements)
            {
                if (!_buildingReqs.ContainsKey(req.buildingName))
                    _buildingReqs[req.buildingName] = new List<BuildingRequirement>();
                _buildingReqs[req.buildingName].Add(req);
                Debug.Log($"Added: {req.buildingName} level {req.level}");
            }
            Debug.Log("=== LoadBuildingRequirements End (Success) ===");
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON Parse error: {e.Message}. Raw JSON: {jsonAsset.text}. Falling back.");
        }
    }

    // 추가: JSON 주석 제거 헬퍼 (CommodityManager에서 복사)
    private string RemoveComments(string json)
    {
        string[] lines = json.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var cleanedLines = new StringBuilder();
        foreach (string line in lines)
        {
            int commentIndex = line.IndexOf("//");
            if (commentIndex >= 0)
            {
                string cleanLine = line.Substring(0, commentIndex).TrimEnd();
                cleanedLines.Append(cleanLine);
            }
            else
            {
                cleanedLines.Append(line);
            }
            cleanedLines.AppendLine();
        }
        string result = cleanedLines.ToString().Trim();
        result = Regex.Replace(result, @"\s+", " ");
        result = result.Replace(" {", "{").Replace("} ", "}").Replace(", ", ",");
        Debug.Log($"Cleaned JSON preview: {result.Substring(0, Math.Min(100, result.Length))}...");
        return result;
    }
}