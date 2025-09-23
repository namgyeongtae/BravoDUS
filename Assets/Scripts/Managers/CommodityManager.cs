using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO; // 추가: 파일 저장

[System.Serializable]
public class ItemData
{
    public int id;
    public string name;
    public string description;
    public int value; // 자원량이나 가격 (필요 시 float으로)
    public int type; // IngredientType enum을 int로 (e.g., 0=Wood, 1=Iron)
    public int stackSize;
    public float productionRate;
    public float amount; // 추가: 초기 자원 수량 (JSON에서 로드)
    public List<RequiredResource> craftingRequirements; // 옵션, 필요 시
}

[System.Serializable]
public class RequiredResource // 옵션 클래스
{
    public int type;
    public int amount;
}

[System.Serializable]
public class ItemDatabaseWrapper // 이게 핵심 – JSON 루트
{
    public List<ItemData> items;
}

public class CommodityManager : IManagerBase
{
    private Dictionary<IngredientType, Ingredient> _ingredients = new Dictionary<IngredientType, Ingredient>();
    public Ingredient GetIngredient(IngredientType type) => _ingredients.TryGetValue(type, out var ingredient) ? ingredient : null;

    public void Init()
    {
        LoadIngredientFromDB();
        LogAmounts(); // 추가: 로드 직후 Amount 로그
    }

    public void Release()
    {
        Debug.Log("=== CommodityManager Release called! Clearing _ingredients. ==="); // 추가: Release 호출 추적
        SaveAmountsToJson(); // 기존
                             //_ingredients.Clear(); // 수정: 임시 주석 - Clear 피함 (Amount 리셋 방지). 필요 시 복구.
    }

    private void LoadIngredientFromDB()
    {
        Debug.Log("=== LoadIngredientFromDB Start ===");
        TextAsset jsonAsset = Resources.Load<TextAsset>("Json/ItemDatabase");
        if (jsonAsset == null)
        {
            Debug.LogError("ItemDatabase.json not found! Check: Assets/Resources/Json/ItemDatabase.json exists? Case-sensitive name? Imported as TextAsset?");
#if UNITY_EDITOR
            string fullPath = UnityEditor.AssetDatabase.FindAssets("ItemDatabase t:TextAsset", new[] { "Assets/Resources/Json" }).Length > 0 ? "Found" : "Not found";
            Debug.Log($"Editor search check: {fullPath}");
#endif
            return; // fallback 제거 - 실패 시 빈 상태로
        }
        Debug.Log($"Loaded: {jsonAsset.name} (length: {jsonAsset.text.Length}, preview: {jsonAsset.text.Substring(0, Math.Min(50, jsonAsset.text.Length))}...)");
        string cleanJson = RemoveComments(jsonAsset.text);
        Debug.Log($"Cleaned JSON full: {cleanJson}"); // 추가: 전체 cleaned JSON 로그 (파싱 전 확인)
        try
        {
            ItemDatabaseWrapper wrapper = JsonUtility.FromJson<ItemDatabaseWrapper>(cleanJson);
            if (wrapper == null || wrapper.items == null || wrapper.items.Count == 0)
            {
                Debug.LogError($"Invalid/empty JSON! Content was: '{jsonAsset.text}'");
                return;
            }
            _ingredients.Clear();
            int loadedCount = 0;
            foreach (var itemData in wrapper.items)
            {
                if (System.Enum.IsDefined(typeof(IngredientType), itemData.type))
                {
                    IngredientType type = (IngredientType)itemData.type;
                    Ingredient ingredient = new Ingredient(type);
                    ingredient.Gather(itemData.amount); // 초기 amount 로드
                    _ingredients.Add(type, ingredient);
                    loadedCount++;
                    Debug.Log($"Added: {type} (id: {itemData.id}, name: {itemData.name}, parsed amount from JSON: {itemData.amount}, actual ingredient Amount after Gather: {ingredient.Amount})"); // 수정: amount 로그 강화
                }
                else
                {
                    Debug.LogWarning($"Skipping invalid type: {itemData.type}");
                }
            }
            Debug.Log($"Loaded {loadedCount} ingredients from JSON. Total in Dict: {_ingredients.Count}");
            Debug.Log("=== LoadIngredientFromDB End (Success) ===");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON Parse error: {e.Message}. Stack: {e.StackTrace}. Raw JSON: {jsonAsset.text}. Cleaned JSON: {cleanJson}");
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
            cleanedLines.AppendLine(); // 줄바꿈 복원
        }
        string result = cleanedLines.ToString().Trim();
        // 빈 줄/불필요 공백 정리 (옵션)
        result = System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " "); // 공백 압축
        result = result.Replace(" {", "{").Replace("} ", "}").Replace(", ", ","); // 간단 포맷팅
        Debug.Log($"Cleaned JSON preview: {result.Substring(0, Math.Min(100, result.Length))}...");
        return result;
    }
    public void LogAmounts()
    {
        foreach (var kvp in _ingredients)
        {
            Debug.Log($"{kvp.Key} Amount: {kvp.Value.Amount}");
        }
    }
    public void AddIngredient(IngredientType type, float amount)
    {
        if (_ingredients.TryGetValue(type, out var ingredient))
        {
            ingredient.Gather(amount);
        }
        else
        {
            Debug.LogWarning($"Ingredient {type} not found");
        }
    }

    // 추가: 종료 시 Amount 저장 (optional, Release에서 호출)
    private void SaveAmountsToJson()
    {
        ItemDatabaseWrapper wrapper = new ItemDatabaseWrapper(); // 기존 Wrapper 재사용
        wrapper.items = new List<ItemData>();
        foreach (var kvp in _ingredients)
        {
            wrapper.items.Add(new ItemData { type = (int)kvp.Key, amount = kvp.Value.Amount }); // Amount 저장
        }
        string json = JsonUtility.ToJson(wrapper, true);
        string path = Application.persistentDataPath + "/ItemDatabaseSaved.json"; // 별도 파일 (원본 JSON 안 덮음)
        System.IO.File.WriteAllText(path, json);
        Debug.Log($"Saved Amount to {path}");
    }
}