using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Projectile : MonoBehaviour
{
    private float Speed = 3.0f;
    protected MainCharacter Owner { get; set; }
    protected Rigidbody2D Rigidbody { get; set; }

    protected Collider2D Collider { get; set; }

    private SpriteRenderer _spriteRenderer;
    private Vector2 target;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = Managers.Resource.Load<Sprite>("bulletatlas_7");
        Rigidbody = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector2(0.5f, 0.5f);
        Collider = gameObject.GetComponent<Collider2D>();

    }

    public void SetInfo(Vector2 origin, Vector2 targetDir)
    {
        transform.position = origin;
        target = targetDir * Speed;
        Rigidbody.velocity = target;
        LayerMask mask = 0;
        mask |= (1 << 6);
        Collider.excludeLayers = mask;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "Collider")
            Destroy(gameObject);
    }

}
