using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;
using static Utility;

public class Boss : Creature
{

    /// <summary>
    /// default collider. Normally, this collider does 2 things (for collision and projectile)
    /// <br/>
    /// when Boss need 2 Collider Use this for projectile 
    /// </summary>
    protected CircleCollider2D GPCollider2D;

    /// <summary>
    /// Collider for general physics
    /// <br/>
    /// when Boss need second collider, Use this for projectile
    /// </summary>
    protected CircleCollider2D PTCollider2D;
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
        Transform temp;
        CreatureType = ECreatureType.Boss;
        BossType = EBossType.None;
        BossState = EBossState.None;
        Rigidbody = GetComponent<Rigidbody2D>();
        AnimatorBottom = transform.GetComponent<Animator>();
        GPCollider2D = transform.GetComponent<CircleCollider2D>();
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

    public void ChangeBossState(EBossState bossState)
    {
        if (bossState == EBossState.Idle) _currentSkill = EBossSkill.Normal;
        BossState = bossState;
    }

    public void ChangeCollider(CircleCollider2D collider, bool on)
    {
        collider.enabled = on;
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
        if (PTCollider2D != null)
            PTCollider2D.enabled = false;
        if (GPCollider2D != null)
            GPCollider2D.enabled = false;

        GameObject go = Managers.Resource.Instantiate("BossDeathEffect");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.GetComponent<Animator>().Play("BossDeathEffect");

        AnimatorBottom.Play("Dead");
        //Debug.Log(_skillName[(int)_currentSkill]);
        float delay = AnimatorBottom.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        yield return new WaitForSeconds(delay * 0.75f);
        FindChildByName(transform, transform.gameObject.name + "_Anim").GetComponent<SpriteRenderer>().enabled = false;
        FindChildByName(transform, transform.gameObject.name + "_Shadow").GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(delay);
        base.OnDead();
    }

    public override void OnDamaged(Creature owner, ESkillType skillType, string name = "")
    {
        base.OnDamaged(owner, skillType);
        Managers.UI.PlayingUI.ChangeBossHpSliderRatio(Hp / MaxHp);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

    }

    public void setTargetPos()
    {
        if (Target == null) return;
        TargetPos = Target.transform.position;
        _startPos = transform.position;
    }

    public void ReflectTargetVecor(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Projectile")
        {
            return;
        }

        //Debug.Log("dirVec: " + dirVec);
        //Vector3 incomingVec = hitPos - dirVec;
        TargetPos = Vector3.Reflect(TargetPos, collision.GetContact(0).normal);

    }

    public void ReflectTargetVecor(Collider2D collider2D)
    {
        if (collider2D.gameObject.tag == "Player")
        {
            return;
        }

        //Debug.Log("dirVec: " + dirVec);
        //Vector3 incomingVec = hitPos - dirVec
        // 충돌한 지점의 좌표.
        // (나의 거리와 충돌한 물체의 콜라이더 좌표중에 제일 가까운 좌표)
        Vector2 contactPoint = collider2D.ClosestPoint(transform.position);
        TargetPos = Vector3.Reflect(TargetPos, contactPoint.normalized);

    }
}
