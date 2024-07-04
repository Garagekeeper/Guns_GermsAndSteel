using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    private string SeedString = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    public string Seed { get; private set; }
    public event Action<int, int, string> ChargeBarEnevnt;

    public void UseActiveItem(int coolDownGage, int coolTime, string type)
    {
        ChargeBarEnevnt?.Invoke(coolDownGage, coolTime, type);
    }

    private string GenerateSeed()
    {
        string temp = "";
        for (int i = 0; i < 10; i++)
        {
            int tempInt = UnityEngine.Random.Range(0, SeedString.Length);
            temp += SeedString[i];
        }

        return temp;
    }

    public void Init()
    {
        SeedString = GenerateSeed();
        Debug.Log(SeedString);
    }


}
