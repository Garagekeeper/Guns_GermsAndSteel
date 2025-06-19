using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TitleScene : UI_Base
{

    protected override void Init()
    {
        base.Init();

        Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);

        //EventSystem이 없으면  UI가 동작하지 않기 때문에
        //체크하고 없으면 붙여준다
        Object obj = GameObject.FindObjectOfType(typeof(EventSystem));
        if (obj == null)
        {
            GameObject go = new GameObject() { name = "@EventSystem" };
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }


        Managers.Resource.LoadAllAsync<Object>("Preload", (key, count, totalCount) =>
        {
            //Debug.Log($"{key} {count}/{totalCount}");
            if (count == totalCount)
            {
                Managers.Data.Init();
                StartCoroutine(Pause());
            }
        });
    }

    IEnumerator Pause()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("ChangeScene");
        SceneManager.LoadScene("MainScene");
    }

}
