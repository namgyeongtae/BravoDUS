using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

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
    }

    public void Release()
    {
        _ingredients.Clear();
    }

    private void LoadIngredientFromDB()
    {
        Debug.Log("=== LoadIngredientFromDB Start ===");
        // 디버그: 전체 Resources 폴더 검색 (UGS.Generated 무시 확인)
        TextAsset[] allTextsInRoot = Resources.LoadAll<TextAsset>(""); // 루트 Resources 전체
        Debug.Log($"Total TextAssets in all Resources: {allTextsInRoot.Length} (including UGS.Generated?)");
        // Json 서브폴더만 검색 (Assets/Resources/Json)
        TextAsset[] allTextsInJson = Resources.LoadAll<TextAsset>("Json");
        Debug.Log($"Found {allTextsInJson.Length} TextAssets in Json subfolder: ");
        foreach (var asset in allTextsInJson)
        {
            Debug.Log($"- {asset.name} (length: {asset.text.Length})");
            Debug.Log($" Exact name: '{asset.name}'"); // 이름 정확히 확인 (공백/.json 체크)
            Debug.Log($" Full content: '{asset.text}'"); // 전체 내용 (작은 따옴표로, truncated 안 됨)
        }
        if (allTextsInJson.Length == 0)
        {
            Debug.LogError("No TextAssets in Json subfolder! Check Assets/Resources/Json/ItemDatabase.json exists. UGS.Generated/Resources is ignored.");
            goto Fallback;
        }
        // 수정: 이름으로 검색해서 ItemDatabase.json 찾기 (첫 번째 무조건 피함)
        TextAsset jsonAsset = null;
        foreach (var asset in allTextsInJson)
        {
            if (asset.name.Contains("ItemDatabase"))
            {
                jsonAsset = asset;
                break;
            }
        }
        if (jsonAsset == null)
        {
            Debug.LogError("ItemDatabase.json not found in Json subfolder! Falling back.");
            goto Fallback;
        }
        Debug.Log($"Loaded: {jsonAsset.name} (length: {jsonAsset.text.Length}, preview: {jsonAsset.text.Substring(0, Math.Min(50, jsonAsset.text.Length))}...)");

        // JSON 전처리: 주석 제거 (Google Sheets 주석 문제 해결)
        string cleanJson = RemoveComments(jsonAsset.text);

        try
        {
            ItemDatabaseWrapper wrapper = JsonUtility.FromJson<ItemDatabaseWrapper>(cleanJson);
            if (wrapper == null || wrapper.items == null || wrapper.items.Count == 0)
            {
                Debug.LogError($"Invalid/empty JSON (parsed items count: {wrapper?.items?.Count ?? 0})! Content was: '{jsonAsset.text}'. Falling back.");
                goto Fallback;
            }
            _ingredients.Clear();
            int loadedCount = 0;
            foreach (var itemData in wrapper.items)
            {
                if (System.Enum.IsDefined(typeof(IngredientType), itemData.type))
                {
                    IngredientType type = (IngredientType)itemData.type;
                    Ingredient ingredient = new Ingredient(type);
                    _ingredients.Add(type, ingredient);
                    loadedCount++;
                    Debug.Log($"Added: {type} (id: {itemData.id}, name: {itemData.name})");
                }
                else
                {
                    Debug.LogWarning($"Skipping invalid type: {itemData.type}");
                }
            }
            Debug.Log($"Loaded {loadedCount} ingredients from JSON. Total in Dict: {_ingredients.Count}");
            Debug.Log("=== LoadIngredientFromDB End (Success) ===");
            return;
        }
        catch (System.ArgumentException e)
        {
            Debug.LogError($"JSON Parse error (format issue): {e.Message}. Raw JSON: {jsonAsset.text}. Falling back.");
            goto Fallback;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Unexpected JSON error: {e.Message}. Falling back.");
            goto Fallback;
        }
    Fallback:
        Debug.Log("=== Falling back to hardcoded ===");
        _ingredients.Clear();
        _ingredients.Add(IngredientType.Wood, new Ingredient(IngredientType.Wood));
        _ingredients.Add(IngredientType.Iron, new Ingredient(IngredientType.Iron));
        _ingredients[IngredientType.Wood].Gather(0f);
        _ingredients[IngredientType.Iron].Gather(0f);
        Debug.Log("Hardcoded Wood/Iron added (Amount: 0 each)");
        Debug.Log("=== LoadIngredientFromDB End (Fallback) ===");
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
}