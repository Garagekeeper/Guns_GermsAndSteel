using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;
using static CoroutineHelper;
using System.Collections;

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
    public Dictionary<int, Data.RoomData> RoomDicTotal { get; private set; } = new();
    public Dictionary<ERoomType, Dictionary<int, List<int>>> RoomDic { get; private set; } = new();
    public List<int> GoldArray { get; private set; } = new();
    public List<int> ShopArray { get; private set; } = new();
    public List<int> SecretArray { get; private set; } = new();
    public List<int> BossArray { get; private set; } = new();
    public List<int> CurseArray { get; private set; } = new();

    public List<Data.RoomItemArrayData> RoomItemArray { get; private set; } = new();


    public void Init()
    {
        ItemDic = LoadJson<Data.ItemDataLoader, int, Data.ItemData>("Item_Data").MakeDict();
        MonsterDic = LoadJson<Data.MonsterDataLoader, int, Data.MonsterData>("Monster_Data").MakeDict();
        RoomDicTotal = LoadJson<Data.RoomDataLoader, int, Data.RoomData>("Room_Data").MakeDict();
        RoomItemArray = LoadJson<Data.RoomItemArrayDataLoader, Data.RoomItemArrayData>("Item_Array_Data").MakeArray();

        SetRoomData();
        SetItemArray();
    }
    private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        //Debug.Log($"{path}");
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
        RoomDic[ERoomType.Start] = new Dictionary<int, List<int>>();
        RoomDic[ERoomType.Normal] = new Dictionary<int, List<int>>();
        RoomDic[ERoomType.Gold] = new Dictionary<int, List<int>>();
        RoomDic[ERoomType.Sacrifice] = new Dictionary<int, List<int>>();
        RoomDic[ERoomType.Curse] = new Dictionary<int, List<int>>();
        RoomDic[ERoomType.Shop] = new Dictionary<int, List<int>>();
        RoomDic[ERoomType.Boss] = new Dictionary<int, List<int>>();
        RoomDic[ERoomType.Angel] = new Dictionary<int, List<int>>();
        RoomDic[ERoomType.Devil] = new Dictionary<int, List<int>>();
        RoomDic[ERoomType.Secret] = new Dictionary<int, List<int>>();

        foreach (Data.RoomData roomData in RoomDicTotal.Values)
        {
            if (!RoomDic[roomData.RoomType].ContainsKey(roomData.Stage))
                RoomDic[roomData.RoomType][roomData.Stage] = new List<int>();

            RoomDic[roomData.RoomType][roomData.Stage].Add(roomData.DataId);
        }
    }

    public void SetItemArray()
    {
        GoldArray.Clear();
        ShopArray.Clear();
        SecretArray.Clear();
        BossArray.Clear();
        CurseArray.Clear();
        foreach (var item in RoomItemArray)
        {
            if (item.RoomType == ERoomType.Gold)
            {
                GoldArray = new List<int>(item.ItemId);
            }
            else if (item.RoomType == ERoomType.Shop)
            {
                ShopArray = new List<int>(item.ItemId);
            }
            else if (item.RoomType == ERoomType.Secret)
            {
                SecretArray = new List<int>(item.ItemId);
            }
            else if (item.RoomType == ERoomType.Boss)
            {
                BossArray = new List<int>(item.ItemId);
            }
            else if (item.RoomType == ERoomType.Curse)
            {
                CurseArray = new List<int>(item.ItemId);
            }
        }

    }
}
