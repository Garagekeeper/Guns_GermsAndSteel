using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum EPlayerState
    {
        Idle,
        MoveHorizen,
        MoveVertical,
        Attack,
        OnDamaged,
        OnDead
    }

    public enum EPlayerFacing
    {
        Down,
        Up,
        Left,
        Right,
    }

}
