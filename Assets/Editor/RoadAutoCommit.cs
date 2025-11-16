// Editor/RoadAutoCommit.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using System;

[InitializeOnLoad]
public static class RoadAutoCommit
{
    private const string FileName = "road_edits.json";

    static RoadAutoCommit() {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Edit Mode로 완전히 돌아온 후에 적용 (씬 변경/저장 허용 시점)
        if (state == PlayModeStateChange.EnteredEditMode)
            EditorApplication.delayCall += ApplyAndSaveIfExists;
    }

    private static void ApplyAndSaveIfExists()
    {
        var path = Path.Combine(Application.persistentDataPath, FileName);
        if (!File.Exists(path)) return;

        try {
            var json = File.ReadAllText(path);
            var log = JsonUtility.FromJson<RoadEditLog>(json);
            if (log == null) { Debug.LogWarning("[RoadAutoCommit] Invalid JSON."); return; }

            var go = GameObject.Find(log.tilemapPathInScene);
            var tilemap = go ? go.GetComponent<Tilemap>() : null;
            if (!tilemap) { Debug.LogWarning($"[RoadAutoCommit] Tilemap not found: {log.tilemapPathInScene}"); return; }

            // 타일 이름 → 자산 해석 (동명이 다수면 첫 번째)
            TileBase Resolve(string tileName) {
                if (string.IsNullOrEmpty(tileName)) return null;
                var guids = AssetDatabase.FindAssets($"{tileName} t:TileBase");
                return guids.Length > 0 ? AssetDatabase.LoadAssetAtPath<TileBase>(AssetDatabase.GUIDToAssetPath(guids[0])) : null;
            }

            foreach (var c in log.placed)
                tilemap.SetTile(c.pos, Resolve(c.tileName));

            foreach (var pos in log.removed)
                tilemap.SetTile(pos, null);

            EditorUtility.SetDirty(tilemap);
            EditorSceneManager.MarkSceneDirty(tilemap.gameObject.scene);

            // 이제야 저장 가능
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();

            Debug.Log($"[RoadAutoCommit] Applied & saved scene from: {path}");
        }
        catch (Exception e) {
            Debug.LogError($"[RoadAutoCommit] Error: {e.Message}");
        }
        finally {
            // 한 번 적용된 로그는 삭제
            try { File.Delete(path); } catch {}
        }
    }
}
#endif
