using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class ResourceManager
{
    private readonly Dictionary<string, AsyncOperationHandle> _handles = new();

    public T LoadSO<T>(string path) where T : ScriptableObject
    {
        return Resources.Load<T>($"ScriptableObjects/{path}");
    }

    public T Load<T>(string path) where T : Object
    {
        if (typeof(T) == typeof(GameObject))
        {
            string name = path;
            int index = name.LastIndexOf('/');
            if (index >= 0)
                name = name.Substring(index + 1);
        }
        return Resources.Load<T>(path);
    }

    public async UniTask<T> LoadAsync<T>(string key) where T : Object
    {
        // 이미 로드된 핸들이 있으면 재사용
        if (_handles.TryGetValue(key, out var h) && h.IsValid())
        {
            return h.Convert<T>().Result;
        }

        var handle = Addressables.LoadAssetAsync<T>(key);
        await handle.ToUniTask();

        if (!handle.IsValid() || handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogWarning($"[Addressables] Load 실패: {key}");
            return null;
        }

        _handles[key] = handle;
        return handle.Result;
    }

    public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        if (original == null)
        {
            Debug.LogError($"Failed to load prefab: {path}");
            return null;
        }
        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;
        return go;
    }

    public GameObject InstantiateAddressable(
        string key,
        Vector3 position,
        Quaternion rotation,
        Transform parent = null)
    {
        AsyncOperationHandle<GameObject> instHandle;
        if (parent != null)
            instHandle = Addressables.InstantiateAsync(key, position, rotation, parent);
        else
            instHandle = Addressables.InstantiateAsync(key, position, rotation);

        GameObject go = instHandle.WaitForCompletion();

        if (!instHandle.IsValid() || instHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogWarning($"[Addressables] Instantiate 실패: {key}");
            if (instHandle.IsValid()) Addressables.Release(instHandle);
            return null;
        }

        go.name = key; // 필요하면 제거
        return go;
    }

    public void Destroy(GameObject go)
    {
        if (go == null)
            return;

        Object.Destroy(go);
    }

    public void Release(GameObject instance)
    {
        if (instance != null)
            Addressables.ReleaseInstance(instance);
    }

    public IEnumerator Destroy(GameObject go, float delay)
    {
        if (go == null)
            yield break;

        yield return new WaitForSeconds(delay);
        Object.Destroy(go);
    }
}