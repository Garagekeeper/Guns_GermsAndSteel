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
        public string Target;
        public EItemEfect Effect;
        public int Value;
        public int Weight;
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
}