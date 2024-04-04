using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;
using static Define;
public class PlayerController : MonoBehaviour
{

    [SerializeField]
    float Speed = 5.0f;

    protected EPlayerState _creatureState = EPlayerState.Idle;
    protected EPlayerFacing _facing = EPlayerFacing.Down;
    public EPlayerState PlayerState
    {
        get { return _creatureState; }
        set
        {
            if (_creatureState != value)
            {
                _creatureState = value;
                UpdateAnimation();
            }
        }
    }

    public EPlayerFacing PlayerFacing
    {
        get { return _facing; }
        set
        {
            if (_facing != value)
            {
                _facing = value;
                UpdateFacing();
            }
        }
    }

    Animator Animator { get; set; }
    Rigidbody2D Rigidbody { get; set; }
    Animation Animation { get; set; }


    SpriteRenderer Head { get; set; }
    SpriteRenderer Bottom { get; set; }

    Sprite CharacterSprite { get; set; }

    private Vector3 _moveDir;

    private void Awake()
    {
        Animator = transform.GetChild(1).GetComponentInChildren<Animator>();
        Rigidbody = GetComponent<Rigidbody2D>();
        Head = transform.Find("Head").GetComponent<SpriteRenderer>();
        Bottom = transform.Find("Bottom").GetComponent<SpriteRenderer>();
    }

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKey("up"))
        {
            Debug.Log("Up");
            //Head.sprite
        }
        if (Input.GetKey("down"))
        {
            Debug.Log("Down");
        }
        if (Input.GetKey("right"))
        {
            Debug.Log("Right");
        }
        if (Input.GetKey("left"))
        {
            Debug.Log("Left");
        }

        Vector2 vel = Rigidbody.velocity;
        vel.x = Input.GetAxis("Horizontal") * Speed;
        vel.y = Input.GetAxis("Vertical") * Speed;

        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
            vel.x = 0;
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S))
            vel.y = 0;


        Rigidbody.velocity = vel;
        if (vel != Vector2.zero)
        {
            if (vel.y != 0 && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)))
            {
                PlayerFacing = vel.y > 0 ? EPlayerFacing.Up : EPlayerFacing.Down;
                PlayerState = EPlayerState.MoveVertical;
            }
            else
            {
                PlayerFacing = vel.x > 0 ? EPlayerFacing.Right : EPlayerFacing.Left;
                PlayerState = EPlayerState.MoveHorizen;
            }
        } else
        {
            PlayerState = EPlayerState.Idle;
        }

        


    }

    public void UpdateFacing()
    {
        switch (_facing)
        {
            case EPlayerFacing.Down:
                Head.flipX = false;
                Head.sprite = Managers.Resource.Load<Sprite>("isaac_down");
                break;
            case EPlayerFacing.Up:
                Head.flipX = false;
                Head.sprite = Managers.Resource.Load<Sprite>("isaac_up");
                break;
            case EPlayerFacing.Right:
                Head.flipX = false;
                Bottom.flipX = false;
                Head.sprite = Managers.Resource.Load<Sprite>("isaac_right");
                break;
            case EPlayerFacing.Left:
                Head.flipX = true;
                Bottom.flipX = true;
                Head.sprite = Managers.Resource.Load<Sprite>("isaac_right");
                break;
        }
    }

    public void UpdateAnimation()
    {
        switch (_creatureState)
        {
            case EPlayerState.Idle:
                Animator.Play("Idle");
                break;
            case EPlayerState.MoveVertical:
                Animator.Play("Walk_Down");
                break;
            case EPlayerState.MoveHorizen:
                Animator.Play("Walk_Horiz");
                break;
            case EPlayerState.Attack:
                break;
            case EPlayerState.OnDamaged:
                break;
            case EPlayerState.OnDead:
                break;

        }
    }
}
