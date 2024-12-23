using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using UnityEngine.Rendering.Universal;
using Unity.Burst.CompilerServices;

public class BoomFly : Monster
{
    void Start()
    {
        Init();
        StartCoroutine(CoUpdateAI());
    }

    public override void Init()
    {
        base.Init();
        Vector3[] dV = { new Vector3(1, 1), new Vector3(1, -1), new Vector3(-1, 1), new Vector3(-1, -1) };
        MonsterType = EMonsterType.Fly;
        CreatureMoveState = ECreatureMoveState.Designated;
        _isFloating = true;
        Speed = 3.0f;

        TargetPos = dV[Random.Range(0, 4)];
        MonsterState = EMonsterState.Move;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        ReflectTargetVecor(collision);
    }

    public override void OnDead()
    {
        MonsterState = EMonsterState.Explosion;
    }

    //Animation Event
    public void Destroyprefab()
    {
        Destroy(gameObject);
    }
}
