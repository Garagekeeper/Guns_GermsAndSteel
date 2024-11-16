using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;
using Random = UnityEngine.Random;

public class Boss_Fistula : Boss
{
    //Total HP: 60 (Large) + 4x15(Medium) +8x8(Small) = 184
    float[] middleHp = { 15f, 15f, 15f, 15f };
    float[] smallHp = { 8f, 8f, 8f, 8f, 8f, 8f, 8f, 8f };
    Vector3 dirVec;
    Vector3[] middleDirVec = new Vector3[4];
    Vector3[] smallDirVec = new Vector3[8];

    private void Awake()
    {
        base.Init();
        Init();
    }

    private void Update()
    {
        Rigidbody.velocity = dirVec.normalized * Speed;
    }

    public override void Init()
    {
        Hp = 60f;
        MaxHp = 60f;
        BossType = EBossType.Fistula;
        BossState = EBossState.Idle;
        CCollider2D = GetComponent<CircleCollider2D>();
        Rigidbody = gameObject.GetComponent<Rigidbody2D>();
        dirVec = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
    }

    public override void OnDamaged(Creature owner, ESkillType skillType, string name = "")
    {
        if (MaxHp == 60)
            base.OnDamaged(owner, skillType);
        else
        {
            switch (skillType)
            {
                case ESkillType.BodySlam:
                    //TODO
                    break;
                case ESkillType.Bomb:
                    ChangepiecesHpValue(-1 * owner.BombDamage, name);
                    break;
                case ESkillType.Projectile:
                    ChangepiecesHpValue(-1 * owner.AttackDamage, name);
                    break;
                case ESkillType.Fire:
                    break;
                case ESkillType.Spike:
                    break;
            }
            Managers.UI.PlayingUI.ChangeBossHpSliderRatio(Hp / MaxHp);
        }
        Debug.Log(MaxHp);
        Debug.Log(Hp);
    }

    public void ChangepiecesHpValue(float value, string name = "")
    {
        int index;
        if (name.Contains("Middle"))
        {
            index = (name[name.Length - 1] - '0') - 1;
            if (middleHp[index] <= 0) return;

            Hp += Mathf.Max(value, -1 * middleHp[index]);
            middleHp[index] += value;
            if (middleHp[index] <= 0)
            {
                middleHp[index] = 0;
                MiddleOnDead(transform.Find(name).gameObject);
            }
        }
        else if (name.Contains("Small"))
        {
            index = (name[name.Length - 1] - '0') - 1;
            if (smallHp[index] <= 0) return;

            Hp += Mathf.Max(value, -1 * smallHp[index]);
            smallHp[index] += value;
            if (smallHp[index] <= 0)
            {
                smallHp[index] = 0;
                SmallOnDead(Utility.FindChildByName(transform, name).gameObject);
            }
        }
        else
        {
            Hp += value;
        }
    }


    public override void OnDead()
    {
        bool isSmallFistulaAlive = false;
        bool isMiddleFistulaAlive = false;
        if (MaxHp == 60f)
        {
            Vector3[] dV = { new Vector3(1, 1), new Vector3(1, -1), new Vector3(-1, 1), new Vector3(-1, -1) };
            gameObject.GetComponent<CircleCollider2D>().enabled = false;
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            Rigidbody.velocity = Vector3.zero;

            int i = 0;
            bool On = false;
            foreach (Transform t in transform)
            {
                if (i <= 1) On = false;
                else On = true;
                t.gameObject.SetActive(On);
                i++;
                if (i <= 2)
                    continue;
                t.gameObject.GetComponent<Boss_Fistula_pieces>().Init(dV[i - 3], 4f);
                ChangepiecesHpValue(15);
                MaxHp += 15f;
            }
        }
        else
        {
            foreach (var hp in smallHp) { if (hp != 0f) isSmallFistulaAlive = true; }
            foreach (var hp in middleHp) { if (hp != 0f) isMiddleFistulaAlive = true; }

            if (isMiddleFistulaAlive || isSmallFistulaAlive) return;

            base.OnDead();
        }
    }

    public void MiddleOnDead(GameObject go)
    {
        Vector3[] dV = { new Vector3(1, 1), new Vector3(-1, -1) };
        go.GetComponent<CircleCollider2D>().enabled = false;
        go.GetComponent<SpriteRenderer>().enabled = false;

        int i = 0;
        bool On = true;
        foreach (Transform t in go.transform)
        {
            if (i > 1)
            {
                On = false;
            }

            t.gameObject.SetActive(On);

            if (i <= 1)
            {
                ChangepiecesHpValue(8f);
                MaxHp += 8f;
                t.gameObject.GetComponent<Boss_Fistula_pieces>().Init(dV[i], 5f);
            }
            i++;
        }
    }

    public void SmallOnDead(GameObject go)
    {
        go.SetActive(false);
        OnDead();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Projectile" || collision.gameObject.tag == "Player")
        {

        }
        else
        {
            Vector3 hitPos = collision.GetContact(0).point;

            Debug.Log("dirVec: " + dirVec);
            //Vector3 incomingVec = hitPos - dirVec;
            Vector3 reflectVec = Vector3.Reflect(dirVec, collision.GetContact(0).normal);
            dirVec = reflectVec;

            //Debug.Log("incomingVec: " + incomingVec);
            Debug.Log("reflectVec: " + reflectVec);
        }
    }
}
