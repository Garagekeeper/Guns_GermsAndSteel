using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Obstacle : BaseObject
{
    public enum ObstacleTypes
    {
        None,
        Spike,
        Fire,
    }

    private float _hp;
    private CircleCollider2D _circleCollider;
    private ObstacleTypes _obstacleType;
    public ObstacleTypes ObstacleType
    {
        get { return _obstacleType; }
        set { _obstacleType = value; }
    }

    private void Awake()
    {

    }

    public void Init(string type)
    {
        _circleCollider = GetComponent<CircleCollider2D>();
        ObstacleType = ObstacleTypes.None;
        if (type == "Spike") ObstacleType = ObstacleTypes.Spike;
        if (type == "Fire")
        {
            ObstacleType = ObstacleTypes.Fire;
            _hp = 10.0f;
        }
    }

    public void SubsFireHp(Creature owner)
    {
        _hp = Mathf.Max(0, _hp - owner.AttackDamage);
        if (_hp <= 0f)
        {
            Destroy(_circleCollider);
            Destroy(this);
            GetComponent<SpriteRenderer>().enabled = false;
            return;
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Creature creature = collision.gameObject.GetComponent<Creature>();
        if (creature != null)
        {
            if (creature.IsFloating != true)
                creature.OnDamaged(null, ESkillType.Spike);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Creature creature = collision.gameObject.GetComponent<Creature>();
        if (creature != null)
        {
            if (creature.IsFloating != true)
                creature.OnDamaged(null, ESkillType.Fire);
        }
    }
}
