using Data;
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
    int BombCount = 1;
    //TODO
    //var SubItem;
    //var ActiveItem;
    //List<int> AcquiredItemList;
    //public int SpaceItemId { get; set; } = 43003;
    //public int QItemId { get; set; } = 43002;

    //public Item SpaceItem { get; set; }
    //public Item QItem { get; set; }

    //protected List<Item> _passiveItem;

    public float DamageByOtherConstant { get; set; } = 0.5f;

    #endregion

    private Vector3 _moveDir;
    //private Type _target;

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

        //SpaceItem = new Item(SpaceItemId);
        //QItem = new Item(QItemId);



        CreatureType = ECreatureType.MainCharacter;
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

        if (Input.GetKeyDown(KeyCode.E))
            SpawnBomb();

        //if (Input.GetKeyDown(KeyCode.Q))
        //    UseItem(QItem);

        //if (Input.GetKeyDown(KeyCode.Space))
        //    UseItem(SpaceItem);

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

    //public void UseItem(Item item)
    //{
    //    if (item.CoolDownGage == item.CoolTime)
    //        ApplyitemEffect(item);
    //}

    //public void ApplyitemEffect(Item item)
    //{
    //    switch (item.ItemEfec)
    //    {
    //        case EItemEfect.Up:
    //            var temp =_target.GetProperty(item.Target).GetValue(this);
    //            break;
    //        case EItemEfect.Down:
    //            break;
    //        case EItemEfect.Teleport:
    //            break;
    //        case EItemEfect.Roll:
    //            break;

    //    }
    //}

    public override void OnDamaged(Creature owner, ESkillType skillType)
    {
        Hp -= DamageByOtherConstant;

        Debug.Log(Hp);

        if (Hp <= 0)
        {
            OnDead();
            return;
        }

    }

    public void SpawnBomb()
    {
        if (BombCount <= 0) return;
        GameObject go = Managers.Resource.Instantiate("Bomb");

        Bomb bomb = go.GetComponent<Bomb>();
        bomb.SetInfo(this);
        BombCount--;
    }

    //public Event


}
