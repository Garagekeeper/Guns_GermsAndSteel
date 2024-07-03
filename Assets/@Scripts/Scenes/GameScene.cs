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

        MainCharacter mc = Managers.Object.Spawn<MainCharacter>(Vector3.zero);

        //TODO map 프리팹화 시 이름을 변경해줘야
        GameObject map = GameObject.Find("Grid");
        Managers.Map.LoadMap(map, "Grid");
    }
}
