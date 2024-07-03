using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Monster : Creature
{
    //public HashSet<>
    private Creature _target;
    public Creature Target
    {
        get => _target;
        set { _target = value; }

    }

    private LineRenderer lr;

    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        //TODO 몬스터 종류에 따른 스프라이트 불러오기
        base.Init();
        HeadSprite = new Sprite[]
       {
            Managers.Resource.Load<Sprite>("isaac_up"),
            Managers.Resource.Load<Sprite>("isaac_down"),
            Managers.Resource.Load<Sprite>("isaac_right"),
       };

        lr = GetComponent<LineRenderer>();

        lr.startWidth = lr.endWidth = 0.1f;
        lr.material.color = Color.blue;
        lr.enabled = false;

        //StartCoroutine(CoUpdateTarget());

    }

    void Update()
    {
        #region Attack

        #endregion

        #region Move
        UpdateMovement();
        #endregion
    }

    void drawPath(List<Vector3Int> path)
    {
        lr.enabled = true;
        //점의 개수
        lr.positionCount = path.Count;

        var from = path.First();
        for (int i=0; i<path.Count-1; i++)
        {
            var to = path[i+1];
            lr.SetPosition(i, from);
            lr.SetPosition(i+1, to);
            from = to;
        }
    }

    private void UpdateMovement()
    {
        Target = FindClosetTarget(this, Managers.Object.MainCharacters.ToList<Creature>());


        Vector3Int startCellPos = Managers.Map.WorldToCell(transform.position);
        Vector3Int destCellPos = Managers.Map.WorldToCell(Target.transform.position);
        List<Vector3Int> path = Managers.Map.FindPath(this, startCellPos, destCellPos);

        Debug.Log(path);
        drawPath(path);
    }

    private void UpdateTarget()
    {
        Target = FindClosetTarget(this, Managers.Object.MainCharacters.ToList<Creature>());
    }

    private IEnumerator CoUpdateTarget()
    {
        //TODO 하드코딩! 수정
        WaitForSeconds wait = new WaitForSeconds(0.5f);
        yield return wait;

        while (true)
        {
            UpdateTarget();
            yield return wait;
        }
    }

    private Creature FindClosetTarget(Creature src, List<Creature> targets)
    {
        float minDistance = float.MaxValue;
        Creature closestTarget = null;

        foreach (Creature target in targets)
        {
            float dist = (src.transform.position - target.transform.position).sqrMagnitude;
            if (target == null || target.isActiveAndEnabled == false)
                continue;

            if (dist < minDistance)
            {
                minDistance = dist;
                closestTarget = target;
            }
        }

        return closestTarget;
    }
}
