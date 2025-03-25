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
        public int Hp;
        public float AttackDamage;
        public float Tears;
        public float Range;
        public float ShotSpeed;
        public float Speed;
        public float Luck;
        public int Life;
        public string SetItem;
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
}