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
        StartCoroutine(CoUpdateAI());
    }

    public override void Init()
    {
        MonsterType = EMonsterType.Fly;
        MonsterState = EMonsterState.Idle;
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
            MonsterState = EMonsterState.Move;
            AnimatorBottom.Play("HostileMove");
        }

    }

}
