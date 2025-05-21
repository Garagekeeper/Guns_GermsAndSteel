using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utility;
using static Define;
using System.Linq;

public class Boss_Gurdy : Boss
{
    private void Awake()
    {
        Init();
        StartCoroutine(CoUpdateAI());
    }

    public override void Init()
    {
        base.Init();
        Hp = 595.0f;
        MaxHp = 595.0f;
        BossType = EBossType.Gurdy;
        BossState = EBossState.Idle;
        CreatureMoveState = ECreatureMoveState.None;
        AttackDamage = 3f;
        PTCollider2D = GetComponent<PolygonCollider2D>();

        _flickerTarget.Add(GetComponent<SpriteRenderer>());
        foreach (var spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
        {
            _flickerTarget.Add(spriteRenderer);
        }
    }


    protected override void UpdateIdle()
    {
        base.UpdateIdle();

        //0.가장 가까운 목표 탐색
        Target = FindClosetTarget(this, Managers.Object.MainCharacters.ToList<Creature>());

        int randomValue = Random.Range(0, 100);

        //1. Skill 실행
        if (randomValue < 40f)
        {
            // 40% 확률로 SkillA 실행
            _currentSkill = EBossSkill.SkillA;
        }
        else if (randomValue < 70f)
        {
            // 30% 확률로 SkillB 실행 (40~70)
            _currentSkill = EBossSkill.SkillB;
        }
        else
        {
            // 나머지 30% 확률로 SkillC 실행 (70~100)
            _currentSkill = EBossSkill.SkillC;
        }
        BossState = EBossState.Skill;
    }

    protected override void UpdateSkill()
    {
        if (_currentSkill == EBossSkill.SkillA)
        {
            if (_coWait != null) return;
            string skillName = "";
            Vector2 dV = Target.transform.position - transform.position;
            if (dV.y > 0)
            {
                if (dV.x > 0)
                    skillName = _skillName[(int)_currentSkill] + "_L";
                else
                    skillName = _skillName[(int)_currentSkill] + "_R";
            }
            else
            {
                if (Mathf.Abs(dV.x) > Mathf.Abs(dV.y) && dV.x > 0)
                    skillName = _skillName[(int)_currentSkill] + "_L";
                if (Mathf.Abs(dV.x) > Mathf.Abs(dV.y) && dV.x < 0)
                    skillName = _skillName[(int)_currentSkill] + "_R";
                if (Mathf.Abs(dV.x) < Mathf.Abs(dV.y))
                    skillName = _skillName[(int)_currentSkill] + "_D";

            }

            AnimatorBottom.Play(skillName, 0, 0);
            if (skillName != AnimatorBottom.GetCurrentAnimatorClipInfo(0)[0].clip.name)
                return;
            float delay = AnimatorBottom.GetCurrentAnimatorClipInfo(0)[0].clip.length;

            StartWait(delay);
        }
        else
        {
            base.UpdateSkill();
        }
    }

    public void Generate5Projectil(int vec)
    {
        Vector2 dV = Vector2.zero;
        if (vec == 0)
        {
            dV = Vector2.right;
        }
        else if (vec == 1)
        {
            dV = Vector2.down;

        }
        else if (vec == 2)
        {
            dV = Vector2.left;
        }

        GenerateProjectile(dV, false, true);
        for (int i = 1; i <= 2; i++)
        {
            GenerateProjectile(VectorRotation2D(dV, 10f * i), false, true);
        }
        for (int i = 1; i <= 2; i++)
        {
            GenerateProjectile(VectorRotation2D(dV, -10f * i), false, true);
        }

    }

    //Spawn Boils
    public void SkillB()
    {
        foreach (Monster m in Managers.Object.Monsters)
        {
            if (m.MonsterType == EMonsterType.Boil) return;
        }

        Transform parent = transform.parent;
        Managers.Object.Spawn<Monster>(transform.localPosition + new Vector3(1.5f, -4f), 10087, "Boil", parent);
        Managers.Object.Spawn<Monster>(transform.localPosition + new Vector3(-2.5f, -4f), 10087, "Boil", parent);
    }

    //Spawn 2 fly or 1 pooter
    public void SkillC()
    {
        int randomValue = Random.Range(0, 100);
        Transform parent = transform.parent;
        if (randomValue > 50)
        {
            Managers.Object.Spawn<Monster>(new Vector3(transform.localPosition.x + 1f, -1.65f), 10010, "Fly", parent);
            Managers.Object.Spawn<Monster>(new Vector3(transform.localPosition.x - 1f, -1.65f), 10010, "Fly", parent);
        }
        else
        {
            Managers.Object.Spawn<Monster>(new Vector3(transform.localPosition.x - 1f, -1.65f), 10001, "Pooter", parent);
        }
    }


}
