using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Item
{
    public int TemplateId { get; set; }

    public EItemType ItemType { get; set; }
    public EItemEfect ItemEfec { get; set; }

    public string Target { get; set; }

    public int Value { get; set; }

    public int CoolTime { get; private set; }
    public int CoolDownGage { get; set; }

    public ItemData TemplateData
    {
        get
        {
            return Managers.Data.ItemDic[TemplateId];
        }
    }

    public Item(int itemId)
    {
        TemplateId = itemId;
        ItemType = TemplateData.Type;
        ItemEfec = TemplateData.Effect;
        CoolTime = TemplateData.CoolTime;
        Target = TemplateData.Target;
        Value = TemplateData.Value;
        CoolDownGage = CoolTime;

    }
}
