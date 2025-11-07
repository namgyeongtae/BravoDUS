using System;
using System.Collections.Generic;
using UnityEngine;

// 게임 전체 저장 데이터의 루트
[Serializable]
public class SaveData
{
    public int version;          // 세이브 버전
    public long savedAt;         // 저장 시간 (UTC Ticks)

    public CommoditySaveData commodities;        // 자원(Ingredient)들
    public List<BuildingSaveData> buildings;     // 빌딩 상태 (다음 단계에서 채움)
    public CityStatsSaveData cityStats;          // 도시 스탯 (EventManager.CityStat)
    public List<ItemSaveData> inventory;         // 인벤토리 (나중에 ItemManager 보고 채움)
}

// ---------------- 자원(Ingredient) ----------------

[Serializable]
public class CommoditySaveData
{
    public List<IngredientSaveData> ingredients = new();
}

[Serializable]
public class IngredientSaveData
{
    public int type;      // IngredientType enum을 int로 저장
    public float amount;  // 해당 자원 보유량
}

// ---------------- 빌딩 ----------------

[Serializable]
public class BuildingSaveData
{
    public string buildingName;   // 씬에서의 이름 (ex: House_01)
    public int buildingType;      // BuildingType enum을 int로 저장

    public float posX;
    public float posY;
    public float posZ;

    public float rotY;

    public int level;             // Building.Level
    public int state;             // Building.State (int로 캐스팅)

    public bool isConstructing;
    public bool isUpgrading;
}


[Serializable]
public class BuffSaveData
{
    public string buffId;
    public float value;
    public float remainTime;
}

// ---------------- 도시 스탯 ----------------

[Serializable]
public class CityStatsSaveData
{
    public float responsePower;   // 대응력
    public float suppressPower;   // 진압력
    public float healPower;       // 치료력

    // 나중에 population, happiness 같은 거 있으면 추가
}

// ---------------- 인벤토리 ----------------

[Serializable]
public class ItemSaveData
{
    public string itemId;
    public int count;
}
