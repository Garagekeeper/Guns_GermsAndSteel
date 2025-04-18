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
        Poop,
        Rock,
        Urn,
    }

    private float _hp;
    private Collider2D Collider;
    private ObstacleTypes _obstacleType;
    public ObstacleTypes ObstacleType
    {
        get { return _obstacleType; }
        set { _obstacleType = value; }
    }

    private void Awake()
    {

    }

    public void Init(string type, int index = 1)
    {
        Collider = GetComponent<CircleCollider2D>();
        ObstacleType = ObstacleTypes.None;
        if (type == "Spike") ObstacleType = ObstacleTypes.Spike;
        if (type == "Fire")
        {
            ObstacleType = ObstacleTypes.Fire;
            _hp = 10.0f;
        }

        if (type == "Poop")
        {
            ObstacleType = ObstacleTypes.Poop;
            _hp = 4.0f;
        }

        if (type == "Rock")
        {
            ObstacleType = ObstacleTypes.Rock;
            GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>("rocks_basement_normal_" + index);
            Collider = GetComponent<BoxCollider2D>();
        }

        if (type == "Urn")
        {
            GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>("rocks_basement_urn_" + index);
            ObstacleType = ObstacleTypes.Urn;
        }
    }

    public void SubsFireHp(Creature owner)
    {
        _hp = Mathf.Max(0, _hp - owner.AttackDamage);
        if (_hp <= 0f)
        {
            Destroy(Collider);
            Destroy(this);
            GetComponent<SpriteRenderer>().enabled = false;
            return;
        }
    }

    public void PoopOnHit(bool byBomb = false)
    {
        if (byBomb)
            _hp = 0;

        _hp = Mathf.Max(0, _hp - 1);
        int index = 4 - (int)_hp;
        GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>("grid_poop_" + index);
        if (_hp <= 0f)
        {
            Destroy(GetComponent<Rigidbody2D>());
            Destroy(Collider);
            Destroy(this);
            
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Creature creature = collision.gameObject.GetComponent<Creature>();
        if (creature != null)
        {
            if (creature.IsFloating == false && ObstacleType == ObstacleTypes.Spike)
                creature.OnDamaged(null, ESkillType.Spike);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Creature creature = collision.gameObject.GetComponent<Creature>();
        if (creature != null)
        {
            if (ObstacleType == ObstacleTypes.Fire)
                creature.OnDamaged(null, ESkillType.Fire);
        }
    }
}
