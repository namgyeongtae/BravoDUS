using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int version = 1;
    public long savedAt;

    public CommoditySaveData commodities = new CommoditySaveData();
    public List<BuildingSaveData> buildings = new List<BuildingSaveData>();
    public CityStatSaveData cityStats = new CityStatSaveData();
    public List<InventoryItemSaveData> inventory = new List<InventoryItemSaveData>();
}

// === 자원 (돈 + 예전 재화 구조) ===
[Serializable]
public class CommoditySaveData
{
    public float money = 0f;                         // 💰 Money
    public List<IngredientSaveData> ingredients = new List<IngredientSaveData>();
}

[Serializable]
public class IngredientSaveData
{
    public int type;      // IngredientType
    public float amount;
}

// === 건물 세이브 데이터 ===
[Serializable]
public class BuildingSaveData
{
    public int buildingType;     // (int)BuildingType
    public string buildingName;  // gameObject.name

    public float posX;
    public float posY;
    public float posZ;

    public float rotY;

    public int level;            // Building.Level
    public int state;            // (int)Building.State

    public bool isConstructing;
    public bool isUpgrading;
}

// === CityStat 세이브용 ===
[Serializable]
public class CityStatSaveData
{
    public float responsePower;
    public float suppressPower;
    public float healPower;
}

// === 인벤토리 세이브 ===
[Serializable]
public class InventoryItemSaveData
{
    public string itemId;
    public int count;
}
