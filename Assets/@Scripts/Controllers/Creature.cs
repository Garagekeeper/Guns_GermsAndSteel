using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using static Utility;

public class Creature : BaseObject
{
    #region BaseStat

    protected Vector3 _targetPos;
    protected Vector3 _startPos;

    public Vector3 TargetPos
    {
        get { return _targetPos; }
        set
        {
            if (value != _targetPos)
            {
                _targetPos = value;
            }
        }
    }

    protected bool _isFloating = false;
    public bool IsFloating
    {
        get { return _isFloating; }
        private set { _isFloating = value; }
    }
    private float _hp;
    public float MaxHp { get; set; }
    //public float MaxDamage { get; set; } = 26f;
    public float Hp
    {
        get { return _hp; }
        set
        {
            if (_hp != value)
            {
                if (_hp > 0)
                    _hp = Mathf.Max(_hp, MaxHp);
                _hp = value;
                if (value <= 0)
                {
                    _hp = 0;
                    OnDead();
                }
            }
        }
    }
    public float Tears { get; set; }
    public float Speed { get; set; }
    public float AttackDamage { get; set; }
    public float Range { get; set; }
    public float ShotSpeed { get; set; }
    public float Luck { get; set; }
    public float BombDamage { get; set; }

    public List<GameObject> Familiar = new();

    public int Life { get; set; } = 0;

    public bool IsGuided { get; set; } = false;

    public ECreatureMoveState CreatureMoveState { get; set; } = ECreatureMoveState.None;


    #endregion

    private Creature _target;
    public Creature Target
    {
        get => _target;
        set { if (_target != value) _target = value; }

    }

    public ECreatureType CreatureType { get; protected set; } = ECreatureType.None;
    protected ECreatureState _creatureState;
    public virtual ECreatureState CreatureState
    {
        get { return _creatureState; }
        set
        {
            if (_creatureState != value)
            {
                _creatureState = value;
                switch (value)
                {
                    case ECreatureState.None:
                        break;
                    case ECreatureState.Idle:
                        UpdateAITick = 0.5f;
                        break;
                    case ECreatureState.Skill:
                        UpdateAITick = 0.0f;
                        break;
                    case ECreatureState.Move:
                        UpdateAITick = 0.0f;
                        break;
                    case ECreatureState.Dead:
                        UpdateAITick = 0.0f;
                        break;
                }
            }
        }
    }

    protected Animator AnimatorHead { get; set; }
    /// <summary>
    /// Use as Default Animator
    /// </summary>
    protected Animator AnimatorBottom { get; set; }
    public Rigidbody2D Rigidbody { get; set; }

    public Collider2D Collider { get; set; }

    protected Sprite[] HeadSprite { get; set; }

    protected SpriteRenderer Head { get; set; }
    protected SpriteRenderer Bottom { get; set; }

    protected ECreatureBottomState _bottomState = ECreatureBottomState.Idle;
    protected ECreatureHeadState _headState = ECreatureHeadState.Idle;
    protected ECreatureHeadDirState _headDirState = ECreatureHeadDirState.None;
    public ECreatureBottomState BottomState
    {
        get { return _bottomState; }
        set
        {
            if (_bottomState != value)
            {
                _bottomState = value;
                UpdateBottomAnimation();
            }
        }
    }

    public bool HeadStateChanged { get; set; } = false;

    public ECreatureHeadState HeadState
    {
        get { return _headState; }
        set
        {
            if (_headState != value)
            {
                _headState = value;
                HeadStateChanged = true;
            }
            else
            {
                HeadStateChanged = false;
            }
        }
    }

    public ECreatureHeadDirState HeadDirState
    {
        get { return _headDirState; }
        set
        {
            if (_headDirState != value || HeadStateChanged)
            {
                _headDirState = value;
                UpdateFacing(true);
            }
        }
    }



    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        //base stat
        Hp = 3.5f;
        MaxHp = 3.5f;
        Tears = 10.0f;
        Speed = 5.0f;
        AttackDamage = 3.5f;
        Range = 1.0f;
        ShotSpeed = 1.0f;
        Luck = 0f;
        BombDamage = 100f;

        //상하체 분리형
        if (transform.childCount != 0)
        {
            AnimatorHead = transform.GetChild(0).GetComponentInChildren<Animator>();
            //AnimatorHead.enabled = false;
            AnimatorBottom = transform.GetChild(1).GetComponentInChildren<Animator>();
            Head = FindChildByNameContain(transform,"Head").GetComponent<SpriteRenderer>();
            Bottom = FindChildByNameContain(transform, "Bottom").GetComponent<SpriteRenderer>();
        }
        //나머지
        else
        {
            AnimatorBottom = transform.GetComponentInChildren<Animator>();
            Bottom = transform.GetComponent<SpriteRenderer>();
        }


        Rigidbody = GetComponent<Rigidbody2D>();
        Collider = GetComponent<CircleCollider2D>() == null ? GetComponent<CapsuleCollider2D>() : GetComponent<CircleCollider2D>();
        LayerMask mask = 0;
        mask |= (1 << 15);
        Collider.excludeLayers = mask;

    }


    public void UpdateFacing(bool isSeperate)
    {
        if (isSeperate)
            UpdateHeadAnimation();
        else
        {
            if (Target == null) return;
            if (Vector3.Cross(Vector2.down, (Target.transform.position - transform.position)).x > 0)
                transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = false;
            else
                transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = true;

        }
    }

    public void UpdateHeadAnimation()
    {
        if (HeadState == ECreatureHeadState.Attack)
        {
            switch (HeadDirState)
            {
                case ECreatureHeadDirState.Up:
                    Head.flipX = false;
                    AnimatorHead.Play("Attack_Up");
                    break;
                case ECreatureHeadDirState.Down:
                    Head.flipX = false;
                    AnimatorHead.Play("Attack_Down");
                    break;
                case ECreatureHeadDirState.Left:
                    Head.flipX = true;
                    AnimatorHead.Play("Attack_Right");
                    break;
                case ECreatureHeadDirState.Right:
                    Head.flipX = false;
                    AnimatorHead.Play("Attack_Right");
                    break;
            }
        }
        else
        {
            switch (HeadDirState)
            {
                case ECreatureHeadDirState.Up:
                    Head.flipX = false;
                    AnimatorHead.Play("Look_Up");
                    break;
                case ECreatureHeadDirState.Down:
                    Head.flipX = false;
                    AnimatorHead.Play("Look_Down");
                    break;
                case ECreatureHeadDirState.Left:
                    Head.flipX = true;
                    AnimatorHead.Play("Look_Right");
                    break;
                case ECreatureHeadDirState.Right:
                    Head.flipX = false;
                    AnimatorHead.Play("Look_Right");
                    break;
            }
        }

    }

    //public void UpdateHeadSprite()
    //{
    //    AnimatorHead.StopPlayback();
    //    switch (HeadDirState)
    //    {
    //        case ECreatureHeadDirState.Up:
    //            Head.flipX = false;
    //            Head.sprite = HeadSprite[0];
    //            break;
    //        case ECreatureHeadDirState.Down:
    //            Head.flipX = false;
    //            Head.sprite = HeadSprite[1];
    //            break;
    //        case ECreatureHeadDirState.Left:
    //            Head.flipX = true;
    //            Head.sprite = HeadSprite[2];
    //            break;
    //        case ECreatureHeadDirState.Right:
    //            Head.flipX = false;
    //            Head.sprite = HeadSprite[2];
    //            break;
    //    }
    //}

    public void UpdateBottomAnimation()
    {
        switch (BottomState)
        {
            case ECreatureBottomState.Idle:
                Bottom.flipX = false;
                AnimatorBottom.Play("Idle");
                break;
            case ECreatureBottomState.MoveDown:
                Bottom.flipX = false;
                AnimatorBottom.Play("Walk_Down");
                break;
            case ECreatureBottomState.MoveUp:
                Bottom.flipX = true;
                AnimatorBottom.Play("Walk_Down");
                break;
            case ECreatureBottomState.MoveLeft:
                Bottom.flipX = true;
                AnimatorBottom.Play("Walk_Horiz");
                break;
            case ECreatureBottomState.MoveRight:
                Bottom.flipX = false;
                AnimatorBottom.Play("Walk_Horiz");
                break;
            case ECreatureBottomState.OnDamaged:
                break;
            case ECreatureBottomState.OnDead:
                break;

        }
    }

    public void GenerateProjectile()
    {
        switch (HeadDirState)
        {
            case ECreatureHeadDirState.Up:
                SpawnProjectile(Vector3.up);
                break;
            case ECreatureHeadDirState.Down:
                SpawnProjectile(Vector3.down);
                break;
            case ECreatureHeadDirState.Left:
                SpawnProjectile(Vector3.left);
                break;
            case ECreatureHeadDirState.Right:
                SpawnProjectile(Vector3.right);
                break;
        }
    }

    public void GenerateProjectile(Vector3 tarGetDir, bool _isRandom = false, bool _isBlood = false)
    {
        SpawnProjectile(tarGetDir, _isRandom, _isBlood);
    }

    public void SpawnProjectile(Vector3 tarGetDir, bool _isRandom = false, bool _isBlood = false)
    {
        GameObject go = Managers.Resource.Instantiate("Projectile");
        go.name = "Projectile";
        Vector2 pos = tarGetDir;

        Projectile projectile = go.GetComponent<Projectile>();
        Vector3 origin = transform.childCount == 0 ? transform.position : transform.GetChild(0).position;
        projectile.SetInfo(origin + tarGetDir * 0.5f, pos, this, _isRandom, _isBlood);
    }

    public Creature FindClosetTarget(Creature src, List<Creature> targets)
    {
        float minDistance = float.MaxValue;
        Creature closestTarget = null;

        foreach (Creature target in targets)
        {
            float dist = (src.transform.position - target.transform.position).sqrMagnitude;
            if (target == null || target.isActiveAndEnabled == false)
                continue;

            if (dist < minDistance)
            {
                minDistance = dist;
                closestTarget = target;
            }
        }

        return closestTarget;
    }

    public virtual void OnDamaged(Creature owner, ESkillType skillType, string name = "")
    {
        switch (skillType)
        {
            case ESkillType.BodySlam:
                //TODO
                break;
            case ESkillType.Bomb:
                ChangeHpValue(-1 * owner.BombDamage);
                break;
            case ESkillType.Projectile:
                ChangeHpValue(-1 * owner.AttackDamage);
                //Debug.Log(Hp);
                break;
            case ESkillType.Fire:
                break;
            case ESkillType.Spike:
                break;
        }

        //Debug.Log(Hp);

    }

    public virtual void ChangeHpValue(float value)
    {
        Hp += value;
    }


    public bool IsDead()
    {
        if (Hp <= 0) return true;
        return false;
    }

    public override void OnDead()
    {
        switch (CreatureType)
        {
            case ECreatureType.Boss:
                Managers.Object.Despawn(this);
                break;
            case ECreatureType.Monster:
                Managers.Object.Despawn(this);
                break;
            case ECreatureType.MainCharacter:
                Managers.Object.Despawn(this);
                break;
            default:
                break;
        }
        CreatureState = ECreatureState.Dead;
        Managers.Game.RoomConditionCheck();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {

    }

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

    public float UpdateAITick { get; protected set; } = 0.0f;

    //코루틴을 사용한 유한상태 머신
    //tic을 조절해서 주기를 정할 수 있다
    protected virtual IEnumerator CoUpdateAI()
    {
        yield break;
    }

    protected virtual void UpdateSkill() { }

    protected virtual void UpdateIdle() { if (Managers.Object.MainCharacters.Count == 0) return; }

    protected virtual void UpdateMove() { }

    protected virtual void UpdateDead() { }

    protected virtual void UpdateExplosion() { }

    public void ReflectTargetVecor(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Projectile"))
        {
            return;
        }

        //Debug.Log("dirVec: " + dirVec);
        //Vector3 incomingVec = hitPos - dirVec;
        TargetPos = Vector3.Reflect(TargetPos, collision.GetContact(0).normal);

    }

    public void ReflectTargetVecor(Collider2D collider2D)
    {
        if (collider2D.gameObject.CompareTag("Player"))
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
