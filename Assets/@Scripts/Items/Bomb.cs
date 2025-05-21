using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;
using static Utility;
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
            var go = collider.GetComponent<MonoBehaviour>();
            if (go == null) go = collider.GetComponentInParent<MonoBehaviour>();
            if (go is IExplodable)
            {
                var targetGo = go as IExplodable;

                if (go is Door)
                    targetGo.OnExplode(Owner, collider.gameObject.name);
                else
                    targetGo.OnExplode(Owner);
            }
        }
    }

    public void Destroyprefab()
    {
        Destroy(gameObject);
    }
}
