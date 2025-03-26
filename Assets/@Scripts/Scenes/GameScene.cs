using UnityEngine;

public class GameScene : MonoBehaviour
{
    private void Awake()
    {
        LoadUI();
        Managers.UI.PlayingUI.gameObject.SetActive(false);
        Managers.Map.Init(() => {  SpawnCharacter(); Managers.Game.RoomConditionCheck(); Managers.UI.PlayingUI.gameObject.SetActive(true);});
    }

    public void LoadUI()
    {
        GameObject go = Managers.Resource.Instantiate("PlayingUI");
        go.name = "PlayingUI";
        Managers.UI.PlayingUI = go.GetComponent<PlayingUI>();

        go = Managers.Resource.Instantiate("GameOverUI");
        go.name = "GameOverUI";
        Managers.UI.GameOverUI = go.GetComponent<GameOverUI>();
        
        go = Managers.Resource.Instantiate("PauseUI");
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
