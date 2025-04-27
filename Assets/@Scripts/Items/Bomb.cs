using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;
using static UnityEngine.GraphicsBuffer;

public class Bomb : MonoBehaviour
{
    public int Range { get; set; } = 3;
    public float AttackDamage { get; set; } = 1.0f;
    public float time { get; set; } = 1;
    public Tilemap Tilemap { get; set; }

    protected Creature Owner { get; set; }

    private Coroutine _coroutine;

    private List<GameObject> _transparentObject;

    private Collider2D[] colliders;

    private ESkillType _skillType = ESkillType.Bomb;

    private Animator _animator;

    public void SetInfo(Creature Owner)
    {
        this.Owner = Owner;
        transform.position = Owner.transform.position;
        _animator = GetComponent<Animator>();

        _coroutine = StartCoroutine(CoReserveExplosion(time));
    }

    private IEnumerator CoReserveExplosion(float limit)
    {
        yield return new WaitForSeconds(limit);
        BombExplosion();
    }

    public void BombExplosion()
    {
        _animator.Play("BombExplode");
        Collider2D[] hit = Physics2D.OverlapBoxAll(transform.position, new Vector2(Range, Range), 0);
        foreach (Collider2D collider in hit)
        {
            var temp = collider.gameObject;
            //TODO 폭탄의 경우 여러 물체와 상호작용한다
            //모든 Object의 조상을 만들어서 Object 타입을 통해 적절한 상호작용으로 교체하자.
            temp.GetComponent<Creature>()?.OnDamaged(Owner, _skillType);
            //Obstacle
            //temp.GetComponent<Obstacle>()?
            //Door
            temp.transform.parent?.GetComponent<Door>()?.Break(temp.name);
            temp.GetComponent<Obstacle>()?.OnExplode();
        }
    }

    public void Destroyprefab()
    {
        Destroy(gameObject);
    }
}
