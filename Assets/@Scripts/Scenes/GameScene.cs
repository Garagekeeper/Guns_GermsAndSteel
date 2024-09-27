using UnityEngine;

public class GameScene : MonoBehaviour
{
    private void Awake()
    {
        Managers.Map.Init(() => { LoadUI(); SpawnCharacter(); Managers.Game.RoomConditionCheck(); });
    }

    public void LoadUI()
    {
        GameObject go = Managers.Resource.Instantiate("PlayingUI");
        go.name = "PlayingUI";
        Managers.UI.PlayingUI = go.GetComponent<PlayingUI>();
    }

    public void SpawnCharacter()
    {
        MainCharacter mc = Managers.Object.Spawn<MainCharacter>(new Vector3(-0.5f, -0.5f, 0));
    }
}
