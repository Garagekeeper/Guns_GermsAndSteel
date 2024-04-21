using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
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
