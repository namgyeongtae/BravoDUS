using UnityEngine;
using System.Collections.Generic;

// JsonUtility는 배열([])을 루트로 직접 읽을 수 없음
// 따라서 JSON 루트를 감싸는 "포장 클래스"가 필요함
[System.Serializable]
public class ItemDataWrapper
{
    public List<Item> items; // JSON의 "items" 배열을 그대로 담을 필드
}
