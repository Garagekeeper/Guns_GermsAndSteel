using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum EPlayerBottomState
    {
        Idle,
        MoveRight,
        MoveLeft,
        MoveUp,
        MoveDown,
        OnDamaged,
        OnDead
    }

    public enum EPlayerHeadState
    {
        Idle,
        Attack,
    }

    public enum EPlayerHeadDirState
    {
        None,
        Up,
        Down,
        Left,
        Right,
    }

}
