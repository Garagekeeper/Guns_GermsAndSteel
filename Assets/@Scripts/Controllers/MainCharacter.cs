using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using static Define;
public class MainCharacter : Creature
{
    #region Stat

    //Todo
    //float PercentageOfDevil;
    //float PercentageOfAngel;
    int Coin;
    int Bomb = 1;
    //TODO
    //var SubItem;
    //var ActiveItem;
    //List<int> AcquiredItemList;
    //
    #endregion

    private Vector3 _moveDir;


    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        HeadSprite = new Sprite[]
       {
            Managers.Resource.Load<Sprite>("isaac_up"),
            Managers.Resource.Load<Sprite>("isaac_down"),
            Managers.Resource.Load<Sprite>("isaac_right"),
       };
    }

    void Update()
    {
        #region Attack
        Vector2 attackVel = Vector2.zero;
        attackVel.x = Input.GetAxis("AttackHorizontal");
        attackVel.y = Input.GetAxis("AttackVertical");

        if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
            attackVel.x *= -1;
        if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.DownArrow))
            attackVel.y *= -1;

        UpdateAttack(attackVel);
        #endregion

        #region Movement
        Vector2 vel = Rigidbody.velocity;
        vel.x = Input.GetAxis("Horizontal") * Speed;
        vel.y = Input.GetAxis("Vertical") * Speed;

        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
            vel.x = 0;
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S))
            vel.y = 0;

        UpdateMovement(vel);
        #endregion
    }
    public void UpdateMovement(Vector2 vel)
    {
        Rigidbody.velocity = vel;

        if (vel != Vector2.zero)
        {
            if (vel.y != 0 && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)))
            {
                BottomState = vel.y > 0 ? ECreatureBottomState.MoveUp : ECreatureBottomState.MoveDown;
                if (HeadState == ECreatureHeadState.Idle)
                    HeadDirState = vel.y > 0 ? ECreatureHeadDirState.Up : ECreatureHeadDirState.Down;
            }
            else if (vel.x != 0)
            {
                BottomState = vel.x > 0 ? ECreatureBottomState.MoveRight : ECreatureBottomState.MoveLeft;
                if (HeadState == ECreatureHeadState.Idle)
                    HeadDirState = vel.x > 0 ? ECreatureHeadDirState.Right : ECreatureHeadDirState.Left;
            }
        }
        else
        {
            BottomState = ECreatureBottomState.Idle;
            if (HeadState == ECreatureHeadState.Idle)
                HeadDirState = ECreatureHeadDirState.Down;
        }
    }

    public void UpdateAttack(Vector2 attackVel)
    {
        if (attackVel != Vector2.zero)
        {
            HeadDirState = ECreatureHeadDirState.None;
            AnimatorHead.enabled = true;
            HeadState = ECreatureHeadState.Attack;

            if (attackVel.y != 0 && (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)))
            {
                HeadDirState = attackVel.y > 0 ? ECreatureHeadDirState.Up : ECreatureHeadDirState.Down;
            }
            else if (attackVel.x != 0)
            {
                HeadDirState = attackVel.x > 0 ? ECreatureHeadDirState.Right : ECreatureHeadDirState.Left;
            }
        }
        else
        {
            AnimatorHead.enabled = false;
            HeadState = ECreatureHeadState.Idle;
        }
    }
    
}
