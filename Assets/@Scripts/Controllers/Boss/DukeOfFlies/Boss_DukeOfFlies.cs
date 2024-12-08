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
    }

    void Update()
    {
        Rigidbody.velocity = TargetPos.normalized * Speed;
    }

    public override void Init()
    {
        base.Init();
        MaxHp = 100f;
        Hp = 100f;
        BossType = EBossType.DukeOfFlies;
        BossState = EBossState.Idle;

        Vector3[] dV = { new Vector3(1, 1), new Vector3(1, -1), new Vector3(-1, 1), new Vector3(-1, -1) };
        TargetPos = dV[UnityEngine.Random.Range(0, 3)];
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ReflectTargetVecor(collision);
    }

    void OnTriggerEnter2D(Collider2D other)
    {

        ReflectTargetVecor(other);
    }

    public void GenerateMonster()
    {

    }

}
