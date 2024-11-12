using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class Projectile : MonoBehaviour
{
    public Creature Owner { get; set; }
    protected Rigidbody2D Rigidbody { get; set; }

    protected Collider2D Collider { get; set; }


    private SpriteRenderer _spriteRenderer;
    private Vector2 _target;
    private Coroutine _coroutine;

    private ESkillType _skillType = ESkillType.Projectile;

    private void Awake()
    {
        _spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        //하드코딩 수정
        _spriteRenderer.sortingOrder = 12;
        Rigidbody = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector2(0.5f, 0.5f);
        Collider = transform.GetChild(1).gameObject.GetComponent<Collider2D>();

    }

    public void SetInfo(Vector2 origin, Vector2 targetDir, Creature owner, bool _isRandom = false, string spriteName = "bulletatlas_7")
    {
        _spriteRenderer.sprite = Managers.Resource.Load<Sprite>("bulletatlas_7");
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



        Collider.excludeLayers = mask;

        _coroutine = StartCoroutine(CoRserveDestroy(Owner.Range));
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        if (other.gameObject.tag == "Trapdoor") return;
        if ("RightDownLeftUpColliderItemHolder".Contains(other.gameObject.name))
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);

        }
        else
        {
            var go = other.GetComponent<Creature>() == null ? other.transform.parent.GetComponent<Creature>() : other.GetComponent<Creature>();
            if (Owner.CreatureType == go.CreatureType) return;
            go.OnDamaged(Owner, _skillType);
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
