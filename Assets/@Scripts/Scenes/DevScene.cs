using UnityEngine;

public class DevScene : MonoBehaviour
{
    private void Awake()
    {
        Managers.Resource.LoadAllAsync<Object>("Preload", (key, count, totalCount) =>
        {
            Debug.Log($"{key} {count}/{totalCount}");
            if (count == totalCount)
            {
                Managers.Data.Init();
                Managers.Game.Init();
                LoadInGame();
            }
        });

    }

    public void LoadInGame()
    {
        Managers.Resource.LoadAllAsync<Object>("InGame", (key, count, totalCount) =>
        {
            Debug.Log($"{key} {count}/{totalCount}");
            if (count == totalCount)
            {
                LoadUI(); SpawnCharacter();
            }
        });
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

    public void SpawnBossMonster()
    {

    }
}
