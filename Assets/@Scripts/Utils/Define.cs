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
    }

    public enum EBossState
    {
        None,
        Idle,
        Skill,
        Move,
        Dead,
        Explosion,
    }

    public enum ECreatureBottomState
    {
        Idle,
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
        Idle,
        Attack,
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
}
