using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;
using static Utility;

public class Monster : Creature
{
    //public HashSet<>

    private LineRenderer lr = null;
    public EMonsterType MonsterType { get; protected set; }

    public GameObject SFXSource { get; protected set; }
    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();

        CreatureSize = ECreatureSize.Middle;
        CreatureType = ECreatureType.Monster;
        Speed = 1.0f;
        Hp = 10.0f;
        Range = 3f;

        HeadSprite = new Sprite[]
       {
            Managers.Resource.Load<Sprite>("isaac_up"),
            Managers.Resource.Load<Sprite>("isaac_down"),
            Managers.Resource.Load<Sprite>("isaac_right"),
       };

#if UNITY_EDITOR
        //if (!_isFloating)
        //{
        //    lr = GetComponent<LineRenderer>();
        //    if (lr != null)
        //    {
        //        lr.startWidth = lr.endWidth = 0.05f;
        //        lr.material.color = Random.ColorHSV();
        //        lr.enabled = false;
        //    }

        //}
#endif
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
        //lr.enabled = true;
        ////점의 개수 
        //lr.positionCount = path.Count;

        //var from = path.First();
        //for (int i = 0; i < path.Count - 1; i++)
        //{
        //    var to = path[i + 1];
        //    lr.SetPosition(i, from);
        //    lr.SetPosition(i + 1, to);
        //    from = to;
        //}
    }

    protected override void UpdateIdle()
    {
        if (Managers.Object.MainCharacters.Count == 0) return;
    }

    protected override void UpdateMove()
    {
        if (_isFloating || CreatureMoveState == ECreatureMoveState.Designated)
        {
            UpdateMovementByDV();
        }
        else
        {
            Target = FindClosetTarget(this, Managers.Object.MainCharacters.ToList<Creature>());
            UpdateMovementByAstar();
        }
    }


    public enum EMonsterSkill
    {
        Normal,
        SkillA,
        SkillB,
        SkillC,
    }
    protected string[] _skillName = { "Normal", "SkillA", "SkillB", "SkillC" };
    protected EMonsterSkill _currentSkill = EMonsterSkill.Normal;

    protected override void UpdateSkill()
    {
        if (_coWait != null) return;
        AnimatorBottom.Play(_skillName[(int)_currentSkill], 0, 0);
        //Debug.Log(_skillName[(int)_currentSkill]);
        if (_skillName[(int)_currentSkill] != AnimatorBottom.GetCurrentAnimatorClipInfo(0)[0].clip.name)
            return;
        float delay = AnimatorBottom.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        StartWait(delay);
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
#if UNITY_EDITOR
        drawPath(path);
#endif
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

    protected override void UpdateExplosion()
    {
        Rigidbody.velocity = Vector3.zero;
        Collider2D[] hit = Physics2D.OverlapBoxAll(transform.position, new Vector2(Range, Range), 0);
        foreach (Collider2D collider in hit)
        {
            var temp = collider.gameObject;
            temp.GetComponent<Creature>()?.OnDamaged(this, ESkillType.Bomb);
        }

        AnimatorBottom.Play("Explosion");
    }

    protected override IEnumerator CoUpdateAI()
    {
        while (true)
        {
            switch (CreatureState)
            {
                case ECreatureState.Idle:
                    UpdateAITick = 0.0f;
                    UpdateIdle();
                    break;
                case ECreatureState.Skill:
                    UpdateAITick = 0.0f;
                    UpdateSkill();
                    break;
                case ECreatureState.Move:
                    UpdateAITick = 0.0f;
                    UpdateMove();
                    break;
                case ECreatureState.Dead:
                    UpdateAITick = 0.0f;
                    UpdateDead();
                    break;
                case ECreatureState.Explosion:
                    UpdateAITick = 1.0f;
                    UpdateExplosion();
                    break;
            }

            if (UpdateAITick > 0)
                yield return new WaitForSeconds(UpdateAITick);
            else
                yield return null;
        }

    }

    public override void OnDamaged(Creature owner, ESkillType skillType, string name = "")
    {
        base.OnDamaged(owner, skillType, name);
        StartCoroutine(CoFlicker());
    }

    //Total 1sec
    public IEnumerator CoFlicker()
    {
        //Change Sprite
        for (int i = 1; i <= 2; i++)
        {
            if (i % 2 == 0)
            {
                if (Head != null)
                    Head.color = new Color32(255, 255, 255, 255);
                Bottom.color = new Color32(255, 255, 255, 255);
            }
            else
            {
                if (Head != null)
                    Head.color = new Color32(255, 127, 127, 255);
                Bottom.color = new Color32(255, 127, 127, 255);
            }
            yield return new WaitForSeconds(0.05f);
        }
        //Change Sprite
        yield return null;
    }

    public override void OnDead()
    {

        if (CreatureState == ECreatureState.Dead) return;
        Rigidbody.velocity = Vector3.zero;
        CreatureState = ECreatureState.Dead;
        StopAllCoroutines();
        if (AnimatorBottom != null)
            AnimatorBottom.enabled = false;

        if (MonsterType == EMonsterType.BoomFly)
        {
            base.OnDead();
        }
        else
        {
            StartCoroutine(MonsterDeadAaim());
        }

    }

    IEnumerator MonsterDeadAaim()
    {
        if (Collider != null)
            Collider.enabled = false;

        float delay;

        //Dead Anim
        GameObject go = Managers.Resource.Instantiate("Monster_Dead_Effect");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        go.GetComponent<Animator>().Play("Monster_Dead_Effect");

        //Dead Sound
        AudioClip audioClip = Managers.Resource.Load<AudioClip>("death burst small 1");
        Managers.Sound.PlaySFX(audioClip, 0.1f);

        //일체형인 경우
        if (transform.GetComponent<SpriteRenderer>() != null)
        {
            transform.GetComponent<SpriteRenderer>().enabled = false;
            delay = 0.1f;
        }
        // 상하체 분리형의 경우
        else
        {
            FindChildByNameContain(transform, "Head").GetComponent<SpriteRenderer>().enabled = false;
            FindChildByNameContain(transform, "Bottom").GetComponent<SpriteRenderer>().enabled = false;

            delay = 0.2f;
        }

        yield return new WaitForSeconds(delay);
        base.OnDead();
    }
}
