using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers m_instance;
    private static Managers Instance { get { Init(); return m_instance; } }

    private DataManager _data = new DataManager();
    private ResourceManager _map = new ResourceManager();
    private ResourceManager _resource = new ResourceManager();
    private GameManager _game = new GameManager();

    public static DataManager Data { get { return Instance?._data; } }
    public static ResourceManager Map { get { return Instance?._map; } }
    public static ResourceManager Resource { get { return Instance?._resource; } }
    public static GameManager Game { get { return Instance?._game; } }

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

            DontDestroyOnLoad(go);
            m_instance = go.GetComponent<Managers>();
        }
    }
}
