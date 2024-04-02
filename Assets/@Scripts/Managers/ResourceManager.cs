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

    //Addresible���� �ش� lable�� ���� ���ҽ� �ε�

    public void LoadAsync<T> (string key, Action<T> callback = null) where T : UnityEngine.Object
    {
        //Cache
        if (_resources.TryGetValue(key, out UnityEngine.Object resource))
        {
            callback?.Invoke(resource as T);
            return;
        }

        string loadKey = key;

        //LoadAssetAsync�� ȣ���ص� �ٷ� ����� �� �ִ°��� �ƴ϶�
        //��ȯ�� AsyncOperationHandle�� ���ؼ� �����ϰ� ����� �� �ִٰ� �Ѵ�.
        //����� ����Ϸ��� �Ⱓ���� �ڵ� ������Ʈ�� �����ؾ� �Ѵٰ� ��
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
