using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    public event Action<int, int, string> ChargeBarEnevnt;

    public void UseActiveItem(int coolDownGage, int coolTime, string type)
    {
        ChargeBarEnevnt?.Invoke(coolDownGage, coolTime,  type);
    }
}
