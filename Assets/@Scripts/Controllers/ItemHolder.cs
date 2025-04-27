using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static Define;
using static Utility;

public class ItemHolder : BaseObject
{
    public EItemType ItemType { get; set; } = EItemType.Null;

    public Item ItemOfItemHolder { get; set; }

    public void Init(RoomClass room, Vector3Int pos = new Vector3Int())
    {
        SetItem(room);

        var parent = room.Obstacle.transform;

        //setPrice
        if (room.RoomType == ERoomType.Shop)
        {
            parent = FindChildByName(room.Transform, "ShopItems");
            FindChildByName(transform, "ShopItemPrice").GetComponent<TextMeshPro>().gameObject.SetActive(true);
            FindChildByName(transform, "ShopItemPrice").GetComponent<TextMeshPro>().text = "15";
        }

        //set pos
        transform.SetParent(parent);
        transform.localPosition = pos + new Vector3(0.5f, 0.5f, 0f);



    }

    public void SetItem(RoomClass room)
    {
        int templateId = Managers.Game.SelectItem(room.RoomType);
        ItemOfItemHolder = new Item(templateId);
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>(Managers.Data.ItemDic[templateId].SpriteName);
    }

    public void ChangeItemOnItemHolder(Item item)
    {
        string ItemSpriteName;
        if (item == null)
        {
            ItemSpriteName = "None";

        }
        else
            ItemSpriteName = Managers.Data.ItemDic[item.TemplateId].SpriteName;

        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>(ItemSpriteName);
        ItemOfItemHolder = item;
    }

}
