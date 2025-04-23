using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class BaseObject : MonoBehaviour
{

    private void Awake()
    {
        Init();
    }

    public virtual void Init()
    {

    }

    public virtual void OnDead()
    {
        //TODO
        //Dead Animation
        Destroy(gameObject);
    }

    public virtual void Onexplode(Creature owner)
    { 

    }
}
