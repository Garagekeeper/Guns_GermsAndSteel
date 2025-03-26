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

        MyStuffImage_1=11,
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
        MyStuffImage_15,


    }

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
        BindTextLegacy(typeof(Texts));
        BindObject(typeof(GameObjects));

        _currentObject = (int)GameObjects.OptionArrow;
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
}
