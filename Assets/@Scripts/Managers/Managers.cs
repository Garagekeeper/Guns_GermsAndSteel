using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers m_instance;
    public static Managers Instance { get { Init(); return m_instance; } }

    private  DataManager _data = new DataManager();
    private  MapManager _map = new MapManager();
    private  ResourceManager _resource = new ResourceManager();
    private  GameManager _game = new GameManager();
    private  UIManager _ui = new UIManager();
    private  ObjectManager _object = new ObjectManager();
    private  SoundManager _sound = new SoundManager();

    public static DataManager Data { get { return Instance?._data; } }
    public static MapManager Map { get { return Instance?._map; } }
    public static ResourceManager Resource { get { return Instance?._resource; } }
    public static GameManager Game { get { return Instance?._game; } }
    public static UIManager UI { get { return Instance?._ui; } }
    public static ObjectManager Object { get { return Instance?._object; } }
    public static SoundManager Sound {  get { return Instance?._sound; } }

    public static void Init()
    {
        if (m_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            GameObject sound = GameObject.Find("SoundPool");
            if (sound == null)
            {
                sound = new GameObject { name = "@SoundPool" };
            }

            DontDestroyOnLoad(go);
            DontDestroyOnLoad(sound);
            m_instance = go.GetComponent<Managers>();
        }
    }


    public void ResetManager()
    {
        _map = null;
        _game = null;
        _ui = null;
        _object = null;
    }
}
