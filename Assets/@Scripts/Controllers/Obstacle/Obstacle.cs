using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class Obstacle : BaseObject, IExplodable
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

    private int _sacrificeCnt;
    bool _isInitDone = false;
    public AudioSource AudioSource { get; private set; }

    public ObstacleTypes ObstacleType
    {
        get { return _obstacleType; }
        set { _obstacleType = value; }
    }

    public void OnEnable()
    {
        if (!_isInitDone) return;
        
        if (ObstacleType == ObstacleTypes.Fire)
        {
            AudioSource.loop = true;
            AudioSource.Play();
        }
    }

    public void Init(string type, int index = 1)
    {
        Collider = GetComponent<CircleCollider2D>();
        ObstacleType = ObstacleTypes.None;
        _sacrificeCnt = 1;

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

        AudioSource = GetComponent<AudioSource>();
        _isInitDone = true;
    }

    // Firewood가 데미지를 받을 때
    public void SubsFireHp(Creature owner, bool byBomb = false)
    {
        // 폭탄은 한번에 없앰
        if (byBomb)
            _hp = 0;
        else 
            _hp = Mathf.Max(0, _hp - owner.AttackDamage);

        if (_hp <= 0f)
        {
            // 그냥 타일맵에 올려진 형태로 남음
            Destroy(Collider);
            Destroy(this);
            GetComponent<SpriteRenderer>().enabled = false;

            //파괴시 소리 재생
            AudioClip audioClip = Managers.Resource.Load<AudioClip>("firedeath hiss");
            Managers.Sound.PlaySFX(audioClip, 0.3f);
            AudioSource.enabled = false;
            return;
        }
    }

    // Poop이 데미지를 받을 때
    public void PoopOnHit(bool byBomb = false)
    {
        AudioClip audioClip = null;
        // 폭탄은 한번에 없앰
        if (byBomb)
            _hp = 0;

        _hp = Mathf.Max(0, _hp - 1);
        int index = 4 - (int)_hp;
        GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>("grid_poop_" + index);

        if (_hp <= 0f)
        {
            // 그냥 타일맵에 올려진 형태로 남음
            Destroy(GetComponent<Rigidbody2D>());
            Destroy(Collider);
            Destroy(this);

            audioClip = Managers.Resource.Load<AudioClip>("plop");
            Managers.Sound.PlaySFX(audioClip, 0.1f);
            //TODO semiwall to cango
        }
        if (audioClip != null)
        Managers.Sound.PlaySFX(audioClip, 0.1f);
    }

    public void OnExplode(Creature owner)
    {
        AudioClip audioClip = null;
        if (ObstacleType == ObstacleTypes.None) return;
        if (ObstacleType == ObstacleTypes.Spike) return;
        if (ObstacleType == ObstacleTypes.Poop) { PoopOnHit(true); return; };
        if (ObstacleType == ObstacleTypes.Fire) { SubsFireHp(null, true); };
        if (ObstacleType == ObstacleTypes.Rock) { audioClip = Managers.Resource.Load<AudioClip>($"rock crumble {Random.Range(0,3)}"); }
        if (ObstacleType == ObstacleTypes.Urn) { audioClip = Managers.Resource.Load<AudioClip>($"pot breaking sound {Random.Range(0,3)}"); }

        //TODO Rock은 흔적을 남기기
        transform.GetComponent<SpriteRenderer>().enabled = false;
        transform.GetComponent<Collider2D>().enabled = false;

        if (audioClip != null)
            Managers.Sound.PlaySFX(audioClip, 0.1f);

        // 길찾기에 쓰이는 충돌 데이터 갱신
        Managers.Map.ChangeCollisionData(transform.position.x - 0.5f, transform.position.y-0.5f, ECellCollisionType.None);

    }

    public void OnExplode(Creature owner, object args)
    {
        //
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Creature creature = collision.gameObject.GetComponent<Creature>();
        if (creature != null)
        {
            if (creature.IsFloating == false && ObstacleType == ObstacleTypes.Spike && Managers.Map.CurrentRoom.RoomType != ERoomType.Sacrifice)
            {
                creature.OnDamaged(null, ESkillType.Spike);
            }
            else if (ObstacleType == ObstacleTypes.Spike && Managers.Map.CurrentRoom.RoomType == ERoomType.Sacrifice)
            {
                creature.OnDamaged(null, ESkillType.Spike);
                Managers.Game.GetSacrificeReward(_sacrificeCnt++);
            }
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
