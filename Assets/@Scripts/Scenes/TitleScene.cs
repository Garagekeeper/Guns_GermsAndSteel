using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TitleScene : MonoBehaviour
{
    private void Start()
    {
        
        //EventSystem이 없으면  UI가 동작하지 않기 때문에
        //체크하고 없으면 붙여준다
        Object obj = GameObject.FindObjectOfType(typeof(EventSystem));
        if (obj == null)
        {
            GameObject go = new GameObject() { name = "@EventSystem" };
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        TextMeshProUGUI startText = GameObject.Find("StartingUI").transform.Find("StartingText").GetComponent<TextMeshProUGUI>();
        Managers.Resource.LoadAllAsync<Object>("Preload", (key, count, totalCount) =>
        {
            //Debug.Log($"{key} {count}/{totalCount}");
            if (count == totalCount)
            {
                Managers.Data.Init();
                Managers.Game.Init();
                startText.text = "Press space Key To Start";
            }
        });
    }

    private void Update()
    {
         if (Input.GetKeyDown(KeyCode.Space)) 
        {
           
            Debug.Log("ChangeScene");
            SceneManager.LoadScene("GameScene");
        }
    }
}
