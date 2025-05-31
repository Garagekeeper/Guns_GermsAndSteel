using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Gaper : Monster
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

        //TODO 파일로 관리하도록 하자
        Hp = 10.0f;
        MaxHp = Hp;
        Speed = 3.5f;

        CreatureState = ECreatureState.Idle;
        CreatureMoveState = ECreatureMoveState.TargetCreature;

        AudioClip audioClip = Managers.Resource.Load<AudioClip>("monster roar");
        Managers.Sound.PlaySFX(audioClip, 0.1f);
    }

    protected override void UpdateIdle()
    {
        CreatureState = ECreatureState.Move;
    }

    protected override void UpdateMove()
    {
        base.UpdateMove();



        Vector2 vel = Rigidbody.velocity;
        if (vel != Vector2.zero)
        {
            if (vel.y != 0)
            {
                BottomState = vel.y > 0 ? ECreatureBottomState.MoveUp : ECreatureBottomState.MoveDown;
            }
            else if (vel.x != 0)
            {
                BottomState = vel.x > 0 ? ECreatureBottomState.MoveRight : ECreatureBottomState.MoveLeft;
            }
        }
    }


}
