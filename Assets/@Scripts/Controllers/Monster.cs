using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class Monster : Creature
{
    //public HashSet<>
    protected bool _isFloating = false;

    private LineRenderer lr;

    //나중에 creature 타음으로 통일하자.... 넘 반복됨...
    public EMonsterType MonsterType { get; protected set; }
    protected EMonsterState _monsterState;
    public virtual EMonsterState MonsterState
    {
        get { return _monsterState; }
        set
        {
            if (_monsterState != value)
            {
                _monsterState = value;
                switch (value)
                {
                    case EMonsterState.None:
                        break;
                    case EMonsterState.Idle:
                        UpdateAITick = 0.5f;
                        break;
                    case EMonsterState.Skill:
                        UpdateAITick = 0.0f;
                        break;
                    case EMonsterState.Move:
                        UpdateAITick = 0.0f;
                        break;
                }
            }
        }
    }

    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();

        //TODO 하드코딩 수정하기
        CreatureType = ECreatureType.Monster;
        Speed = 1.0f;
        Hp = 10.0f;

        ////Monster끼리는 충돌 X
        //LayerMask mask = 0;
        //mask |= (1 << 7);
        //Collider.excludeLayers = mask;

        //TODO 몬스터 종류에 따른 스프라이트 불러오기
        HeadSprite = new Sprite[]
       {
            Managers.Resource.Load<Sprite>("isaac_up"),
            Managers.Resource.Load<Sprite>("isaac_down"),
            Managers.Resource.Load<Sprite>("isaac_right"),
       };


        //if (!_isFloating)
        //{
        //    lr = GetComponent<LineRenderer>();

        //    lr.startWidth = lr.endWidth = 0.05f;
        //    lr.material.color = Random.ColorHSV();
        //    lr.enabled = false;

        //    //StartCoroutine(CoUpdateTarget());
        //}
    }

    void Update()
    {
        #region Attack

        #endregion

        #region Move
        #endregion
    }

    void drawPath(List<Vector3Int> path)
    {
        lr.enabled = true;
        //점의 개수 
        lr.positionCount = path.Count;

        var from = path.First();
        for (int i = 0; i < path.Count - 1; i++)
        {
            var to = path[i + 1];
            lr.SetPosition(i, from);
            lr.SetPosition(i + 1, to);
            from = to;
        }
    }

    protected override void UpdateIdle()
    {

    }

    protected override void UpdateMove()
    {
        if (_isFloating || CreatureMoveState == ECreatureMoveState.Designated )
        {
            UpdateMovementByDV();
        }
        else
        {
            Target = FindClosetTarget(this, Managers.Object.MainCharacters.ToList<Creature>());
            UpdateMovementByAstar();
        }
    }


    // Based on A* path finding
    public void UpdateMovementByAstar()
    {
        Target = FindClosetTarget(this, Managers.Object.MainCharacters.ToList<Creature>());


        Vector3Int startCellPos = Managers.Map.WorldToCell(transform.position);
        Vector3Int destCellPos = Managers.Map.WorldToCell(Target.transform.position);
        List<Vector3Int> path = Managers.Map.FindPath(this, startCellPos, destCellPos);

        if (path.Count < 2)
            return;

        drawPath(path);

        Vector3 dir = path[1] - path[0];
        Vector3 normalizedDir = dir.normalized;

        Vector2 vel = Rigidbody.velocity;
        vel.x = normalizedDir.x * Speed;
        vel.y = normalizedDir.y * Speed;

        Rigidbody.velocity = vel;
    }

    public void UpdateMovementByDV()
    {
        if (Target)
            Rigidbody.velocity = (Target.transform.position - transform.position).normalized * Speed;
        else
            Rigidbody.velocity = TargetPos.normalized * Speed;
    }

    protected override IEnumerator CoUpdateAI()
    {
        while (true)
        {
            switch (MonsterState)
            {
                case EMonsterState.Idle:
                    UpdateIdle();
                    break;
                case EMonsterState.Skill:
                    UpdateSkill();
                    break;
                case EMonsterState.Move:
                    UpdateMove();
                    break;
                case EMonsterState.Dead:
                    break;
                case EMonsterState.Explosion:
                    break;
            }

            if (UpdateAITick > 0)
                yield return new WaitForSeconds(UpdateAITick);
            else
                yield return null;
        }

    }


}
