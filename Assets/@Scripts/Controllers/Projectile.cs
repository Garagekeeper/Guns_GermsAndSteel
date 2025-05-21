using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static Define;
using static UnityEngine.Rendering.DebugUI;
using Random = UnityEngine.Random;

public class Projectile : MonoBehaviour
{
    public Creature Owner { get; set; }
    protected Rigidbody2D Rigidbody { get; set; }

    protected CircleCollider2D Collider { get; set; }

    private SpriteRenderer _spriteRenderer;
    private Vector2 _target;
    private Coroutine _coroutine;

    int spriteSize = 0;
    bool _isBlood = false;

    //private bool _isColliding = false;

    private ESkillType _skillType = ESkillType.Projectile;

    private void Awake()
    {
        _spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        //하드코딩 수정
        _spriteRenderer.sortingOrder = 12;
        Rigidbody = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector2(1f, 1f);
        Collider = transform.GetChild(1).gameObject.GetComponent<CircleCollider2D>();

    }

    public void SetInfo(Vector2 origin, Vector2 targetDir, Creature owner, bool isRandom = false, bool isBlood = false)
    {
        //공격력에 따라서 눈물의 크기가 바뀌도록
         _isBlood = isBlood;
        float atk = owner.AttackDamage;
        int size = 0;
        float radius = 0;

        #region sprite/radiusSize
        if (atk < 0.5f)
        {
            radius = 0.1f;
            size = 0;
        }
        else if (atk < 0.75f)
        {
            radius = 0.14f;
            size = 1;
        }
        else if (atk < 0.9f)
        {
            radius = 0.14f;
            size = 2;
        }
        else if (atk < 1f)
        {
            radius = 0.17f;
            size = 3;
        }
        else if (atk < 2.2f)
        {
            radius = 0.2f;
            size = 4;
        }
        else if (atk < 3f)
        {
            radius = 0.2f;
            size = 5;
        }
        else if (atk < 3.5f)
        {
            radius = 0.23f;
            size = 6;
        }
        else if (atk < 4.2f)
        {
            radius = 0.25f;
            size = 7;
        }
        else if (atk < 4.8f)
        {
            radius = 0.31f;
            size = 8;
        }
        else if (atk < 5f)
        {
            radius = 0.34f;
            size = 9;
        }
        else if (atk < 7.5f)
        {
            radius = 0.37f;
            size = 10;
        }
        else if (atk < 9f)
        {
            radius = 0.43f;
            size = 11;
        }
        else if (atk >= 9f)
        {
            radius = 0.46f;
            size = 12;
        }
        #endregion

        spriteSize = ((int)size);
        Collider.radius = radius;
        //transform.GetChild(1).position = new Vector3(0, -(Collider.radius * 4 - (Collider.radius * 4 - Collider.radius / 2) * (size / 12f)), 0);
        transform.GetChild(1).position = new Vector3(0,0,0);

        

        GetComponent<Animator>().enabled = true;

        Owner = owner;
        transform.position = origin;
        if (isRandom)
        {
            if (Mathf.Abs(targetDir.x) > Mathf.Abs(targetDir.y)) transform.position += new Vector3(Random.Range(0.0f, 1.0f) * targetDir.x, Random.Range(-1.0f, 1.0f));
            if (Mathf.Abs(targetDir.x) < Mathf.Abs(targetDir.y)) transform.position += new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(0.0f, 1.0f) * targetDir.y);
        }
        Vector2 correction = new Vector2(0, 0);

        //?
        if (Vector2.Dot(targetDir, owner.Rigidbody.velocity) > 0)
            correction = owner.Rigidbody.velocity * 0.5f;
        _target = targetDir * Owner.ShotSpeed * 10 + correction;

        LayerMask mask = 0;
        if (owner.CreatureType == ECreatureType.MainCharacter) mask |= (1 << 6);
        if (owner.CreatureType == ECreatureType.Monster) mask |= (1 << 7);
        if (owner.CreatureType == ECreatureType.Boss) mask |= (1 << 10);
        mask |= (1 << 8);
        mask |= (1 << 14);
        //TimerTriger
        mask |= (1 << 16);
        //Pickup
        mask |= (1 << 17);

        Collider.excludeLayers = mask;

        _coroutine = StartCoroutine(CoRserveDestroy(Owner.Range));
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        //if (_isColliding) return;
        //_isColliding = true;
        if (other == null) return;
        if (other.gameObject.tag == "TrapDoor") return;
        if (other.gameObject.tag == "ItemHolder") return;
        if (other.gameObject.tag == "ProjectileCollider" || other.gameObject.tag == "Door" )
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);
        }
        else if (other.gameObject.CompareTag("Fire"))
        {
            other.GetComponent<Obstacle>().SubsFireHp(Owner);
            if (_coroutine != null)
                StopCoroutine(_coroutine);
        }
        else if(other.gameObject.CompareTag("Poop"))
        {
            other.GetComponent<Obstacle>().PoopOnHit();
            if (_coroutine != null)
                StopCoroutine(_coroutine);
        }
        else if (other.gameObject.CompareTag("Rock") || other.gameObject.CompareTag("Urn"))
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);
        }else if (other.gameObject.CompareTag("Boss") && Owner.CreatureType == ECreatureType.Monster)
        {
            return;
        }
        else
        {
            var go = Utility.GetTFromParentComponent<Creature>(other.gameObject);
            if (go != null && Owner.CreatureType != go.CreatureType)
                go.OnDamaged(Owner, _skillType, other.gameObject.name);
            else return;
        }
        if (_coroutine != null)
            StopCoroutine(_coroutine);

        DestroyProjectile();
    }

    private IEnumerator CoRserveDestroy(float lifetime)
    {
        Rigidbody.velocity = _target;
        SpriteDownAnim();
        yield return new WaitForSeconds(lifetime * 0.9f);
        DestroyProjectile();
    }

    private IEnumerator CoDestroy()
    {
        Animator anim = transform.GetComponent<Animator>();
        int index = 0;
        if (spriteSize <= 3)
            index = 3;
        else if (spriteSize <= 7)
            index = 7;
        else if (spriteSize <= 12)
            index = 12;

        string animstr = _isBlood ? $"RBulletDestroy_{index}" : $"BulletDestroy_{index}";
        anim.Play(animstr);
        yield return null;

        float delay = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        Collider.enabled = false;
        yield return new WaitForSeconds(delay);

        Destroy(gameObject);
    }

    private void SpriteDownAnim()
    {
        //transform.GetChild(0).position = Vector3.Lerp(transform.position, new Vector3(0,-0.4f,0), 6f * Time.fixedDeltaTime);
        //transform.GetChild(0).GetComponent<Rigidbody2D>().gravityScale = 1;

        string animstr = _isBlood ? $"RBulletDown_{spriteSize}" : $"BulletDown_{spriteSize}";
        GetComponent<Animator>().Play(animstr);
    }

    private void DestroyProjectile()
    {
        Rigidbody.velocity = Vector2.zero;
        StartCoroutine(CoDestroy());
    }

}
