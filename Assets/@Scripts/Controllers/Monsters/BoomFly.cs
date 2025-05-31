using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using UnityEngine.Rendering.Universal;
using Unity.Burst.CompilerServices;

public class BoomFly : Monster
{
    public GameObject SFXSource { get; private set; }

    void Awake()
    {
        Init();
    }

    private void Start()
    {
        StartCoroutine(CoUpdateAI());
    }

    public override void Init()
    {
        base.Init();
        Vector3[] dV = { new Vector3(1, 1), new Vector3(1, -1), new Vector3(-1, 1), new Vector3(-1, -1) };
        MonsterType = EMonsterType.BoomFly;
        CreatureSize = ECreatureSize.Small;
        CreatureMoveState = ECreatureMoveState.Designated;
        _isFloating = true;
        Speed = 3.0f;
        Range = 5.0f;

        TargetPos = dV[Random.Range(0, 4)];
        CreatureState = ECreatureState.Move;

        //audio
        AudioClip audioClip = Managers.Resource.Load<AudioClip>("insect swarm");
        SFXSource = Managers.Sound.PlaySFX(audioClip, 0.05f, true);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        ReflectTargetVecor(collision);
    }

    public override void OnDead()
    {
        CreatureState = ECreatureState.Explosion;
    }

    //Animation Event
    public void Destroyprefab()
    {
        // pool에 반납
        Managers.Sound.ReturnSFXToPool(SFXSource);
        base.OnDead();
    }

    public void PlayExplosionSound()
    {
        AudioClip audioClip = Managers.Resource.Load<AudioClip>($"boss explosions {Random.Range(0, 2)}");
        Managers.Sound.PlaySFX(audioClip, 0.15f);
    }

}
