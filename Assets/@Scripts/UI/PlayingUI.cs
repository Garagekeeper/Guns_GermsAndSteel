using UnityEngine;
using UnityEngine.UI;

public class PlayingUI : UI_Base
{
    enum Images
    {
        SpaceItem,
        ChargeBarBG,
        ChargedBar,
        ChargeBar,
        QItem,
        Heart1,
        Heart2,
        Heart3,
        Heart4,
        Heart5,
        Heart6,
        Heart7,
        Heart8,
        Heart9,
        Heart10,
        Heart11,
        Heart12,
        FadeImage,
    }

    enum Texts
    {
        AttackDamageText,
        TearsText,
        RangeText,
        ShotSpeedText,
        SpeedText,
        LuckText,
        CoinText,
        BombText,
        KeyText,
    }

    enum GameObjects
    {
        BossHp,
        Minimap_Pannel,
        Charge,
    }

    protected override void Init()
    {
        base.Init();

        BindImage(typeof(Images));
        BindTextLegacy(typeof(Texts));
        BindObject(typeof(GameObjects));
        Managers.Game.ChargeBarEnevnt -= ChangeChargeGage;
        Managers.Game.ChargeBarEnevnt += ChangeChargeGage;
    }

    protected void ChangeChargeGage(int currentGage, int coolTime, string type)
    {
        GetImage((int)Images.ChargedBar).fillAmount = (1f / coolTime) * currentGage;
    }

    public void ChangeChargeBarSize(string name, int coolTime)
    {
        if (coolTime == 0)
            GetObject((int)GameObjects.Charge).SetActive(false);
        else
            GetObject((int)GameObjects.Charge).SetActive(true);

        Sprite sprite = Managers.Resource.Load<Sprite>(name + coolTime);
        GetImage((int)Images.ChargeBar).gameObject.GetComponent<Image>().sprite = sprite;
    }

    public void ChangeSpaceItem(string name)
    {
        Sprite sprite = Managers.Resource.Load<Sprite>(name);
        GetImage((int)Images.SpaceItem).gameObject.GetComponent<Image>().sprite = sprite;
    }
    public void ChangeQItem(string name)
    {
        Sprite sprite = Managers.Resource.Load<Sprite>(name);
        GetImage((int)Images.QItem).gameObject.GetComponent<Image>().sprite = sprite;
    }

    public void RefreshText(MainCharacter player)
    {
        GetTextLegacy((int)Texts.AttackDamageText).text = player.AttackDamage.ToString();
        GetTextLegacy((int)Texts.TearsText).text = player.Tears.ToString();
        GetTextLegacy((int)Texts.RangeText).text = player.Range.ToString();
        GetTextLegacy((int)Texts.ShotSpeedText).text = player.ShotSpeed.ToString();
        GetTextLegacy((int)Texts.SpeedText).text = player.Speed.ToString();
        GetTextLegacy((int)Texts.LuckText).text = player.Luck.ToString();
        GetTextLegacy((int)Texts.CoinText).text = player.Coin.ToString();
        GetTextLegacy((int)Texts.BombText).text = player.BombCount.ToString();
        GetTextLegacy((int)Texts.KeyText).text = player.KeyCount.ToString();
    }

    public void RefreshHpImage(MainCharacter player)
    {
        float hp = player.Hp;
        for (int i = 0; i < 12; i++)
        {
            string spriteNmae;
            if ((int)hp / (i + 1) != 0) spriteNmae = "ui_hearts_0";
            else if (i < hp && hp < i + 1) spriteNmae = "ui_hearts_1";
            else spriteNmae = "ui_hearts_18";
            GetImage((int)Images.Heart1 + i).gameObject.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(spriteNmae);
        }
    }

    public void RefreshUI(MainCharacter player)
    {
        RefreshText(player);
        RefreshHpImage(player);
    }

    public void BossHpActive(bool isActive)
    {
        GetObject((int)GameObjects.BossHp).SetActive(isActive);
    }

    public void ChangeBossHpSliderRatio(float ratio)
    {
        GetObject((int)GameObjects.BossHp).GetComponent<Slider>().value = ratio;
    }

    public GameObject GetMinimapPannel()
    {
        return GetObject((int)GameObjects.Minimap_Pannel);
    }

    public void SetFadeImageAlpha(float alpha)
    {
        GetImage((int)Images.FadeImage).GetComponent<Image>().color = new Color(0, 0, 0, alpha);
    }

}
