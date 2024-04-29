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
    public float AttackDamage { get; set; } = 1;
    public float time { get; set; } = 1;
    public Tilemap Tilemap { get; set; }

    protected Creature Owner { get; set; }

    private Coroutine _coroutine;

    private List<GameObject> _transparentObject;

    private Collider2D[] colliders;

    private ESkillType _skillType = ESkillType.Bomb;

    public void SetInfo(Creature Owner)
    {
        this.Owner = Owner;
        transform.position = Owner.transform.position;

        _coroutine = StartCoroutine(CoReserveExplosion(time));
    }

    private IEnumerator CoReserveExplosion(float limit)
    {
        yield return new WaitForSeconds(limit);
        BombExplosion();
        //Destroy(gameObject);
    }

    public void BombExplosion()
    {
        Collider2D[] hit = Physics2D.OverlapBoxAll(transform.position, new Vector2(Range, Range), 0);
        foreach (Collider2D collider in hit)
        {
            var temp = collider.gameObject;
            temp.GetComponent<Creature>().OnDamaged(Owner, _skillType);
        }

        Destroy(gameObject);
    }
}
