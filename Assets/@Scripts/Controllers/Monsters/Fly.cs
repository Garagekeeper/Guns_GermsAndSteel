using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class Fly : Monster
{
    enum EFlyType
    {
        Neutral,
        Hostile,
    }

    EFlyType FlyType { get; set; }

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
        CreatureSize = ECreatureSize.Small;
        CreatureState = ECreatureState.Idle;
        MonsterType = EMonsterType.Fly;
        _isFloating = true;
        base.Init();

        // 중립 적대 구분 (50%)
        if (Random.Range(1, 101) <= 50)
        {
            FlyType = EFlyType.Neutral;
            Speed = 0.5f;
            AnimatorBottom.Play("NeutralIdle");
        }
        else
        {
            FlyType = EFlyType.Hostile;
            AnimatorBottom.Play("HostileIdle");
        }

        //audio
        AudioClip audioClip = Managers.Resource.Load<AudioClip>("insect swarm");
        SFXSource = Managers.Sound.PlaySFX(audioClip, 0.5f, true);
    }

    protected override void UpdateIdle()
    {
        // 0.중립일 경우 patrol
        if (FlyType == EFlyType.Neutral)
        {
            //1)patrol은 여기서 간단하게 처리
            UpdateAITick = 1.0f;

            Rigidbody.velocity =  new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * Speed;
        }


        // 1.적대일 경우 플레이어 쫒아감
        else
        {
            Target = FindClosetTarget(this, Managers.Object.MainCharacters.ToList<Creature>());
            CreatureState = ECreatureState.Move;
            AnimatorBottom.Play("HostileMove");
        }
    }

    public override void OnDead()
    {
        // pool에 반납
        Managers.Sound.ReturnSFXToPool(SFXSource);
        base.OnDead();
    }

}
