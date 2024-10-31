using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class Boss : Creature
{
    public EBossType BossType { get; protected set; } = 0;

    protected EBossState _bossState;
    public virtual EBossState BossState
    {
        get { return _bossState; }
        set
        {
            if (_bossState != value)
            {
                _bossState = value;
                switch (value)
                {
                    case EBossState.None:
                        break;
                    case EBossState.Idle:
                        UpdateAITick = 0.5f;
                        break;
                    case EBossState.Skill:
                        UpdateAITick = 0.0f;
                        break;
                }
            }
        }
    }

    public enum EBossSkill
    {
        Normal,
        SkillA,
        SkillB,
        SkillC,
    }
    protected string[] _skillName = { "Normal", "SkillA", "SkillB", "SkillC" };
    protected EBossSkill _currentSkill = EBossSkill.Normal;

    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        CreatureType = ECreatureType.Boss;
        BossType = EBossType.None;
        BossState = EBossState.None;
        Rigidbody = GetComponent<Rigidbody2D>();
        Range = 10;
        Tears = 5.0f;

#if UNITY_EDITOR
        Managers.UI.PlayingUI.BossHpActive(true);
#endif 
    }
    public float UpdateAITick { get; protected set; } = 0.0f;

    //코루틴을 사용한 유한상태 머신
    //tic을 조절해서 주기를 정할 수 있다
    protected IEnumerator CoUpdateAI()
    {
        while (true)
        {
            switch (BossState)
            {
                case EBossState.Idle:
                    UpdateIdle();
                    break;
                case EBossState.Skill:
                    UpdateSkill();
                    break;
                case EBossState.Move:
                    UpdateMove();
                    break;
            }

            if (UpdateAITick > 0)
                yield return new WaitForSeconds(UpdateAITick);
            else
                yield return null;
        }

    }

    protected virtual void UpdateSkill() { }
    protected virtual void UpdateIdle() { }
    protected virtual void UpdateMove() { }

    //UpdateAITick이 짧기 때문에
    //애니메이션이 재생중에서는 다른 스킬을 재생할 수 없도록 처리
    #region Wait
    protected Coroutine _coWait = null;

    protected void StartWait(float seconds)
    {
        CancelWait();
        _coWait = StartCoroutine(CoWait(seconds));
    }

    IEnumerator CoWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _coWait = null;
    }

    protected void CancelWait()
    {
        if (_coWait != null)
            StopCoroutine(_coWait);
        _coWait = null;
    }
    #endregion

    private void OnCollisionEnter2D(Collision2D collision)
    {

    }

    public override void OnDamaged(Creature owner, ESkillType skillType)
    {
        base.OnDamaged(owner, skillType);
        Managers.UI.PlayingUI.BossHpSlider(Hp / MaxHp);
    }
}
