using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;
using static Utility;

public class Boss_Mom : Boss
{
    Transform _legTransForm;
    CapsuleCollider2D _legCollider;
    Transform[] _doorTransform = new Transform[4];
    private DG.Tweening.Sequence sequence;
    float _timer = 0;
    bool _bR = false;
    bool _bD = false;
    bool _bL = false;
    bool _bU = false;



    private void Awake()
    {
        Init();
        StartCoroutine(CoUpdateAI());
        Managers.Map.CurrentRoom.Doors.SetActive(false);

    }

    public override void Init()
    {
        CreatureType = ECreatureType.Boss;
        Rigidbody = transform.GetComponent<Rigidbody2D>();
        AnimatorBottom = transform.GetComponent<Animator>();
        Bottom = FindChildByName(transform, transform.gameObject.name.Replace("(Clone)", "").Trim() + "_Sprite")?.GetComponent<SpriteRenderer>();
        Speed = 3f;

#if UNITY_EDITOR
        Managers.UI.PlayingUI.BossHpActive(true);
#endif 

        Hp = 645.0f;
        MaxHp = 645.0f;
        BossType = EBossType.Mom;
        BossState = EBossState.Idle;
        CreatureMoveState = ECreatureMoveState.TargetCreature;
        AttackDamage = 5f;
        _legTransForm = FindChildByName(transform, "Boss_Mom_Leg");
        _legCollider = FindChildByName(_legTransForm, "Boss_Mom_Leg_Calf").GetComponent<CapsuleCollider2D>();

        _doorTransform[0] = FindChildByName(transform, "Boss_Mom_Door_Right");
        _doorTransform[1] = FindChildByName(transform, "Boss_Mom_Door_Down");
        _doorTransform[2] = FindChildByName(transform, "Boss_Mom_Door_Left");
        _doorTransform[3] = FindChildByName(transform, "Boss_Mom_Door_Up");


        _flickerTarget.Add(FindChildByName(transform, "Boss_Mom_Leg_Thigh").GetComponent<SpriteRenderer>());
        _flickerTarget.Add(FindChildByName(transform, "Boss_Mom_Leg_Calf").GetComponent<SpriteRenderer>());

        _flickerTarget.Add(FindChildByName(transform, "Boss_Mom_Door_Right_Spawn_Object").GetComponent<SpriteRenderer>());
        _flickerTarget.Add(FindChildByName(transform, "Boss_Mom_Door_Right_Hand").GetComponent<SpriteRenderer>());

        _flickerTarget.Add(FindChildByName(transform, "Boss_Mom_Door_Down_Spawn_Object").GetComponent<SpriteRenderer>());
        _flickerTarget.Add(FindChildByName(transform, "Boss_Mom_Door_Down_Hand").GetComponent<SpriteRenderer>());

        _flickerTarget.Add(FindChildByName(transform, "Boss_Mom_Door_Left_Spawn_Object").GetComponent<SpriteRenderer>());
        _flickerTarget.Add(FindChildByName(transform, "Boss_Mom_Door_Left_Hand").GetComponent<SpriteRenderer>());

        _flickerTarget.Add(FindChildByName(transform, "Boss_Mom_Door_Up_Spawn_Object").GetComponent<SpriteRenderer>());
        _flickerTarget.Add(FindChildByName(transform, "Boss_Mom_Door_Up_Hand").GetComponent<SpriteRenderer>());
        LayerMask mask = 0;
        //player
        mask |= (1 << 6);
        _doorTransform[0].GetComponent<BoxCollider2D>().includeLayers = mask;
        _doorTransform[1].GetComponent<BoxCollider2D>().includeLayers = mask;
        _doorTransform[2].GetComponent<BoxCollider2D>().includeLayers = mask;
        _doorTransform[3].GetComponent<BoxCollider2D>().includeLayers = mask;
        mask = 0;

        mask |= (1 << 15);
        _legCollider.excludeLayers = mask;


    }

    private void Update()
    {
        if (_bR || _bD || _bL || _bU) _timer = MathF.Max(_timer + Time.deltaTime, 3f);
        else _timer = 0;
    }

    protected override void UpdateIdle()
    {
        UpdateAITick = 0f;
        // 0. 현재 맵에 소환된 몬스터가 있으면 종료, player가 없으면 종료
        if (Managers.Object.MainCharacters.Count == 0) return;
        if (Managers.Object.Monsters.Count > 0) return;
        int randValue = UnityEngine.Random.Range(0, 100);
        _currentSkill = EBossSkill.Normal;

        // 1. 플레이어가 문에 가까이 있는경우 SkillC 실행
        if (_timer >= 1.5f)
        {
            _currentSkill = EBossSkill.SkillC;
        }
        // 2.30프로의 확률로 발찍기
        else if (randValue < 50f)
        {
            Target = FindClosetTarget(this, Managers.Object.MainCharacters.ToList<Creature>());
            _currentSkill = EBossSkill.SkillA;
        }
        // 70프로 확률로 SkillB
        else
        {
            _currentSkill = EBossSkill.SkillB;
        }

        BossState = EBossState.Skill;

    }

    protected override void UpdateSkill()
    {
        UpdateAITick = 0.5f;
        switch (_currentSkill)
        {
            case EBossSkill.SkillA:
                base.UpdateSkill(); break;
            case EBossSkill.SkillB:
                SkillB(); break;
            case EBossSkill.SkillC:
                SkillC(); break;
        }
    }

    // 1) 4방향 문에 object 소환
    // 2) 몬스터 소환 (maximum 3)
    public void SkillB()
    {
        if (_coWait != null) return;
        Transform parent = transform.parent;

        AudioClip audioClip = Managers.Resource.Load<AudioClip>($"mom{UnityEngine.Random.Range(1, 4)}");
        Managers.Sound.PlaySFX(audioClip, 0.2f);

        List<int> numbers = new() { 0, 1, 2, 3 };
        Stack<Transform> stack = new();
        String[] spawnObjectName = { "boss_054_mom_4", "boss_054_mom_5", "boss_054_mom_6", "boss_054_mom_7", };
        Sprite open = Managers.Resource.Load<Sprite>("boss_054_mom_3");
        Shuffle(numbers);
        float delay = 0f;


        for (int i = 0; i < 3; i++)
        {
            var tf = _doorTransform[numbers[i]];
            Transform spawnObject = tf.GetChild(0);
            Sprite spawnObjectSprite = Managers.Resource.Load<Sprite>(spawnObjectName[numbers[i]]);
            Vector3 spawnDir = (Vector3)VectorRotation2D(new Vector2(2, -2), tf.eulerAngles.z) + tf.localPosition;
            Vector2 shakeDir = VectorRotation2D(new Vector2(0.1f, 0), tf.eulerAngles.z);

            stack.Push(tf);
            sequence = DOTween.Sequence();
            //로컬 기준 좌우
            sequence.Append(tf.DOShakePosition(1f, shakeDir, 10, 90, false, false))
                //상하 크기
                .Join(tf.DOScaleY(1.2f, 0.5f).SetLoops(2, LoopType.Yoyo))
                .Append(DOTween.To(() => 0f, x =>
                {
                    //door sprite 변환, 
                    tf.GetComponent<SpriteRenderer>().sprite = open;
                    //문에서 나타나는 object sprite 변환 및 활성화
                    spawnObject.GetComponent<SpriteRenderer>().sprite = spawnObjectSprite;
                    spawnObject.gameObject.SetActive(true);
                    //몬스터 소환
                    int id = Managers.Game.SelectRandomMonster();
                    Managers.Object.Spawn<Monster>(transform.localPosition + spawnDir, id, parent);
                }, 0f, 0f))
                //해당 애니메이션에서의 딜레이
                .AppendInterval(2f)
                //다른 애니메이션 사이의 딜레이
                .SetDelay(delay)
                .OnComplete(() => { EndSkillB(stack); });

            delay += 0.5f; // 다음 Sequence는 0.5초 후 실행
            //Spawn Object
            //Spawn Monsters
        }

        //while (stack.Count > 0)
        //{

        //}
        StartWait(5f);
    }

    public void EndSkillB(Stack<Transform> stack)
    {
        if (BossState == EBossState.Dead) return;
        Sprite closed = Managers.Resource.Load<Sprite>("boss_054_mom_10");
        float delay = 0f;

        while (stack.Count > 0)
        {
            sequence = DOTween.Sequence();
            var tf = stack.Pop();
            Vector2 shakeDir = VectorRotation2D(new Vector2(0.1f, 0), tf.eulerAngles.z);

            sequence.Append(tf.DOShakePosition(1f, shakeDir, 10, 90, false, false))
                .Join(tf.DOScaleY(1.2f, 0.5f).SetLoops(2, LoopType.Yoyo))
                .Append(DOTween.To(() => 0f, x =>
                {
                    //door sprite 변환, 
                    tf.GetComponent<SpriteRenderer>().sprite = closed;
                    //문에서 나타나는 object비활성화
                    tf.GetChild(0).gameObject.SetActive(false);
                }, 0f, 0f))
                .SetDelay(delay)
                .OnComplete(() => { BossState = EBossState.Idle; });
            delay += 0.5f;
        }

    }

    // 플레이어가 문에 가까이 있으면 손을 소환
    public void SkillC()
    {
        if (_coWait != null) return;

        AudioClip audioClip = Managers.Resource.Load<AudioClip>($"mom{UnityEngine.Random.Range(1, 4)}");
        Managers.Sound.PlaySFX(audioClip, 0.2f);

        sequence = DOTween.Sequence();
        Transform doorTransform = null;
        Transform handTransform = null; ;

        if (_bR)
        {
            doorTransform = _doorTransform[0];
            handTransform = doorTransform.GetChild(1);
        }
        else if (_bD)
        {
            doorTransform = _doorTransform[1];
            handTransform = doorTransform.GetChild(1);
        }
        else if (_bL)
        {
            doorTransform = _doorTransform[2];
            handTransform = doorTransform.GetChild(1);
        }
        else if (_bU)
        {
            doorTransform = _doorTransform[3];
            handTransform = doorTransform.GetChild(1);
        }

        if (doorTransform == null || handTransform == null) return;

        Vector2 shakeDir = VectorRotation2D(new Vector2(0.1f, 0), doorTransform.eulerAngles.z);

        sequence.Append(doorTransform.DOShakePosition(1f, shakeDir, 10, 90, false, false))
               //상하 크기
               .Join(doorTransform.DOScaleY(1.2f, 0.5f).SetLoops(2, LoopType.Yoyo))
               .Append(DOTween.To(() => 0f, x =>
               {
                   handTransform.gameObject.SetActive(true);
                   AudioClip audioClip = Managers.Resource.Load<AudioClip>("evil laugh");
                   Managers.Sound.PlaySFX(audioClip, 0.2f);
               }, 0f, 0f))
               //해당 애니메이션에서의 딜레이
               .AppendInterval(2f)
               .Append(DOTween.To(() => 0f, x =>
               {
                   handTransform.gameObject.SetActive(false);
                   _bR = _bD = _bL = _bU = false;
               }, 0f, 0f))
               .OnComplete(() => { _currentSkill = EBossSkill.Normal; BossState = EBossState.Idle; });

        StartWait(5f);

    }


    // 발찍기할 때 움직임
    protected override void UpdateMove()
    {
        if (BossState == EBossState.Dead) return;
        UpdateAITick = 0f;

        if (CreatureMoveState == ECreatureMoveState.TargetCreature && BossState == EBossState.Move)
            _legTransForm.position = Vector3.Lerp(_legTransForm.position, Target.transform.position, Time.deltaTime * 2f);

    }

    public override void OnDead()
    {
        BossState = EBossState.Dead;
        // dotween anim kill
        sequence.Kill(true);
        sequence = null;

        //audio
        AudioClip audioClip = Managers.Resource.Load<AudioClip>("vox death");
        Managers.Sound.PlaySFX(audioClip, 0.3f);

        Managers.Object.Despawn(this);
        Managers.UI.PlayingUI.BossHpActive(false);
        Managers.Map.CurrentRoom.Doors.SetActive(true);

        CreatureState = ECreatureState.Dead;
        StopAllCoroutines();
        Managers.Game.RoomConditionCheck();

    }

    private void OnDestroy()
    {
        sequence.Kill(true);
        sequence = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 0. Idle일때만 타이머를 늘린다.
        if (BossState != EBossState.Idle) return;

        // 1. 어느 구역인지 확인


        foreach (Transform tf in _doorTransform)
        {
            // Bounds.Intersects()를 사용하여 두 Collider의 경계가 겹치는지 확인
            if (tf.GetComponent<BoxCollider2D>().bounds.Intersects(collision.bounds))
            {
                var zoneName = tf.name;
                if (zoneName.Contains("Right")) _bR = true;
                if (zoneName.Contains("Down")) _bD = true;
                if (zoneName.Contains("Left")) _bL = true;
                if (zoneName.Contains("Up")) _bU = true;
                return;
            }
        }
    }

    public override void OnExplode(Creature owner)
    {
        AudioClip audioClip = Managers.Resource.Load<AudioClip>($"hurt {UnityEngine.Random.Range(1, 4)}");
        Managers.Sound.PlaySFX(audioClip, 0.3f);
        base.OnExplode(owner);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _bR = _bD = _bL = _bU = false;
    }

    public void PlayEvilLaugh()
    {
        AudioClip audioClip = Managers.Resource.Load<AudioClip>("forest boss stomp");
        Managers.Sound.PlaySFX(audioClip, 0.3f);
    }

    public void PlayStampSound()
    {
        AudioClip audioClip = Managers.Resource.Load<AudioClip>("grunt");
        Managers.Sound.PlaySFX(audioClip, 0.3f);
        audioClip = Managers.Resource.Load<AudioClip>("forest boss stomp");
        Managers.Sound.PlaySFX(audioClip, 0.3f);
    }
}
