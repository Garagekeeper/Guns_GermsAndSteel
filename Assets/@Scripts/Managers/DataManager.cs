using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public interface IRALoader<RItem>
{
    List<RItem> MakeArray();
}


public class DataManager
{
    public Dictionary<int, Data.ItemData> ItemDic { get; private set; } = new Dictionary<int, Data.ItemData>();
    public Dictionary<int, Data.MonsterData> MonsterDic { get; private set; } = new Dictionary<int, Data.MonsterData>();
    public Dictionary<int, Data.RoomData> RoomDicTemp { get; private set; } = new();
    public Dictionary<ERoomType, Dictionary<int, List<string>>> RoomDic { get; private set; } = new();
    public List<int> GoldArray { get; private set; } = new();
    public List<int> ShopArray { get; private set; } = new();
    public List<int> SecretArray { get; private set; } = new();
    public List<int> BossArray { get; private set; } = new();

    public List<Data.RoomItemArrayData> RoomItemArray {  get; private set; } = new();  


    public void Init()
    {
        ItemDic = LoadJson<Data.ItemDataLoader, int, Data.ItemData>("Item_Data").MakeDict();
        MonsterDic = LoadJson<Data.MonsterDataLoader, int, Data.MonsterData>("Monster_Data").MakeDict();
        RoomDicTemp = LoadJson<Data.RoomDataLoader, int, Data.RoomData>("Room_Data").MakeDict();
        RoomItemArray = LoadJson<Data.RoomItemArrayDataLoader, Data.RoomItemArrayData>("Item_Array_Data").MakeArray();

        SetRoomData();
        SetItemArray();
    }
    private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        Debug.Log($"{path}");
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"{path}");
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }

    private Loader LoadJson<Loader, RItem>(string path) where Loader : IRALoader<RItem>
    {
        Debug.Log($"{path}");
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"{path}");
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }

    private void SetRoomData()
    {
        RoomDic[ERoomType.Start] = new Dictionary<int, List<string>>();
        RoomDic[ERoomType.Normal] = new Dictionary<int, List<string>>();
        RoomDic[ERoomType.Gold] = new Dictionary<int, List<string>>();
        RoomDic[ERoomType.Sacrifice] = new Dictionary<int, List<string>>();
        RoomDic[ERoomType.Curse] = new Dictionary<int, List<string>>();
        RoomDic[ERoomType.Shop] = new Dictionary<int, List<string>>();
        RoomDic[ERoomType.Boss] = new Dictionary<int, List<string>>();
        RoomDic[ERoomType.Angel] = new Dictionary<int, List<string>>();
        RoomDic[ERoomType.Devil] = new Dictionary<int, List<string>>();
        RoomDic[ERoomType.Secret] = new Dictionary<int, List<string>>();

        foreach (Data.RoomData roomData in RoomDicTemp.Values)
        {
            if (!RoomDic[roomData.RoomType].ContainsKey(roomData.Stage))
                RoomDic[roomData.RoomType][roomData.Stage] = new List<string>();

            RoomDic[roomData.RoomType][roomData.Stage].Add(roomData.PrefabName);
        }

        RoomDicTemp.Clear();
        RoomDicTemp = null;
    }

    private void SetItemArray()
    {
        foreach (var item in RoomItemArray)
        {
            if (item.RoomType.Count < 1) return;
            foreach (var type in item.RoomType)
            {
                if (type == ERoomType.Gold)
                {
                    GoldArray.Add(item.ItemId);
                }
                else if (type == ERoomType.Shop)
                {
                    ShopArray.Add(item.ItemId);
                }
                else if (type == ERoomType.Secret)
                {
                    SecretArray.Add(item.ItemId);
                }
                else if (type == ERoomType.Boss)
                {
                    BossArray.Add(item.ItemId);
                }
                //TODO Angel, Devile
            }
        }
    }
}
