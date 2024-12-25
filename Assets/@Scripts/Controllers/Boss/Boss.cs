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
    /// when Boss need 2 Collider Use this for collision
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
        CreatureType = ECreatureType.Boss;
        BossType = EBossType.None;
        BossState = EBossState.None;
        Rigidbody =  transform.GetComponent<Rigidbody2D>();
        AnimatorBottom = transform.GetComponent<Animator>();
        GPCollider2D = transform.GetComponent<CircleCollider2D>();
        Range = 10;
        Tears = 5.0f;
        Speed = 3f;

#if UNITY_EDITOR
        Managers.UI.PlayingUI.BossHpActive(true);
#endif 
    }

    //코루틴을 사용한 유한상태 머신
    //tic을 조절해서 주기를 정할 수 있다
    protected override IEnumerator CoUpdateAI()
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

    protected override void UpdateSkill()
    {
        if (_coWait != null) return;

        float delay = 0;

        AnimatorBottom.Play(_skillName[(int)_currentSkill], 0, 0);
        //Debug.Log(_skillName[(int)_currentSkill]);
        if (_skillName[(int)_currentSkill] != AnimatorBottom.GetCurrentAnimatorClipInfo(0)[0].clip.name)
            return;
        delay = AnimatorBottom.GetCurrentAnimatorClipInfo(0)[0].clip.length;

        StartWait(delay);
    }

    protected override void UpdateMove()
    {
        if (BossState == EBossState.Dead) return;

        if (CreatureMoveState == ECreatureMoveState.TargetCreature && BossState == EBossState.Move)
            transform.position = Vector3.Lerp(transform.position, TargetPos, Time.deltaTime * 2f);
        if (CreatureMoveState == ECreatureMoveState.Designated)
            Rigidbody.velocity = TargetPos.normalized * Speed;

    }

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
        Rigidbody.velocity = Vector3.zero;
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
        FindChildByName(transform, transform.gameObject.name + "_Sprite").gameObject.SetActive(false);
        FindChildByName(transform, transform.gameObject.name + "_Shadow").gameObject.SetActive(false);
        yield return new WaitForSeconds(delay * 0.25f);
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

   
}
