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
    Vector3[] middleDirVec = new Vector3[4];
    Vector3[] smallDirVec = new Vector3[8];

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        //Rigidbody.velocity = TargetPos.normalized * Speed;
        // 큰 몸체가 살아있는 경우
        if (MaxHp == 60f)
            UpdateMove();
    }

    public override void Init()
    {
        base.Init();
        BossType = EBossType.Fistula;
        BossState = EBossState.Idle;
        CreatureMoveState = ECreatureMoveState.Designated;
        TargetPos = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
        MaxHp = 60f;
        Hp = 60f;

        _flickerTarget.Add(GetComponent<SpriteRenderer>());
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
    }

    // HP 값 변경
    public void ChangepiecesHpValue(float value, string name = "")
    {
        int index;
        // 중간 사이즈
        if (name.Contains("Middle"))
        {
            // middle 사이즈의 체력을 담은 배열을 참조하기 위해 인덱스 추출
            index = (name[name.Length - 1] - '0') - 1;
            if (middleHp[index] <= 0) return;

            // 전체 Hp에서 공격력, 중간 체력 중 작은 값을 뺀다 (중간이 죽을때 남은 체력보다 공격력이 많으면 체력만큼만 차감)
            Hp += Mathf.Min(value, -1 * middleHp[index]);
            // 중간 체력을 0과 value 사이의 값으로 조절한다.
            middleHp[index] = Mathf.Clamp(middleHp[index] + value, 0, value);

            if (middleHp[index] <= 0)
            {
                middleHp[index] = 0;
                MiddleOnDead(transform.Find(name).gameObject);
            }
        }
        // 작은 사이즈
        else if (name.Contains("Small"))
        {
            index = (name[name.Length - 1] - '0') - 1;
            if (smallHp[index] <= 0) return;

            Hp += Mathf.Min(value, -1 * smallHp[index]);
            smallHp[index] = Mathf.Clamp(smallHp[index] + value, 0, value);
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
        // 분리되지 않은 상태
        if (MaxHp == 60f)
        {
            Vector3[] dV = { new Vector3(1, 1), new Vector3(1, -1), new Vector3(-1, 1), new Vector3(-1, -1) };
            gameObject.GetComponent<CircleCollider2D>().enabled = false;
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            Rigidbody.velocity = Vector3.zero;

            int i = 0;
            foreach (Transform t in transform)
            {
                if (t.gameObject.name.Contains("Shadow")) t.gameObject.SetActive(false);
                else
                {
                    t.gameObject.SetActive(true);
                    t.gameObject.GetComponent<Boss_Fistula_pieces>().Init(dV[i], 4f, this);
                    ChangepiecesHpValue(15);
                    MaxHp += 15f;
                    i++;
                }

            }
        }
        // 분리된 상태
        else
        {
            foreach (var hp in smallHp) { if (hp != 0f) isSmallFistulaAlive = true; }
            foreach (var hp in middleHp) { if (hp != 0f) isMiddleFistulaAlive = true; }

            if (isMiddleFistulaAlive || isSmallFistulaAlive) return;

            base.OnDead();
        }
    }

    // 중간 사이즈 fistula가 죽을 때
    public void MiddleOnDead(GameObject go)
    {
        Vector3[] dV = { new Vector3(1, 1), new Vector3(-1, -1) };
        go.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        go.GetComponent<CircleCollider2D>().enabled = false;
        go.GetComponent<SpriteRenderer>().enabled = false;

        int i = 0;
        bool On = true;
        // 조금 더러운 코드 (수정 필요)
        foreach (Transform t in go.transform)
        {
            // shadow gameobject는 끈다
            if (i > 1)
            {
                On = false;
            }

            // 나머지 small pieces는 킨다
            t.gameObject.SetActive(On);

            // small pieces는 소환하고, 체력을 증가시킴
            if (i <= 1)
            {
                ChangepiecesHpValue(8f);
                MaxHp += 8f;
                t.gameObject.GetComponent<Boss_Fistula_pieces>().Init(dV[i], 5f, this);
            }
            i++;
        }
    }

    public void SmallOnDead(GameObject go)
    {
        go.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        go.SetActive(false);
        
        Managers.Object.Spawn<Monster>(go.transform.position, 0, "Maggot");

        OnDead();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ReflectTargetVecor(collision);
    }
}
