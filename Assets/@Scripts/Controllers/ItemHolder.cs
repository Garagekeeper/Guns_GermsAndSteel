using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ItemHolder : MonoBehaviour
{
    public int ItemId;
    public EItemType ItemType { get; set; } = EItemType.Null;

    public void ChangeItemOnItemHolder(int TemplateID)
    {
        string ItemSpriteName;
        if (TemplateID == 0)
        {
            ItemSpriteName = "None";

        }
        else
            ItemSpriteName = Managers.Data.ItemDic[TemplateID].SpriteName;
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>(ItemSpriteName);
        ItemId = TemplateID;

    }

}
