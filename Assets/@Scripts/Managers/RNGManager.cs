using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RNGManager
{
    public long Sn { get; private set; }
    public int A { get; private set; } = 1103515245;
    public int C { get; private set; } = 12345;
    public long M { get; private set; } = (long)1 << 31;

    public int Rand_Cnt { get; private set; } = 0;

    RNGManager()
    {
        Sn = 0;
    }

    public RNGManager(string seed)
    {
        SeedToInt(seed);
    }

    public RNGManager(long seed)
    {
        Sn = seed;
    }

    private void SeedToInt(string seed)
    {
        int temp = 0;
        long res = 0;
        long power = 1;
        for (int i = 8; i >= 0; i--)
        {
            if (seed[i] == '-') continue;
            if (seed[i] < 'A')
                temp += seed[i] - '0';
            if (seed[i] >= 'A')
                temp += 10 + (seed[i] - 'A');

            res += temp * power;
            power *= 36;
            temp = 0;
        }
        Sn = res % M;
    }

    #region RAND_FUNCTIONS
    public void Rand()
    {
        // Sn+1 = (A x Sn + C ) Mod M
        Sn = (A * Sn + C) % M;
        Rand_Cnt++;
    }

    // 반드시 0이상의 수를 입력
    /// <summary>
    /// [left, right]
    /// </summary>
    /// <param name="left">MinInclusive</param>
    /// <param name="right">MaxInclusive</param>
    /// <returns></returns>
    public int RandInt(int left, int right)
    {
        if (left == 0 && right == 0) return 0;
        Rand();
        return (int)((Sn % (right - left + 1)) + left);
    }
    
    // 반드시 0이상의 수를 입력
    public int RandInt(int left, long right)
    {
        Rand();
        return (int)((Sn % (right - left + 1)) + left);
    }

    // 반드시 0이상의 수를 입력
    public int RandInt(int right)
    {
        return RandInt(0, right);
    }

    // 반드시 0이상의 수를 입력
    public float RandFloat()
    {
        Rand();
        return (float)((Sn % 1000)) / 1000.0f;
    }

    // P보다 작을 경우 true;
    public bool Chance(int p)
    {

        return (RandInt(1, 100) <= p);
    }

    public T Choice<T>(List<T> rooms)
    {
        int cnt = rooms.Count();
        int temp = RandInt(0, cnt - 1);
        return rooms[temp];
    }

    public int Next()
    {
        return RandInt(0, Sn - 1);
    }


    #endregion
}
