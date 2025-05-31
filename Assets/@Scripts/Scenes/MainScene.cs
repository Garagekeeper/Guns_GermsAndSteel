using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Utility;

public class MainScene : UI_Base
{
    private Stack<GameObject> _uiStack = new Stack<GameObject>();
    public enum UIGameObjects
    {
        None,
        TitleImageUI = 1,
        FileSelectUI,
        GameMenuUI = 3,
        NEWRUN,
        CONTINUE,
        CHALLENGES,
        STATS,
        OPTIONS,
        OptionMenuUI = 9,
        SFX,
        MUSIC,
        CONTROLS,
        POPUPS,
        PENOPACITY,
        MAPOPPACITY,
        LANGUAGE,
        ACTIVECAM,
        GAMMA,
        FILTER,
        FULLSCREEN,
    }

    private int _currentUI = 0;
    private int _targetUI = 0;

    private AudioClip _audioClip;

    public int CurrentUI
    {
        get { return _currentUI; }
        set
        {
            if (_currentUI != value)
            {
                _currentUI = value;
            }
        }
    }
    public int TargetUI
    {
        get { return _targetUI; }
        set
        {
            if (_targetUI != value)
            {
                MoveArrow(_targetUI, value);
                _targetUI = value;
            }
        }
    }

    private float smoothSpeed = 6f;
    public Vector3 TargetPos { get; set; }

    [SerializeField]
    private GameObject _cam;

    protected override void Init()
    {
        base.Init();

        {
            Managers.UI.PauseUI = null;
            Managers.UI.PlayingUI = null;
            Managers.UI.GameOverUI = null;
        }

        {
            Managers.Object.ClearObjectManager(true);
        }

        {

        }

        Managers.Game.Init();
        Managers.Sound.Init();
        TargetPos = new Vector3(0, 0, -10);
        BindObject(typeof(UIGameObjects));
        _uiStack.Push(GetObject((int)UIGameObjects.TitleImageUI));
        CurrentUI = (int)UIGameObjects.TitleImageUI;
        TargetUI = (int)UIGameObjects.None;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {

            //push ui;
            GoToNextUI();

        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {

            //pop ui;
            GoToPrevUI();

        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            int minValue = 0;
            int maxValue;
            int range;

            if (CurrentUI < (int)UIGameObjects.GameMenuUI) return;

            if (TargetUI >= (int)UIGameObjects.NEWRUN && TargetUI <= (int)UIGameObjects.OPTIONS)
            {
                _audioClip = Managers.Resource.Load<AudioClip>("menu_scroll");
                Managers.Sound.PlaySFX(_audioClip, 1f);
                minValue = (int)UIGameObjects.NEWRUN;
                maxValue = (int)UIGameObjects.OPTIONS;

                // 모듈 연산으로 순환
                range = maxValue - minValue + 1;
                TargetUI = ((TargetUI - minValue - 1 + range) % range + minValue);
                return;
            }

            if (TargetUI >= (int)UIGameObjects.SFX && TargetUI <= (int)UIGameObjects.FULLSCREEN)
            {
                _audioClip = Managers.Resource.Load<AudioClip>("menu_scroll");
                Managers.Sound.PlaySFX(_audioClip, 1f);
                minValue = (int)UIGameObjects.SFX;
                maxValue = (int)UIGameObjects.FULLSCREEN;

                // 모듈 연산으로 순환
                range = maxValue - minValue + 1;
                TargetUI = ((TargetUI - minValue - 1 + range) % range + minValue);
                return;
            }


        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {

            int minValue = 0;
            int maxValue;
            int range;

            if (CurrentUI < (int)UIGameObjects.GameMenuUI) return;

            if (TargetUI >= (int)UIGameObjects.NEWRUN && TargetUI <= (int)UIGameObjects.OPTIONS)
            {
                _audioClip = Managers.Resource.Load<AudioClip>("menu_scroll");
                Managers.Sound.PlaySFX(_audioClip, 1f);

                minValue = (int)UIGameObjects.NEWRUN;
                maxValue = (int)UIGameObjects.OPTIONS;

                // 모듈 연산으로 순환
                range = maxValue - minValue + 1;
                TargetUI = ((TargetUI - minValue + 1 + range) % range + minValue);
                return;
            }

            if (TargetUI >= (int)UIGameObjects.SFX && TargetUI <= (int)UIGameObjects.FULLSCREEN)
            {
                _audioClip = Managers.Resource.Load<AudioClip>("menu_scroll");
                Managers.Sound.PlaySFX(_audioClip, 1f);

                minValue = (int)UIGameObjects.SFX;
                maxValue = (int)UIGameObjects.FULLSCREEN;

                // 모듈 연산으로 순환
                range = maxValue - minValue + 1;
                TargetUI = ((TargetUI - minValue + 1 + range) % range + minValue);
                return;
            }
        }
    }

    private void LateUpdate()
    {
        MoveCameraWithLerp();
    }

    public void MoveCameraWithLerp()
    {
        _cam.transform.position = Vector3.Lerp(_cam.transform.position, TargetPos, smoothSpeed * Time.fixedDeltaTime);
    }

    // CurrentUI <= 현재 보여지는 UI
    // TargetUI  <= 현재 UI에서 선택된 메뉴 
    public void GoToNextUI()
    {
        if (TargetUI != (int)UIGameObjects.NEWRUN)
        {
            _audioClip = Managers.Resource.Load<AudioClip>("paper_in");
            Managers.Sound.PlaySFX(_audioClip, 1f);
        }

        switch (CurrentUI)
        {
            case (int)UIGameObjects.TitleImageUI:
                CurrentUI = (int)UIGameObjects.FileSelectUI;
                TargetPos += new Vector3(0, -13.5f, 0);
                break;
            case (int)UIGameObjects.FileSelectUI:
                CurrentUI = (int)UIGameObjects.GameMenuUI;
                TargetUI = (int)UIGameObjects.NEWRUN;
                TargetPos += new Vector3(0, -13.5f, 0);
                break;
            case (int)UIGameObjects.GameMenuUI:
                if (TargetUI == (int)UIGameObjects.NEWRUN)
                {
                    SceneManager.LoadScene("GameScene");
                }
                else if (TargetUI == (int)UIGameObjects.OPTIONS)
                {
                    CurrentUI = (int)UIGameObjects.OptionMenuUI;
                    TargetUI = (int)UIGameObjects.SFX;
                    TargetPos += new Vector3(24f, -13.5f, 0);
                }
                break;
        }
    }

    // CurrentUI <= 현재 보여지는 UI
    // TargetUI  <= 현재 UI에서 선택된 메뉴 
    public void GoToPrevUI()
    {
        if (CurrentUI == (int)UIGameObjects.TitleImageUI)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
        }
        _audioClip = Managers.Resource.Load<AudioClip>("paper_out");
        Managers.Sound.PlaySFX(_audioClip, 1f);
        switch (CurrentUI)
        {
            case (int)UIGameObjects.FileSelectUI:
                CurrentUI = (int)UIGameObjects.TitleImageUI;
                TargetUI = (int)UIGameObjects.None;
                TargetPos += new Vector3(0, 13.5f, 0);
                break;
            case (int)UIGameObjects.GameMenuUI:
                CurrentUI = (int)UIGameObjects.FileSelectUI;
                TargetUI = (int)UIGameObjects.None;
                TargetPos += new Vector3(0, 13.5f, 0);
                break;
            case (int)UIGameObjects.OptionMenuUI:
                CurrentUI = (int)UIGameObjects.GameMenuUI;
                TargetUI = (int)UIGameObjects.NEWRUN;
                TargetPos += new Vector3(-24f, 13.5f, 0);
                break;
        }

    }

    public void MoveArrow(int origin, int next)
    {

        if (origin == 0)
        {
            FindChildByName(GetObject(next).transform, "Arrow", false)?.gameObject.SetActive(true);
        }
        else if (next == 0)
        {
            FindChildByName(GetObject(origin).transform, "Arrow", false)?.gameObject.SetActive(false);
        }
        else
        {
            FindChildByName(GetObject(origin).transform, "Arrow", false)?.gameObject.SetActive(false);
            FindChildByName(GetObject(next).transform, "Arrow", false)?.gameObject.SetActive(true);
        }
    }
}
