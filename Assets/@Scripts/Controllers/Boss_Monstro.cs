using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static Define;

public class Boss_Monstro : Boss
{
    private Vector3 _targetPos;
    private Vector3 _startPos;
    Vector2 projectileDir = Vector2.zero;
    private new PolygonCollider2D Collider;
    private void Awake()
    {
        base.Init();
        Init();
        StartCoroutine(CoUpdateAI());
    }

    private void Update()
    {
        UpdateFacing(false);
        Rigidbody.velocity = Vector3.zero;
        //Debug.Log(Hp);
        //Debug.Log(Time.deltaTime);
    }

    public override void Init()
    {
        Hp = 250.0f;
        MaxHp = 250.0f;
        BossType = EBossType.Monstro;
        BossState = EBossState.Idle;

        AnimatorBottom = transform.GetComponent<Animator>();
        Collider = GetComponent<PolygonCollider2D>();

        //Debug
        //foreach (var clip in AnimatorBottom.runtimeAnimatorController.animationClips)
        //{
        //    Debug.Log(clip.name);
        //}
    }

    protected override void UpdateIdle()
    {
        if (Managers.Object.MainCharacters.Count == 0) return; 
        projectileDir = Vector3.zero;

        //0. 가장 가까운 목표 검색
        Target = FindClosetTarget(this, Managers.Object.MainCharacters.ToList<Creature>());
        //1. 목표와의 거리 계산
        Vector2 dir = Target.transform.position - transform.position;
        Vector2 normalizedDir = dir.normalized;
        float distanceSqaure = dir.sqrMagnitude;
        //2. 목표가 너무 먼 경우
        if (distanceSqaure > 16)
        {
            BossState = EBossState.Skill;
            _currentSkill = EBossSkill.SkillB;
        }
        //3. 목표가 가까운 경우
        else
        {
            //1) 보스가 동서남북(근처)에 있는 경우
            bool Up = Vector2.Dot(Vector2.up, normalizedDir)  > MathF.Cos(10 * Mathf.Deg2Rad);
            bool Right = Vector2.Dot(Vector2.right, normalizedDir) > MathF.Cos(10 * Mathf.Deg2Rad);
            bool Down = Vector2.Dot(Vector2.down, normalizedDir) > MathF.Cos(10 * Mathf.Deg2Rad);
            bool Left = Vector2.Dot(Vector2.left, normalizedDir) > MathF.Cos(10 * Mathf.Deg2Rad);

            if (Up) { projectileDir = Vector2.up; }
            if (Right) { projectileDir = Vector2.right; }
            if (Down) { projectileDir = Vector2.down; }
            if (Left) { projectileDir = Vector2.left; }

            //SkillC
            if (projectileDir != Vector2.zero)
            {
                BossState = EBossState.Skill;
                _currentSkill = EBossSkill.SkillC;
            }

            //2) 그 이외의  경우
            //SkillA
            else
            {
                BossState = EBossState.Skill;
                _currentSkill = EBossSkill.SkillA;
            }

            //3) 거리가 멀어지면 Idle 상태로
            if (distanceSqaure > 16)
            {
                BossState = EBossState.Idle;
                _currentSkill = EBossSkill.Normal;
            }
        }

    }

    protected override void UpdateSkill()
    {
        //현재 애니메이션이 재생중인 경우 종료
        if (_coWait != null) return;

        float delay = 0;
        base.UpdateSkill();

        AnimatorBottom.Play(_skillName[(int)_currentSkill], 0, 0);
        //Debug.Log(_skillName[(int)_currentSkill]);
        delay = AnimatorBottom.GetCurrentAnimatorClipInfo(0)[0].clip.length;

        StartWait(delay);
    }

    protected override void UpdateMove()
    {
        //_startPos = transform.position;
        transform.position = Vector3.Lerp(transform.position, _targetPos, Time.deltaTime * 2f);
    }

    public void SkillC()
    {
        //부채꼴 모양으로 projectile 생성
        //for (int i = 0; i < 10; i++)
        //{
        //    float random = UnityEngine.Random.Range(-10, 10) * Mathf.Deg2Rad;
        //    Debug.Log("random" + random);

        //    //a * cosΘ - b* sinΘ,  a*sinΘ + b*cosΘ ;
        //    var temp = new Vector3(projectileDir.x * Mathf.Cos(random) - projectileDir.y * Mathf.Sin(random), projectileDir.x * Mathf.Sin(random) + projectileDir.y * Mathf.Cos(random));
        //    GenerateProjectile(temp);
        //    Debug.Log(temp);
        //}

       for (int i=0; i<10; i++)
        {
            GenerateProjectile(projectileDir,true);
        }
    }

    public void setTargetPos()
    {
        if (Target == null) return; 
        _targetPos = Target.transform.position;
        _startPos = transform.position;
    }

    public void ChangeBossState(EBossState bossState)
    {
        if (bossState == EBossState.Idle) _currentSkill = EBossSkill.Normal;
        BossState = bossState;
    }

    public void ChangeCollider(int on)
    {
        Collider.enabled = on == 1 ? true : false;
        transform.GetChild(0).GetComponent<CircleCollider2D>().enabled = on == 1 ? true : false;
    }

    public override void OnDead()
    {
        base.OnDead();
        Managers.UI.PlayingUI.BossHpActive(false);
    }
}
