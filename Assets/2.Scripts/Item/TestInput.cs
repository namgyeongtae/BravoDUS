using UnityEngine;

// 키보드 입력으로 아이템을 테스트하는 스크립트
// 빈 오브젝트에 붙여서 ItemManager, Building 연결하면 됨
public class TestInput : MonoBehaviour
{
        public Building targetBuilding; // 아이템을 적용할 대상 건물 (테스트용)

    void Update()
    {
        // 1번 키 → 첫 번째 아이템 사용
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Managers.Item.UseItem(0, targetBuilding);
        }

        // 2번 키 → 두 번째 아이템 사용
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Managers.Item.UseItem(1, targetBuilding);
        }
    }
}
