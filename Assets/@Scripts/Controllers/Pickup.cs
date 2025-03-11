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

    private void Awake()
    {

    }


    public override void Init() 
    {
        PickupSpriteRenderer = GetOrAddComponent<SpriteRenderer>(gameObject);

        //TODO SET sprite;
        SetPickupSprite();
        //TODO refresh Amount(just coin, doubled coin) 

        //TODO refresh collider
        //or just add collider (selected)
        PickupCollider = GetOrAddComponent<PolygonCollider2D>(gameObject);
    }

    public void Init(EPICKUP_TYPE epickup_type)
    {
        PickupType = epickup_type;
        Init();
    }

    public void SetPickupSprite()
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
                spriteName = "pickup_016_bomb";
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
}
