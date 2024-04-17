using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using static Define;
public class MainCharacter : MonoBehaviour
{

    [SerializeField]
    float Speed = 5.0f;

    Animator AnimatorHead { get; set; }
    Animator AnimatorBottom { get; set; }
    Rigidbody2D Rigidbody { get; set; }

    Sprite[] HeadSprite { get; set; }

    SpriteRenderer Head { get; set; }
    SpriteRenderer Bottom { get; set; }


    private Vector3 _moveDir;

    protected EPlayerBottomState _bottomState = EPlayerBottomState.Idle;
    protected EPlayerHeadState _headState = EPlayerHeadState.Idle;
    protected EPlayerHeadDirState _headDirState = EPlayerHeadDirState.None;
    public EPlayerBottomState BottomState
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

    public EPlayerHeadState HeadState
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

    public EPlayerHeadDirState HeadDirState
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
        AnimatorHead = transform.GetChild(0).GetComponentInChildren<Animator>();
        AnimatorHead.enabled = false;
        AnimatorBottom = transform.GetChild(1).GetComponentInChildren<Animator>();

        Rigidbody = GetComponent<Rigidbody2D>();
        Head = transform.Find("Head").GetComponent<SpriteRenderer>();
        Bottom = transform.Find("Bottom").GetComponent<SpriteRenderer>();

        HeadSprite = new Sprite[]
        {
            Managers.Resource.Load<Sprite>("isaac_up"),
            Managers.Resource.Load<Sprite>("isaac_down"),
            Managers.Resource.Load<Sprite>("isaac_right"),
        };
    }

    void Start()
    {

    }

    void Update()
    {
        #region Attack
        Vector2 attackVel = Vector2.zero;
        attackVel.x = Input.GetAxis("AttackHorizontal");
        attackVel.y = Input.GetAxis("AttackVertical");

        if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
            attackVel.x *= -1;
        if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.DownArrow))
            attackVel.y *= -1;

        UpdateAttack(attackVel);
        #endregion

        #region Movement
        Vector2 vel = Rigidbody.velocity;
        vel.x = Input.GetAxis("Horizontal") * Speed;
        vel.y = Input.GetAxis("Vertical") * Speed;

        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
            vel.x = 0;
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S))
            vel.y = 0;

        UpdateMovement(vel);
        #endregion
    }

    public void UpdateAttack(Vector2 attackVel)
    {
        if (attackVel != Vector2.zero)
        {
            HeadDirState = EPlayerHeadDirState.None;
            AnimatorHead.enabled = true;
            HeadState = EPlayerHeadState.Attack;

            if (attackVel.y != 0 && (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)))
            {
                HeadDirState = attackVel.y > 0 ? EPlayerHeadDirState.Up : EPlayerHeadDirState.Down;
            }
            else if (attackVel.x != 0)
            {
                HeadDirState = attackVel.x > 0 ? EPlayerHeadDirState.Right : EPlayerHeadDirState.Left;
            }
        }
        else
        {
            AnimatorHead.enabled = false;
            HeadState = EPlayerHeadState.Idle;
        }
    }

    public void UpdateMovement(Vector2 vel)
    {
        Rigidbody.velocity = vel;

        if (vel != Vector2.zero)
        {
            if (vel.y != 0 && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)))
            {
                BottomState = vel.y > 0 ? EPlayerBottomState.MoveUp : EPlayerBottomState.MoveDown;
                if (HeadState == EPlayerHeadState.Idle)
                    HeadDirState = vel.y > 0 ? EPlayerHeadDirState.Up : EPlayerHeadDirState.Down;
            }
            else if (vel.x != 0)
            {
                BottomState = vel.x > 0 ? EPlayerBottomState.MoveRight : EPlayerBottomState.MoveLeft;
                if (HeadState == EPlayerHeadState.Idle)
                    HeadDirState = vel.x > 0 ? EPlayerHeadDirState.Right : EPlayerHeadDirState.Left;
            }
        }
        else
        {
            BottomState = EPlayerBottomState.Idle;
            if (HeadState == EPlayerHeadState.Idle)
                HeadDirState = EPlayerHeadDirState.Down;
        }
    }

    public void UpdateFacing()
    {
        if (HeadState == EPlayerHeadState.Attack)
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
            case EPlayerHeadDirState.Up:
                Head.flipX = false;
                AnimatorHead.Play("Attack_Up");
                break;
            case EPlayerHeadDirState.Down:
                Head.flipX = false;
                AnimatorHead.Play("Attack_Down");
                break;
            case EPlayerHeadDirState.Left:
                Head.flipX = true;
                AnimatorHead.Play("Attack_Right");
                break;
            case EPlayerHeadDirState.Right:
                Head.flipX = false;
                AnimatorHead.Play("Attack_Right");
                break;
        }
    }

    public void UpdateHeadSprite()
    {
        switch (HeadDirState)
        {
            case EPlayerHeadDirState.Up:
                Head.flipX = false;
                Head.sprite = HeadSprite[0];
                break;
            case EPlayerHeadDirState.Down:
                Head.flipX = false;
                Head.sprite = HeadSprite[1];
                break;
            case EPlayerHeadDirState.Left:
                Head.flipX = true;
                Head.sprite = HeadSprite[2];
                break;
            case EPlayerHeadDirState.Right:
                Head.flipX = false;
                Head.sprite = HeadSprite[2];
                break;
        }
    }

    public void UpdateBottomAnimation()
    {
        switch (BottomState)
        {
            case EPlayerBottomState.Idle:
                Bottom.flipX = false;
                AnimatorBottom.Play("Idle");
                break;
            case EPlayerBottomState.MoveDown:
                Bottom.flipX = false;
                AnimatorBottom.Play("Walk_Down");
                break;
            case EPlayerBottomState.MoveUp:
                Bottom.flipX = true;
                AnimatorBottom.Play("Walk_Down");
                break;
            case EPlayerBottomState.MoveLeft:
                Bottom.flipX = true;
                AnimatorBottom.Play("Walk_Horiz");
                break;
            case EPlayerBottomState.MoveRight:
                Bottom.flipX = false;
                AnimatorBottom.Play("Walk_Horiz");
                break;
            case EPlayerBottomState.OnDamaged:
                break;
            case EPlayerBottomState.OnDead:
                break;

        }
    }


    public void GenerateProjectile()
    {
        switch (HeadDirState)
        {
            case EPlayerHeadDirState.Up:
                SpawnProjectile(0,1);
                break;
            case EPlayerHeadDirState.Down:
                SpawnProjectile(0, -1);
                break;
            case EPlayerHeadDirState.Left:
                SpawnProjectile(-1, 0);
                break;
            case EPlayerHeadDirState.Right:
                SpawnProjectile(1, 0);
                break;
        }
    }

    public void SpawnProjectile(float x, float y)
    {
        GameObject go = Managers.Resource.Instantiate("Projectile");
        go.name = "Projectile";
        Vector2 pos;
        pos.x =  x;
        pos.y =  y;

        Projectile projectile = go.GetComponent<Projectile>();
        projectile.SetInfo(transform.GetChild(0).transform.position,pos);
    }
}
