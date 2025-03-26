using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : UI_Base
{
    enum Images 
    {
        Overview,
        StageName,
        BodyImage,
        DeadBy,
        DeathItem_1,
        DeathItem_2,
        DeathItem_3,
        DeathItem_4,
        DeathItem_5,
        DeathItem_6,
        DeathItem_7,
        DeathItem_8,
        DeathItem_9,
        DeathItem_10,
    }

    enum Texts
    {
        SeedText
    }
    
    enum GameObjects
    {

    }

    protected override void Init()
    {
        base.Init();

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        //BindObject(typeof(GameObjects));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 1;
            Managers.UI.GameOverUI.gameObject.SetActive(false);
            Managers.UI.PlayingUI.SetFadeImageAlpha(1);
            Destroy(Managers.Map.Map);
            Managers.Object.ClearObjectManager(true);

            SceneManager.LoadScene("MainScene");
        }

        if (Input.GetKeyDown(KeyCode.Space))
            Managers.Game.RestartGame();
    }
}
