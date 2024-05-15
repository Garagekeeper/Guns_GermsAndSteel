using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    protected void UseAllGage(int coolDownGage, string type)
    {
        if (type == "UP") return;

        var temp = (int)Images.UnChargedBar1;

        for (int i = 0; i < 6; i++)
        {
            GetImage(temp + i).gameObject.SetActive(true);
        }

    }

    protected void ChargeGage(int coolDownGage, string type)
    {
        if (type == "Down") return;

        var temp = (int)Images.UnChargedBar1;

        GetImage(temp + coolDownGage).gameObject.SetActive(false);

    }


}
