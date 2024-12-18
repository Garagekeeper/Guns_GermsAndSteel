using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static Define;

public class Maggot : Monster
{
    float _accumulatedTime = 0.0f;
    float _accumulatedTimeSkill = 0.0f;
    string _animationName = "";
    private RaycastHit2D hit;
    private int layerMask;

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
        //Player
        layerMask = 1 << 6;
    }

    private void Update()
    {
        
        if (hit = Physics2D.Raycast(transform.position, TargetPos, 21f, layerMask))
        {
            Debug.DrawRay(transform.position, TargetPos * hit.distance, Color.red);
            _accumulatedTimeSkill += Time.deltaTime;
            if (_accumulatedTime > 0.3)
            {
                AnimatorBottom.enabled = false;
                MonsterState = EMonsterState.Skill;
                _accumulatedTime = 0;
            }

        }
        else
        {
            Debug.DrawRay(transform.position, TargetPos * 21f, Color.red);
        }
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

        MaggotType = EMaggotType.Charger;
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

    protected override void UpdateSkill()
    {
        switch (MaggotType)
        {
            case EMaggotType.Charger:
                UpdateChargerSkill();
                break;
        }
    }

    public void UpdateMaggotIdle()
    {
        //0. 50프로의 확률로 멈추거나 이동함
        if (Random.Range(1, 101) <= 50)
            return;

        //1. 4가지 방향중 랜덤으로 선택
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
        //1. 50프로의 확률로 멈추거나 이동함
        if (Random.Range(1, 101) <= 50)
            return;

        //2. 4가지 방향중 랜덤으로 선택
        Vector2[] dV = { Vector2.right, Vector2.down, Vector2.left, Vector2.up };
        int index = Random.Range(0, 4);
        switch (index)
        {
            case 0:
                Bottom.flipX = false;
                Bottom.sprite = Managers.Resource.Load<Sprite>("monster_113_charger_0");
                _animationName = "ChargerRight";
                break;
            case 1:
                Bottom.sprite = Managers.Resource.Load<Sprite>("monster_113_charger_8");
                _animationName = "ChargerDown";
                break;
            case 2:
                Bottom.flipX = true;
                Bottom.sprite = Managers.Resource.Load<Sprite>("monster_113_charger_0");
                _animationName = "ChargerRight";
                break;
            case 3:
                Bottom.sprite = Managers.Resource.Load<Sprite>("monster_113_charger_4");
                _animationName = "ChargerUp";
                break;
            default:
                break;

        }

        TargetPos = dV[index];
        MonsterState = EMonsterState.Move;
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

    public void UpdateChargerSkill()
    {
        _accumulatedTime += Time.deltaTime;
        if (_accumulatedTime > 1.5f)
        {
            Rigidbody.velocity = Vector3.zero;
            AnimatorBottom.enabled = true;
            MonsterState = EMonsterState.Idle;
            _accumulatedTime = 0f;
            return;
        }

        switch ((Vector2)TargetPos)
        {
            case Vector2 v when v.Equals(Vector2.right):
                Bottom.flipX = false;
                Bottom.sprite = Managers.Resource.Load<Sprite>("monster_113_charger_13");
                break;
            case Vector2 v when v.Equals(Vector2.down):
                Bottom.sprite = Managers.Resource.Load<Sprite>("monster_113_charger_12");
                break;
            case Vector2 v when v.Equals(Vector2.left):
                Bottom.flipX = true;
                Bottom.sprite = Managers.Resource.Load<Sprite>("monster_113_charger_13");
                break;
            case Vector2 v when v.Equals(Vector2.up):
                Bottom.sprite = Managers.Resource.Load<Sprite>("monster_113_charger_14");
                break;
        }

        Rigidbody.velocity = TargetPos * 10f;
    }

}
