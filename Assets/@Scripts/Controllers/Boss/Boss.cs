using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class Boss : Creature
{
    protected PolygonCollider2D PgCollider2D;
    protected CircleCollider2D CCollider2D;
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
        Speed = 3f;

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
                case EBossState.Dead:
                    break;
                case EBossState.Explosion:
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

    public override void OnDamaged(Creature owner, ESkillType skillType, string name = "")
    {
        base.OnDamaged(owner, skillType);
        Managers.UI.PlayingUI.ChangeBossHpSliderRatio(Hp / MaxHp);
    }

    public void ChangeBossState(EBossState bossState)
    {
        if (bossState == EBossState.Idle) _currentSkill = EBossSkill.Normal;
        BossState = bossState;
    }

    public void ChangeCollider(int on)
    {
        PgCollider2D.enabled = on == 1 ? true : false;
        transform.GetChild(0).GetComponent<CircleCollider2D>().enabled = on == 1 ? true : false;
    }

    public void setTargetPos()
    {
        if (Target == null) return;
        _targetPos = Target.transform.position;
        _startPos = transform.position;
    }

    public override void OnDead()
    {
        if (BossState == EBossState.Dead) return;
        BossState = EBossState.Dead;
        Managers.UI.PlayingUI.BossHpActive(false);

        if (gameObject.GetComponent<Animator>() != null)
            StartCoroutine(BossDeadAnim());
    }

    IEnumerator BossDeadAnim()
    {
        if (CCollider2D != null)
            CCollider2D.enabled = false;
        if (PgCollider2D != null)
            PgCollider2D.enabled = false;

        gameObject.GetComponent<SpriteRenderer>().enabled = true;
        GameObject go = Managers.Resource.Instantiate("BossDeathEffect");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.GetComponent<Animator>().Play("BossDeathEffect");

        AnimatorBottom.Play("Dead");
        //Debug.Log(_skillName[(int)_currentSkill]);
        float delay = AnimatorBottom.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        yield return new WaitForSeconds(delay * 0.75f);
        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(delay);
        base.OnDead();
    }

}
