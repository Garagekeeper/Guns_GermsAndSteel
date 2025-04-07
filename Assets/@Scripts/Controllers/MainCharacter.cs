using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using static Utility;
public class MainCharacter : Creature
{
    #region Stat

    //Todo
    //float PercentageOfDevil;
    //float PercentageOfAngel;
    public int Coin { get; private set; } = 0;
    public int BombCount { get; private set; } = 1;
    public int KeyCount { get; private set; } = 0;

    //public float EmptyHearts { get; private set; } = 0;
    //TODO
    //var SubItem;
    //var ActiveItem;
    public List<Item> AcquiredPassiveItemList { get; private set; }
    List<Familiar> AcquiredFamiliarItemList;



    public event Action<Item> UseActiveItem;
    public int SpaceItemId { get; set; } = 45044;
    public int QItemId { get; set; } = 44002;

    public Item SpaceItem { get; set; }
    public Item QItem { get; set; }

    private bool _canMove = true;
    private bool _canAttack = true;
    public bool OneTimeActive { get; set; } = false;

    private bool _isInvincible = false;

    private bool _isPause = false;

    private bool _canGetItem = true;

    public bool IsPause
    {
        get { return _isPause; }
        set
        {
            if (value == true)
            {
                Time.timeScale = 0;
                Managers.UI.PauseUI.gameObject.SetActive(true);
            }
            else
            {
                Managers.UI.PauseUI.gameObject.SetActive(false);
                Time.timeScale = 1;
            }
            _isPause = value;
        }
    }

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

            if (value == true)
            {
                //StartCoroutine(DelayBoolChange());
                _canMove = true;
            }
            else
            {
                _canMove = value;
                BottomState = ECreatureBottomState.Idle;
                HeadState = ECreatureHeadState.Idle;
            }


        }
    }

    public bool CanAttack
    {
        get { return _canAttack; }
        set
        {
            if (value == true)
            {
                _canAttack = true;
            }
            else
            {
                _canAttack = value;
                HeadDirState = ECreatureHeadDirState.Down;
                HeadState = ECreatureHeadState.Idle;
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
        StopAllCoroutines();

        base.Init();
#if UNITY_EDITOR
        AttackDamage = 3f;
#endif
        HeadSprite = new Sprite[]
       {
            Managers.Resource.Load<Sprite>("isaac_up"),
            Managers.Resource.Load<Sprite>("isaac_down"),
            Managers.Resource.Load<Sprite>("isaac_right"),
       };

        PressingTime = 0;

        MaxHp = 16f;

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


        IsPause = false;
        Managers.UI.ResfreshUIAll(this);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            IsPause = !IsPause;
        }

        if (IsPause) return;

        #region Attack
        Vector2 attackVel = Vector2.zero;
        if (CanAttack && HeadState != ECreatureHeadState.GetItem)
        {
            attackVel.x = Input.GetAxis("AttackHorizontal");
            attackVel.y = Input.GetAxis("AttackVertical");

            if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
                attackVel.x *= -1;
            if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.DownArrow))
                attackVel.y *= -1;
        }
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
            Managers.UI.ResfreshUIAll(this);
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

            OnDead();
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
            if (HeadState == ECreatureHeadState.GetItem)
            {
                HeadDirState = ECreatureHeadDirState.Down;
            }
            else
            {
                HeadState = ECreatureHeadState.Idle;
                if (BottomState == ECreatureBottomState.Idle)
                    HeadDirState = ECreatureHeadDirState.Down;
            }

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
        Managers.UI.ResfreshUIAll(this);
    }

    public void GetItem(ItemHolder itemHolder)
    {
        if (itemHolder == null) return;

        bool active = false;
        Item itemFromItemHolder = itemHolder.ItemOfItemHolder;
        Item itemFromItemPlayer = null;
        
        //0. ItemHolder의 아이템변경
        if (itemFromItemHolder.ItemType == EItemType.ActiveItem)
        {
            //0-0) active item의 경우 UI끄기
            itemFromItemPlayer = SpaceItem;
            Managers.UI.PlayingUI.SpaceItemAndChargeBarActive(false);
        }
        else if (itemFromItemHolder.ItemType == EItemType.Pills || itemFromItemHolder.ItemType == EItemType.Cards)
        {
            itemFromItemPlayer = QItem;
        }

        itemHolder.ChangeItemOnItemHolder(itemFromItemPlayer);
        Managers.UI.PlayingUI.ChangeItemDescription(itemFromItemHolder);

        GameObject ItemImage = FindChildByName(transform, "Item").gameObject;
        ItemImage.GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>(itemFromItemHolder.SpriteName);

        //1. player의 state가 GetItem으로 변함
        HeadState = ECreatureHeadState.GetItem;
        //2. 코루틴을 호출, 코루틴이 완료될 때 아이템이 플레이어에게 들어온다.
        StartCoroutine(DelayedGet(() => {
            // 2-0) active item의 경우 들고있는 아이템과 item holder의 아이템을 swap
            if (itemFromItemHolder.ItemType == EItemType.ActiveItem)
            {
                active = true;
                if (SpaceItem == null)
                {
                    SpaceItem = itemFromItemHolder;
                    SpaceItemId = itemFromItemHolder.TemplateId;
                }
                else
                {
                    ChangeSpaceItem(itemFromItemHolder);
                }

                // 2-1) 꺼진 UI 켜주기
                Managers.UI.PlayingUI.SpaceItemAndChargeBarActive(true);
            }

            if (itemFromItemHolder.ItemType == EItemType.Passive)
                AcquiredPassiveItemList.Add(itemFromItemHolder);

            if (itemFromItemHolder.ItemType == EItemType.Familliar)
            {
                //AcquiredFamiliarItemList.Add(item);
            }

            ApplyPassiveItemEffect(itemFromItemHolder);
            //그림자 끄끼
            if (itemHolder.transform.GetChild(1) != null) itemHolder.transform.GetChild(1).gameObject.SetActive(active);

            //적용된 능력치 UI 갱신
            Managers.UI.ResfreshUIAll(this);

            //기본 상태로 변환
            HeadState = ECreatureHeadState.Idle;
            HeadDirState = ECreatureHeadDirState.Down;
            BottomState = ECreatureBottomState.None;
            BottomState = ECreatureBottomState.Idle;
        }));

    }

    IEnumerator DelayedGet(Action callback)
    {
        GameObject ItemImage = FindChildByName(transform, "Item").gameObject;
        Managers.UI.PlayingUI.ItemDescriptionActive(true);
        ItemImage.SetActive(true);
        _canGetItem = false;
        yield return new WaitForSeconds(2f);
        ItemImage.SetActive(false);
        Managers.UI.PlayingUI.ItemDescriptionActive(false);
        callback.Invoke();
        yield return new WaitForSeconds(1.5f);
        _canGetItem = true;
        
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
        Managers.UI.ResfreshUIAll(this);
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

    //TODO
    //Pickup으로 바꾸기
    public void ChangeQItem(int id)
    {
        ChangeQItem(new Item(id));
    }

    //TODO
    //Pickup으로 바꾸기
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

    public void GetPickup(Pickup pickup)
    {
        EPICKUP_TYPE pickupType = pickup.PickupType;
        switch (pickupType)
        {
            case EPICKUP_TYPE.PICKUP_HEART:
                Hp += 2;
                break;
            case EPICKUP_TYPE.PICKUP_COIN:
                Coin += 1;
                break;
            case EPICKUP_TYPE.PICKUP_BOMB:
                BombCount += 1;
                break;
            case EPICKUP_TYPE.PICKUP_KEY:
                KeyCount += 1;
                break;
            case EPICKUP_TYPE.PICKUP_LIL_BATTERY:
                break;
            case EPICKUP_TYPE.PICKUP_BATTERY:
                break;
            case EPICKUP_TYPE.PICKUP_PILL:
                //TODO
                //ChangeQItem();
                break;
            case EPICKUP_TYPE.PICKUP_TAROT_CARD:
                //TODO
                //ChangeQItem();
                break;
            case EPICKUP_TYPE.PICKUP_RUNE:
                //TODO
                //ChangeQItem();
                break;
            case EPICKUP_TYPE.PICKUP_CHEST:
                Managers.Game.SpawnChestAndGrabBagAward(pickup);
                pickup.SetPickupSprite("pickup_005_chests_6");
                Destroy(pickup);
                return;
            case EPICKUP_TYPE.PICKUP_GRAB_BAG:
                Managers.Game.SpawnChestAndGrabBagAward(pickup);
                break;
            case EPICKUP_TYPE.PICKUP_TRINKET:
                //Managers.Game.Trinket();
                break;
            default:
                break;
        }
        Managers.UI.ResfreshUIAll(this);
        Managers.Object.Despawn(pickup);
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

        if (collision.transform.CompareTag("ItemHolder") && _canGetItem)
        {
            ItemHolder itemHolder = collision.transform.GetComponent<ItemHolder>();
            if (itemHolder.ItemOfItemHolder != null)
            {
                GetItem(itemHolder);
            }
        }

        if (collision.transform.CompareTag("Pickup"))
        {
            Pickup pickup = collision.transform.GetComponent<Pickup>();

            if (pickup == null) return;

            GetPickup(pickup);
        }

        if (collision.transform.CompareTag("ClearBox"))
        {
            Managers.Game.ClearGame();
        }
    }

    public override void OnDead()
    {
        if (Managers.Object.MainCharacters.Count > 1)
            base.OnDead();

        else
            Managers.Game.GameOver();
    }

}
