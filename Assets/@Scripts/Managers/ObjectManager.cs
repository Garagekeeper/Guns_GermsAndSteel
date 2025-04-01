using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

using static Define;
using static Utility;

public class ObjectManager
{
    public HashSet<MainCharacter> MainCharacters { get; } = new HashSet<MainCharacter>();
    public HashSet<Monster> Monsters { get; } = new HashSet<Monster>();
    public HashSet<Boss> Bosses { get; } = new HashSet<Boss>();
    public HashSet<Pickup> Pickups { get; } = new HashSet<Pickup>();

    // spawn creature
    public T Spawn<T>(Vector3 pos, int templateID = 0, string prfabName = "", Transform parent = null) where T : Creature
    {
        System.Type type = typeof(T);
        if (type == typeof(MainCharacter))
        {
            GameObject go = Managers.Resource.Instantiate("Player");
            go.name = "Player" + Managers.Game.Seed;
            MainCharacter mc = go.GetComponent<MainCharacter>();
            mc.transform.position = pos;
            MainCharacters.Add(mc);
            return mc as T;
        }
        if (type == typeof(Monster))
        {
            GameObject monster = Managers.Resource.Instantiate(prfabName);
            monster.name = prfabName;
            Monster mt = monster.GetComponent<Monster>();
            mt.transform.SetParent(parent);
            mt.transform.localPosition = pos;

            GameObject spawn_effect = Managers.Resource.Instantiate("Monster_Spawn_Effect");
            spawn_effect.transform.SetParent(monster.transform);
            spawn_effect.gameObject.SetActive(true);
            spawn_effect.transform.localPosition = Vector3.zero;

            Monsters.Add(mt);
            return mt as T;
        }
        if (type == typeof(Boss))
        {
            GameObject go = Managers.Resource.Instantiate(prfabName);
            go.name = prfabName;
            Boss bs = go.GetComponent<Boss>();
            bs.transform.SetParent(parent);
            bs.transform.localPosition = pos;
            Bosses.Add(bs);
            return bs as T;
        }
        return null;
    }

    //spawn pickup
    public T Spawn<T>(Vector3 pos, EPICKUP_TYPE epickup_type, Transform parent = null) where T : Pickup
    {
        if (epickup_type == EPICKUP_TYPE.PICKUP_NULL) return null;

        string prefabName = "Pickup";

        GameObject go = Managers.Resource.Instantiate(prefabName);
        if (parent != null) 
            go.transform.parent = parent;

        go.name = prefabName;
        go.transform.localPosition = pos;
        Pickup pickup = go.GetComponent<Pickup>();
        
        if (pickup != null)
        {
            pickup.Init(epickup_type);
            Pickups.Add(pickup);
            return pickup as T;
        }

        return null;
    }

    // spawn obstacle
    public Obstacle SpawnObstacle(Vector3 localPos, string prefabName, Transform parent)
    {
        GameObject go = Managers.Resource.Instantiate(prefabName);

        if (parent != null)
            go.transform.parent = parent;

        go.name = prefabName;
        go.transform.localPosition = localPos + new Vector3(0.5f, 0.5f,0);
        Obstacle obstacle = go.GetComponent<Obstacle>();

        if (obstacle != null)
        {
            obstacle.Init(prefabName);
            return obstacle;
        }

        return null;
    }

    
    public void Despawn<T>(T obj) where T : BaseObject
    {
        if (obj is Creature creature)
        {
            if (creature.CreatureType == ECreatureType.MainCharacter)
            {
                MainCharacters.Remove(obj as MainCharacter);
            }
            else if (creature.CreatureType == ECreatureType.Monster)
            {
                Monsters.Remove(obj as Monster);
            }
            else if (creature.CreatureType == ECreatureType.Boss)
            {
                Bosses.Remove(obj as Boss);
            }
        }
        else if (obj is Pickup pickup)
        {
            Pickups.Remove(pickup);
        }
       Object.Destroy(obj.gameObject);
    }

    public void DespawnMonsters(RoomClass room)
    {
        Transform mgo = FindChildByName(room.Transform, "Monster");
        foreach(Transform monsters in mgo)
        {
            Despawn(monsters.gameObject.GetComponent<Creature>());
        }
    }

    public void ClearObjectManager(bool includePlayer = false)
    {
        if (includePlayer)
            MainCharacters.Clear();
        Monsters.Clear();
        Bosses.Clear();
        Pickups.Clear();
    }

}

