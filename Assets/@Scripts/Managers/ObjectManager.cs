using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    public HashSet<Monster> Monsters { get; } = new HashSet<Monster>();
    public HashSet<MainCharacter> MainCharacters { get; } = new HashSet<MainCharacter>();
    public T Spawn<T>(object pos, int templateID = 0, string prfabName = "") where T : Creature
    {
        System.Type type = typeof(T);
        if (type == typeof(MainCharacter))
        {
            GameObject go = Managers.Resource.Instantiate("Player");
            go.name = "Player";
            MainCharacter mc = go.GetComponent<MainCharacter>();
            MainCharacters.Add(mc);
            return mc as T;
        }
        if (type == typeof(Monster))
        {
            GameObject go = Managers.Resource.Instantiate("Monster");
            go.name = "Monster";
            Monster mt = go.GetComponent<Monster>();
            Monsters.Add(mt);
            return mt as T;
        }
        return null;
    }
}

