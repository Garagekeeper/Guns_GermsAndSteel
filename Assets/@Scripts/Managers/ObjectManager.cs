using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Define;

public class ObjectManager
{
    public HashSet<MainCharacter> MainCharacters { get; } = new HashSet<MainCharacter>();
    public HashSet<Monster> Monsters { get; } = new HashSet<Monster>();
    public HashSet<Boss> Bosses { get; } = new HashSet<Boss>();
    public HashSet<Pickup> Pickups { get; } = new HashSet<Pickup>();

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
            GameObject go = Managers.Resource.Instantiate(prfabName);
            go.name = prfabName;
            Monster mt = go.GetComponent<Monster>();
            mt.transform.position = pos;
            Monsters.Add(mt);
            return mt as T;
        }
        if (type == typeof(Boss))
        {
            GameObject go = Managers.Resource.Instantiate(prfabName);
            go.name = prfabName;
            Boss bs = go.GetComponent<Boss>();
            bs.transform.position = pos;
            Bosses.Add(bs);
            return bs as T;
        }
        return null;
    }

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

    public void ClearObjectManager()
    {
        MainCharacters.Clear();
        Monsters.Clear();
        Bosses.Clear();
        Pickups.Clear();
    }

}

