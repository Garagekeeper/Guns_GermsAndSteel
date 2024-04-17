using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHead : MonoBehaviour
{
    public MainCharacter Owner { get; set; }

    private void Awake()
    {
        Owner = transform.parent.GetComponent<MainCharacter>();
    }
    public void OnAttackEventCalled()
    {
        Owner.GenerateProjectile();
    }
}
