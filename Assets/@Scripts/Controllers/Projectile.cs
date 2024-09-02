using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
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
        _spriteRenderer = GetComponent<SpriteRenderer>();
        //하드코딩 수정
        _spriteRenderer.sprite = Managers.Resource.Load<Sprite>("bulletatlas_7");
        _spriteRenderer.sortingOrder = 12;
        Rigidbody = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector2(0.5f, 0.5f);
        Collider = gameObject.GetComponent<Collider2D>();

    }

    public void SetInfo(Vector2 origin, Vector2 targetDir, Creature owner)
    {
        Owner = owner;
        transform.position = origin;
        Vector2 correction = new Vector2(0, 0);
        
        if (Vector2.Dot(targetDir, owner.Rigidbody.velocity) > 0)
            correction = owner.Rigidbody.velocity * 0.5f;
        _target = targetDir * Owner.Tears + correction;
        Rigidbody.velocity = _target;
        
        LayerMask mask = 0;
        if (owner.CreatureType == ECreatureType.MainCharacter) mask |= (1 << 6);
        if (owner.CreatureType == ECreatureType.Monster) mask |= (1 << 7);



        Collider.excludeLayers = mask;

        _coroutine = StartCoroutine(CoRserveDestroy(Owner.Range));
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if ("RightDownLeftUpCollider".Contains(other.gameObject.name))
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);
            Destroy(gameObject);
        }
        else
        {
            other.GetComponent<Creature>().OnDamaged(Owner, _skillType);
        }

        
    }

    private IEnumerator CoRserveDestroy(float lifetime)
    {
        yield return new WaitForSeconds(lifetime * 0.8f);
        if (Mathf.Abs(Rigidbody.velocity.x) > Mathf.Abs(Rigidbody.velocity.y))
            Rigidbody.velocity += new Vector2(0, -2.0f);
        yield return new WaitForSeconds(lifetime * 0.2f);
        Destroy(gameObject);
    }

}
