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
    List<Item> AcquiredPassiveItemList;
    List<Familiar> AcquiredFamiliarItemList;



    public event Action<Item> UseActiveItem;
    public int SpaceItemId { get; set; } = 45044;
    public int QItemId { get; set; } = 44002;

    public Item SpaceItem { get; set; }
    public Item QItem { get; set; }

    private bool _canMove = true;
    private bool _oneTimeActive = false;

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
        if (CanMove)
        {
            vel.x = Input.GetAxis("Horizontal") * Speed;
            vel.y = Input.GetAxis("Vertical") * Speed;

            if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
                vel.x = 0;
            if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S))
                vel.y = 0;

        }
        else
        {
            vel = Vector2.zero;
        }

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
            HeadState = ECreatureHeadState.Idle;
            if (BottomState == ECreatureBottomState.Idle)
                HeadDirState = ECreatureHeadDirState.Down;
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

    public void GetItem(Item item)
    {
        if (item.ItemType == EItemType.Familliar)
        {
            AcquiredFamiliarItemList.Add(new Familiar(item));
        }
        else if (item.ItemType == EItemType.Passive)
        {
            AcquiredPassiveItemList.Add(item);
            ApplyPassiveItemEffect(item);
        }
        else if (item.ItemType == EItemType.ActiveItem)
        {
            if (SpaceItem == null)
            {
                SpaceItem = item;
                SpaceItemId = item.TemplateId;
            }
            else
                ChangeSpaceItem(SpaceItem, item.TemplateId);
        }
        else if (item.ItemType == EItemType.Cards)
        {
            if (QItem == null)
            {
                QItem = item;
                QItemId = item.TemplateId;
            }
            else
                ChangeQItem(QItem, item.TemplateId);
        }
        else if (item.ItemType == EItemType.Pills)
        {
            if (QItem == null)
            {
                QItem = item;
                QItemId = item.TemplateId;
            }
            else
                ChangeQItem(QItem, item.TemplateId);
        }
    }

    public void DeleteItem(Item item)
    {

    }

    public void ApplyPassiveItemEffect(Item item, bool isOneTime = false)
    {
        if (isOneTime)
        {
            //TODO
        }

        Hp += item.Hp;
        AttackDamage += item.AttackDamage;
        Tears += item.Tears;
        Range += item.Range;
        ShotSpeed += item.ShotSpeed;
        Speed += item.Speed;
        Luck += item.Luck;
        Life += item.Life;
        //item.SetItem;
        //item.ShotType;
    }

    //issac에는 그 방에서만 능력치를 올려주는 액티브 아이템이 있다.
    //일회성 버프 아이템은 패시브 아이템처럼 적용하되, 방이 클리어되면회수 되도록한다.
    //다른 액티브 아이템은 
    private void HandleUsingActiveItem(Item item)
    {
        if (item.EffectOfActive == ESpecialEffectOfActive.Null)
            ApplyPassiveItemEffect(item, true);
        else
        {
            switch (item.EffectOfActive)
            {
                case ESpecialEffectOfActive.RandomTeleport:
                    Managers.Game.TPToNormalRandom();
                    break;
                case ESpecialEffectOfActive.UncheckedRoomTeleport:
                    break;
                case ESpecialEffectOfActive.Roll:
                    break;
            }
        }
    }

    public override void OnDamaged(Creature owner, ESkillType skillType)
    {
        Hp -= DamageByOtherConstant;

        Debug.Log(Hp);
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Door" && Managers.Map.CurrentRoom.IsClear)
        {
            CanMove = false;
            Managers.Game.GoToNextRoom(collision.transform.name);
        }

        if (collision.transform.tag == "StageDoor" && Managers.Map.CurrentRoom.IsClear)
        {
            Managers.Game.GoToNextStage();
        }

        if (collision.transform.tag == "Monster")
        {
            OnDamaged(collision.gameObject.GetComponent<Monster>(), ESkillType.BodySlam);
        }
    }
}
