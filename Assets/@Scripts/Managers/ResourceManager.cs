using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class ResourceManager
{
    private Dictionary<string, UnityEngine.Object> _resources = new Dictionary<string, UnityEngine.Object>();
    private Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();



    public T Load<T>(string key) where T : UnityEngine.Object
    {
        if (_resources.TryGetValue(key, out Object resource))
        {
            return resource as T;
        }
        return null;
    }

    #region Addresible
    //Addresible에서 해당 lable을 가진 리소스 로딩

    public void LoadAsync<T>(string key, bool isSprite = false, Action<T> callback = null) where T : UnityEngine.Object
    {
        //Cache
        if (_resources.TryGetValue(key, out UnityEngine.Object resource))
        {
            callback?.Invoke(resource as T);
            return;
        }
        string loadKey = key;

        if (isSprite)
        {
            var asyncOpHandle = Addressables.LoadAssetAsync<IList<Sprite>>(loadKey);
            asyncOpHandle.Completed += (op) =>
            {
                foreach (var val in op.Result)
                {
                    key = val.name;
                    if (_resources.ContainsKey(key)) continue;
                    _resources.Add(key, val);
                    _handles.Add(key, asyncOpHandle);
                }
                callback?.Invoke(op.Result as T);
            };
        }
        else
        {
            var asyncOpHandle = Addressables.LoadAssetAsync<T>(loadKey);
            asyncOpHandle.Completed += (op) =>
            {

                _resources.Add(key, op.Result);
                _handles.Add(key, asyncOpHandle);
                callback?.Invoke(op.Result);
            };

        }

        // 라벨 이름으로 라벨을 찾습니다.
        //AddressableAssetGroup[] labeledGroups = settings.FindGroup

        //LoadAssetAsync를 호출해도 바로 사용할 수 있는것이 아니라
        //반환된 AsyncOperationHandle를 통해서 접근하고 사용할 수 있다고 한다.
        //결과를 사용하려는 기간에는 핸들 오브젝트를 유지해야 한다고 함

    }


    public void LoadAllAsync<T>(string lable, Action<string, int, int> callback) where T : UnityEngine.Object
    {
        var asyncOpHandle = Addressables.LoadResourceLocationsAsync(lable, typeof(T));
        asyncOpHandle.Completed += (op) =>
        {
            int total = op.Result.Count;
            int loaded = 0;
            bool isSprite = false;

            foreach (var result in op.Result)
            {
                isSprite = false;
                var tempChar = result.PrimaryKey;
                if (result.InternalId.Contains(".png"))
                    isSprite = true;

                LoadAsync<T>(result.PrimaryKey, isSprite, (obj) =>
                {
                    loaded++;
                    callback?.Invoke(result.PrimaryKey, loaded, total);
                });
            }
        };
    }

    #endregion
    public GameObject Instantiate(string key, Transform parent = null)
    {
        // key에 해당하는 프리팹을 인스턴스화한다.
        GameObject prefab = Load<GameObject>(key);

        if (prefab == null)
        {
            Debug.LogError($"Faild to load:{key} prefab");
            return null;
        }

        GameObject go = Object.Instantiate(prefab, parent);
        go.name = prefab.name;

        return go;
    }


}
