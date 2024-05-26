using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class PlayingUI : UI_Base
{
    enum Images
    {
        SpaceItem,
        ChargeBarBG,
        ChargedBar,
        UnChargedBar1,
        UnChargedBar2,
        UnChargedBar3,
        UnChargedBar4,
        UnChargedBar5,
        UnChargedBar6,
        ChargeBar,
        QItem,
    }
    protected override void Init()
    {
        base.Init();

        BindImage(typeof(Images));
        Managers.Game.ChargeBarEnevnt -= UseAllGage;
        Managers.Game.ChargeBarEnevnt += UseAllGage;
        Managers.Game.ChargeBarEnevnt -= ChargeGage;
        Managers.Game.ChargeBarEnevnt += ChargeGage;
    }

    protected void UseAllGage(int coolDownGage, int coolTime, string type)
    {
        if (type == "Up") return;

        var temp = (int)Images.UnChargedBar1;

        for (int i = 0; i < 6; i++)
        {
            GetImage(temp + i).gameObject.SetActive(true);
        }

    }

    protected void ChargeGage(int coolDownGage, int coolTime, string type)
    {
        if (type == "Down") return;

        var temp = 0 ;
        if (coolDownGage == 1)
            temp = (int)Images.UnChargedBar1;
        else 
            temp = (int)Images.UnChargedBar1 + (6 / coolTime);

        for (int i=0; i< 6/coolTime; i++)
        {
            GetImage( (temp) + i).gameObject.SetActive(false);

        }

    }

    public void ChangeChargeBarSize(string name)
    {
        Sprite sprite = Managers.Resource.Load<Sprite>(name);
        GetImage((int)Images.ChargeBar).gameObject.GetComponent<Image>().sprite = sprite;
    }

    public void ChangeSpaceItem(string name)
    {
        Sprite sprite = Managers.Resource.Load<Sprite>(name);
        GetImage((int)Images.SpaceItem).gameObject.GetComponent<Image>().sprite= sprite;
    }
    public void ChangeQItem(string name)
    {
        Sprite sprite = Managers.Resource.Load<Sprite>(name);
        GetImage((int)Images.QItem).gameObject.GetComponent<Image>().sprite= sprite;
    }


}
