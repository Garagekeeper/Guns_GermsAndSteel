using Data;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static Define;

public class Item
{
    public int TemplateId { get; set; }
    public string Description {  get; set; }
    public string SpriteName { get; set; }
    public string Name { get; set; }
    public int CoolTime { get; private set; }
    public EItemType ItemType { get; set; }

    public float Hp { get; set; }
    public float DmgUp { get; set; }
    public float FlatDmgUp { get; set; }
    public float Multiplier { get; set; }
    public float Tears { get; set; }
    public float Range { get; set; }
    public float ShotSpeed { get; set; }
    public float Speed { get; set; }
    public float Luck { get; set; }
    public int Life { get; set; }
    public string SetItem { get; set; }
    public EPICKUP_TYPE PickupType { get; set; }
    public int PickupCount { get; set; }
    public EShotType ShotType { get; set; }
    public ESpecialEffectOfActive EffectOfActive { get; set; }
    public int FamiliarID { get; set; }


    public int CurrentGage { get; set; }



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
        Description = TemplateData.Description;
        Name = TemplateData.Name;
        CoolTime = TemplateData.CoolTime;
        ItemType = TemplateData.Type;
        Hp = TemplateData.Hp;
        DmgUp = TemplateData.DmgUp;
        FlatDmgUp = TemplateData.FlatDmgUp;
        Multiplier = TemplateData.Multiplier;
        Tears = TemplateData.Tears;
        Range = TemplateData.Range;
        ShotSpeed = TemplateData.ShotSpeed;
        Speed = TemplateData.Speed;
        Luck = TemplateData.Luck;
        SetItem = TemplateData.SetItem;
        PickupType = TemplateData.PickupType;
        PickupCount = TemplateData.PickupCount;
        ShotType = TemplateData.ShotType;
        EffectOfActive = TemplateData.EffectOfActive;
        CurrentGage = CoolTime;

    }
}
