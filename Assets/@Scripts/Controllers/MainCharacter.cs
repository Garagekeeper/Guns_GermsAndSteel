using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define;
public class MainCharacter : Creature
{
    #region Stat

    //Todo
    //float PercentageOfDevil;
    //float PercentageOfAngel;
    public int Coin { get; private set; } = 0;
    public int BombCount { get; private set; } = 1;
    public int KeyCount { get; private set; } = 0;
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
    public bool OneTimeActive { get; set; } = false;

    private bool _isInvincible = false;
    public bool IsInvincible
    {
        get { return _isInvincible; }
        set
        {
            _isInvincible = value;
            if (_isInvincible != value)
            {
                //왜인진 mask 부분이 동작하지 않는데
                //의도한 대로 움직이니까 일단? 놔둠
                _isInvincible = value;
                int mask = 0;
                mask |= 1 << (int)ELayer.Projectile;
                mask |= 1 << (int)ELayer.Boss;
                mask |= 1 << (int)ELayer.Obstacle;
                mask |= 1 << (int)ELayer.Monster;
                if (value)
                    Collider.excludeLayers = mask;
                else
                    Collider.includeLayers = mask;

            }
        }
    }
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
                    StartCoroutine(DelayBoolChange());
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
    private float _pressingTime = 0;

    public float PressingTime
    {
        get { return _pressingTime; }
        set
        {
            if (_pressingTime != value)
            {
                _pressingTime = value;
                Managers.UI.PlayingUI.SetFadeImageAlpha(_pressingTime);
            }
        }
    }

    System.Random random = new();
    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
#if UNITY_EDITOR
        AttackDamage = 50f;
#endif
        HeadSprite = new Sprite[]
       {
            Managers.Resource.Load<Sprite>("isaac_up"),
            Managers.Resource.Load<Sprite>("isaac_down"),
            Managers.Resource.Load<Sprite>("isaac_right"),
       };

        PressingTime = 0;

        SpaceItem = new Item();
        QItem = new Item();

        UseActiveItem -= HandleUsingActiveItem;
        UseActiveItem += HandleUsingActiveItem;
        CreatureType = ECreatureType.MainCharacter;
        CanMove = true;

        //충돌 레이어 설정
        LayerMask mask = 0;
        if (CreatureType == ECreatureType.MainCharacter) mask |= (1 << (int)ELayer.Player);
        Collider.excludeLayers = mask;

        AcquiredPassiveItemList = new List<Item>();
        AcquiredFamiliarItemList = new List<Familiar>();

        ChangeSpaceItem(SpaceItemId);
        ChangeQItem(QItemId);
        Managers.UI.PlayingUI.RefreshUI(this);

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
        {
            SpawnBomb();
            Managers.UI.PlayingUI.RefreshUI(this);
        }

        if (Input.GetKeyDown(KeyCode.Q))
            UseItem(QItem);

        if (Input.GetKeyDown(KeyCode.Space))
            UseItem(SpaceItem);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            SpaceItem.CurrentGage = Math.Min(SpaceItem.CurrentGage + 1, SpaceItem.CoolTime);
            Managers.Game.UseActiveItem(SpaceItem.CurrentGage, SpaceItem.CoolTime);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            Managers.Game.RoomClear();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            //Managers.UI.ShowUpStatUI();
        }

        //Restart with fade out
        if (Input.GetKey(KeyCode.R))
        {
            PressingTime += Time.deltaTime;
            if (PressingTime > 1)
            {
                PressingTime = 0;
                //Managers.Map.DestroyMap();
                //Managers.Map.LoadMap();
                Managers.Game.RestartGame();
            }
        }
        else
        {
            if (PressingTime > 0)
            {
                PressingTime = Mathf.Max(PressingTime - Time.deltaTime, 0);
            }

        }

        #endregion
    }
    private void UpdateMovement(Vector2 vel)
    {
        if (Rigidbody.velocity == vel) return; 
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
            if (item.CurrentGage == item.CoolTime)
            {
                item.CurrentGage = 0;
                UseActiveItem?.Invoke(item);
                Managers.Game.UseActiveItem(item.CurrentGage, item.CoolTime);
                //ApplyItemEffect(item);
            }
        }
        else if (item.ItemType == EItemType.Cards || item.ItemType == EItemType.Pills)
        {
            UseActiveItem?.Invoke(item);
            ChangeQItem(null);
            QItem = null;
        }
        Managers.UI.PlayingUI.RefreshUI(this);
    }

    public void GetItem(ItemHolder itemHolder)
    {
        bool active = false;
        Item item = itemHolder.ItemOfItemHolder;
        if (item.ItemType == EItemType.Familliar)
        {
            itemHolder.ChangeItemOnItemHolder(null);
            //GameObject go = Managers.Resource.Instantiate("Player");
            //AcquiredFamiliarItemList.Add(item);
        }
        else if (item.ItemType == EItemType.Passive)
        {
            itemHolder.ChangeItemOnItemHolder(null);
            AcquiredPassiveItemList.Add(item);
            ApplyPassiveItemEffect(item);
        }
        else if (item.ItemType == EItemType.ActiveItem)
        {
            active = true;
            itemHolder.ChangeItemOnItemHolder(SpaceItem);
            if (SpaceItem == null)
            {
                SpaceItem = item;
                SpaceItemId = item.TemplateId;
            }
            else
                ChangeSpaceItem(item);
        }
        Managers.UI.PlayingUI.RefreshUI(this);
        if (itemHolder.transform.GetChild(1) != null) itemHolder.transform.GetChild(1).gameObject.SetActive(active);
    }

    public void DeleteItem(Item item)
    {

    }

    public void ApplyPassiveItemEffect(Item item, bool isOneTime = false)
    {
        if (isOneTime)
        {
            OneTimeActive = true;
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
    //다른 액티브 아이템은 enum을통해서 별도로 효과를 적용시켜준다. 
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

    public override void OnDamaged(Creature owner, ESkillType skillType, string name = "")
    {
        if (IsInvincible) return;

        Hp -= DamageByOtherConstant;
        IsInvincible = true;
        StartCoroutine(CoInvincible());
        Managers.UI.PlayingUI.RefreshUI(this);
        //Debug.Log(Hp);
    }

    //Total 1sec
    public IEnumerator CoInvincible()
    {
        //Change Sprite
        for (int i = 1; i <= 10; i++)
        {
            if (i % 2 == 0)
            {
                Head.color = new Color32(255, 255, 255, 90);
                Bottom.color = new Color32(255, 255, 255, 90);
            }
            else
            {
                Head.color = new Color32(255, 255, 255, 180);
                Bottom.color = new Color32(255, 255, 255, 180);
            }
            yield return new WaitForSeconds(0.1f);
        }

        //Change Sprite
        yield return null;
        Head.color = new Color32(255, 255, 255, 255);
        Bottom.color = new Color32(255, 255, 255, 255);
        IsInvincible = false;
    }

    public void SpawnBomb()
    {
        if (BombCount <= 0) return;
        GameObject go = Managers.Resource.Instantiate("Bomb");

        Bomb bomb = go.GetComponent<Bomb>();
        bomb.SetInfo(this);
        BombCount--;
    }

    public void ChangeSpaceItem(int id)
    {
        ChangeSpaceItem(new Item(id));
    }

    public void ChangeSpaceItem(Item item)
    {
        SpaceItem = item;
        SpaceItemId = item.TemplateId;
        Managers.UI.PlayingUI.ChangeSpaceItem(SpaceItem.SpriteName);
        Managers.UI.PlayingUI.ChangeChargeBarSize("ui_chargebar_", SpaceItem.CoolTime);
        Managers.Game.UseActiveItem(item.CurrentGage, item.CoolTime);
    }

    public void ChangeQItem(int id)
    {
        ChangeQItem(new Item(id));
    }

    public void ChangeQItem(Item item)
    {
        string spriteName = null;
        if (item != null)
        {
            spriteName = item.SpriteName;
            if (item.ItemType == EItemType.Pills)
            {
                Managers.UI.PlayingUI.ChangeQItem(item.SpriteName + (random.Next() % 13));
                return;
            }
        }
        QItem = item;
        Managers.UI.PlayingUI.ChangeQItem(spriteName);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.transform.CompareTag("Door") && Managers.Map.CurrentRoom.IsClear)
        {
            CanMove = false;
            Managers.Game.GoToNextRoom(collision.transform.name);
        }

        if (collision.transform.CompareTag("TrapDoor") && Managers.Map.CurrentRoom.IsClear)
        {
            Managers.Game.GoToNextStage();
        }

        if (collision.transform.CompareTag("Monster") || collision.transform.CompareTag("Boss"))
        {
            OnDamaged(collision.gameObject.GetComponent<Creature>(), ESkillType.BodySlam);
        }

        if (collision.transform.CompareTag("ItemHolder"))
        {
            ItemHolder itemHolder = collision.transform.GetComponent<ItemHolder>();
            if (itemHolder.ItemOfItemHolder != null)
            {
                GetItem(itemHolder);
            }
        }

        if (collision.transform.CompareTag("ClearBox"))
        {
            Managers.Game.ClearGame();
        }

    }
}
