using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class SaveLoadService : MonoBehaviour
{
    private const string SaveFileName = "save.json";
    private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    [SerializeField] private BuildingPrefabDB _buildingPrefabDB;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Save key pressed");
            SaveGame();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Load key pressed");
            LoadGame();
        }
    }

    // ================== Save ==================
    public void SaveGame()
    {
        SaveData data = CollectCurrentGameState();

        string json = JsonUtility.ToJson(data, true);

        try
        {
            File.WriteAllText(SavePath, json);
            Debug.Log($"Game saved to: {SavePath}\n{json}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveGame failed: {e}");
        }
    }

    // 현재 게임 상태 → SaveData로 수집
    private SaveData CollectCurrentGameState()
    {
        var data = new SaveData();
        data.version = 1;
        data.savedAt = System.DateTime.UtcNow.Ticks;

        // 1) 자원(돈 + 기존 Ingredient) 저장
        data.commodities = new CommoditySaveData();

        // 💰 Money 저장
        data.commodities.money = Managers.Commodity.Money;
        Debug.Log($"[Save] Money = {data.commodities.money}");

        // ⚙️ 기존 Ingredient(Wood, Iron 등)도 같이 저장 (나중에 완전 제거 가능)
        var dict = Managers.Commodity.Ingredients; // IReadOnlyDictionary
        foreach (var kvp in dict)
        {
            IngredientType type = kvp.Key;
            Ingredient ingredient = kvp.Value;

            data.commodities.ingredients.Add(new IngredientSaveData
            {
                type = (int)type,
                amount = ingredient.Amount
            });
        }

        // 2) CityStat 저장 (EventManager 내부)
        var city = Managers.Event.CityStat;
        data.cityStats = new CityStatSaveData
        {
            responsePower = city.ResponsePower,
            suppressPower = city.SuppressPower,
            healPower = city.HealPower
        };

        // 3) 빌딩 저장
        data.buildings = new List<BuildingSaveData>();

        var crafting = CraftingManager.Instance;
        if (crafting != null && crafting.Buildings != null && crafting.Buildings.Count > 0)
        {
            foreach (var b in crafting.Buildings)
            {
                if (b == null) continue;
                data.buildings.Add(b.ToSaveData());
            }
        }
        else
        {
            // 혹시 CraftingManager 없거나 비어 있으면 Find로 fallback
            var allBuildings = GameObject.FindObjectsOfType<Building>();
            foreach (var b in allBuildings)
            {
                data.buildings.Add(b.ToSaveData());
            }
        }

        // 4) 인벤토리 (아직 사용 안 하면 빈 리스트 유지)
        data.inventory = new List<InventoryItemSaveData>();

        return data;
    }

    // ================== Load ==================
    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("LoadGame: save file not found.");
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            Debug.Log($"Raw JSON:\n{json}");

            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data == null)
            {
                Debug.LogError("LoadGame: JsonUtility returned null.");
                return;
            }

            ApplyLoadedGameState(data);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LoadGame failed: {e}");
        }
    }

    private void ApplyLoadedGameState(SaveData data)
    {
        // 0) 💰 Money 복원
        if (data.commodities != null)
        {
            float money = data.commodities.money;
            Managers.Commodity.SetMoney(money);
            Debug.Log($"[Load] Money = {money}");
        }
        else
        {
            Managers.Commodity.SetMoney(0f);
            Debug.Log("[Load] No commodities found. Money reset to 0.");
        }

        // 1) Ingredient 자원 복원 (기존 구조 유지 – 나중에 삭제 가능)
        var dict = Managers.Commodity.Ingredients;
        foreach (var kvp in dict)
        {
            kvp.Value.SetAmount(0);
        }

        if (data.commodities != null && data.commodities.ingredients != null)
        {
            foreach (var ingData in data.commodities.ingredients)
            {
                IngredientType type = (IngredientType)ingData.type;
                var ingredient = Managers.Commodity.GetIngredient(type);
                if (ingredient != null)
                {
                    ingredient.SetAmount(ingData.amount);
                }
            }
        }

        // 2) CityStat 복원
        if (data.cityStats != null)
        {
            var city = Managers.Event.CityStat;
            city.ResponsePower = data.cityStats.responsePower;
            city.SuppressPower = data.cityStats.suppressPower;
            city.HealPower = data.cityStats.healPower;
        }

        // 3) 기존 빌딩 제거
        var crafting = CraftingManager.Instance;
        if (crafting != null && crafting.Buildings != null)
        {
            foreach (var b in crafting.Buildings)
            {
                if (b == null) continue;
                GameObject.Destroy(b.gameObject);
            }
            crafting.Buildings.Clear();
        }
        else
        {
            var allBuildings = GameObject.FindObjectsOfType<Building>();
            foreach (var b in allBuildings)
            {
                GameObject.Destroy(b.gameObject);
            }
        }

        // 4) 세이브된 빌딩 다시 생성
        if (data.buildings != null)
        {
            foreach (var bData in data.buildings)
            {
                SpawnBuildingFromSave(bData);
            }
        }

        Debug.Log("ApplyLoadedGameState: money + resources + CityStat + buildings restored.");
    }

    // ================== Building Spawn ==================
    private Building SpawnBuildingFromSave(BuildingSaveData data)
    {
        // 0) DB 체크
        if (_buildingPrefabDB == null)
        {
            Debug.LogError("[SaveLoadService] BuildingPrefabDB is not assigned.");
            return null;
        }

        // 1) 세이브된 타입 → enum
        BuildingType type = (BuildingType)data.buildingType;

        // 2) DB에서 프리팹 가져오기
        GameObject prefab = _buildingPrefabDB.GetPrefab(type);
        if (prefab == null)
        {
            Debug.LogError($"[SaveLoadService] Prefab not found for BuildingType: {type}");
            return null;
        }

        // 3) Instantiate
        GameObject go = Instantiate(prefab);
        go.name = prefab.name;

        var building = go.GetComponent<Building>();
        if (building == null)
        {
            Debug.LogError($"Spawned prefab has no Building component: {prefab.name}");
            return null;
        }

        // 4) 세이브 데이터로 상태 복원
        building.ApplySaveData(data);

        // 5) CraftingManager 리스트에도 등록
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.AddBuilding(building);
        }

        return building;
    }
}
