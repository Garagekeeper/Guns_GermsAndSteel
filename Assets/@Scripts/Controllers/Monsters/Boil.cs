using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using DG.Tweening;
using System.Linq;

public class Boil : Monster
{
    private int _growSize;
    Sequence sequence;

    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        MonsterType = EMonsterType.Boil;
        CreatureState = ECreatureState.Idle;
        Hp = 20.0f;
        MaxHp = 20.0f;
        AttackDamage = 2f;
        _growSize = 0;
        _isFloating = false;
    }

    protected override void UpdateIdle()
    {
        Target = FindClosetTarget(this, Managers.Object.MainCharacters.ToList<Creature>());
        base.UpdateIdle();
        if (_growSize >= 9)
        {
            CreatureState = ECreatureState.Skill;
            _currentSkill = EMonsterSkill.SkillA;
        }
        else
        {
            _growSize = Mathf.Min(9, _growSize + 1);
            UpdateAITick = 1.0f;
            ChangeBoilSprite();
        }
    }

    protected override void UpdateSkill()
    {
        UpdateAITick = 1.0f;
        if (CreatureState == ECreatureState.Dead) return;
        if (_coWait != null) return;

        float delay = 0;

        sequence.Kill();
        sequence = null;
        sequence = DOTween.Sequence();
        sequence.Append(transform.DOShakeScale(1, 0.1f, 10, 90, false));
        sequence.Append(transform.DOShakeScale(0.1f, new Vector3(0, 0.2f, 0), 10, 90, false));
        sequence.Append(DOTween.To(() => 0f, x => SkillA(), 0f, 0f));
        sequence.Join(DOTween.To(() => 0f, x => x = 0, 0f, 0.5f));
        sequence.OnComplete(() => CreatureState = ECreatureState.Idle);
        sequence.Play();

        delay = sequence.Duration();

        StartWait(delay);
    }

    public override void OnDamaged(Creature owner, ESkillType skillType, string name = "")
    {
        base.OnDamaged(owner, skillType, name);
        if (CreatureState == ECreatureState.Dead) return;

        _growSize = Mathf.Clamp((int)(Hp / 2), 0, 9);
        ChangeBoilSprite();

    }

    public void ChangeBoilSprite()
    {
        Bottom.sprite = Managers.Resource.Load<Sprite>("monster_087_boil_" + (9 - _growSize));
    }

    public void SkillA()
    {
        for (int i = 0; i < 4; i++)
        {
            GenerateProjectile((Target.transform.position - transform.position).normalized, true, true);
            //GenerateProjectile(Vector2.up, false, true);
        }
    }
}
