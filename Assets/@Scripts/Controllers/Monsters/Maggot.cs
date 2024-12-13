using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static Define;

public class Maggot : Monster
{
    float _accumulatedTime = 0.0f;
    string _animationName = "";

    enum EMaggotType
    {
        Maggot,
        Charger,
    }

    EMaggotType MaggotType { get; set; }

    private void Awake()
    {
        Init();
        StartCoroutine(CoUpdateAI());
    }

    public override void Init()
    {
        MonsterType = EMonsterType.Maggot;
        MonsterState = EMonsterState.Idle;
        CreatureMoveState = ECreatureMoveState.Designated;
        Hp = 5.0f;
        _isFloating = false;
        base.Init();

        //if (Random.Range(1, 101) <= 50)
        //{
        //    MaggotType = EMaggotType.Maggot;
        //    Speed = 0.5f;
        //    AnimatorBottom.Play("MaggotlIdle");
        //}
        //else
        //{
        //    MaggotType = EMaggotType.Charger;
        //    AnimatorBottom.Play("ChargerIdle");
        //}

        MaggotType = EMaggotType.Maggot;
        Speed = 1f;
    }

    protected override void UpdateIdle()
    {
        Rigidbody.velocity = Vector3.zero;
        switch (MaggotType)
        {
            case EMaggotType.Maggot:
                UpdateMaggotIdle();
                break;
            case EMaggotType.Charger:
                UpdateChargerIdle();
                break;
        }
    }

    public void UpdateMaggotIdle()
    {
        if (Random.Range(1, 101) <= 50)
            return;

        Vector2[] dV = { Vector2.right, Vector2.down, Vector2.left, Vector2.up };
        int index = Random.Range(0, 4);
        switch (index)
        {
            case 0:
                Bottom.flipX = false;
                Bottom.sprite = Managers.Resource.Load<Sprite>("monster_105_maggot_0");
                _animationName = "MaggotRight";
                break;
            case 1:
                Bottom.sprite = Managers.Resource.Load<Sprite>("monster_105_maggot_8");
                _animationName = "MaggotDown";
                break;
            case 2:
                Bottom.flipX = true;
                Bottom.sprite = Managers.Resource.Load<Sprite>("monster_105_maggot_0");
                _animationName = "MaggotRight";
                break;
            case 3:
                Bottom.sprite = Managers.Resource.Load<Sprite>("monster_105_maggot_4");
                _animationName = "MaggotUp";
                break;
            default:
                break;

        }

        TargetPos = dV[index];
        MonsterState = EMonsterState.Move;
    }

    public void UpdateChargerIdle()
    {

    }

    protected override void UpdateMove()
    {
        _accumulatedTime += Time.deltaTime;
        if (_accumulatedTime > 1)
        {
            Rigidbody.velocity = Vector3.zero;
            MonsterState = EMonsterState.Idle;
            _accumulatedTime = 0f;
            return;
        }

        if (_coWait != null) return;

        AnimatorBottom.Play(_animationName, 0, 0);
        if (_animationName != AnimatorBottom.GetCurrentAnimatorClipInfo(0)[0].clip.name)
            return;
        var delay = AnimatorBottom.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        StartWait(delay);




        base.UpdateMove();
    }

}
