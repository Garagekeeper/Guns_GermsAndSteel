using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Base : MonoBehaviour
{
    private void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {

    }

    protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();

    //게임의 UI 및 GameObject를 이름으로 찾아서 Object로 들고있도록 해준다.
    // 사용시 enum에 바인딩할 UI 및 GameObject의 타입을 매개변수로 넘겨준다.
    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        //1. 바인딩할 UI 및 GameObject의 배열을 만든다.
        //2. 배열의 크기만큼 Object 객체의 배열을 만든다.
        //3. 2에서 만든 배열을 멤버 변수(딕셔너리)에 추가한다.
        string[] uiNames = Enum.GetNames(type);
        UnityEngine.Object[] objects = new UnityEngine.Object[uiNames.Length];
        _objects.Add(typeof(T), objects);

        //바인딩할 개수만큼 반복한다.
        for (int i = 0; i < uiNames.Length; i++)
        {
            //반복문을 돌면서 Object 배열에 객체를 저장한다.
            //바인딩할 목표가 GameObject인 경우별도 처리(?)
            if (typeof(T) == typeof(GameObject))
                objects[i] = FindChild(gameObject, uiNames[i], true);
            else
                objects[i] = FindChild<T>(gameObject, uiNames[i], true);

            if (objects[i] == null)
                Debug.Log($"Failed to bind({uiNames[i]})");
        }
    }

    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null) return null;

        if (recursive)
        {
            //True를 입력해줘야 iactive도 검색한다
            foreach (T component in go.GetComponentsInChildren<T>(true))
            {
                if (component.name == name)
                    return component;
            }
        }
        else
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }

        }

        return null;
    }


    protected void BindObject(Type type) { Bind<GameObject>(type); }
    protected void BindImage(Type type) { Bind<Image>(type); }
    protected void BindText(Type type) { Bind<TMP_Text>(type); }
    protected void BindButton(Type type) { Bind<Button>(type); }


    protected T Get<T>(int idx) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objects = null;
        if (_objects.TryGetValue(typeof(T), out objects) == false)
            return null;

        return objects[idx] as T;
    }


    protected GameObject GetObject(int idx) { return Get<GameObject>(idx); }
    protected Image GetImage(int idx) { return Get<Image>(idx); }
    protected TMP_Text GetText(int idx) { return Get<TMP_Text>(idx); }
    protected Button GetButton(int idx) { return Get<Button>(idx); }

}


