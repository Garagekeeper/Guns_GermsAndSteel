using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static Define;
using static UnityEngine.Rendering.DebugUI;
using static Utility;
public class MainCharacter : Creature
{
    #region Stat

    //Todo
    //float PercentageOfDevil;
    //float PercentageOfAngel;
    public int Coin { get; private set; } = 15;
    public int BombCount { get; private set; } = 10;
    public int KeyCount { get; private set; } = 1;

    //public float EmptyHearts { get; private set; } = 0;
    //TODO
    //var SubItem;
    //var ActiveItem;
    public List<Item> AcquiredPassiveItemList { get; private set; }
    List<Familiar> AcquiredFamiliarItemList;

    // Damage

    // 캐릭터 기본 데미지
    private float _charBaseDmg = 5.5f;
    // 아이템으로 증가된 데미지의 총합
    private float _totalDmgups = 0f;
    //그냥 깡으로 올라가는 데미지
    private float _flatDmgUps = 0f;
    //최종으로 곱해지는 배율
    private float _multiplier = 1f;

    private float _tearDelay = 2.75f;
    //private float _tearDelay = 0.7f;
    private float _tearMax = 5f;

    public float CharBaseDmg
    {
        get { return _charBaseDmg; }
        set
        {
            if (value != _charBaseDmg)
            {
                _charBaseDmg = value;
            }
        }

    }
    public float TotalDmgUp
    {
        get { return _totalDmgups; }
        set
        {
            if (value != _totalDmgups)
            {
                _totalDmgups = value;
            }
        }
    }
    public float FlatDmgUp
    {
        get { return _flatDmgUps; }
        set
        {
            _flatDmgUps = value;
        }
    }
    public float Multiplier
    {
        get { return _multiplier; }
        set
        {
            if (value != _multiplier)
            {
                if (value == 0) return;
                _multiplier = value;
            }
        }
    }


    public override float Tears
    {
        get => _tears;
        set
        {
            if (_tears != value)
            {
                _tears = value;
                if (value > _tearMax)
                    _tears = _tearMax;
                TearDelay = _tears;
            }
        }
    }


    //value = Tears
    public float TearDelay
    {
        get { return _tearDelay; }
        set
        {
            if (value >= _tearMax) _tearDelay = 0;
            else if (value >= 0 && value < _tearMax) _tearDelay = (16 - 6 * MathF.Sqrt(value * 1.3f + 1));
            else if (value < 0 && value > -0.77f) _tearDelay = (16 - 6 * MathF.Sqrt(value * 1.3f + 1));
            else if (value <= -0.77f) _tearDelay = 16 - 6 * Tears;
            OnTearsChange();
        }
    }

    public int SpaceItemId { get; set; } = 45105;
    //public int QItemId { get; set; } = 44002;

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
            if (SceneManager.GetActiveScene().name == "DevScene") return;

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

            if (_isInvincible != value)
            {
                ////왜인진 mask 부분이 동작하지 않는데
                ////의도한 대로 움직이니까 일단? 놔둠
                //_isInvincible = value;
                //int mask = 0;
                //mask |= 1 << (int)ELayer.Projectile;
                //mask |= 1 << (int)ELayer.Boss;
                //mask |= 1 << (int)ELayer.Obstacle;
                //mask |= 1 << (int)ELayer.Monster;
                //if (value)
                //    Collider.excludeLayers = mask;
                //else
                //    Collider.includeLayers = mask;

                Physics2D.IgnoreLayerCollision((int)ELayer.Player, (int)ELayer.Projectile, value);
                Physics2D.IgnoreLayerCollision((int)ELayer.Player, (int)ELayer.Boss, value);
                Physics2D.IgnoreLayerCollision((int)ELayer.Player, (int)ELayer.Obstacle, value);
                Physics2D.IgnoreLayerCollision((int)ELayer.Player, (int)ELayer.Monster, value);
                _isInvincible = value;

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

    // 게임오브젝트가 비활성화 될 때 애니메이터의 파라미터가 초기화되기때문에
    // 별도로 저장
    private float _animSpeedSave = 1;


    public float DamageByOtherConstant { get; set; } = 0.5f;

    public void AddCoin(int amount)
    {
        Coin += amount;
        Managers.UI.ResfreshUIAll(this);
    }

    public void AddKey(int amount)
    {
        KeyCount += amount;
        Managers.UI.ResfreshUIAll(this);
    }

    public void AddBomb(int amount)
    {
        BombCount += amount;
        Managers.UI.ResfreshUIAll(this);
    }

    public void CalcAttackDamage()
    {
        AttackDamage = (float)Math.Round((CharBaseDmg * Mathf.Sqrt(TotalDmgUp * 1.2f + 1) + FlatDmgUp) * Multiplier, 2);
    }

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
        CalcAttackDamage();
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

        CreatureType = ECreatureType.MainCharacter;
        CanMove = true;

        //충돌 레이어 설정
        LayerMask mask = 0;
        if (CreatureType == ECreatureType.MainCharacter) mask |= (1 << (int)ELayer.Player);
        Collider.excludeLayers = mask;

        AcquiredPassiveItemList = new List<Item>();
        AcquiredFamiliarItemList = new List<Familiar>();

        ChangeSpaceItem(SpaceItemId);
        //ChangeQItem(QItemId);


        IsPause = false;
        if (SceneManager.GetActiveScene().name == "DevScene")
            return;

        Managers.UI.ResfreshUIAll(this);

    }

    void Update()
    {

        if (CreatureState == ECreatureState.Dead) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            IsPause = !IsPause;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            //Managers.UI.ShowUpStatUI();

            OnDead();
            return;
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
            Managers.Game.ChangeItemGage(SpaceItem, 1);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            Managers.Game.RoomClear();
        }

        if (Input.GetKeyDown (KeyCode.C))
        {
            Managers.Game.GoToNextStage();
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

    protected override void OnTearsChange()
    {
        // 2.73 <- 기준
        float animSpeed = (12f / (TearDelay + 1f)) / 4f;
        AnimatorHead.SetFloat("AnimSpeed", animSpeed);
        _animSpeedSave = animSpeed;
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
        if (item == null || item.CurrentGage == 0)
            return;

        if (item.ItemType == EItemType.Passive) return;

        if (item.ItemType == EItemType.ActiveItem)
        {
            if (item.CurrentGage != item.CoolTime) return;

            Managers.Game.ChangeItemGage(item, item.CoolTime * -1);

            //Managers.Game.UseActiveItem(item.CurrentGage, item.CoolTime);
            //ApplyItemEffect(item);

        }
        else if (item.ItemType == EItemType.Cards || item.ItemType == EItemType.Pills)
        {
            ChangeQItem(null);
            QItem = null;
        }
        HandleUsingActiveItem(item);
        Managers.UI.ResfreshUIAll(this);
    }

    public void GetItem(ItemHolder itemHolder)
    {
        if (itemHolder == null) return;

        var priceGo = FindChildByName(itemHolder.transform, "ShopItemPrice");
        if (priceGo.gameObject.activeSelf == true && Managers.Map.CurrentRoom.RoomType == ERoomType.Shop)
        {
            var price = Int32.Parse(priceGo.GetComponent<TextMeshPro>().text);
            if (price > 0)
            {
                if (price > Coin) return;

                Coin -= price;
                priceGo.GetComponent<TextMeshPro>().text = "0";
                priceGo.gameObject.SetActive(false);
            }
        }

        bool active = false;
        Item itemFromItemHolder = itemHolder.ItemOfItemHolder;
        Item itemFromItemPlayer = null;

        AudioClip audioClip = Managers.Resource.Load<AudioClip>("power up 7");
        Managers.Sound.PlaySFX(audioClip, 0.3f);

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
        StartCoroutine(DelayedGet(() =>
        {
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

            GameObject ItemImage = FindChildByName(transform, "Item").gameObject;
            if (SpaceItem != null)
                ItemImage.GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>(SpaceItem.SpriteName);

            //기본 상태로 변환
            HeadState = ECreatureHeadState.Idle;
            HeadDirState = ECreatureHeadDirState.Down;
            BottomState = ECreatureBottomState.None;
            BottomState = ECreatureBottomState.Idle;
            transform.GetComponent<Animator>().enabled = true;
        }));

    }

    IEnumerator DelayedGet(Action callback)
    {
        GameObject ItemImage = FindChildByName(transform, "Item").gameObject;
        Managers.UI.PlayingUI.ItemDescriptionActive(true);
        transform.GetComponent<Animator>().enabled = false;
        ItemImage.SetActive(true);
        _canGetItem = false;
        yield return new WaitForSeconds(2f);
        ItemImage.SetActive(false);
        Managers.UI.PlayingUI.ItemDescriptionActive(false);
        callback.Invoke();
        _canGetItem = true;

    }

    public void DeleteItem(Item item)
    {

    }

    // 패시브 아이템 적용
    public void ApplyPassiveItemEffect(Item item, bool isOneTime = false)
    {
        // 1회성 아이템인지 확인
        if (isOneTime)
        {
            OneTimeActive = true;
        }

        // 기본 스택 적용
        Hp += item.Hp;
        TotalDmgUp += item.DmgUp;
        FlatDmgUp += item.FlatDmgUp;
        Multiplier = item.Multiplier;
        Tears += item.Tears;
        Range += item.Range;
        ShotSpeed += item.ShotSpeed;
        Speed += item.Speed;
        Luck += item.Luck;
        Life += item.Life;

        // 픽업 아이템인 경우
        if (item.PickupType == EPICKUP_TYPE.PICKUP_COIN)
        {
            Coin += item.PickupCount;
        }
        else if (item.PickupType == EPICKUP_TYPE.PICKUP_KEY)
        {
            KeyCount += item.PickupCount;
        }
        else if (item.PickupType == EPICKUP_TYPE.PICKUP_BOMB)
        {
            BombCount += item.PickupCount;
        }

        //데미지 계산
        CalcAttackDamage();
        //적용된 능력치 UI 갱신
        Managers.UI.ResfreshUIAll(this);

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
            transform.GetComponent<Animator>().Play("UseItem", 0, 0);
            switch (item.EffectOfActive)
            {
                case ESpecialEffectOfActive.RandomTeleport:
                    Managers.Game.TPToNormalRandom();
                    break;
                case ESpecialEffectOfActive.UncheckedRoomTeleport:
                    break;
                case ESpecialEffectOfActive.Roll:
                    Managers.Game.Roll(this, "item");
                    break;
            }
        }
    }

    public override void OnDamaged(Creature owner, ESkillType skillType, string name = "")
    {
        if (IsInvincible) return;

//에디터에서는 무적
#if !UNITY_EDITOR
        Hp -= DamageByOtherConstant;
#endif
        IsInvincible = true;
        StartCoroutine(CoInvincible());
        Managers.UI.ResfreshUIAll(this);
        AudioClip audioClip = Managers.Resource.Load<AudioClip>($"hurt grunt {UnityEngine.Random.Range(0, 3)}");
        Managers.Sound.PlaySFX(audioClip, 0.2f);
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
        Managers.Game.ChangeItemGage(SpaceItem, SpaceItem.CoolTime);

        GameObject ItemImage = FindChildByName(transform, "Item").gameObject;
        var spritename = Managers.Data.ItemDic[SpaceItemId].SpriteName;
        ItemImage.GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>(spritename);
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

        var priceGo = FindChildByName(pickup.transform, "ShopItemPrice");
        if (priceGo.gameObject.activeSelf == true && Managers.Map.CurrentRoom.RoomType == ERoomType.Shop)
        {
            var price = Int32.Parse(priceGo.GetComponent<TextMeshPro>().text);
            if (price == 0) throw new Exception($"price err while getting shop item");

            if (price > Coin) return;

            Coin -= price;
        }

        EPICKUP_TYPE pickupType = pickup.PickupType;
        switch (pickupType)
        {
            case EPICKUP_TYPE.PICKUP_HEART:
                Hp += 1;
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
                pickup.DestroyPickup();
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

        if (collision.transform.CompareTag("Door"))
        {
            int index = 0;
            switch (collision.transform.name)
            {
                case "Right":
                    index = 0;
                    break;
                case "Down":
                    index = 1;
                    break;
                case "Left":
                    index = 2;
                    break;
                case "Up":
                    index = 3;
                    break;
            }

            if (collision.transform.parent.GetComponent<Door>().eDoorState[index] == EDoorState.Broken || collision.transform.parent.GetComponent<Door>().eDoorState[index] == EDoorState.BrokenOpen)
            {
                CanMove = false;

                Managers.Game.GoToNextRoom(collision.transform.name);
                return;

            }

            if (collision.transform.parent.GetComponent<Door>().eDoorState[index] == EDoorState.KeyClosed || collision.transform.parent.GetComponent<Door>().eDoorState[index] == EDoorState.CoinClosed)
            {
                collision.transform.parent.GetComponent<Door>().Open(index, false, this);
                return;
            }

            if (Managers.Map.CurrentRoom.IsClear)
            {
                CanMove = false;
                Managers.Game.GoToNextRoom(collision.transform.name);
            }

        }

        if (collision.transform.CompareTag("HoleInWall"))
        {
            int index = 0;
            switch (collision.transform.name)
            {
                case "Right":
                    index = 0;
                    break;
                case "Down":
                    index = 1;
                    break;
                case "Left":
                    index = 2;
                    break;
                case "Up":
                    index = 3;
                    break;
            }


            if (collision.transform.parent.GetComponent<Door>().eDoorState[index] == EDoorState.Opened)
            {
                CanMove = false;

                Managers.Game.GoToNextRoom(collision.transform.name);
                return;

            }
        }

        if (collision.transform.CompareTag("SpikeDoor") && Managers.Map.CurrentRoom.IsClear)
        {
            CanMove = false;
            OnDamaged(this, ESkillType.Spike);
            Managers.Game.GoToNextRoom(collision.transform.name);
        }

        if (collision.transform.CompareTag("TrapDoor") && Managers.Map.CurrentRoom.IsClear)
        {
            Managers.Sound.PlaySFX(Managers.Resource.Load<AudioClip>("boss fight intro jingle_01"), 0.3f);
            Managers.Game.GameScene.StopPlayingBG();
            Managers.Game.GoToNextStage();
            Managers.UI.PlayingUI.activeStatgeLoading(() => { // stage bgm
                Managers.Game.GameScene.PlayStageBGM();
            });
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
        {
            CreatureState = ECreatureState.Dead;
            Managers.Game.GameOver();
        }

    }

    private void OnEnable()
    {
        AnimatorHead.SetFloat("AnimSpeed", _animSpeedSave);
    }

}
