using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data;
using UnityEngine;
using static Define;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class GameManager
{
    private CameraController _cam;
    public CameraController Cam
    {
        get
        {
            if (_cam == null)
            {
                _cam = Object.FindObjectOfType<CameraController>();
            }

            return _cam;
        }
    }

    public event Action<int, int, string> ChargeBarEnevnt;
    public void UseActiveItem(int coolDownGage, int coolTime, string type)
    {
        ChargeBarEnevnt?.Invoke(coolDownGage, coolTime, type);
    }

    #region MAP_GENERATING

    private string SeedString = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public string Seed { get; private set; }
    public long Sn { get; set; }
    //백준 30043의 수치를 참고하였음
    public int A { get; private set; } = 1103515245;
    public int C { get; private set; } = 12345;
    public long M { get; private set; } = (long)1 << 31;

    public int Rand_Cnt { get; private set; } = 0;

    public int N { get; set; }

    public int StageNumber { get; set; } = 0;

    int _baseRoomCountMax = 20;
    int _baseRoomCountMin = 10;


    public void Init()
    {
        Seed = GenerateSeed();
        SeedToInt();
        Debug.Log(Seed);

        //0. N(스테이지에 만들 방의 개수) 설정
        N = (int)(Sn % ((_baseRoomCountMax - _baseRoomCountMin) + 1 + StageNumber * 2) + _baseRoomCountMin);

    }

    private string GenerateSeed()
    {
        string temp = "";
        for (int i = 0; i < 9; i++)
        {
            int tempInt = UnityEngine.Random.Range(0, SeedString.Length);
            if (i == 4)
                temp += "-";
            else
                temp += SeedString[tempInt];
        }

        return temp;
    }

    private void SeedToInt()
    {
        int temp = 0;
        long res = 0;
        long power = 1;
        for (int i = 8; i >= 0; i--)
        {
            if (Seed[i] == '-') continue;
            if (Seed[i] < 'A')
                temp += Seed[i] - '0';
            if (Seed[i] >= 'A')
                temp += 10 + (Seed[i] - 'A');

            res = res + temp * power;
            power = power * 36;
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

    public int RandInt(int left, int right)
    {
        Rand();
        return (int)((Sn % (right - left + 1)) + left);
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


    #endregion

    #endregion

    public void GoToNextStage() { }
    public void GoToNextRoom(string dir)
    {
        RoomClass currentRoom = Managers.Map.CurrentRoom;
        Vector3 newPos = new Vector3();
        int index = 0;
        if (currentRoom == null)
        {
            Debug.Log("Err Current Room is Null!");
        }

        if (currentRoom.IsClear)
        {
            switch (dir)
            {
                case "Right":
                    index = 0;
                    break;
                case "Down":
                    index = 1;
                    break;
                case "Left":
                    index = 2;
                    break;
                case "Up":
                    index = 3;
                    break;
            }
            newPos.x = currentRoom._adjacencentRooms[index].XPos;
            newPos.y = currentRoom._adjacencentRooms[index].YPos;

        }
        //playerMove
        MovePlayerToNextRoom(index);
        //CameraMove
        MoveCameraToNextRoom(currentRoom._adjacencentRooms[index]);
        Managers.Map.CurrentRoom = currentRoom._adjacencentRooms[index];

        
        foreach (var temp in Managers.Object.MainCharacters)
        {
            temp.CanMove = true;
        }
    }


    public void MovePlayerToNextRoom(int index)
    {
        Vector3 newPos = new Vector3();
        switch (index)
        {
            case 0:
                newPos = new Vector3(4.5f, 0);
                break;
            case 1:
                newPos = new Vector3(0, -4.5f);
                break;
            case 2:
                newPos = new Vector3(-4.5f, 0);
                break;
            case 3:
                newPos = new Vector3(0, 4.5f);
                break;
        }

        foreach (var temp in Managers.Object.MainCharacters)
        {
            temp.transform.position = temp.transform.position + newPos;
        }
    }

    public void MoveCameraToNextRoom(RoomClass nextRoom)
    {
        Vector3 newPos = nextRoom.Transform.position;
        newPos.z = -10;
        Managers.Game.Cam.TargetPos = newPos;
    }

    public void TPToNormalRandom()
    {
        List<RoomClass> list = new List<RoomClass>();
        foreach (RoomClass r in Managers.Map.Rooms)
        {
            if (r.RoomType == RoomClass.ERoomType.Normal)
            {
                list.Add(r);
            }
        }

        RoomClass chosen = Choice(list);
        Vector3 newPos = chosen.Transform.position;
        foreach (var mc in Managers.Object.MainCharacters)
        {
            mc.CanMove = false;
        }

        foreach (var mc in Managers.Object.MainCharacters)
        {
            mc.transform.position = newPos;
        }

        Managers.Map.CurrentRoom = chosen;

        foreach (var mc in Managers.Object.MainCharacters)
        {
            mc.CanMove = true;
        }

        newPos.z = -10f;
        Cam.MoveCameraWithoutLerp(newPos);
    }
}
