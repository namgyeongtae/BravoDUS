using System.Collections.Generic;
using UnityEngine;

public class ListWrapper<T>
{
    public List<T> items;
}

public class JsonUtils
{
    public static List<T> SerializeList<T>(string json)
    {
        string jsonPath = $"Json/{json}";
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonPath);
        if (jsonFile == null)
        {
            Debug.LogError($"{jsonPath}.json 파일을 찾을 수 없음!");
            return null;
        }

        var wrapper = JsonUtility.FromJson<ListWrapper<T>>(jsonFile.text);

        return wrapper.items;
    }
}
