#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SceneSaveTool
{
    [MenuItem("Tools/PlayMode/Save Open Scenes")] // Ctrl/Cmd + Shift + Alt + S
    public static void SaveOpenScenes()
    {
        // 플레이 중에도 호출 가능. 저장 후 플레이를 멈추면 저장본으로 재로딩됩니다.
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("[PlayMode Save] Open Scenes & Assets saved.");
    }
}
#endif