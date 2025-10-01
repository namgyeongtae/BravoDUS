using UnityEngine;
using System.Collections.Generic;

// JSON 파일을 불러와서 Item 리스트로 변환하는 유틸리티 클래스
public static class ItemLoader
{
    // fileName: Resources 폴더 안의 파일 이름 (확장자 포함)
    public static List<Item> LoadFromJson(string fileName)
    {
        // Resources 폴더에서 텍스트 파일 불러오기
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);

        // 파일이 없으면 에러 로그 출력 후 빈 리스트 반환
        if (jsonFile == null)
        {
            Debug.LogError($"{fileName}.json 파일을 찾을 수 없음!");
            return new List<Item>();
        }

        // JSON 문자열 → ItemDataWrapper 객체로 변환
        ItemDataWrapper wrapper = JsonUtility.FromJson<ItemDataWrapper>(jsonFile.text);

        // wrapper.items 반환 (실제 아이템 리스트)
        return wrapper.items;
    }
}