using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;

[System.Serializable]
public class BuildingRequirement
{
    public string buildingName;
    public int level;
    public List<RequiredResource> requiredResources;
    public int requiredGovernmentLevel;
    public int constructionTime;
    public float productionRate;
    public int cycleSeconds;
    public string description;
}

[System.Serializable]
public class BuildingRequirementsWrapper
{
    public List<BuildingRequirement> requirements;
}

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }

    [SerializeField] private List<Building> buildings = new List<Building>();

    private bool isDragging = false;
    private float lastInputTime = 0f;
    private const float inputCooldown = 0.2f;

    private Dictionary<string, List<BuildingRequirement>> _buildingReqs = new Dictionary<string, List<BuildingRequirement>>();

    public List<Building> Buildings => buildings;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        buildings.AddRange(FindObjectsOfType<Building>());
        LoadBuildingRequirements();
    }

    void Update()
    {
        HandleTouchInput();
#if UNITY_EDITOR
        HandleMouseInput();
#endif
        HandleKeyInput();
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (UIUtils.IsPointerOverUIObject(touch.position))
            {
                Debug.Log("Touch input ignored due to pointer over game object.");
                return;
            }

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

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

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

    private void HandleKeyInput()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R))
        {
            Building building = GetComponent<Building>();
            if (building != null)
            {
                Debug.Log($"Key press detected for {gameObject.name} - CurrentState: {building.CurrentState}");
                if (building.CurrentState == Building.State.Ruin)
                {
                    // building.StartConstruction();
                    Managers.UI.AddPanel<UIBuildButtonGroup>(building, true);
                }
                else if (building.CurrentState == Building.State.Base)
                {
                    //building.Upgrade();
                    Managers.UI.GetUI<SceneUI>("SceneUI").ToggleBuildingSelection(building);
                }
            }
        }
#endif
    }

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
                    // building.StartConstruction();
                    var buildButtonGroup = Managers.UI.GetUI<UIBuildButtonGroup>();
                    if (buildButtonGroup != null && buildButtonGroup.SelectedBuilding != building)
                    {
                        buildButtonGroup.Close();
                    }
                    
                    Managers.UI.AddPanel<UIBuildButtonGroup>(building);
                }
                else if (building.CurrentState == Building.State.Base)
                {
                    //building.Upgrade();
                    Debug.Log($"Key press detected for {gameObject.name} - BuildingType: {building.BuildingType}");
                    Managers.UI.GetUI<SceneUI>("SceneUI").ToggleBuildingSelection(building);
                }
            }
        }
        else
        {
            Debug.Log("Raycast missed - No hit detected");
        }
    }

    public void AddBuilding(Building newBuilding)
    {
        buildings.Add(newBuilding);
    }

    public bool StartBuildingConstruction(Building building)
    {
        if (!CheckResources(building, 1))
        {
            Debug.Log("자원이 부족해서 실패하였습니다."); // 추가: 한국어 실패 로그
            Managers.Commodity.LogAmounts(); // 추가: 현재 자원 로그 출력 (Wood: X, Iron: Y)
            Debug.LogWarning($"Construction failed for {building.name}: Insufficient resources or government level.");
            return false;
        }
        ConsumeResources(building, 1);
        building.StartConstruction();
        return true;
    }

    public void UpgradeBuilding(Building building)
    {
        int nextLevel = building.Level + 1;
        if (nextLevel > 10 || !CheckResources(building, nextLevel))
        {
            Debug.Log("자원이 부족해서 실패하였습니다."); // 추가: 한국어 실패 로그
            Managers.Commodity.LogAmounts(); // 추가: 현재 자원 로그 출력 (Wood: X, Iron: Y)
            Debug.LogWarning($"Upgrade failed for {building.name}: Max level reached or insufficient resources/government level.");
            return;
        }
        ConsumeResources(building, nextLevel);
        building.Upgrade();
        var req = GetRequirement(building.name, nextLevel);
        if (req != null)
        {
            Debug.Log($"Applying productionRate {req.productionRate} and cycle {req.cycleSeconds} for {building.name} level {nextLevel}. Update ResourceProducer if needed.");
        }
    }

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
        // 수정: Government 빌딩은 정부 레벨 체크 스킵 (초기 업그레이드 허용)
        if (building.name != "Government" && GetGovernmentLevel() < req.requiredGovernmentLevel)
        {
            Debug.LogWarning($"Government level too low: Required {req.requiredGovernmentLevel}, Current {GetGovernmentLevel()}");
            return false;
        }
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

    public void ConsumeResources(Building building, int targetLevel)
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
            return;
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