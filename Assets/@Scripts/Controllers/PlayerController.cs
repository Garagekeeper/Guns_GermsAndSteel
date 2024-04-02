using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using static Define;
public class PlayerController : MonoBehaviour
{

    [SerializeField]
    float Speed = 5.0f;

    protected EPlayerState _creatureState = EPlayerState.Idle;

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

    Animator Animator { get; set; }
    Rigidbody2D Rigidbody { get; set; }
    Animation Animation { get; set; }


    SpriteRenderer Head { get; set; }
    SpriteRenderer Bottom { get; set; }

    Sprite CharacterSprite { get; set; }
    Sprite[] Head_Sprites { get; set; }
    Sprite[] Bottom_Sprites { get; set; }

    private float xMove;
    private float yMove;

    void Start()
    {
        Animator = GetComponentInChildren<Animator>();
        Rigidbody = GetComponent<Rigidbody2D>();
        Head = transform.Find("Head").GetComponent<SpriteRenderer>();
        Bottom = transform.Find("Bottom").GetComponent<SpriteRenderer>();

        CharacterSprite = Resources.Load<Sprite>("@Resources/Sprites/Issac/characters/costumes/character_001_isaac");
        Debug.Log("Sprite Load");
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

        
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
        {
            if (Math.Abs(xMove) < 0.05)
                xMove = 0f;

            xMove *= 0.9f;
        }
        else
        {
            xMove = Input.GetAxis("Horizontal");
        }

        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S))
        {
            if (Math.Abs(yMove) < 0.05)
                yMove = 0f;

            yMove *= 0.9f;
        }
        else
        {
            yMove = Input.GetAxis("Vertical");
        }


        Vector3 _moveDir = new Vector3(xMove, yMove, 0);

        if (_moveDir != Vector3.zero)
        {
            PlayerState = EPlayerState.Move;
            transform.Translate(_moveDir * Time.deltaTime * Speed);
        }
        else
        {
            PlayerState = EPlayerState.Idle;
        }
    }

    public void UpdateAnimation()
    {
        switch (_creatureState)
        {
            case EPlayerState.Idle:
                Animator.Play("Idle");
                break;
            case EPlayerState.Move:
                Animator.Play("Walk_Down");
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
