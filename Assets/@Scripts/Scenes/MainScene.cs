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
        TitleImageUI = 0,
        FileSelectUI,
        GameMenuUI = 2,
        NEWRUN,
        CONTINUE,
        CHALLENGES,
        STATS,
        OPTIONS,
        OptionMenuUI = 8,
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

    private int _cursoredUI = 0;

    public int CursoredUI
    {
        get { return _cursoredUI; }
        set
        {
            if (_cursoredUI != value)
            {
                MoveArrow(_cursoredUI, value);
                _cursoredUI = value;
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
        TargetPos = new Vector3(0, 0, -10);
        BindObject(typeof(UIGameObjects));
        _uiStack.Push(GetObject((int)UIGameObjects.TitleImageUI));
        CursoredUI = (int)UIGameObjects.FileSelectUI;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {

            //push ui;
            PushUI();

        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {

            //pop ui;
            PopUI();

        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            int minValue = 0;
            int maxValue;
            int range;

            if (CursoredUI >= 3 && CursoredUI <= 7)
            {
                minValue = 3;
                maxValue = 7;

                // 모듈 연산으로 순환
                range = maxValue - minValue + 1;
                CursoredUI = ((CursoredUI - minValue - 1 + range) % range + minValue);
                return;
            }

            if (CursoredUI >= 9 && CursoredUI <= 19)
            {
                minValue = 9;
                maxValue = 19;

                // 모듈 연산으로 순환
                range = maxValue - minValue + 1;
                CursoredUI = ((CursoredUI - minValue - 1 + range) % range + minValue);
                return;
            }

        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {

            int minValue = 0;
            int maxValue;
            int range;

            if (CursoredUI >= 3 && CursoredUI <= 7)
            {
                minValue = 3;
                maxValue = 7;

                // 모듈 연산으로 순환
                range = maxValue - minValue + 1;
                CursoredUI = ((CursoredUI - minValue + 1 + range) % range + minValue);
                return;
            }

            if (CursoredUI >= 9 && CursoredUI <= 19)
            {
                minValue = 9;
                maxValue = 19;

                // 모듈 연산으로 순환
                range = maxValue - minValue + 1;
                CursoredUI = ((CursoredUI - minValue + 1 + range) % range + minValue);
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

    public void PushUI()
    {
        if (CursoredUI == (int)UIGameObjects.NEWRUN)
        {
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            if (CursoredUI == (int)UIGameObjects.OPTIONS)
            {
                _uiStack.Push(GetObject((int)UIGameObjects.OptionMenuUI));
            }
            else if (CursoredUI == (int)UIGameObjects.FileSelectUI)
            {
                _uiStack.Push(GetObject((int)UIGameObjects.GameMenuUI));
            }
            else
            {
                _uiStack.Push(GetObject(CursoredUI));
            }

            switch (CursoredUI)
            {
                case (int)UIGameObjects.FileSelectUI:
                    CursoredUI = (int)UIGameObjects.GameMenuUI;
                    TargetPos += new Vector3(0, -13.5f, 0);
                    break;
                case (int)UIGameObjects.GameMenuUI:
                    CursoredUI = (int)UIGameObjects.NEWRUN;
                    TargetPos += new Vector3(0, -13.5f, 0);
                    break;
                case (int)UIGameObjects.OPTIONS:
                    TargetPos += new Vector3(24f, -13.5f, 0);
                    CursoredUI = (int)UIGameObjects.SFX;
                    break;
            }
        }


    }

    public void PopUI()
    {
        if (_uiStack.Count == 1)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
        }

        if (_uiStack.Peek().gameObject.name == UIGameObjects.OptionMenuUI.ToString())
        {
            _uiStack.Pop();
            TargetPos -= new Vector3(24f, -13.5f, 0);
            CursoredUI = (int)UIGameObjects.NEWRUN;
        }
        else if (_uiStack.Peek().gameObject.name == UIGameObjects.GameMenuUI.ToString())
        {
            _uiStack.Pop();
            TargetPos -= new Vector3(0, -13.5f, 0);
            CursoredUI = (int)UIGameObjects.FileSelectUI;
        }
        else if (_uiStack.Peek().gameObject.name == UIGameObjects.GameMenuUI.ToString())
        {
            _uiStack.Pop();
            TargetPos -= new Vector3(0, -13.5f, 0);
            CursoredUI = (int)UIGameObjects.TitleImageUI;
        }
    }

    public void MoveArrow(int origin, int next)
    {
        if (origin == 0)
        {

        }
        else
        {
            FindChildByName(GetObject(origin).transform, "Arrow",false)?.gameObject.SetActive(false);
            FindChildByName(GetObject(next).transform, "Arrow",false)?.gameObject.SetActive(true);
        }
    }
}
