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

        go = Managers.Resource.Instantiate("Player");
        go.name = "Player";

       
    }
}
