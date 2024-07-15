using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using static Define;
using static UnityEditor.Progress;
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
    List<int> AcquiredItemList;



    public event Action<Item> UseActiveItem;
    public int SpaceItemId { get; set; } = 44;
    public int QItemId { get; set; } = 43002;

    public Item SpaceItem { get; set; }
    public Item QItem { get; set; }

    private bool _canMove = true;

    IEnumerator DelayBoolChange()
    {
        yield return new WaitForSeconds(0.5f);
        _canMove = true;
    }

    public bool CanMove
    {
        get { return _canMove; }
        set
        {
            if (_canMove != value)
            {
                if (value == true)
                    StartCoroutine("DelayBoolChange");
                else
                {
                    _canMove = value;
                    BottomState = ECreatureBottomState.Idle;
                    HeadState = ECreatureHeadState.Idle;
                }

            }
        }
    }
    //protected List<Item> _passiveItem;

    public float DamageByOtherConstant { get; set; } = 0.5f;

    #endregion

    public RoomClass CurrentRoom { get; set; }
    private Type _target;
    System.Random random = new System.Random();
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

        SpaceItem = new Item();
        QItem = new Item();

        UseActiveItem -= HandleUsingActiveItem;
        UseActiveItem += HandleUsingActiveItem;
        CreatureType = ECreatureType.MainCharacter;
        CanMove = true;

        LayerMask mask = 0;
        if (CreatureType == ECreatureType.MainCharacter) mask |= (1 << 6);
        Collider.excludeLayers = mask;


        ChangeSpaceItem(SpaceItem, SpaceItemId);
        ChangeQItem(QItem, QItemId);
        Managers.UI.PlayingUI.ChangeChargeBarSize("ui_chargebar_" + (9 - SpaceItem.CoolTime));


        CurrentRoom = Managers.Map.CurrentRoom;
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

        if (Input.GetKeyDown(KeyCode.Q))
            UseItem(QItem);

        if (Input.GetKeyDown(KeyCode.Space))
            UseItem(SpaceItem);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            SpaceItem.CoolDownGage = Math.Min(SpaceItem.CoolDownGage + 1, SpaceItem.CoolTime);
            Managers.Game.UseActiveItem(SpaceItem.CoolDownGage, SpaceItem.CoolTime, "Up");
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            Managers.Map.RoomClear();
        }


        #endregion
    }
    private void UpdateMovement(Vector2 vel)
    {
        if (CanMove == false) return;
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

    public void UseItem(Item item)
    {
        if (item == null)
            return;

        if (item.ItemType == EItemType.ActiveItem)
        {
            if (item.CoolDownGage == item.CoolTime)
            {
                item.CoolDownGage = 0;
                UseActiveItem?.Invoke(item);
                Managers.Game.UseActiveItem(item.CoolDownGage, item.CoolTime, "Down");
                //ApplyItemEffect(item);
            }
        }
        else
        {
            UseActiveItem?.Invoke(item);
            ChangeQItem(item, 0);
            QItem = null;
        }

    }

    public void ApplyItemEffect(Item item)
    {
        switch (item.ItemEfec)
        {
            case EItemEfect.Up:
                var temp = _target.GetProperty(item.Target).GetValue(this);
                break;
            case EItemEfect.Down:
                break;
            case EItemEfect.Teleport:
                break;
            case EItemEfect.Roll:
                break;

        }
    }

    private void HandleUsingActiveItem(Item item)
    {
        if (item.Target == "Position")
        {
            Managers.Game.TPToNormalRandom();
        }
        else
        {
            switch (item.Target)
            {
                case "AttackDamage":
                    AttackDamage += (float)((int)item.ItemEfec) * item.Value;
                    //TODO
                    //add multiplyer
                    break;

                case "Hp":
                    Hp += (float)((int)item.ItemEfec) * item.Value;
                    //TODO
                    //HPCheck (dead Check, MaxCheck)
                    break;
            }
        }

        Debug.Log(AttackDamage);
        Debug.Log(Hp);

    }

    public override void OnDamaged(Creature owner, ESkillType skillType)
    {
        Hp -= DamageByOtherConstant;

        Debug.Log(Hp);

        //TODO HpCheck
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

    public void ChangeSpaceItem(Item item, int id)
    {
        item.ChangeItem(id);
        Managers.UI.PlayingUI.ChangeSpaceItem(item.SpriteName);
    }

    public void ChangeQItem(Item item, int id)
    {
        item.ChangeItem(id);
        if (item.ItemType == EItemType.Pills)
        {
            Managers.UI.PlayingUI.ChangeQItem(item.SpriteName + (random.Next() % 13));
            return;
        }
        Managers.UI.PlayingUI.ChangeQItem(item.SpriteName);
    }


    public void AcquireItem()
    {
        //TODO
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Door")
        {
            CanMove = false;
            Managers.Game.GoToNextRoom(collision.transform.name);
        }

        if (collision.transform.tag == "Monster")
        {
            OnDamaged(collision.transform.gameObject.GetComponent<Monster>(), ESkillType.BodySlam);
        }
    }
}
