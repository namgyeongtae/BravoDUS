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

    // ================== 여기부터 진짜 구현 ==================

    private SaveData CollectCurrentGameState()
    {
        var data = new SaveData();
        data.version = 1;
        data.savedAt = System.DateTime.UtcNow.Ticks;

        // 1) 자원(Ingredient) 저장
        data.commodities = new CommoditySaveData();
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
        data.cityStats = new CityStatsSaveData
        {
            responsePower = city.ResponsePower,
            suppressPower = city.SuppressPower,
            healPower = city.HealPower
        };

        // 3) 빌딩, 인벤토리는 나중에 채움
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
        data.inventory = new List<ItemSaveData>();

        return data;
    }

    private void ApplyLoadedGameState(SaveData data)
    {
        
        // 1) 자원 복원 (이미 구현되어 있던 부분 유지)
        var dict = Managers.Commodity.Ingredients;
        foreach (var kvp in dict)
        {
            kvp.Value.SetAmount(0); // 또는 Consume로 0 만들기
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

        if (data.commodities != null && data.commodities.ingredients != null)
        {
            foreach (var ingData in data.commodities.ingredients)
            {
                IngredientType type = (IngredientType)ingData.type;
                var ingredient = Managers.Commodity.GetIngredient(type);
                if (ingredient != null)
                {
                    // 원하는 방식 선택:
                    // ① 현재 값 무시하고 덮어쓰기
                    ingredient.SetAmount(ingData.amount);

                    // ② 0에서 시작해서 Gather로 채우기 (SetAmount 없을 때)
                    // ingredient.Gather(ingData.amount);
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

        // 3) 기존 빌딩 제거 🔥
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

        // 4) 세이브된 빌딩 다시 생성 🔥
        if (data.buildings != null)
        {
            foreach (var bData in data.buildings)
            {
                SpawnBuildingFromSave(bData);
            }
        }

        Debug.Log("ApplyLoadedGameState: resources + CityStat + buildings restored.");
    }

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

        // 3) 그냥 Instantiate로 생성 (ResourceManager 안 써도 됨)
        GameObject go = Instantiate(prefab);
        go.name = prefab.name; // 보기 좋게 이름 맞춰주기

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
