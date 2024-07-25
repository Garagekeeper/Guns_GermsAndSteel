using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : MonoBehaviour
{
    private void Awake()
    {
        GameObject go = Managers.Resource.Instantiate("PlayingUI");
        go.name = "PlayingUI";

        Managers.UI.PlayingUI = go.GetComponent<PlayingUI>();

        MainCharacter mc = Managers.Object.Spawn<MainCharacter>(new Vector3(-0.5f, -0.5f, 0));

        Managers.Map.GenerateStage();
        Managers.Map.LoadMap();
    }
}
