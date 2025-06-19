using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;
using static UnityEngine.Rendering.DebugUI;
using static Utility;
using DG.Tweening;

public class Boss_GurdyJr : Boss
{
    private Sequence sequence;
    private Vector2 _previousVelocity;
    private Vector2 _vel;
    private void Awake()
    {
        Init();
        StartCoroutine(CoUpdateAI());
    }

    private void FixedUpdate()
    {
        if (BossState == EBossState.Dead)
        {
            Rigidbody.velocity = Vector2.zero;
            return;
        }

            if (Rigidbody.velocity.magnitude > 0.01f)
        {
            _previousVelocity = Rigidbody.velocity;
        }

        Rigidbody.velocity = _vel;
    }

    public override void Init()
    {
        base.Init();
        Hp = 250.0f;
        MaxHp = 250.0f;
        BossType = EBossType.GurdyJr;
        BossState = EBossState.Idle;
        CreatureMoveState = ECreatureMoveState.None;
        AttackDamage = 5f;
        Speed = 8f;
        Bottom = FindChildByName(transform, transform.gameObject.name.Replace("(Clone)", "").Trim() + "_Face").GetComponent<SpriteRenderer>();

        _flickerTarget.Add(FindChildByName(transform, "Boss_GurdyJr_Sprite").GetComponent<SpriteRenderer>());
        _flickerTarget.Add(FindChildByName(transform, "Boss_GurdyJr_Face").GetComponent<SpriteRenderer>());
    }

    protected override void UpdateIdle()
    {
        if (Managers.Object.MainCharacters.Count == 0) return;
        if (BossState == EBossState.Dead) return;

        _vel = Vector3.zero;

        //0. 가장 가까운 목표 탐색 및 거리 계산
        Target = FindClosetTarget(this, Managers.Object.MainCharacters.ToList<Creature>());

        //Phase1
        if (Hp > MaxHp / 2)
        {
            int rnd = Random.Range(0, 3);
            //SkillA
            if (rnd == 0)
            {
                _currentSkill = EBossSkill.SkillA;
            }
            //SkillB
            else if (rnd == 1)
            {
                _currentSkill = EBossSkill.SkillB;
            }
            //SkillC
            else if (rnd == 2)
            {
                _currentSkill = EBossSkill.SkillC;
            }
        }
        //Phase2
        else
        {
            _currentSkill = EBossSkill.SkillC;
            //SkillC continuously
        }

        BossState = EBossState.Skill;
    }

    protected override void UpdateSkill()
    {
        if (CreatureState == ECreatureState.Dead) return;
        if (_coWait != null) return;

        sequence.Kill();
        sequence = null;
        sequence = DOTween.Sequence();

        switch (_currentSkill)
        {
            case EBossSkill.SkillA:
                SkillA();
                break;
            case EBossSkill.SkillB:
                SkillB();
                break;
            case EBossSkill.SkillC:
                SkillC();
                break;
            default:
                break;
        }

        sequence.Play();
        float delay = 0;
        delay = sequence.Duration();
        StartWait(delay);
    }

    //Spawn Pooter
    public void SkillA()
    {
        sequence.Append(DOTween.To(() => 0f, x => Bottom.sprite = Managers.Resource.Load<Sprite>("boss_021_gurdyjr_6"), 0f, 0f));
        sequence.Append(transform.DOShakeScale(1, 0.1f, 10, 90, false));
        sequence.Insert(0.5f, DOTween.To(() => 0f, x => Bottom.sprite = Managers.Resource.Load<Sprite>("boss_021_gurdyjr_2"), 0f, 0f));
        sequence.Insert(0.5f, DOTween.To(() => 0f, x => SpawnPooter(), 0f, 0f));
        sequence.Append(DOTween.To(() => 0f, x => Bottom.sprite = Managers.Resource.Load<Sprite>("boss_021_gurdyjr_8"), 0f, 0f));
        sequence.Append(transform.DOShakeScale(0.5f, 0.1f, 10, 90, false));
        sequence.OnComplete(() => { BossState = EBossState.Idle; _currentSkill = EBossSkill.Normal; });
    }

    //Jump and generate 8 projectile
    public void SkillB()
    {
        AudioClip audioClip = Managers.Resource.Load<AudioClip>("monster roar");
        Managers.Sound.PlaySFX(audioClip, 0.5f);

        sequence.Append(DOTween.To(() => 0f, x => x = 1, 0f, 0.5f));
        sequence.Append(transform.DOShakeScale(1, 0.1f, 10, 90, false));
        sequence.Join(transform.DOJump(transform.position, 3, 1, 0.5f));
        sequence.Insert(0.7f, DOTween.To(() => 0f, x => Bottom.sprite = Managers.Resource.Load<Sprite>("boss_021_gurdyjr_4"), 0f, 0f));
        sequence.Insert(0.9f, DOTween.To(() => 0f, x => Bottom.sprite = Managers.Resource.Load<Sprite>("boss_021_gurdyjr_8"), 0f, 0f));
        sequence.Insert(0.9f, DOTween.To(() => 0f, x => Managers.Sound.PlaySFX(Managers.Resource.Load<AudioClip>("forest boss stomp"), 0.1f), 0f, 0f));
        sequence.Insert(0.95f, DOTween.To(() => 0f, x => Generate8Projectil(), 0f, 0f));
        sequence.Append(transform.DOShakeScale(0.5f, 0.1f, 10, 90, false));
        sequence.OnComplete(() => { BossState = EBossState.Idle; _currentSkill = EBossSkill.Normal; });
    }

    //charge attack
    public void SkillC()
    {
        sequence.Append(transform.DOShakeScale(3f, 0.1f, 10, 90, false));
        sequence.Join(DOTween.To(() => 0f, x => Bottom.sprite = Managers.Resource.Load<Sprite>("boss_021_gurdyjr_11"), 0f, 0f));
        sequence.Join(DOTween.To(() => 0f, x => ChargeAttackt(), 0f, 0f));
        sequence.Append(DOTween.To(() => 0f, x => _vel = Vector2.zero, 0f, 0.5f));
        sequence.Join(DOTween.To(() => 0f, x => Bottom.sprite = Managers.Resource.Load<Sprite>("boss_021_gurdyjr_8"), 0f, 0f));
        sequence.OnComplete(() => { BossState = EBossState.Idle; _currentSkill = EBossSkill.Normal;});
    }

    public void SpawnPooter()
    {
        Transform parent = transform.parent;
        Managers.Object.Spawn<Monster>(transform.localPosition - new Vector3(0, 0.5f, 0), 10001, "Pooter", parent);
    }

    public void Generate8Projectil()
    {
        Vector2 dV = Vector2.right;
        for (int i = 0; i < 8; i++)
        {
            GenerateProjectile(VectorRotation2D(dV, 360 / 8 * i), false, true);
        }
    }

    public void ChargeAttackt()
    {
        _vel = (Target.transform.position - transform.position).normalized * Speed;
    }

    public void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Projectile")
        {
            return;
        }
        if (_currentSkill == EBossSkill.SkillC)
        {
            _vel = Vector3.Reflect(_previousVelocity, collision.GetContact(0).normal);
        }
    }

    public override void OnDead()
    {
        sequence.Kill();
        sequence = null;
        base.OnDead();
    }

    private void OnDestroy()
    {
        sequence.Kill();
        sequence = null;
    }

}
