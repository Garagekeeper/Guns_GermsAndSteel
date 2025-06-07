using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static Define;

public class Pooter : Monster
{
    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        StartCoroutine(CoUpdateAI());
    }

    public override void Init()
    {
        base.Init();
        MonsterType = EMonsterType.Pooter;
        CreatureSize = ECreatureSize.Small;
        CreatureState = ECreatureState.Idle;
        Hp = 8.0f;
        MaxHp = 8.0f;
        AttackDamage = 2f;
        ShotSpeed = 0.7f;
        _isFloating = true;

        //audio
        AudioClip audioClip = Managers.Resource.Load<AudioClip>("insect swarm");
        SFXSource = Managers.Sound.PlaySFX(audioClip, 0.05f, true);
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();
        if ("Idle" != AnimatorBottom.GetCurrentAnimatorClipInfo(0)[0].clip.name)
            AnimatorBottom.Play("Idle", 0, 0);

        // 0. 가장 가까운 타겟 탐색
        Target = FindClosetTarget(this, Managers.Object.MainCharacters.ToList<Creature>());
        if (Target == null) return; 

        Vector2 dir = Target.transform.position - transform.position;
        Vector2 normalizedDir = dir.normalized;
        float distanceSqaure = dir.sqrMagnitude;

        // 1.범위내에 Target이 없을 경우 patrol
        if (distanceSqaure > 25)
        {
            //1)patrol은 여기서 간단하게 처리
            UpdateAITick = 1.0f;

            Rigidbody.velocity = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * Speed;

        }
        //1. 범위 내에 있는 경우 SkillA()
        else
        {
            _currentSkill = EMonsterSkill.SkillA;
            CreatureState = ECreatureState.Skill;
        }
    }

    public void SKillA()
    {
        Vector3 targetDir = (Target.transform.position - transform.position).normalized;
        GenerateProjectile(targetDir, false, true);
    }

    public override void OnDead()
    {
        // pool에 반납
        Managers.Sound.ReturnSFXToPool(SFXSource);
        base.OnDead();
    }

}
