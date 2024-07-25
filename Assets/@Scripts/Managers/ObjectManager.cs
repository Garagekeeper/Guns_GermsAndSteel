using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    public HashSet<MainCharacter> MainCharacters { get; } = new HashSet<MainCharacter>();
    public HashSet<Monster> Monsters { get; } = new HashSet<Monster>();
    public T Spawn<T>(Vector3 pos, int templateID = 0, string prfabName = "") where T : Creature
    {
        System.Type type = typeof(T);
        if (type == typeof(MainCharacter))
        {
            GameObject go = Managers.Resource.Instantiate("Player");
            go.name = "Player";
            MainCharacter mc = go.GetComponent<MainCharacter>();
            mc.transform.position = pos;
            MainCharacters.Add(mc);
            return mc as T;
        }
        if (type == typeof(Monster))
        {
            GameObject go = Managers.Resource.Instantiate("Monster");
            go.name = "Monster";
            Monster mt = go.GetComponent<Monster>();
            mt.transform.position = pos;
            Monsters.Add(mt);
            return mt as T;
        }
        return null;
    }

    public void Despawn<T>(T obj) where T : Creature
    {
        System.Type type = typeof(T);

        if (type == typeof(MainCharacter))
        {
            MainCharacters.Remove(obj as MainCharacter);
            Object.Destroy(obj);
        }
        else if (type == typeof(Monster))
        {
            Monsters.Remove(obj as Monster);
            Object.Destroy(obj);
        }
        
        //Managers.Map.RemoveObject(obj);
    }
}

