using UnityEngine;

// 아이템 종류를 구분하는 열거형
// 숫자로 JSON에 저장할 때: 0 = InstantBuild, 1 = SpeedUp
public enum ItemType
{
    InstantBuild,   // 즉시 건설 완료
    SpeedUp         // 건설 시간 단축
}

[System.Serializable] // JsonUtility가 직렬화/역직렬화를 지원하도록 표시
public class Item
{
    public string itemName;     // 아이템 이름
    public string description;  // 아이템 설명
    public ItemType itemType;   // 아이템 타입 (InstantBuild / SpeedUp)
    public float value;         // 아이템 효과 값 (단축 시간 등)
    public int quantity;        // 현재 개수

    // 아이템 사용 로직
    public void Use(Building building)
    {
        if (quantity <= 0) // 개수가 없으면 사용 불가
        {
            Debug.Log($"{itemName}이(가) 없음!");
            return;
        }

        switch (itemType)
        {
            case ItemType.InstantBuild:
                building.constructionTime = 0f; // 즉시 건설 완료
                Debug.Log($"{itemName} 사용! 건설 즉시 완료됨");
                break;

            case ItemType.SpeedUp:
                building.constructionTime -= value; // 일정 시간 단축
                if (building.constructionTime < 0f) building.constructionTime = 0f; // 음수 방지
                Debug.Log($"{itemName} 사용! 건설 시간이 {value}초 줄어듦");
                break;
        }

        // 사용 후 개수 차감
        quantity--;
    }
}
