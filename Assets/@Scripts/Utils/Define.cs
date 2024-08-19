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
        MainCharacter,
        Monster,
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
        ActiveItem,
        Cards,
        Pills,
        Passive
    }

    public enum EItemEfect
    {
        Down = -1,
        Roll,
        Up,
        Teleport,
    }

    public enum ETileType
    {
        Fire,

    }
}
