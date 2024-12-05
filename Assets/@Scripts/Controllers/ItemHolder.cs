using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class ItemHolder : MonoBehaviour
{
    public EItemType ItemType { get; set; } = EItemType.Null;

    public Item ItemOfItemHolder { get; set; }

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
