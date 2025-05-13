using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using static Utility;

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
        AttackDamageTextvariation,
        TearsTextvariation,
        RangeTextvariation,
        ShotSpeedTextvariation,
        SpeedTextvariation,
        LuckTextvariation,
        CoinText,
        BombText,
        KeyText,
    }

    enum TTexts
    {
        Name,
        Description
    }

    enum GameObjects
    {
        BossHp,
        Minimap_Pannel,
        Charge,
        ItemDescription,
        StageLoadingUI,
    }

    private Coroutine _coroutine;

    protected override void Init()
    {
        base.Init();

        BindImage(typeof(Images));
        BindTextLegacy(typeof(Texts));
        BindObject(typeof(GameObjects));
        BindText(typeof(TTexts));
    }

    //protected void ChangeChargeGage(int currentGage, int coolTime)
    //{
    //    GetImage((int)Images.ChargedBar).fillAmount = (1f / coolTime) * currentGage;
    //}

    public void ChangeChargeGage (Item item)
    {
        var temp = GetImage((int)Images.ChargeBar);
        GetImage((int)Images.ChargedBar).fillAmount = (1f / item.CoolTime) * item.CurrentGage;
    }

    public void ChangeChargeBarSize(string name, int coolTime)
    {
        if (coolTime == 0)
            GetObject((int)GameObjects.Charge).SetActive(false);
        else
            GetObject((int)GameObjects.Charge).SetActive(true);

        Sprite sprite = Managers.Resource.Load<Sprite>(name + coolTime);
        GetImage((int)Images.ChargeBar).sprite = sprite;
    }

    public void ChangeSpaceItem(string name)
    {
        Sprite sprite = Managers.Resource.Load<Sprite>(name);
        GetImage((int)Images.SpaceItem).sprite = sprite;
    }
    public void ChangeQItem(string name)
    {
        Sprite sprite = null;
        if (name == null)
        {
            GetImage((int)Images.QItem).gameObject.SetActive(false);
        }
        else
        {
            GetImage((int)Images.QItem).gameObject.SetActive(true);
            sprite = Managers.Resource.Load<Sprite>(name);
        }
        GetImage((int)Images.QItem).sprite = sprite;
    }

    public void RefreshText(MainCharacter player)
    {

        if (gameObject.activeSelf == true)
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(CRefreshUI(player));

        }

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

    IEnumerator CRefreshUI(MainCharacter player)
    {

        float AttackDamagevariation = player.AttackDamage - float.Parse(GetTextLegacy((int)Texts.AttackDamageText).text);
        float TearsTextvariation = player.Tears - float.Parse(GetTextLegacy((int)Texts.TearsText).text);
        float RangeTextvariation = player.Range - float.Parse(GetTextLegacy((int)Texts.RangeText).text);
        float ShotSpeedTextvariation = player.ShotSpeed - float.Parse(GetTextLegacy((int)Texts.ShotSpeedText).text);
        float SpeedTextvariation = player.Speed - float.Parse(GetTextLegacy((int)Texts.SpeedText).text);
        float LuckTextvariation = player.Luck - float.Parse(GetTextLegacy((int)Texts.LuckText).text);

        GetTextLegacy((int)Texts.AttackDamageTextvariation).color = new Color(1, 1, 1, 0);
        GetTextLegacy((int)Texts.TearsTextvariation).color = new Color(1, 1, 1, 0);
        GetTextLegacy((int)Texts.RangeTextvariation).color = new Color(1, 1, 1, 0);
        GetTextLegacy((int)Texts.ShotSpeedTextvariation).color = new Color(1, 1, 1, 0);
        GetTextLegacy((int)Texts.SpeedTextvariation).color = new Color(1, 1, 1, 0);
        GetTextLegacy((int)Texts.LuckTextvariation).color = new Color(1, 1, 1, 0);

        GetTextLegacy((int)Texts.AttackDamageTextvariation).text = AttackDamagevariation > 0 ? $"+{AttackDamagevariation}" : AttackDamagevariation < 0 ? AttackDamagevariation.ToString() : "";
        GetTextLegacy((int)Texts.TearsTextvariation).text = TearsTextvariation > 0 ? $"+{TearsTextvariation}" : TearsTextvariation < 0 ? TearsTextvariation.ToString() : "";
        GetTextLegacy((int)Texts.RangeTextvariation).text = RangeTextvariation > 0 ? $"+{RangeTextvariation}" : RangeTextvariation < 0 ? RangeTextvariation.ToString() : "";
        GetTextLegacy((int)Texts.ShotSpeedTextvariation).text = ShotSpeedTextvariation > 0 ? $"+{ShotSpeedTextvariation}" : ShotSpeedTextvariation < 0 ? ShotSpeedTextvariation.ToString() : "";
        GetTextLegacy((int)Texts.SpeedTextvariation).text = SpeedTextvariation > 0 ? $"+{SpeedTextvariation}" : SpeedTextvariation < 0 ? SpeedTextvariation.ToString() : "";
        GetTextLegacy((int)Texts.LuckTextvariation).text = LuckTextvariation > 0 ? $"+{LuckTextvariation}" : LuckTextvariation < 0 ? LuckTextvariation.ToString() : "";

        GetTextLegacy((int)Texts.AttackDamageTextvariation).color = AttackDamagevariation > 0 ? new Color(0, 1, 0, 0) : new Color(1, 0, 0, 0);
        GetTextLegacy((int)Texts.TearsTextvariation).color = TearsTextvariation > 0 ? new Color(0, 1, 0, 0) : new Color(1, 0, 0, 0);
        GetTextLegacy((int)Texts.RangeTextvariation).color = RangeTextvariation > 0 ? new Color(0, 1, 0, 0) : new Color(1, 0, 0, 0);
        GetTextLegacy((int)Texts.ShotSpeedTextvariation).color = ShotSpeedTextvariation > 0 ? new Color(0, 1, 0, 0) : new Color(1, 0, 0, 0);
        GetTextLegacy((int)Texts.SpeedTextvariation).color = SpeedTextvariation > 0 ? new Color(0, 1, 0, 0) : new Color(1, 0, 0, 0);
        GetTextLegacy((int)Texts.LuckTextvariation).color = LuckTextvariation > 0 ? new Color(0, 1, 0, 0) : new Color(1, 0, 0, 0);


        for (int i = 1; i <= 5; i++)
        {
            yield return new WaitForSeconds(0.1f);
            Color c;
            c = GetTextLegacy((int)Texts.AttackDamageTextvariation).color;
            c.a = i / 5f;
            GetTextLegacy((int)Texts.AttackDamageTextvariation).color = c;

            c = GetTextLegacy((int)Texts.TearsTextvariation).color;
            c.a = i / 5f;
            GetTextLegacy((int)Texts.TearsTextvariation).color = c;

            c = GetTextLegacy((int)Texts.RangeTextvariation).color;
            c.a = i / 5f;
            GetTextLegacy((int)Texts.RangeTextvariation).color = c;

            c = GetTextLegacy((int)Texts.ShotSpeedTextvariation).color;
            c.a = i / 5f;
            GetTextLegacy((int)Texts.ShotSpeedTextvariation).color = c;

            c = GetTextLegacy((int)Texts.SpeedTextvariation).color;
            c.a = i / 5f;
            GetTextLegacy((int)Texts.SpeedTextvariation).color = c;

            c = GetTextLegacy((int)Texts.LuckTextvariation).color;
            c.a = i / 5f;
            GetTextLegacy((int)Texts.LuckTextvariation).color = c;

        }

        yield return new WaitForSeconds(1);

        for (int i = 10; i >= 1; i--)
        {
            yield return new WaitForSeconds(0.1f);
            Color c;
            c = GetTextLegacy((int)Texts.AttackDamageTextvariation).color;
            c.a = i / 10f;
            GetTextLegacy((int)Texts.AttackDamageTextvariation).color = c;

            c = GetTextLegacy((int)Texts.TearsTextvariation).color;
            c.a = i / 10f;
            GetTextLegacy((int)Texts.TearsTextvariation).color = c;

            c = GetTextLegacy((int)Texts.RangeTextvariation).color;
            c.a = i / 10f;
            GetTextLegacy((int)Texts.RangeTextvariation).color = c;

            c = GetTextLegacy((int)Texts.ShotSpeedTextvariation).color;
            c.a = i / 10f;
            GetTextLegacy((int)Texts.ShotSpeedTextvariation).color = c;

            c = GetTextLegacy((int)Texts.SpeedTextvariation).color;
            c.a = i / 10f;
            GetTextLegacy((int)Texts.SpeedTextvariation).color = c;

            c = GetTextLegacy((int)Texts.LuckTextvariation).color;
            c.a = i / 10f;
            GetTextLegacy((int)Texts.LuckTextvariation).color = c;
        }

        string temp = "";
        GetTextLegacy((int)Texts.AttackDamageTextvariation).text = temp;
        GetTextLegacy((int)Texts.TearsTextvariation).text = temp;
        GetTextLegacy((int)Texts.RangeTextvariation).text = temp;
        GetTextLegacy((int)Texts.ShotSpeedTextvariation).text = temp;
        GetTextLegacy((int)Texts.SpeedTextvariation).text = temp;
        GetTextLegacy((int)Texts.LuckTextvariation).text = temp;

        GetTextLegacy((int)Texts.AttackDamageTextvariation).color = new Color(1, 1, 1, 0);
        GetTextLegacy((int)Texts.TearsTextvariation).color = new Color(1, 1, 1, 0);
        GetTextLegacy((int)Texts.RangeTextvariation).color = new Color(1, 1, 1, 0);
        GetTextLegacy((int)Texts.ShotSpeedTextvariation).color = new Color(1, 1, 1, 0);
        GetTextLegacy((int)Texts.SpeedTextvariation).color = new Color(1, 1, 1, 0);
        GetTextLegacy((int)Texts.LuckTextvariation).color = new Color(1, 1, 1, 0);


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
            GetImage((int)Images.Heart1 + i).sprite = Managers.Resource.Load<Sprite>(spriteNmae);
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
        GetImage((int)Images.FadeImage).color = new Color(0, 0, 0, alpha);
    }

    public void SpaceItemAndChargeBarActive(bool value)
    {
        GetObject((int)GameObjects.Charge).SetActive(value);
        GetImage((int)Images.SpaceItem).enabled = value;
    }

    public void ItemDescriptionActive(bool value)
    {
        GetObject((int)GameObjects.ItemDescription).SetActive(value);
    }

    public void ChangeItemDescription(Item item)
    {
        GetText((int)TTexts.Name).text = item.Name;
        GetText((int)TTexts.Description).text = item.Description;
    }

    public void activeStatgeLoading()
    {
        GameObject stageLoadingUi = GetObject((int)GameObjects.StageLoadingUI);
        stageLoadingUi.SetActive(true);

        float delay = stageLoadingUi.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.length;
        StartCoroutine(BubbleActive(delay));
    }

    IEnumerator BubbleActive(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject stageLoadingUi = GetObject((int)GameObjects.StageLoadingUI);
        stageLoadingUi.GetComponent<Animator>().Play("Loop");

        Animator animator = FindChildByName(stageLoadingUi.transform, "Bubble").GetComponent<Animator>();
        animator.Play("Nightmare_bubble_5");
        //다음 프레임에 에니메이션이 교체되고 딜레이를 측정하자
        yield return null;
        delay = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;

        yield return new WaitForSeconds(delay);
        stageLoadingUi.SetActive(false);
    }

    void OnDisable()
    {
        GetTextLegacy((int)Texts.AttackDamageTextvariation).color = new Color(1, 1, 1, 0);
        GetTextLegacy((int)Texts.TearsTextvariation).color = new Color(1, 1, 1, 0);
        GetTextLegacy((int)Texts.RangeTextvariation).color = new Color(1, 1, 1, 0);
        GetTextLegacy((int)Texts.ShotSpeedTextvariation).color = new Color(1, 1, 1, 0);
        GetTextLegacy((int)Texts.SpeedTextvariation).color = new Color(1, 1, 1, 0);
        GetTextLegacy((int)Texts.LuckTextvariation).color = new Color(1, 1, 1, 0);
    }
}
