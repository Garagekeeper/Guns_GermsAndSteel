using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Define;

public class Item
{
    public int TemplateId { get; set; }

    public string SpriteName { get; set; }
    public string Name { get; set; }
    public int CoolTime { get; private set; }
    public EItemType ItemType { get; set; }

    public int Hp { get; set; }
    public float AttackDamage { get; set; }
    public float Tears { get; set; }
    public float Range { get; set; }
    public float ShotSpeed { get; set; }
    public float Speed { get; set; }
    public float Luck { get; set; }
    public int Life { get; set; }
    public string SetItem { get; set; }
    public EShotType ShotType { get; set; }
    public ESpecialEffectOfActive EffectOfActive { get; set; }
    public int FamiliarID { get; set; }


    public int CoolDownGage { get; set; }



    public ItemData TemplateData
    {
        get
        {
            return Managers.Data.ItemDic[TemplateId];
        }
    }

    public Item() { }

    public Item(int itemId)
    {
        TemplateId = itemId;
        SpriteName = TemplateData.SpriteName;
        Name = TemplateData.Name;
        CoolTime = TemplateData.CoolTime;
        ItemType = TemplateData.Type;
        AttackDamage = TemplateData.AttackDamage;
        Tears = TemplateData.Tears;
        Range = TemplateData.Range;
        ShotSpeed = TemplateData.ShotSpeed;
        Speed = TemplateData.Speed;
        Luck = TemplateData.Luck;
        SetItem = TemplateData.SetItem;
        ShotType = TemplateData.ShotType;
        EffectOfActive = TemplateData.EffectOfActive;
        CoolDownGage = CoolTime;

    }

    public void ChangeItem(int itemId)
    {
        if (itemId == 0)
        {
            TemplateId = 0;
            SpriteName = "NONE";
        }
        else
        {
            TemplateId = itemId;
            SpriteName = TemplateData.SpriteName;
            Name = TemplateData.Name;
            CoolTime = TemplateData.CoolTime;
            ItemType = TemplateData.Type;
            AttackDamage = TemplateData.AttackDamage;
            Tears = TemplateData.Tears;
            Range = TemplateData.Range;
            ShotSpeed = TemplateData.ShotSpeed;
            Speed = TemplateData.Speed;
            Luck = TemplateData.Luck;
            SetItem = TemplateData.SetItem;
            ShotType = TemplateData.ShotType;
            EffectOfActive = TemplateData.EffectOfActive;
            CoolDownGage = CoolTime;
        }

    }
}
