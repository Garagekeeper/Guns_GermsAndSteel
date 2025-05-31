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
        AudioClip audioClip = Managers.Resource.Load<AudioClip>("summonsound");
        System.Type type = typeof(T);
        if (type == typeof(MainCharacter))
        {
            GameObject go = Managers.Resource.Instantiate("Player");
            go.name = "Player" + Managers.Game.Seed;
            MainCharacter mc = go.GetComponent<MainCharacter>();
            mc.transform.position = pos + new Vector3(0.5f, 0.5f, 0);
            MainCharacters.Add(mc);
            return mc as T;
        }
        if (type == typeof(Monster))
        {
            GameObject monster = Managers.Resource.Instantiate(prfabName, parent);
            monster.name = prfabName;
            Monster mt = monster.GetComponent<Monster>();
            mt.transform.localPosition = pos + new Vector3(0.5f, 0.5f, 0);

            GameObject spawn_effect = Managers.Resource.Instantiate("Monster_Spawn_Effect", monster.transform);
            spawn_effect.gameObject.SetActive(true);
            spawn_effect.transform.localPosition = Vector3.zero;

            string spawn_effect_string = "Monster_Spawn_Effect";
            if (mt.CreatureSize == ECreatureSize.Small) spawn_effect_string = "Monster_Spawn_Effect_Small";
            spawn_effect.GetComponent<Animator>().Play(spawn_effect_string);

            Monsters.Add(mt);
            CoroutineHelper.Instance.StartMyCoroutine(WaitSpawn());
            Managers.Sound.PlaySFX(audioClip, 0.1f);
            return mt as T;
        }
        if (type == typeof(Boss))
        {
            GameObject go = Managers.Resource.Instantiate(prfabName, parent);
            go.name = prfabName;
            Boss bs = go.GetComponent<Boss>();
            bs.transform.localPosition = pos + new Vector3(0.5f, 0.5f, 0);
            Bosses.Add(bs);
            return bs as T;
        }
        return null;
    }
    public T Spawn<T>(Vector3 pos, int templateID, Transform parent) where T : Creature
    {
        return Spawn<T>(pos, templateID, Managers.Data.MonsterDic[templateID].PrefabName, parent);
    }

    public IEnumerator WaitSpawn()
    {
        yield return new WaitForSecondsRealtime(0.5f);


        foreach (var temp in Managers.Object.Monsters)
        {
            temp.GetComponent<Monster>().enabled = true;
            if (FindChildByName(temp.transform, "Monster_Spawn_Effect") == null) continue;
            Object.Destroy(FindChildByName(temp.transform, "Monster_Spawn_Effect").gameObject);
        }
    }

    //spawn pickup
    public T Spawn<T>(Vector3 pos, EPICKUP_TYPE epickup_type, Transform parent = null, Vector3 dir = default, bool soundPlay = false) where T : Pickup
    {
        if (epickup_type == EPICKUP_TYPE.PICKUP_NULL) return null;

        string prefabName = "Pickup";

        GameObject go = Managers.Resource.Instantiate(prefabName, parent);

        go.name = prefabName;
        go.transform.localPosition = pos + new Vector3(0.5f, 0.5f, 0);
        Pickup pickup = go.GetComponent<Pickup>();

        if (pickup != null)
        {
            pickup.Init(epickup_type, dir, soundPlay);
            Pickups.Add(pickup);
            return pickup as T;
        }

        return null;
    }

    // spawn obstacle
    public Obstacle SpawnObstacle(Vector3 localPos, string prefabName, Transform parent = null, int index = 1)
    {
        GameObject go = Managers.Resource.Instantiate(prefabName, parent);

        go.name = prefabName;
        go.transform.localPosition = localPos + new Vector3(0.5f, 0.5f, 0);
        Obstacle obstacle = go.GetComponent<Obstacle>();

        if (obstacle != null)
        {
            obstacle.Init(prefabName, index);
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
            Object.Destroy(obj.gameObject);
        }
        else if (obj is Pickup pickup)
        {
            Pickups.Remove(pickup);
            pickup.DestroyPickup();
        }
    }

    public void DespawnMonsters(RoomClass room)
    {
        Transform mgo = FindChildByName(room.Transform, "Monster");
        foreach (Transform monsters in mgo)
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

