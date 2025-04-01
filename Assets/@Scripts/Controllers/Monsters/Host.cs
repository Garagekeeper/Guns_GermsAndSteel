using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using static Define;
using static Utility;
using UnityEngine.Rendering;

public class Host : Monster
{
    enum EHostState
    {
        PopUp,
        Down,
        Shrunk,
    }

    private EHostState HostState { get; set; }
    private float _popUpDealay = 0f;
    //private float _popUpDealay = 0f;
    Vector2 projectileDir = Vector2.zero;
    Sequence sequence;

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        StartCoroutine(CoUpdateAI());
    }

    public override void Init()
    {
        base.Init();
        Hp = 14.0f;
        MaxHp = 14.0f;
        CreatureType = ECreatureType.Monster;
        CreatureState = ECreatureState.Idle;
        MonsterType = EMonsterType.Host;
        HostState = EHostState.Down;
        AttackDamage = 2f;
    }

    protected override void UpdateIdle()
    {
        if (Managers.Object.MainCharacters.Count == 0) return;
        if (CreatureState == ECreatureState.Dead) return;
        _popUpDealay = Mathf.Max(_popUpDealay - Time.deltaTime, 0);
        if (_popUpDealay > 3.5f)
        {
            HostState = EHostState.Shrunk;
            Bottom.sprite = Managers.Resource.Load<Sprite>("monster_122_host_2");
        }
        else if (_popUpDealay <= 3.5f)
        {
            HostState = EHostState.Down;
            Bottom.sprite = Managers.Resource.Load<Sprite>("monster_122_host_0");
        }

        if (_popUpDealay > 0)
        {
            return;
        }


        //0. 가장 가까운 목표 탐생
        Target = FindClosetTarget(this, Managers.Object.MainCharacters.ToList<Creature>());
        //1. 목표와의 거리 계산
        Vector2 dir = Target.transform.position - transform.position;
        Vector2 normalizedDir = dir.normalized;
        float distanceSqaure = dir.sqrMagnitude;

        //2-1
        //목표가 너무 먼 경우
        if (distanceSqaure > 9)
        {
            return;
        }
        //2-3
        //목표가 가까운 경우
        else
        {
            projectileDir = normalizedDir;
            CreatureState = ECreatureState.Skill;
        }

        //3. Shrunk 상태인 경우
    }

    protected override void UpdateSkill()
    {
        if (CreatureState == ECreatureState.Dead) return;
        if (_coWait != null) return;

        float delay = 0;

        sequence.Kill();
        sequence = null;
        sequence = DOTween.Sequence();
        sequence.Append(transform.DOShakeScale(1, new Vector3(0.1f, 0, 0), 10, 90, false));
        //DOTween.To(시작값(getter), setter, 결과값, float duration);
        sequence.Append(DOTween.To(() => 0f, x => Bottom.sprite = Managers.Resource.Load<Sprite>("monster_122_host_1"), 0f, 0f));
        sequence.Join(DOTween.To(() => 0f, x => projectileDir = (Target.transform.position - transform.position).normalized, 0f, 0f));
        sequence.Join(DOTween.To(() => 0f, x => HostState = EHostState.PopUp, 0f, 0f));
        sequence.Append(transform.DOShakeScale(0.3f, new Vector3(0, 0.1f, 0), 10, 90, false));
        sequence.Append(DOTween.To(() => 0f, x => HostSkill(), 0f, 0f));
        sequence.Append(DOTween.To(() => 0f, x => x = 0, 0f, 0.7f));
        sequence.Append(DOTween.To(() => 0f, x => Bottom.sprite = Managers.Resource.Load<Sprite>("monster_122_host_0"), 0f, 0f));
        sequence.Join(DOTween.To(() => 0f, x => HostState = EHostState.Down, 0f, 0f));
        sequence.OnComplete(() => CreatureState = ECreatureState.Idle);
        sequence.Play();

        delay = sequence.Duration();

        StartWait(delay);
    }

    public void HostSkill()
    {
        GenerateProjectile(projectileDir, false, true);
        GenerateProjectile(VectorRotation2D(projectileDir, 10f), false, true);
        GenerateProjectile(VectorRotation2D(projectileDir, -10f), false, true);
    }

    //객체가 파괴될때 진행중인 sequence를 종료한다.
    private void OnDestroy()
    {
        sequence.Kill();
        sequence = null;
    }

    public override void OnDead()
    {
        base.OnDead();
    }

    public override void OnDamaged(Creature owner, ESkillType skillType, string name = "")
    {
        if (HostState == EHostState.PopUp)
            base.OnDamaged(owner, skillType, name);
        else
        {
            _popUpDealay = Mathf.Min(_popUpDealay + 1f, 8f);
        }
    }

}
