using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    public event Action<int, string> ChargeBarEnevnt;

    public void UseActiveItem(int coolDownGage, string type)
    {
        ChargeBarEnevnt?.Invoke(coolDownGage, type);
    }
}
