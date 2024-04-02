using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class ResourceManager 
{
    private Dictionary<string, UnityEngine.Object> _resources = new Dictionary<string, UnityEngine.Object>();
    private Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();

    //Addresible에서 해당 lable을 가진 리소스 로딩

    public void LoadAsync<T> (string key, Action<T> callback = null) where T : UnityEngine.Object
    {
        //Cache
        if (_resources.TryGetValue(key, out UnityEngine.Object resource))
        {
            callback?.Invoke(resource as T);
            return;
        }

        string loadKey = key;

        //LoadAssetAsync를 호출해도 바로 사용할 수 있는것이 아니라
        //반환된 AsyncOperationHandle를 통해서 접근하고 사용할 수 있다고 한다.
        //결과를 사용하려는 기간에는 핸들 오브젝트를 유지해야 한다고 함
        var asyncOpHandle = Addressables.LoadAssetAsync<T>(loadKey);
        asyncOpHandle.Completed += (op) =>
        {
            _resources.Add(key, op.Result);
            _handles.Add(key, asyncOpHandle);
            callback?.Invoke(op.Result);
        };
    }


    public void LoadAllAsync<T> (string lable, Action<string, int ,int> callback) where T : UnityEngine.Object
    {
        var asyncOpHandle = Addressables.LoadResourceLocationsAsync(lable, typeof(T));
        asyncOpHandle.Completed += (op) =>
        {
            int total = op.Result.Count;
            int loaded = 0;

            foreach (var result in op.Result)
            {
                LoadAsync<T>(result.PrimaryKey, (obj) =>
                {
                    loaded++;
                    callback?.Invoke(result.PrimaryKey, loaded, total);
                });
            }
        };
    }
}
