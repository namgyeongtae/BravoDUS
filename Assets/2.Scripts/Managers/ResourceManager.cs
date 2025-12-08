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
        // return Resources.Load<T>($"ScriptableObjects/{path}");
        var handle = Addressables.LoadAssetAsync<T>(path);

        handle.WaitForCompletion();

        return handle.Result;
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

        GameObject go = instHandle.WaitForCompletion();     // 사용 시 주의 !!!!! 
        // -> WaitForCompletion()은 동기 함수를 비동기 처럼 돌리고 싶을 때 사용하는데 이거 쓰면 메인 쓰레드가 블로킹 된다. (즉, 결국 완전히 싱글 스레드로 돌리는데 이걸 블로킹해서 비동기 효과를 보는 것)
        // -> 블로킹 된 상태로 메인 쓰레드에 진입하고자 하는 코드가 실행되면 오류가 발생!
        // -> 이 타이밍에 다른 오브젝트들로 인해 Awake나 Start 처럼 MonoBehaviour에 의해 메인 스레드에 진입할 경우 개발자가 해결하기 어렵다.
        // -> 다른 함수로 인해 메인 스레드에 진입하면 그 함수의 호출을 늦추어 해결할 수는 있지만 MonoBehaviour에 의해 호출 되는 함수(Awake, Start 등) 
        // -> 개발자가 직접 그 함수들의 호출 타이밍을 늦출 수 없기 때문에 해결하기가 난처해진다.
        // -> 마주치는 에러 : Exception: Reentering the Update method is not allowed. This can happen when calling WaitForCompletion on an operation while inside of a callback.

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