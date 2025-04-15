using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseUI : UI_Base
{
    int _currentObject;
    enum Images
    {
        PauseMain=0,
        SpeedGage,
        TearsGage,
        AttackDamageGage,
        RangeGage,
        ShotSpeedGage,
        LuckGage,

        MyStuffImage_0=11,
        MyStuffImage_1,
        MyStuffImage_2,
        MyStuffImage_3,
        MyStuffImage_4,
        MyStuffImage_5,
        MyStuffImage_6,
        MyStuffImage_7,
        MyStuffImage_8,
        MyStuffImage_9,
        MyStuffImage_10,
        MyStuffImage_11,
        MyStuffImage_12,
        MyStuffImage_13,
        MyStuffImage_14,


    }

    private int _itemCount = 0;

    enum Texts
    {
        SeedText
    }

    enum GameObjects
    {
        OptionArrow = 0,
        ResumeArrow = 1,
        ExitArrow = 2,
    }

    protected override void Init()
    {
        base.Init();

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindObject(typeof(GameObjects));

        _currentObject = (int)GameObjects.OptionArrow;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            int max = Enum.GetValues(typeof(GameObjects)).Length;
            Get<GameObject>(_currentObject).SetActive(false);
            _currentObject = (((_currentObject - 1) % max) + max) % max;
            Get<GameObject>(_currentObject).SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            int max = Enum.GetValues(typeof(GameObjects)).Length;
            Get<GameObject>(_currentObject).SetActive(false);
            _currentObject = (((_currentObject + 1) % max) + max) % max;
            Get<GameObject>(_currentObject).SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            switch (_currentObject)
            {
                case 0:
                    break;
                case 1:
                    _currentObject = 1;
                    foreach (var mc in Managers.Object.MainCharacters)
                    {
                        mc.IsPause = false;
                    }
                    break;
                   
                case 2:
                    Time.timeScale = 1;
                    Managers.UI.PauseUI.gameObject.SetActive(false);
                    Managers.UI.PlayingUI.SetFadeImageAlpha(1);
                    Destroy(Managers.Map.Map);
                    Managers.Object.ClearObjectManager(true);
                    SceneManager.LoadScene("MainScene");
                    break;
            }
        }
    }
    public void RefreshUI(MainCharacter player)
    {
        RefreshPauseStat(player);
        RefreshItemList(player);
        RefreshSeedText();
    }

    public void RefreshPauseStat(MainCharacter player)
    {
        GetImage((int)Images.TearsGage).sprite = Managers.Resource.Load<Sprite>("PauseMain_right_" + Math.Min((int)player.Tears,7));
        GetImage((int)Images.RangeGage).sprite = Managers.Resource.Load<Sprite>("PauseMain_right_" + Math.Min((int)player.Range,7));
        GetImage((int)Images.SpeedGage).sprite = Managers.Resource.Load<Sprite>("PauseMain_right_" + Math.Min((int)player.Speed,7));
        GetImage((int)Images.LuckGage).sprite = Managers.Resource.Load<Sprite>("PauseMain_left_" + Math.Min((int)player.Luck, 7));
        GetImage((int)Images.AttackDamageGage).sprite = Managers.Resource.Load<Sprite>("PauseMain_left_" + Math.Min((int)player.AttackDamage, 7));
        GetImage((int)Images.ShotSpeedGage).sprite = Managers.Resource.Load<Sprite>("PauseMain_left_" + Math.Min((int)player.ShotSpeed, 7));
    }

    public void RefreshItemList(MainCharacter player)
    {
        if (player == null) return;
        if (_itemCount > 14) return;

        foreach (var item in player.AcquiredPassiveItemList)
        {
            if (_itemCount > 14) break;
            //TODO
            //GetImage((int)(Images.DeathItem_0 + _itemCount)).sprite = ;
            _itemCount++;
        }
    }

    public void RefreshSeedText()
    {
        GetText((int)Texts.SeedText).text = Managers.Game.Seed.Insert(4, "\n");
    }
}

