using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Define
{
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

    public enum ECreatureHeadState
    {
        Idle,
        Attack,
    }

    public enum ECreatureHeadDirState
    {
        None,
        Up,
        Down,
        Left,
        Right,
    }

}
