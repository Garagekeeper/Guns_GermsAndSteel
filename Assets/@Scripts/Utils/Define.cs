using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Define
{

    public const char MAP_TOOL_WALL = '0';
    public const char MAP_TOOL_NONE = '1';
    public const char MAP_TOOL_SEMI_WALL = '2';
    public const char MAP_TOOL_DOOR = '3';

    public enum ECellCollisionType
    {
        Wall,
        None,
        SemiWall,
    }
    public enum ECreatureType
    {
        None,
        Boss,
        MainCharacter,
        Monster,
    }

    public enum EBossType
    {
        None,
        Monstro,
        Mom,
        Fistula,
        DukeOfFlies,
        Gurdy,
        GurdyJr,
    }

    public enum EBossState
    {
        None,
        Dead,
        Explosion,
        Idle,
        Move,
        Skill,
    }

    public enum ECreatureState
    {
        None,
        Dead,
        Explosion,
        Idle,
        Move,
        Skill,
    }

    public enum ECreatureBottomState
    {
        None,
        Idle,
        Move,
        MoveRight,
        MoveLeft,
        MoveUp,
        MoveDown,
        OnDamaged,
        OnDead
    }

    public enum ECreatureHeadDirState
    {
        None,
        Up,
        Down,
        Left,
        Right,
    }

    public enum ECreatureHeadState
    {
        None,
        Idle,
        Attack,
        Skill,
        Explosion,
        GetItem,
    }

    public enum ECreatureMoveState
    {
        None,
        TargetCreature,
        Designated,
    }

    public enum ECreatureSize
    {
        Middle,
        Small,
        Large,
    }

    public enum ESkillType
    {
        BodySlam,
        Bomb,
        Fire,
        Projectile,
        Spike,
    }

    public enum EItemType
    {
        Null,
        ActiveItem,
        Cards,
        Pills,
        Passive,
        Familliar,
    }

    public enum EMonsterType
    {
        None,
        BoomFly,
        Fly,
        Maggot,
        Host,
        Boil,
        Pooter,
    }

    public enum ESpecialEffectOfActive
    {
        Null = 0,
        RandomTeleport,
        UncheckedRoomTeleport,
        Roll,

    }


    public enum EShotType
    {
        Null = 0,
        Guided,
        Boomerang,
    }

    public enum ETileType
    {
        Fire,

    }

    public enum ELayer
    {
        Player = 6,
        Monster,
        Projectile,
        Door,
        Boss,
        HitBox,
        ItemHolder,
        Obstacle,
    }

    public enum EPICKUP_TYPE
    { 
        PICKUP_NULL,
        PICKUP_HEART,
        PICKUP_COIN,
        PICKUP_BOMB,
        PICKUP_KEY,
        PICKUP_LIL_BATTERY,
        PICKUP_BATTERY,
        PICKUP_PILL,
        PICKUP_TAROT_CARD,
        PICKUP_RUNE,
        PICKUP_CHEST,
        PICKUP_GRAB_BAG,
        PICKUP_TRINKET,
    }

}
