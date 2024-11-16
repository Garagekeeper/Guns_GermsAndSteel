using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Fistula_pieces : MonoBehaviour
{
    private Vector3 _dirVec;
    private Rigidbody2D _rb;
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

    public void Init(Vector3 dirVec, float speed)
    {
        _rb = GetComponent<Rigidbody2D>();
        Speed = speed;
        DirVec = dirVec;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Projectile" || collision.gameObject.tag == "Player")
        {

        }
        else
        {
            Vector3 reflectVec = Vector3.Reflect(DirVec, collision.GetContact(0).normal);
            DirVec = reflectVec;
        }
    }
}
