using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utility;
using static Define;

public class Boss_DukeOfFlies : Boss
{
    private void Awake()
    {
        Init();
        StartCoroutine(CoUpdateAI());
    }

    void Update()
    {

    }

    public override void Init()
    {
        base.Init();
        MaxHp = 100f;
        Hp = 100f;
        BossType = EBossType.DukeOfFlies;
        BossState = EBossState.Idle;
        CreatureMoveState = ECreatureMoveState.Designated;

        _flickerTarget.Add(FindChildByName(transform, "Boss_DukeOfFlies_Sprite").GetComponent<SpriteRenderer>());

        Vector3[] dV = { new Vector3(1, 1), new Vector3(1, -1), new Vector3(-1, 1), new Vector3(-1, -1) };
        TargetPos = dV[UnityEngine.Random.Range(0, 3)];

        //Monster Boss 충돌 X
        LayerMask mask = 0;
        mask |= (1 << 7);
        GPCollider2D.excludeLayers = mask;
    }

    protected override void UpdateIdle()
    {
        if (Managers.Object.MainCharacters.Count == 0) return;

        //0. 스킬을 안쓸지 결정 50%
        if (Random.Range(0, 101) <= 50 && Managers.Object.Monsters.Count==0)
        {
            BossState = EBossState.Skill;
            _currentSkill = EBossSkill.SkillA;
        }

    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        ReflectTargetVecor(collision);
    }

    void OnTriggerEnter2D(Collider2D other)
    {

        ReflectTargetVecor(other);
    }

    private void OnAnimatorMove()
    {
        UpdateMove();
    }

    public void SpawnFlies()
    {
        for (int i = 0; i < 5; i++)
            Managers.Object.Spawn<Monster>(transform.position - new Vector3(0, 0.5f, 2), 0, "Fly");
    }
}
