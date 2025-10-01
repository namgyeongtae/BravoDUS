using UnityEngine;
using System.Collections.Generic;

// 아이템 인벤토리를 관리하는 매니저
public class ItemManager : MonoBehaviour
{
    public List<Item> items = new List<Item>(); // 현재 보유 중인 아이템 리스트

    private void Start()
    {
        // 게임 시작 시 JSON에서 아이템 불러오기
        // Assets/Resources/items.json
        items = ItemLoader.LoadFromJson("items.json");
        Debug.Log($"아이템 {items.Count}개 로드 완료");
    }

    // 새로운 아이템 추가)
    public void AddItem(Item newItem)
    {
        // 같은 이름의 아이템이 이미 있는지 검색
        Item existingItem = items.Find(i => i.itemName == newItem.itemName);

        if (existingItem != null)
        {
            // 있으면 개수만 증가
            existingItem.quantity += newItem.quantity;
            Debug.Log($"{newItem.itemName} {newItem.quantity}개 추가 (총 {existingItem.quantity}개");
        }
        else
        {
            // 없으면 새로 추가
            items.Add(newItem);
            Debug.Log($"{newItem.itemName} 획득! (총 {newItem.quantity}개");
        }
    }

    // 아이템 사용
    public void UseItem(int index, Building targetBuilding)
    {
        if (index < 0 || index >= items.Count) // 올바른 범위 : (index >= 0 && index < items.Count)
        {
            Debug.LogWarning("잘못된 아이템 인덱스!");
            return;
        }

        Item item = items[index];
        item.Use(targetBuilding); // 실제 효과 발동

        // 개수가 0이면 인벤토리에서 제거
        if (item.quantity <= 0)
        {
            items.RemoveAt(index);
            Debug.Log($"{item.itemName} 제거됨 (개수 0)");
        }
    }
}
