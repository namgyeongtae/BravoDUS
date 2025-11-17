using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO; // 파일 저장

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
    public float amount; // 초기 자원 수량 (JSON에서 로드)
    public List<RequiredResource> craftingRequirements; // 옵션, 필요 시
}

[System.Serializable]
public class RequiredResource
{
    public int type;
    public int amount;
}

[System.Serializable]
public class ItemDatabaseWrapper // JSON 루트
{
    public List<ItemData> items;
}

public class CommodityManager : IManagerBase
{
    // ==========================
    // 💰 Money(지갑) 파트
    // ==========================
    private float _money;
    public float Money => _money;

    public void SetMoney(float amount)
    {
        _money = Mathf.Max(0, amount);
        Debug.Log($"[Wallet] SetMoney = {_money}");
    }

    public void AddMoney(float amount)
    {
        _money = Mathf.Max(0, _money + amount);
        Debug.Log($"[Wallet] AddMoney {amount} -> {_money}");
    }

    public bool HasMoney(float amount)
    {
        return _money >= amount;
    }

    public bool TrySpend(float amount)
    {
        if (_money < amount)
        {
            Debug.Log($"[Wallet] TrySpend FAIL. need:{amount}, have:{_money}");
            return false;
        }

        _money -= amount;
        Debug.Log($"[Wallet] TrySpend OK. spent:{amount}, remain:{_money}");
        return true;
    }

    public void LogMoney()
    {
        Debug.Log($"[Wallet] Current Money = {_money}");
    }

    // ==========================
    // 🪵 기존 Ingredient 파트
    // ==========================
    private Dictionary<IngredientType, Ingredient> _ingredients =
        new Dictionary<IngredientType, Ingredient>();

    public Ingredient GetIngredient(IngredientType type) =>
        _ingredients.TryGetValue(type, out var ingredient) ? ingredient : null;

    public IReadOnlyDictionary<IngredientType, Ingredient> Ingredients => _ingredients;

    public void Init()
    {
        LoadIngredientFromDB();
        LogAmounts(); // Wood, Iron 등 로드 로그

        // 🔥 테스트용 시작 머니 (나중에 필요 없으면 0으로 바꾸거나 제거해도 됨)
        SetMoney(1000f);
        LogMoney();
    }

    public void Release()
    {
        Debug.Log("=== CommodityManager Release called! ===");
        SaveAmountsToJson();
        // _ingredients.Clear(); // 필요하면 나중에 다시 켜기
    }

    private void LoadIngredientFromDB()
    {
        Debug.Log("=== LoadIngredientFromDB Start ===");
        TextAsset jsonAsset = Resources.Load<TextAsset>("Json/ItemDatabase");
        if (jsonAsset == null)
        {
            Debug.LogError("ItemDatabase.json not found! Check: Assets/Resources/Json/ItemDatabase.json");
#if UNITY_EDITOR
            string fullPath = UnityEditor.AssetDatabase.FindAssets(
                "ItemDatabase t:TextAsset",
                new[] { "Assets/Resources/Json" }).Length > 0 ? "Found" : "Not found";
            Debug.Log($"Editor search check: {fullPath}");
#endif
            return;
        }

        Debug.Log($"Loaded: {jsonAsset.name} (length: {jsonAsset.text.Length}, " +
                  $"preview: {jsonAsset.text.Substring(0, Math.Min(50, jsonAsset.text.Length))}...)");

        string cleanJson = RemoveComments(jsonAsset.text);
        Debug.Log($"Cleaned JSON full: {cleanJson}");

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
                if (Enum.IsDefined(typeof(IngredientType), itemData.type))
                {
                    IngredientType type = (IngredientType)itemData.type;
                    Ingredient ingredient = new Ingredient(type);
                    ingredient.Gather(itemData.amount); // 초기 amount 로드
                    _ingredients.Add(type, ingredient);
                    loadedCount++;
                    Debug.Log($"Added: {type} (id: {itemData.id}, name: {itemData.name}, " +
                              $"parsed amount from JSON: {itemData.amount}, " +
                              $"actual ingredient Amount after Gather: {ingredient.Amount})");
                }
                else
                {
                    Debug.LogWarning($"Skipping invalid type: {itemData.type}");
                }
            }

            Debug.Log($"Loaded {loadedCount} ingredients from JSON. Total in Dict: {_ingredients.Count}");
            Debug.Log("=== LoadIngredientFromDB End (Success) ===");
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON Parse error: {e.Message}. Stack: {e.StackTrace}." +
                           $" Raw JSON: {jsonAsset.text}. Cleaned JSON: {cleanJson}");
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
        result = System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " ");
        result = result.Replace(" {", "{").Replace("} ", "}").Replace(", ", ",");

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

    private void SaveAmountsToJson()
    {
        ItemDatabaseWrapper wrapper = new ItemDatabaseWrapper();
        wrapper.items = new List<ItemData>();

        foreach (var kvp in _ingredients)
        {
            wrapper.items.Add(new ItemData
            {
                type = (int)kvp.Key,
                amount = kvp.Value.Amount
            });
        }

        string json = JsonUtility.ToJson(wrapper, true);
        string path = Application.persistentDataPath + "/ItemDatabaseSaved.json";
        File.WriteAllText(path, json);
        Debug.Log($"Saved Amount to {path}");
    }
}
