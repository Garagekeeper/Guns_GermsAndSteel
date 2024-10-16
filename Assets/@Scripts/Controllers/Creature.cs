using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Creature : BaseObject
{
    #region BaseStat
    private float _hp;
    public float Hp
    {
        get { return _hp; }
        set
        {
            if (_hp != value)
            {
                if (value <= 0)
                    OnDead();
                else
                    _hp = value;
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

    public List<GameObject> Familiar = new List<GameObject>();

    public int Life { get; set; } = 0;

    public bool IsGuided { get; set; } = false;


    #endregion

    private Creature _target;
    public Creature Target
    {
        get => _target;
        set { _target = value; }

    }

    public ECreatureType CreatureType { get; protected set; } = ECreatureType.None;

    protected Animator AnimatorHead { get; set; }
    protected Animator AnimatorBottom { get; set; }
    public Rigidbody2D Rigidbody { get; set; }

    protected CircleCollider2D Collider { get; set; }

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
        Tears = 10.0f;
        Speed = 5.0f;
        AttackDamage = 3.5f;
        Range = 1.0f;
        ShotSpeed = 1.0f;
        Luck = 0f;
        BombDamage = 100f;

        AnimatorHead = transform.GetChild(0).GetComponentInChildren<Animator>();
        //AnimatorHead.enabled = false;
        AnimatorBottom = transform.GetChild(1).GetComponentInChildren<Animator>();

        Rigidbody = GetComponent<Rigidbody2D>();
        Collider = GetComponent<CircleCollider2D>();
        Head = transform.Find("Head").GetComponent<SpriteRenderer>();
        Bottom = transform.Find("Bottom").GetComponent<SpriteRenderer>();

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

    public void GenerateProjectile(Vector3 tarGetDir, bool _isRandom = false)
    {
        SpawnProjectile(tarGetDir, _isRandom);
    }

    public void SpawnProjectile(Vector3 tarGetDir, bool _isRandom = false)
    {
        GameObject go = Managers.Resource.Instantiate("Projectile");
        go.name = "Projectile";
        Vector2 pos = tarGetDir;

        Projectile projectile = go.GetComponent<Projectile>();
        projectile.SetInfo(transform.GetChild(0).transform.position + tarGetDir * 0.5f, pos, this, _isRandom);
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

    public virtual void OnDamaged(Creature owner, ESkillType skillType)
    {
        switch (skillType)
        {
            case ESkillType.BodySlam:
                //TODO
                break;
            case ESkillType.Bomb:
                Hp -= BombDamage;
                break;
            case ESkillType.Projectile:
                Hp -= AttackDamage;
                break;
            case ESkillType.Fire:
                break;
            case ESkillType.Spike:
                break;
        }

        Debug.Log(Hp);

        if (Hp < 0)
        {
            OnDead();
            return;
        }

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
                Managers.Object.Bosses.Remove((Boss)this);
                break;
            case ECreatureType.Monster:
                Managers.Object.Monsters.Remove((Monster)this);
                break;
            case ECreatureType.MainCharacter:
                Managers.Object.MainCharacters.Remove((MainCharacter)this);
                break;
            default:
                break;
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {

    }

}
