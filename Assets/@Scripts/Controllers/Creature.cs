using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Creature : MonoBehaviour
{
    #region BaseStat
    public float Hp { get; set; }
    public float AttackSpeed { get; set; }
    public float Speed { get; set; }
    public float AttackDamage { get; set; }
    public float Range { get; set; }
    public float ShotSpeed { get; set; }
    public float Luck { get; set; }

    int Coin;
    int Bomb = 1;
    #endregion

    protected Animator AnimatorHead { get; set; }
    protected Animator AnimatorBottom { get; set; }
    public Rigidbody2D Rigidbody { get; set; }

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

    public ECreatureHeadState HeadState
    {
        get { return _headState; }
        set
        {
            if (_headState != value)
            {
                _headState = value;
            }
        }
    }

    public ECreatureHeadDirState HeadDirState
    {
        get { return _headDirState; }
        set
        {
            if (_headDirState != value)
            {
                _headDirState = value;
                UpdateFacing();
            }
        }
    }



    private void Awake()
    {
        Init();
    }

    public virtual void Init()
    {
        //base stat
        Hp = 3.5f;
        AttackSpeed = 10.0f;
        Speed = 5.0f;
        AttackDamage = 3.5f;
        Range = 1.0f;
        ShotSpeed = 1.0f;
        Luck = 0f;

        AnimatorHead = transform.GetChild(0).GetComponentInChildren<Animator>();
        AnimatorHead.enabled = false;
        AnimatorBottom = transform.GetChild(1).GetComponentInChildren<Animator>();

        Rigidbody = GetComponent<Rigidbody2D>();
        Head = transform.Find("Head").GetComponent<SpriteRenderer>();
        Bottom = transform.Find("Bottom").GetComponent<SpriteRenderer>();
    }


    public void UpdateFacing()
    {
        if (HeadState == ECreatureHeadState.Attack)
        {
            AnimatorHead.enabled = true;
            UpdateHeadAnimation();
        }
        else
        {
            AnimatorHead.enabled = false;
            UpdateHeadSprite();
        }


    }

    public void UpdateHeadAnimation()
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

    public void UpdateHeadSprite()
    {
        switch (HeadDirState)
        {
            case ECreatureHeadDirState.Up:
                Head.flipX = false;
                Head.sprite = HeadSprite[0];
                break;
            case ECreatureHeadDirState.Down:
                Head.flipX = false;
                Head.sprite = HeadSprite[1];
                break;
            case ECreatureHeadDirState.Left:
                Head.flipX = true;
                Head.sprite = HeadSprite[2];
                break;
            case ECreatureHeadDirState.Right:
                Head.flipX = false;
                Head.sprite = HeadSprite[2];
                break;
        }
    }

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
                SpawnProjectile(0, 1);
                break;
            case ECreatureHeadDirState.Down:
                SpawnProjectile(0, -1);
                break;
            case ECreatureHeadDirState.Left:
                SpawnProjectile(-1, 0);
                break;
            case ECreatureHeadDirState.Right:
                SpawnProjectile(1, 0);
                break;
        }
    }

    public void SpawnProjectile(float x, float y)
    {
        GameObject go = Managers.Resource.Instantiate("Projectile");
        go.name = "Projectile";
        Vector2 pos;
        pos.x = x;
        pos.y = y;

        Projectile projectile = go.GetComponent<Projectile>();
        projectile.SetInfo(transform.GetChild(0).transform.position, pos, this);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject go = collision.gameObject;
        if (go.name == "Projectile" && go.GetComponent<Projectile>().Owner)
            return;
        //TODO
    }

}