using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Fistula_pieces : MonoBehaviour, IExplodable
{
    private Vector3 _dirVec;
    private Rigidbody2D _rb;
    //큰 덩어리 게임 오브젝트
    private Boss_Fistula _fistula;
    public float Speed { get; private set; }

    public Vector3 DirVec
    {
        get { return _dirVec; }
        set
        {
            if (_dirVec != value)
            {
                _dirVec = value;
                _rb.velocity = DirVec * Speed;
            }
        }
    }

    private void Update()
    {
        if (_rb != null) return;
        _rb.velocity = DirVec.normalized * Speed;
    }

    public void Init(Vector3 dirVec, float speed, Boss_Fistula fistula)
    {
        _rb = GetComponent<Rigidbody2D>();
        Speed = speed;
        DirVec = dirVec;
        _fistula = fistula;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Projectile" || collision.gameObject.tag == "Player")
        {
            StartCoroutine(CoFlicker());
        }
        else
        {
            Vector3 reflectVec = Vector3.Reflect(DirVec, collision.GetContact(0).normal);
            DirVec = reflectVec;
        }
    }

    public IEnumerator CoFlicker()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        //Change Sprite
        for (int i = 1; i <= 2; i++)
        {
            if (i % 2 == 0)
            {
                spriteRenderer.color = new Color32(255, 255, 255, 255);
            }
            else
            {
                spriteRenderer.color = new Color32(255, 127, 127, 255);
            }
            yield return new WaitForSeconds(0.05f);
        }

        //Change Sprite
        yield return null;
    }

    public void OnExplode(Creature owner)
    {
        _fistula.OnExplode(owner, gameObject.name);
    }
    public void OnExplode(Creature owner, object args)
    {
        
    }
}
