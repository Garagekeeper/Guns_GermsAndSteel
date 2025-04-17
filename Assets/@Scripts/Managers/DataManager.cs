using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}


public class DataManager
{
    public Dictionary<int, Data.ItemData> ItemDic { get; private set; } = new Dictionary<int, Data.ItemData>();
    public Dictionary<int, Data.MonsterData> MonsterDic { get; private set; } = new Dictionary<int, Data.MonsterData>();
    public Dictionary<int, Data.RoomData> RoomDicTemp { get; private set; } = new();
    public Dictionary<ERoomType, Dictionary<int, List<string>>> RoomDic { get; private set; } = new();

    public void Init()
    {
        ItemDic = LoadJson<Data.ItemDataLoader, int, Data.ItemData>("Item_Data").MakeDict();
        MonsterDic = LoadJson<Data.MonsterDataLoader, int, Data.MonsterData>("Monster_Data").MakeDict();
        RoomDicTemp = LoadJson<Data.RoomDataLoader, int, Data.RoomData>("Room_Data").MakeDict();

        SetRoomData();
    }
    private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
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
}
