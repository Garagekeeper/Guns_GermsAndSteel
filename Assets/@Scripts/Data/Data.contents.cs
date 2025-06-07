using System;
using System.Collections;
using System.Collections.Generic;
using static Define;


namespace Data
{
    [Serializable]
    public class BaseData
    {
        public int DataId;
        public string Description;
        public string SpriteName;
    }

    [Serializable]
    public class ItemData : BaseData
    {
        public string Name;
        public int CoolTime;
        public EItemType Type;
        public float Hp;
        public float DmgUp;
        public float FlatDmgUp;
        public float Multiplier;
        public float Tears;
        public float Range;
        public float ShotSpeed;
        public float Speed;
        public float Luck;
        public int Life;
        public string SetItem;
        public EPICKUP_TYPE PickupType;
        public int PickupCount;
        public EShotType ShotType;
        public ESpecialEffectOfActive EffectOfActive;
        public int Weight;
    }

    [Serializable]
    public class MonsterData : BaseData
    {
        public string PrefabName;
    }

    [Serializable]
    public class RoomData
    {
        public int DataId;
        public ERoomType RoomType;
        public int Stage = 0;
        public string PrefabName;
        public EDeadEndType CannotSpawnType;

    }

    [Serializable]
    public class RoomItemArrayData
    {
        public int DataId;
        public ERoomType RoomType;
        public List<int> ItemId;
    }


    [Serializable]
    public class ItemDataLoader : ILoader<int, ItemData>
    {
        public List<ItemData> items = new List<ItemData>();

        public Dictionary<int, ItemData> MakeDict()
        {
            Dictionary<int, ItemData> dict = new Dictionary<int, ItemData>();
            foreach (ItemData item in items)
                dict.Add(item.DataId, item);

            return dict;
        }
    }

    [Serializable]
    public class MonsterDataLoader : ILoader<int, MonsterData>
    {
        public List<MonsterData> monsters = new List<MonsterData>();
        public Dictionary<int, MonsterData> MakeDict()
        {
            Dictionary<int, MonsterData> dict = new Dictionary<int, MonsterData>();
            foreach (MonsterData monster in monsters)
                dict.Add(monster.DataId,monster);

            return dict;
        }
    }

    [Serializable]
    public class RoomDataLoader : ILoader<int, RoomData>
    {
        public List<RoomData> rooms = new List<RoomData>();

        public Dictionary<int, RoomData> MakeDict()
        {
            Dictionary<int, RoomData> dict = new();
            foreach (RoomData room in rooms)
                dict.Add(room.DataId, room);

            return dict;
        }
    }

    public class RoomItemArrayDataLoader : IRALoader<RoomItemArrayData>
    {
        public List<RoomItemArrayData> RoomItemArray = new();


        public List<RoomItemArrayData> MakeArray()
        {
            return RoomItemArray;
        }
    }


}