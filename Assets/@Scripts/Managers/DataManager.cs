using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    public Dictionary<int, Data.ItemData> ItemDic { get; private set; } = new Dictionary<int, Data.ItemData>();
    public void Init()
    {
        ItemDic = LoadJson<Data.ItemDataLoader, int, Data.ItemData>("Item_ActiveData").MakeDict();
    }
        private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        Debug.Log($"{path}");
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"{path}");
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }
}
