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

    private bool _isColliding = false;

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

    public void SetInfo(Vector2 origin, Vector2 targetDir, Creature owner, bool _isRandom = false, bool _isBlood = false)
    {
        //공격력에 따라서 눈물의 크기가 바뀌도록
        int index = _isBlood ? 13 : 0;
        int size = Mathf.Clamp(((Mathf.RoundToInt(owner.AttackDamage) - 1 / 3) + 1), 0, 12);
        _spriteRenderer.sprite = Managers.Resource.Load<Sprite>("bulletatlas_" + (size + index));
        Collider.radius = _spriteRenderer.bounds.size.x / 2;
        //transform.GetChild(1).position = new Vector3(0, -(Collider.radius * 4 - (Collider.radius * 4 - Collider.radius / 2) * (size / 12f)), 0);
        transform.GetChild(1).position = new Vector3(0,0,0);



        Owner = owner;
        transform.position = origin;
        if (_isRandom)
        {
            if (Mathf.Abs(targetDir.x) > Mathf.Abs(targetDir.y)) transform.position += new Vector3(Random.Range(0.0f, 1.0f) * targetDir.x, Random.Range(-1.0f, 1.0f));
            if (Mathf.Abs(targetDir.x) < Mathf.Abs(targetDir.y)) transform.position += new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(0.0f, 1.0f) * targetDir.y);
        }
        Vector2 correction = new Vector2(0, 0);

        if (Vector2.Dot(targetDir, owner.Rigidbody.velocity) > 0)
            correction = owner.Rigidbody.velocity * 0.5f;
        _target = targetDir * Owner.Tears + correction;
        Rigidbody.velocity = _target;

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
        if (_isColliding) return;
        _isColliding = true;
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
        else
        {
            var go = Utility.GetTFromParentComponent<Creature>(other.gameObject);
            if (go == null) return;
            if (Owner.CreatureType == go.CreatureType) return;
            go.OnDamaged(Owner, _skillType, other.gameObject.name);
        }
        Destroy(gameObject);
    }

    private IEnumerator CoRserveDestroy(float lifetime)
    {
        yield return new WaitForSeconds(lifetime * 0.1f);
        if (Mathf.Abs(Rigidbody.velocity.x) > Mathf.Abs(Rigidbody.velocity.y))
            SpriteDownAnim();
        yield return new WaitForSeconds(lifetime * 0.9f);
        DestroyProjectile();
    }

    private void SpriteDownAnim()
    {
        //transform.GetChild(0).position = Vector3.Lerp(transform.position, new Vector3(0,-0.4f,0), 6f * Time.fixedDeltaTime);
        //transform.GetChild(0).GetComponent<Rigidbody2D>().gravityScale = 1;
        transform.GetComponent<Animator>().Play("BulletDown");
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }

}
