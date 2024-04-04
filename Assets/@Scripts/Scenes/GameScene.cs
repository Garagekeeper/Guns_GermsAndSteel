using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : MonoBehaviour
{
    private void Awake()
    {
        GameObject go = Managers.Resource.Instantiate("Player");
        go.name = "Player";


    }
}
