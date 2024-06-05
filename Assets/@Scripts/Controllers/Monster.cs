using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : Creature
{
    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        //TODO 몬스터 종류에 따른 스프라이트 불러오기
    }

    void Update()
    {
        #region Attack

        #endregion
    }

    private void UpdateMovement(Vector2 Vel)
    {

    }
}
