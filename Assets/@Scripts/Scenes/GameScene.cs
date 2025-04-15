using System.Collections;
using UnityEngine;
using static Utility;

public class GameScene : MonoBehaviour
{
    GameObject LoadImage;
    GameObject Bg;

    private void Awake()
    {
        LoadImage = FindChildByName(transform, "LoadingImage").gameObject;
        Bg = FindChildByName(transform, "BG").gameObject;

        LoadImage.SetActive(true);
        Bg.SetActive(true);

        LoadImage.GetComponent<Animator>().Play("loadimages-" + Random.Range(1, 57).ToString("D3"));

        LoadUI();
        Managers.UI.PlayingUI.gameObject.SetActive(false);
        Managers.Map.Init(() => {
            StartCoroutine(loading());
        });
    }

    IEnumerator loading()
    {
        yield return new WaitForSeconds(1);
        SpawnCharacter();
        Managers.Game.RoomConditionCheck();

        LoadImage.SetActive(false);
        Bg.SetActive(false);

        Managers.UI.PlayingUI.gameObject.SetActive(true);
    }

    public void LoadUI()
    {
        GameObject parent =  GameObject.Find("UI");
        
        GameObject go = Managers.Resource.Instantiate("PlayingUI", parent.transform);
        go.name = "PlayingUI";
        Managers.UI.PlayingUI = go.GetComponent<PlayingUI>();


        go = Managers.Resource.Instantiate("GameOverUI", parent.transform);
        go.name = "GameOverUI";
        Managers.UI.GameOverUI = go.GetComponent<GameOverUI>();
        
        go = Managers.Resource.Instantiate("PauseUI", parent.transform);
        go.name = "PauseUI";
        Managers.UI.PauseUI = go.GetComponent<PauseUI>();

    }

    public void SpawnCharacter()
    {
        MainCharacter mc = Managers.Object.Spawn<MainCharacter>(new Vector3(-0.5f, -0.5f, 0));
    }

    public void LoadMiniMap()
    {
        Managers.Map.GenerateMinimap();
    }

}
