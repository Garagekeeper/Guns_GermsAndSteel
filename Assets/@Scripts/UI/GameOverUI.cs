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
        DeathItem_0,
        DeathItem_1,
        DeathItem_2,
        DeathItem_3,
        DeathItem_4,
        DeathItem_5,
        DeathItem_6,
        DeathItem_7,
        DeathItem_8,
        DeathItem_9,
    }

    private int _itemCount = 0;

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

        gameObject.SetActive(false);
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

    public void RefreshUI(MainCharacter player)
    {
        RefreshItemList(player);
        RefreshStageName();
        RefreshSeedText();
    }

    public void RefreshItemList(MainCharacter player)
    {
        if (player == null) return;
        if (_itemCount > 9) return;

        foreach (var item in player.AcquiredPassiveItemList)
        {
            if (_itemCount > 9) break;
            //TODO
            //GetImage((int)(Images.DeathItem_0 + _itemCount)).sprite = ;
            _itemCount++;
        }

    }

    public void RefreshStageName()
    {
        string imageNmae;
        if (Managers.Game.StageNumber > 7)
        {
            imageNmae = "Stage_Name_6";
        }
        else
            imageNmae = "Stage_Name_" + (Managers.Game.StageNumber - 1);

        GetImage((int)Images.StageName).sprite = Managers.Resource.Load<Sprite>(imageNmae);
    }

    public void RefreshSeedText()
    {
        GetText((int)Texts.SeedText).text = Managers.Game.Seed.Insert(4,"\n");
    }

    


}
