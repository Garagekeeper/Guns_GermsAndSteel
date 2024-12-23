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
        Collider2D[] hit = Physics2D.OverlapBoxAll(transform.position, new Vector2(Range, Range), 0);
        foreach (Collider2D collider in hit)
        {
            var temp = collider.gameObject;
            //TODO 폭탄의 경우 여러 물체와 상호작용한다
            //모든 Object의 조상을 만들어서 Object 타입을 통해 적절한 상호작용으로 교체하자.
            temp.GetComponent<Creature>()?.OnDamaged(this, ESkillType.Bomb);
        }
        MonsterState = EMonsterState.Dead;
        Destroy(gameObject);
    }
}
