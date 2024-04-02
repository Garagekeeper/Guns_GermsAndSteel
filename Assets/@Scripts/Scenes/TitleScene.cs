using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TitleScene : MonoBehaviour
{
    public bool Init()
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

        return true;
    }

    private void Update()
    {
         if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) 
        {
            Managers.Resource.LoadAllAsync<Object>("Preload", (key, count, totalCount) =>
            {
                Debug.Log($"{key} {count}/{totalCount}");

                if (count == totalCount)
                {
                    //Managers.Data.Init();
                }
            });

            Debug.Log("ChangeScene");
            SceneManager.LoadScene("DevScene");
        }
    }
}
