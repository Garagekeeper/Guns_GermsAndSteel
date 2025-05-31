using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Define;
using static Utility;

public class Pickup : BaseObject 
{
    public EPICKUP_TYPE PickupType 
    {  
        get; 
        set; 
    }

    public int Amount { get; private set; } = 1;
    public SpriteRenderer PickupSpriteRenderer { get; private set; }
    public PolygonCollider2D PickupCollider { get; private set; }

    public Rigidbody2D Rigidbody { get; private set; }

    private Vector3 _dirVec  = Vector3.zero;

    bool _playSpawnSound;

    private void Awake()
    {

    }

    private void FixedUpdate()
    {
        
    }

    public override void Init() 
    {
        PickupSpriteRenderer = GetOrAddComponent<SpriteRenderer>(FindChildByName(transform, "Pickup_Sprite").gameObject);
        Rigidbody = GetOrAddComponent<Rigidbody2D>(gameObject);

        PickupCollider = GetOrAddComponent<PolygonCollider2D>(gameObject);
        InitPickupSprite();
        
        // polygoncollide 형태 재설정 
        var pointsList = new List<Vector2>();
        PickupCollider.pathCount = 0; // 기존 경로 제거
        PickupSpriteRenderer.sprite.GetPhysicsShape(0, pointsList);
        PickupCollider.points = pointsList.ToArray();
        PickupCollider.enabled = false;

        // 마찰력 설정
        Rigidbody.drag = 2;
        if (PickupType == EPICKUP_TYPE.PICKUP_CHEST)
            Rigidbody.drag = 100;

        // 튀어나오는 효과를 가진 픽업들을 위한 방향 벡터
        Rigidbody.velocity = _dirVec;

        GetComponent<Animation>().Play("Pickup_Spawn");

        // 소환 소리 재생
        AudioClip audioClip = null;
        if (PickupType == EPICKUP_TYPE.PICKUP_COIN)
        {
            audioClip = Managers.Resource.Load<AudioClip>("penny drop 1");
        }
        else if (PickupType == EPICKUP_TYPE.PICKUP_KEY)
        {
            audioClip = Managers.Resource.Load<AudioClip>("key drop 2");
        }
        else if (PickupType == EPICKUP_TYPE.PICKUP_CHEST)
        {
            audioClip = Managers.Resource.Load<AudioClip>("chest drop 1");
        }

        if (audioClip != null && _playSpawnSound)
            Managers.Sound.PlaySFX(audioClip, 0.3f);
    }

    public void Init(EPICKUP_TYPE epickup_type, Vector3 dir = default, bool spawnSoundPlay = false)
    {
        PickupType = epickup_type;
        _dirVec = dir;
        _playSpawnSound = spawnSoundPlay;
        Init();
    }

    public void InitPickupSprite()
    {
        string spriteName;
        switch (PickupType)
        {
            case EPICKUP_TYPE.PICKUP_HEART:
                spriteName = "pickup_001_heart_0";
                break;
            case EPICKUP_TYPE.PICKUP_COIN:
                spriteName = "pickup_002_coin_0";
                break;
            case EPICKUP_TYPE.PICKUP_BOMB:
                spriteName = "pickup_016_bomb_0";
                break;
            case EPICKUP_TYPE.PICKUP_KEY:
                spriteName = "pickup_003_key_0";
                break;
            case EPICKUP_TYPE.PICKUP_LIL_BATTERY:
                spriteName = "pickup_018_littlebattery_0";
                break;
            case EPICKUP_TYPE.PICKUP_BATTERY:
                spriteName = "pickup_018_littlebattery_1";
                break;
            case EPICKUP_TYPE.PICKUP_PILL:
                spriteName = "pickup_007_pill_0";
                break;
            case EPICKUP_TYPE.PICKUP_TAROT_CARD:
                spriteName = "pickup_0017_heart_0";
                break;
            case EPICKUP_TYPE.PICKUP_RUNE:
                spriteName = "pickup_007_pill_12";
                break;
            case EPICKUP_TYPE.PICKUP_CHEST:
                spriteName = "pickup_005_chests_5";
                break;
            case EPICKUP_TYPE.PICKUP_GRAB_BAG:
                spriteName = "grabbag";
                break;
            case EPICKUP_TYPE.PICKUP_TRINKET:
                //TODO Trinket
                spriteName = "trophy";
                break;
            default:
                spriteName = "None";
                break;
        }

        PickupSpriteRenderer.sprite = Managers.Resource.Load<Sprite>(spriteName);

    }

    public void SetPickupCount()
    {

    }

    public void RefreshCollider()
    {

    }

    public void SetPickupSprite(string spriteName)
    {
        PickupSpriteRenderer.sprite = Managers.Resource.Load<Sprite>(spriteName);
    }

    public void SetCollider(int value)
    {
        if (PickupType == EPICKUP_TYPE.PICKUP_CHEST && value == 0) return;
        PickupCollider.enabled = value == 1 ? true : false;
    }

    public void DestroyPickup()
    {
        // 획득시 소리 재생
        AudioClip audioClip = null;
        if (PickupType == EPICKUP_TYPE.PICKUP_COIN)
        {
            audioClip = Managers.Resource.Load<AudioClip>("pickup_penny_02");
        }
        else if (PickupType == EPICKUP_TYPE.PICKUP_KEY)
        {
            audioClip = Managers.Resource.Load<AudioClip>("key pickup guantlet 4");
        }
        else if (PickupType == EPICKUP_TYPE.PICKUP_CHEST)
        {
            audioClip = Managers.Resource.Load<AudioClip>("chest open 1");
        }
        else if (PickupType == EPICKUP_TYPE.PICKUP_BOMB)
        {
            audioClip = Managers.Resource.Load<AudioClip>("fetus feet");
        }
        else if (PickupType == EPICKUP_TYPE.PICKUP_HEART)
        {
            audioClip = Managers.Resource.Load<AudioClip>("boss 2 bubbles");
        }

        if (audioClip != null)
        {
            Managers.Sound.PlaySFX(audioClip, 0.3f);
            PickupCollider.enabled = false;

            // 상자의 경우 오브젝트가 남아있어야함
            if (PickupType != EPICKUP_TYPE.PICKUP_CHEST)
            {
                PickupSpriteRenderer.enabled = false;
                Destroy(gameObject, audioClip.length);
            }
        }
        else
        {
            Destroy(gameObject);
        }

        
    }

}
